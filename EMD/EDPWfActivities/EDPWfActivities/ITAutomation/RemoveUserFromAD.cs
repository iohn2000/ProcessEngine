using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using Kapsch.IS.EDP.WFActivity.ITAutomationWebService;
using Kapsch.IS.ITAutomation.Shared.XML;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.WFActivity.ITAutomation
{
    public class RemoveUserFromAD: BaseEDPActivity, IProcessStep, IActivityValidator
    {
        #region Variables

        /// <summary>
        /// Name of the task which is connected to the script started by IT Automation Service
        /// </summary>
        public virtual string TaskName { get { return "RemoveADUser"; } }

        /// <summary>
        /// Name of the XML Property for the UserID 
        /// </summary>
        public const string VAR_USERID = "UserID";
        public const string VAR_SCRIPTRUNID = "ScriptRunID";

        private string UserID;
        /// <summary>
        /// ID of the actually generated ScriptRun as reference for the WaitActivity 
        /// </summary>
        private string ScriptRunID;

        private ITAutomationWebService.ServiceITAutomationClient serviceClient;
        private ITAutomationWebService.ServiceITAutomationClient ServiceClient
        {
            get
            {
                if (this.serviceClient == null)
                {
                    this.serviceClient = new ITAutomationWebService.ServiceITAutomationClient();
                }

                return this.serviceClient;
            }
        }
        #endregion

        #region constructor

        public RemoveUserFromAD() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }
        #endregion

        #region ActivityMethods
        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            try
            {
                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_USERID, false);
                this.UserID = tmp.VarValue;
            }
            catch (BaseException bEx)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, bEx, msg + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get Workflow-variables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            //set WorkflowmodelInstance as client reference so we can connect a WF to IT AutomationScript
            String clientReference = engineContext.WorkflowModel.InstanceID;
            try
            { //try to initialize WebService. If we cannot connect just retry
                var tmp = this.ServiceClient;
            }
            catch (Exception ex)
            {
                //TODO Test this behaviour
                base.logErrorAndReturnStepState(engineContext, ex, "could not connect to Webservice: " + ServiceClient.Endpoint.Address.Uri.AbsoluteUri.ToString(), EnumStepState.NotCompleted);
            }

            String scriptData = "";

            XmlScriptRunHelper xmlScriptRunHelper = new XmlScriptRunHelper();
            Dictionary<String, String> entries = new Dictionary<string, string>();
            entries.Add(VAR_USERID, this.UserID);
            xmlScriptRunHelper.AddScriptRunData(entries);

            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xmlScriptRunHelper.XmlScriptRun.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                scriptData = stringWriter.GetStringBuilder().ToString();
            }

            ResultObjectItem serviceResult = null;

            // Do call and WaitItem within transaction
            try
            {
                serviceResult = ServiceClient.CreateScriptRun(this.TaskName, scriptData, clientReference);
            }
            catch (Exception ex)
            {
                string msg = "Error creating ITAutomation ScriptRun for this Equipment.";
                if (ex.InnerException != null)
                {
                    msg += " inner Exception: " + ex.InnerException.Message;
                }
                result = this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            logger.Debug("AutomationServieCall resulted with CreatedScriptRun ID: " + serviceResult.ScriptRunID + "with Status: " + serviceResult.Status);

            //get scriptRunID and store result
            this.ScriptRunID = Convert.ToString(serviceResult.ScriptRunID);

            if (!(serviceResult.Status > EnumStatusItem.NOT_DEFINED && serviceResult.Status < EnumStatusItem.ERROR))
            {
                String msg = ("WebServices resultet Status: " + serviceResult.Status.ToString() + " with message " + serviceResult.ErrorMessage);
                return base.logErrorAndReturnStepState(
                    engineContext,
                    new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN, msg),
                    msg,
                    EnumStepState.ErrorToHandle);
            }
            else //TODO possibly, there are other states to retry
            {
                // queue the script after successfully creating it
                ResultObjectItem queueResult = ServiceClient.QueueScript(serviceResult.ScriptRunID);
                if (queueResult.ErrorClass == EnumErrorClassItem.OK)
                {
                    result.ReturnValue = "Successfully created ScriptRun and queued creation script for: " + UserID;
                    logger.Debug("resulting ok");
                }
                else
                {
                    String msg = ("Trying to queue script. WebServices resultet Status: " + queueResult.Status.ToString() + " with message " + queueResult.ErrorMessage);
                    return base.logErrorAndReturnStepState(
                        engineContext,
                        new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN, msg),
                        msg,
                        EnumStepState.ErrorToHandle);
                }
            }

            logger.Info(this.TaskName + " Run results: " + result.ReturnValue);
            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            // create Activity Variable
            engineContext.SetActivityVariable(VAR_SCRIPTRUNID, ScriptRunID, true);
            // setup return object 
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            result.ReturnValue = "Script "+ TaskName + " successfully ran for: " + UserID;
            return result;
        }

        # endregion

        #region Validation

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
