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
    /// <summary>
    /// <value name="UserID">TestExternal</value>
    /// <value name = "FirstName" > ExternalFirst </ value >
    /// < value name="FamilyName">ExternalLast</value>
    /// <value name = "DisplayName" > ExternalFirst ExternalLast</value>
    /// <value name = "Gender" > M </ value >
    /// < value name="Site">999</value>
    /// <value name = "CompanyShortName" > KBC - TEST </ value >
    /// < value name="Country">AT</value>
    /// <value name = "EmploymentType" > EMPLO </ value >
    /// < value name="CredentialSend">andreas.frank @kapsch.net</value>
    /// <value name = "UserTypeDescription" > ADUserExternalSupplier </ value >
    /// </summary>
    public class AddUserToADActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        #region Variables

        /// <summary>
        /// Name of the task which is connected to the script started by IT Automation Service
        /// </summary>
        public virtual string TaskName { get { return "CreateADUser"; } }

        /// <summary>
        /// Name of the XML Property for the UserID 
        /// </summary>
        public const string VAR_USERID = "UserID";

        public const string VAR_FIRSTNAME = "FirstName";
        public const string VAR_FAMILYNAME = "FamilyName";
        public const string VAR_DISPLAYNAME = "DisplayName";
        public const string VAR_GENDER = "Gender";
        public const string VAR_SITE = "Site";
        public const string VAR_COMPANYSHORTNAME = "CompanyShortName";
        public const string VAR_COUNTRY = "Country";
        public const string VAR_EMPLOYMENTTYPE = "EmploymentType";
        public const string VAR_MAILADRESS = "MailAdress";
        public const string VAR_USERTYPE = "UserType";
        public const string VAR_PERSONID = "PersonID";

        public const string VAR_SCRIPTRUNID = "ScriptRunID";

        private string UserID;
        private string FamilyName;
        private string FirstName;
        private string DisplayName;
        private string Gender;
        private string Site;
        private string CompanyShortName;
        private string Country;
        private string EmploymentType;
        private string MailAdress;
        private string UserType;
        private string PersonID;

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

        public AddUserToADActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
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
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_FIRSTNAME, false);
                this.FirstName = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_FAMILYNAME, false);
                this.FamilyName = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_DISPLAYNAME, false);
                this.DisplayName = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_GENDER, false);
                this.Gender = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_SITE, false);
                this.Site = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_COMPANYSHORTNAME, false);
                this.CompanyShortName = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_COUNTRY, false);
                this.Country= tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_EMPLOYMENTTYPE, false);
                this.EmploymentType = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_MAILADRESS, false);
                this.MailAdress = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_USERTYPE, false);
                this.UserType = tmp.VarValue;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_PERSONID, false);
                this.PersonID = tmp.VarValue;

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
            entries.Add(VAR_FIRSTNAME, this.FirstName);
            entries.Add(VAR_FAMILYNAME, this.FamilyName);
            entries.Add(VAR_DISPLAYNAME, this.DisplayName);
            entries.Add(VAR_GENDER, this.Gender);
            entries.Add(VAR_SITE, this.Site);
            entries.Add(VAR_COMPANYSHORTNAME, this.CompanyShortName);
            entries.Add(VAR_COUNTRY, this.Country);
            entries.Add(VAR_EMPLOYMENTTYPE, this.EmploymentType);
            entries.Add("CredentialSend", this.MailAdress);
            entries.Add("UserTypeDescription", this.UserType);
            entries.Add(VAR_PERSONID, this.PersonID);
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
                String msg = ("WebServices resulted Status: " + serviceResult.Status.ToString() + " with message " + serviceResult.ErrorMessage);
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
                    String msg = ("Trying to queue script. WebServices resulted Status: " + queueResult.Status.ToString() + " with message " + queueResult.ErrorMessage);
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
            result.ReturnValue = "Successfully created CreateUser-ScriptRun for: " + UserID;
            return result;
        }
        #endregion

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
