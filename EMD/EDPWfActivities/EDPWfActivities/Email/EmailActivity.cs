using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.TemplateEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.Email
{
    public class EmailActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {
        internal Dictionary<string, string> renderDictionary;
        private string sender = "";
        internal string EmailSubject;
        internal List<Tuple<string, string, string>> empfaengerList;
        internal string EmailTemplate = "";
        private string demoRecipient;
        private string effectedPersonEmplGuid = "";
        private bool IsBodyHtml;


        public EmailActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("Successfully sent. ", EnumStepState.Complete);
            logger.Debug(string.Format("{0} : EmailActivity.Finish() executed. (empty function)", base.getWorkflowLoggingContext(engineContext)));
            return ret;
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StringWriter sw;
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);

            this.renderDictionary = new Dictionary<string, string>();
            this.empfaengerList = new List<Tuple<string, string, string>>(); // item1 = mail, 2=firname, 3=familyname

            try
            {

                Variable tmp = base.GetProcessedActivityVariable(engineContext, "emailBody", true);
                if (tmp == null)
                    this.EmailTemplate = "";
                else
                    this.EmailTemplate = tmp.VarValue;
                this.renderDictionary.Add("emailBody", this.EmailTemplate);

                tmp = engineContext.GetWorkflowVariable("0.EffectedPersonEmploymentGuid");
                this.effectedPersonEmplGuid = tmp.VarValue;

                tmp = base.GetProcessedActivityVariable(engineContext, "isBodyHtml", true);
                this.IsBodyHtml = true;
                if (tmp != null)
                {
                    this.renderDictionary.Add("isBodyHtml", tmp.VarValue);
                    if (!bool.TryParse(tmp.VarValue, out this.IsBodyHtml))
                        this.IsBodyHtml = true;
                }
                else
                {
                    this.renderDictionary.Add("isBodyHtml", "true");
                }

                //get all 0.
                var nullPunkts = (List<Variable>)engineContext.WorkflowModel.GetPunktVariables("[starts-with(@name,'0.')]");
                foreach (var item in nullPunkts)
                {
                    tmp = engineContext.GetWorkflowVariable(item.Name);
                    this.renderDictionary.Add(item.Name, item.VarValue);
                }

                //
                // build sender, take demo mode into account
                //
                Variable senderVar = null;

                try
                {
                    senderVar = base.GetProcessedActivityVariable(engineContext, "sender", false);
                    this.sender = senderVar.VarValue;
                }
                catch (Exception) { }
                if (this.sender == null || string.IsNullOrWhiteSpace(this.sender))
                    this.sender = "KIBSI-EDP-NoReply@kapsch.net";
                //
                // build subject
                //
                tmp = base.GetProcessedActivityVariable(engineContext, "subject", true);
                if (tmp == null)
                    this.EmailSubject = "";
                else
                    this.EmailSubject = tmp.GetStringValue();

                //
                // build demo recipient
                //
                this.demoRecipient = base.GetSettingWithFallbackToAppConfig(engineContext, "demoRecipient", "DefaultDemoModeRecipient", "KIBSI-EDP-admin@kapsch.net");
                if (base.isDemoModeOn(engineContext))
                {
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : DEMO Mode is ON. Recipient is now : " + demoRecipient);
                }

                //
                //build recipient(s)
                //
                string queryString = "";

                try
                {
                    //get email from person empl guid
                    string recipientEMPLGuid = base.GetProcessedActivityVariable(engineContext, "recipient", false).GetStringValue();

                    // handle multiple recipients separated by semi-colon ;
                    // todo FindTaskApprover can to that, prepare a List<string>

                    string[] separator = { ";" };
                    List<string> allRecipientsCodes = recipientEMPLGuid.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList();

                    sw = new StringWriter();
                    ObjectDumper.Write(allRecipientsCodes, 2, sw);
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : allRecipientsCodes : " + sw.ToString());

                    this.empfaengerList = this.BuildEmpfaengerListeFromRecipientsCode(engineContext, allRecipientsCodes);

                    sw = new StringWriter();
                    ObjectDumper.Write(this.empfaengerList, 5, sw);
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : after @@ queries  empfaengerList : " + sw.ToString());


                }
                catch (Exception ex)
                {
                    string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error, Query " + queryString + " does not return a value.";
                    return base.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.ErrorToHandle);
                }
            }
            catch (Exception ex)
            {
                string errMsg = "error running Init() : " + ex.Message;
                return base.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.ErrorToHandle);
            }

            return ret;
        }

        internal List<Tuple<string, string, string>> BuildEmpfaengerListeFromRecipientsCode(EngineContext engineContext, List<string> allRecipientsCodes)
        {
            StringWriter sw;
            var listOfRecipients = new TaskItemManager().FindTaskApproverForEffectedPerson(allRecipientsCodes, this.effectedPersonEmplGuid);
            sw = new StringWriter();
            ObjectDumper.Write(listOfRecipients, 4, sw);
            logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : after call 'FindTaskApproverForEffectedPerson()' listOfRecipients : " + sw.ToString());

            EntityQuery entityQuery = new EntityQuery();
            Type propType;
            string mm, fi, fa;
            foreach (var recp in listOfRecipients)
            {
                if (recp.Item1 != null)
                {
                    try
                    {
                        mm = entityQuery.Query("MainMail@@" + recp.Item2, out propType).ToString();
                    }
                    catch (Exception ex)
                    {
                        string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error trying to get MainMail for person:" + recp.Item2;
                        throw new Exception(errMsg, ex);
                    }

                    try
                    {
                        fi = entityQuery.Query("FirstName@@" + recp.Item2, out propType).ToString();
                    }
                    catch (Exception ex)
                    {
                        string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error trying to get FirstName for person:" + recp.Item2;
                        throw new Exception(errMsg, ex);
                    }

                    try
                    {
                        fa = entityQuery.Query("FamilyName@@" + recp.Item2, out propType).ToString();
                    }
                    catch (Exception ex)
                    {
                        string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error trying to get FamilyName for person:" + recp.Item2;
                        throw new Exception(errMsg, ex);
                    }

                }
                else // special case where there is no empl or pers, just an email
                {
                    mm = recp.Item2;
                    fi = "";
                    fa = "";
                }
                Tuple<string, string, string> oneItem = new Tuple<string, string, string>(mm, fi, fa);
                this.empfaengerList.Add(oneItem);
            }

            return this.empfaengerList;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("Send mail: ", EnumStepState.Complete);

            //add all activity specific variables and
            //    all 0. variables to dictionary for render engine
            ITemplateEngine renderer = new NustacheRenderer();

            try
            {
                WfHelper wfHelper = new WfHelper();
                WorkflowMailer emailer = new WorkflowMailer(base.GetEmailSubjectPrefix());


                DatabaseAccess db = new DatabaseAccess();
                DocumentTemplate docTemplate = db.GetDocumentTemplateByName(this.EmailTemplate);

                foreach (var rec in this.empfaengerList)
                {
                    // item1 = mail, 2=firname, 3=familyname
                    wfHelper.AddOrUpdateDictionary(this.renderDictionary, "RecipientFamilyname", rec.Item3);
                    wfHelper.AddOrUpdateDictionary(this.renderDictionary, "RecipientFirstname", rec.Item2);
                    wfHelper.AddOrUpdateDictionary(this.renderDictionary, "RecipientMainMail", rec.Item1);
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + " : RecipientMainMail : " + rec.Item1);

                    string renderedContent = renderer.RenderTemplateFromString(docTemplate.TMPL_Content, this.renderDictionary);

                    var recpMails = base.isDemoModeOn(engineContext) ? new List<string>() { this.demoRecipient } : new List<string>() { rec.Item1 };

                    emailer.SendEmail(this.sender, recpMails, this.EmailSubject, renderedContent, this.IsBodyHtml);

                    logger.Debug(string.Format("{0} : Email-Activity : To={1} Subject={2}", base.getWorkflowLoggingContext(engineContext), rec.Item1, this.EmailSubject));

                    ret.ReturnValue += " " + rec.Item1;
                }
                ret.ReturnValue += " " + this.EmailSubject + " ";

            }
            catch (BaseException bEx)
            {
                string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error trying to send an email in EmailActivity";
                ret = base.logErrorAndReturnStepState(engineContext, bEx, errMsg, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string errMsg = base.getWorkflowLoggingContext(engineContext) + " : Error trying to send an email in EmailActivity";
                ret = base.logErrorAndReturnStepState(engineContext, ex, errMsg, EnumStepState.ErrorToHandle);
            }

            return ret;
        }

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
}
