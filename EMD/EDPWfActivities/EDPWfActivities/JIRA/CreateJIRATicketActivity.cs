using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.JIRA
{
    public class CreateJIRATicketActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {

        private const string VAR_TICKETBODY = "TicketBody";
        private const string VAR_CLIENTREFERENCEID = "ClientReferenceID";
        
        //public string TicketContent = "";
        public string ClientReferenceID = "empty";
        public string TicketBody;

        public CreateJIRATicketActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            string errorMessage = "error trying to get Workflow-variables";

            try
            {
                Variable tmp;
                tmp = base.GetProcessedActivityVariable(engineContext, VAR_TICKETBODY, false);
                this.TicketBody = tmp.VarValue;
            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = null;

            try
            {
                this.ClientReferenceID = this.CreateJiraTicket(base.getWorkflowLoggingContext(engineContext));

                result = new StepReturn("Created JIRA ticket with ID: " + this.ClientReferenceID, EnumStepState.Complete);
            }
            catch (BaseException bEx)
            {
                string errorMessage = "Error while creating JIRA call. Please contact the EDP Team.";
                result =  this.logErrorAndReturnStepState(engineContext, bEx, errorMessage, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error while creating JIRA call. Please contact the EDP Team.";
                result = this.logErrorAndReturnStepState(engineContext, ex, errorMessage, EnumStepState.ErrorToHandle);
            }

            #region TEST Code
            //TODO this is only test code - REMOVE start
            //string filePath = @"c:\temp\CreateJIRATicketActivity_" + engineContext.WorkflowModel.InstanceID + ".txt";
            //string webServiceURL = "ClientReference: " + this.ClientReferenceID;
            //webServiceURL += "\r\n WOIN: " + engineContext.WorkflowModel.InstanceID;
            //webServiceURL += "\r\n === \r\n" + this.TicketBody + "\r\n ===";
            //webServiceURL += "\r\n === \r\n TESTURL: http://s900b112:7092/Webservice/EDPLiteService.svc/rest/SetStatus/" + engineContext.WorkflowModel.InstanceID
            //    + "/" + this.ClientReferenceID + "/OK/somemessage" + "\r\n ===";

            //try
            //{
            //    File.WriteAllText(filePath, webServiceURL);
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.Message, ex);
            //}

            //logger.Info(base.getWorkflowLoggingContext(engineContext) + " created file: " + filePath);

            //REMOVE end 
            #endregion
            return result;
        }

        /// <summary>
        /// public for unit testing 
        /// </summary>
        /// <returns>issue key if successfull, if failure : error2handle</returns>
        public string CreateJiraTicket(string loggingContext)
        {
            string issueKey;

            // create a connection to JIRA using the Rest client
            string username = ConfigurationManager.AppSettings["JiraTicketUserName"].ToString();
            string password = ConfigurationManager.AppSettings["JiraTicketUserPassword"].ToString();
            string jiraUrl  = ConfigurationManager.AppSettings["JiraUrl"].ToString();

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) || String.IsNullOrWhiteSpace(jiraUrl))
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN, "Misconfigured Jira call in appplication config (Quartz).");

            logger.Debug(loggingContext + "Create Jira call with user: " + username + " and password with length: " + password.Length + " JiraUrl: " + jiraUrl);

            var client = new RestClient(jiraUrl + "/rest/servicedeskapi/request");
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            try
            {
                Stopwatch sw = new Stopwatch();
                
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("application/json", this.TicketBody, ParameterType.RequestBody);

                sw.Start();
                var response = client.Execute(request);
                logger.Debug(loggingContext + " JiraCall Execute() time elapsed[ms]: " + sw.ElapsedMilliseconds.ToString());

                JObject jResp = JObject.Parse(response.Content);
                issueKey = (string)jResp["issueKey"];
                logger.Debug(loggingContext + " JiraCall Response parsed time elapsed[ms]: " + sw.ElapsedMilliseconds.ToString());

                if (issueKey == null)
                    throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN, "Error Parsing Jira Response:\r\n" + jResp.ToString());

                sw.Stop();
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_RUN,ex);
            }

            return issueKey;
        }

        public void TestSimluateJiraReturnCall_EDPLite(string woin, string clientRefID, string loggingContext)
        {
            // EDPLiteRestUrl
            string EDPLiteRestUrl = ConfigurationManager.AppSettings["EDPLiteRestUrl"].ToString();

            string restURL = EDPLiteRestUrl + "SetStatus/" + woin + "/" + clientRefID + "/OK/testcall";
            logger.Debug(loggingContext + " EDP Liste Call REST Url: " + restURL);
            RestClient client = new RestClient(restURL);
            //client.Authenticator = new HttpBasicAuthenticator(username, password);

            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("cache-control", "no-cache");
            //request.AddParameter("application/json", this.TicketBody, ParameterType.RequestBody);

            var response = client.Execute(request);

            //TESTURL: http://s900b112:7092/Webservice/EDPLiteService.svc/rest/SetStatus/" + engineContext.WorkflowModel.InstanceID
            //    + "/" + this.ClientReferenceID + "/OK/somemessage" + "\r\n ===";

        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);

            engineContext.SetActivityVariable(VAR_CLIENTREFERENCEID, this.ClientReferenceID);

            return result;
        }





        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }
    }
}
