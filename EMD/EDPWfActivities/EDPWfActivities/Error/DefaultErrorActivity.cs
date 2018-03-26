using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.Error
{
    public class DefaultErrorActivity : Email.EmailActivity, IProcessStep, IActivityValidator
    {
        private const string VAR_ERRORMESSAGE = "ErrorMessage";
        private const string VAR_ENTITYGUID = "EntityGuid";
        private const string VAR_WORKFLOWNAME = "WorkflowName";
        private const string VAR_WORKFLOWINSTANCE = "WorkflowInstance";
        private const string VAR_ERRORDETAILS = "ErrorDetails";
        private const string SETTING_DEFAULTERRORRECIPIENTS = "ProcessEngine.EDP.DefaultErrorActivity.MailRecipient";
        private const string SETTING_DEFAULTERRORMAILTEMPLATE = "ProcessEngine.EDP.DefaultErrorActivity.MailTemplate";

        public DefaultErrorActivity() : base()
        {
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            string defaultRecipientCodes = "";
            try
            {
                base.PostInitialize(engineContext);

                // if no recipient configures please use default from app.config
                if (this.empfaengerList.Count < 1)
                {
                    defaultRecipientCodes = base.GetSettingWithFallbackToAppConfig(engineContext, "xxxxxxxxx", SETTING_DEFAULTERRORRECIPIENTS, "MAIL_KIBSI-EDP-admin@kapsch.net");
                    string[] separator = { ";" };
                    List<string> allRecipientsCodes = defaultRecipientCodes.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();

                    this.empfaengerList = base.BuildEmpfaengerListeFromRecipientsCode(engineContext, allRecipientsCodes);
                }

                if (string.IsNullOrWhiteSpace(this.EmailTemplate))
                {
                    this.EmailTemplate = base.GetSettingWithFallbackToAppConfig(engineContext, "xxx", SETTING_DEFAULTERRORMAILTEMPLATE, "DefaultEdpErrorEmail");
                }

                if (string.IsNullOrWhiteSpace(this.EmailSubject))
                {
                    this.EmailSubject = "EDP Workflow Error Email : " + engineContext.WorkflowModel.InstanceID;
                }
            }
            catch (Exception ex)
            {
                string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error bulding recipients from codes: '" + defaultRecipientCodes + "'.";
                return base.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.Complete);
            }

            try
            {
                // add error msg to renderDictionary and call it 
                this.renderDictionary.Add(VAR_ERRORMESSAGE, engineContext.LastStepReturn.ReturnValue);
                this.renderDictionary.Add(VAR_ERRORDETAILS, engineContext.LastStepReturn.DetailedDescription);

                //Woin
                string woin = engineContext.WorkflowModel.InstanceID;
                this.renderDictionary.Add(VAR_WORKFLOWINSTANCE, woin);

                // get processentity info
                ProcessEntityManager prenMgr = new ProcessEntityManager();
                List<EMDProcessEntity> woinList = prenMgr.GetList("WFI_ID = \"" + woin + "\"");

                if (woinList.Count > 0)
                {
                    // sort and take latest; just to be Fehlertolerant
                    woinList.Sort((x, y) => y.Created.CompareTo(x.Created));
                    EMDProcessEntity theEntity = woinList.First();
                    this.renderDictionary.Add(VAR_ENTITYGUID, theEntity.EntityGuid);
                    this.renderDictionary.Add(VAR_WORKFLOWNAME, theEntity.WFD_Name);
                }
                else
                {
                    this.renderDictionary.Add(VAR_ENTITYGUID, "not found");
                    this.renderDictionary.Add(VAR_WORKFLOWNAME, "not found");
                }
            }
            catch (Exception ex)
            {
                string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error bulding render dictionary for error activity.";
                return base.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.Complete);
            }

            // get all the variables
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            try
            {
                base.Run(engineContext);
            }
            catch (Exception ex)
            {
                string errMsg = base.getWorkflowLoggingContext(engineContext) + " Error in DefaultErrorActivity.Run() base (=email)";
                logger.Error(errMsg, ex);
            }
            try
            {

                this.WriteToProcessEntity(engineContext);
            }
            catch (Exception ex)
            {
                string errMsg = base.getWorkflowLoggingContext(engineContext) + " Error in DefaultErrorActivity.WriteToProcessEntity()";
                logger.Error(errMsg, ex);
            }

            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        private void WriteToProcessEntity(EngineContext engineContext)
        {
            string modifyComment = engineContext.WorkflowDefinitionName + "-" + engineContext.WorkflowModel.InstanceID + " Update by DefaultErrorActivity.";

            EMDProcessEntity pren = null;
            string wfInstance = engineContext.WorkflowModel.InstanceID;

            ProcessEntityManager prenM = new ProcessEntityManager(null, modifyComment);
            var entities = prenM.GetList("WFI_ID=\"" + wfInstance + "\"");

            if (entities.Count > 1)
            {
                //found more than one processentity for this wf: Write Warning and all but first one.
                logger.Warn(base.getWorkflowLoggingContext(engineContext) + " Duplicate Entries in ProcessEntity found for this WFI_instance");
                pren = entities[0];
            }
            else
            {
                //standard case ... write data
                pren = entities[0];
            }

            pren.WFResultMessages += Environment.NewLine + "Error on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                engineContext.LastStepReturn.ReturnValue + Environment.NewLine + engineContext.LastStepReturn.DetailedDescription;

            prenM.UpdateOrCreate(pren);
        }

        #region validation
        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
