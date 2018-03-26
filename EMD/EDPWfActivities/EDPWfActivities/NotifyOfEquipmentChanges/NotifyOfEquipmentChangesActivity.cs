using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.TemplateEngine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.NotifyOfEquipmentChanges
{
    public class NotifyOfEquipmentChangesActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        private Variable ResultEqHandling;
        private Variable RequestingPersonEmploymentGuid;
        private Variable EffectedPersonEmploymentGuid;

        private Variable Subject;
        private string Sender;
        private string DemoRecipient;
        private Variable wfVariableTargetDate;
        private DateTime TargetDate;
        private Dictionary<string, object> RenderDictionary = new Dictionary<string, object>();
        private List<EqInfo> EqInfos = new List<EqInfo>();
        private WfHelper wfHelper;
        List<Tuple<string, string, string>> recipientList = new List<Tuple<string, string, string>>();
        private string emailTemplateString = "";
        private bool IsHtmlBody;


        public NotifyOfEquipmentChangesActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            DatabaseAccess db = new DatabaseAccess();
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            this.wfHelper = new WFActivity.WfHelper(base.getWorkflowLoggingContext(engineContext));

            try
            {
                this.ResultEqHandling = base.GetProcessedActivityVariable(engineContext, "ResultEqHandling");
                this.RequestingPersonEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "RequestingPersonEmploymentGuid");
                this.EffectedPersonEmploymentGuid = base.GetProcessedActivityVariable(engineContext, "EffectedPersonEmploymentGuid");

                //
                //
                //
                this.Subject = base.GetProcessedActivityVariable(engineContext, "Subject");
                //
                // get mail template
                //
                Variable EmailBody;
                EmailBody = base.GetProcessedActivityVariable(engineContext, "EmailBody");
                DocumentTemplate docTemplate = db.GetDocumentTemplateByName(EmailBody.VarValue);
                this.emailTemplateString = docTemplate.TMPL_Content;
                //
                // Sender
                //
                Variable SenderVar;
                try
                {
                    SenderVar = base.GetProcessedActivityVariable(engineContext, "Sender");
                    this.Sender = SenderVar.VarValue;
                }
                catch (Exception) { }
                if (this.Sender == null || string.IsNullOrWhiteSpace(this.Sender))
                    this.Sender = "KIBSI-EDP-NoReply@kapsch.net";

                //
                // body html
                //
                Variable html;
                html = base.GetProcessedActivityVariable(engineContext, "IsBodyHtml");
                if (html != null)
                {
                    bool? tmp = html.GetBooleanValue().Value;
                    if (tmp != null)
                        this.IsHtmlBody = tmp.Value;
                    else
                        this.IsHtmlBody = true;
                }
                else
                    this.IsHtmlBody = true;

                //
                // build demo recipient
                //
                this.DemoRecipient = base.GetSettingWithFallbackToAppConfig(engineContext, "DemoRecipient", "DefaultDemoModeRecipient", "KIBSI-EDP-admin@kapsch.net");
                if (base.isDemoModeOn(engineContext))
                {
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : DEMO Mode is ON. Recipient is now : " + this.DemoRecipient);
                }


                this.wfVariableTargetDate = base.GetProcessedActivityVariable(engineContext, "TargetDate", false);
                if (!string.IsNullOrWhiteSpace(this.wfVariableTargetDate.VarValue))
                {
                    this.TargetDate = this.wfVariableTargetDate.GetDateValue().Value;
                }
                else
                {
                    string errMsg = " Error parsing TargetDate date: " + this.wfVariableTargetDate.VarValue ?? "" + " continue without Error2Handle";
                    logger.Error(base.getWorkflowLoggingContext(engineContext) + errMsg);
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, errMsg);
                }




                //
                // build RenderDictionary with list of eq with results
                //
                this.addEqResultsToRenderDictionary();

                //
                // build recipient list
                //
                Variable Recipient = base.GetProcessedActivityVariable(engineContext, "Recipient");
                this.recipientList = this.wfHelper.BuildRecipientListForMailing(Recipient.VarValue, this.EffectedPersonEmploymentGuid.VarValue);
                //
                // add all 0.Vars to RenderDictionary
                //
                this.RenderDictionary = wfHelper.AddAllNullPunksVarsToRenderDictionary(engineContext, this.RenderDictionary);

            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string msg = "error trying to get workflowvariables";
                return this.logErrorAndReturnStepState(engineContext, ex, msg, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            logger.Info(base.getWorkflowLoggingContext(engineContext) + " Run() started");
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            ITemplateEngine renderer = new NustacheRenderer();
            try
            {
                WorkflowMailer emailer = new WorkflowMailer(base.GetEmailSubjectPrefix());
                //IEmailSender emailer = new WebServiceEmailSender();

                foreach (var rec in this.recipientList)
                {
                    // item1 = mail, 2=firname, 3=familyname
                    this.addOrUpdate(this.RenderDictionary, "RecipientFamilyname", rec.Item3);
                    this.addOrUpdate(this.RenderDictionary, "RecipientFirstname", rec.Item2);
                    this.addOrUpdate(this.RenderDictionary, "RecipientMainMail", rec.Item1);
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + "RecipientMainMail : " + rec.Item1);

                    string renderedContent = renderer.RenderTemplateFromString(this.emailTemplateString, this.RenderDictionary);

                    var recpMails = base.isDemoModeOn(engineContext) ? new List<string>() { this.DemoRecipient } : new List<string>() { rec.Item1 };

                    emailer.SendEmail(this.Sender, recpMails, this.Subject.VarValue, renderedContent, IsHtmlBody);

                    try
                    {
                        string logLine = string.Format("{0} : Email-Activity : To={1} Subject={2}", base.getWorkflowLoggingContext(engineContext), rec.Item1, this.Subject.VarValue);
                        logger.Debug(logLine);
                    }
                    catch (Exception) { }
                }
            }
            catch (BaseException bEx)
            {
                return logErrorAndReturnStepState(engineContext, bEx, "Error trying to send an email in NotifyOfEquipmentChangeActivity.", EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                return logErrorAndReturnStepState(engineContext,ex,"Error trying to send an email in NotifyOfEquipmentChangeActivity.",EnumStepState.ErrorToHandle);
            }
            return result;
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("", EnumStepState.Complete);
            return result;
        }


        #region Helpers
        private void addEqResultsToRenderDictionary()
        {
            EqInfo einfo = new EqInfo();
            bool header = true;
            // 0=obreGuid;1=eqdeGuid;2=eqName;3=action;4=keep;5=available\r\n
            foreach (string line in this.ResultEqHandling.VarValue.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (header)
                {
                    header = false;
                    continue;
                }
                var cols = line.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                einfo = new EqInfo();
                einfo.EqName = cols[2];
                einfo.IsAvailable = cols[5];
                einfo.Keep = cols[4];
                einfo.WorkflowAction = cols[3];

                this.EqInfos.Add(einfo);
            }
            if (RenderDictionary.ContainsKey("EqInfos")==false) this.RenderDictionary.Add("EqInfos", this.EqInfos);
        }
        private Dictionary<string, object> addOrUpdate(Dictionary<string, object> dic, string key, string val)
        {
            if (dic.ContainsKey(key))
                dic[key] = val;
            else
                dic.Add(key, val);
            return dic;
        }
        #endregion

        #region Validation

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

    public class EqInfo
    {
        public string EqName { get; set; }
        public string WorkflowAction { get; set; }
        public string Keep { get; set; }
        public string IsAvailable { get; set; }
    }
}
