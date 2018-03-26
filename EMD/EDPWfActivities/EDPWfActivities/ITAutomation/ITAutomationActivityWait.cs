using Kapsch.IS.EDP.WFActivity.ITAutomationWebService;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.ITAutomation
{
    public class ITAutomationActivityWait : BaseEDPAsyncActivity, IActivityValidator, IProcessStep
    {
        public const string VAR_SCRIPTRUNID = "ScriptRunID";
        public const string VAR_RETURNSTATUS = "returnStatus";
        public const string VAR_RETURNMESSAGE = "returnMessage";
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

        public EnumStatusItem ScriptStatus { get; private set; }
        public string ScriptMessage { get; private set; }

        public ITAutomationActivityWait() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public override StepReturn PostInitialize(EngineContext engineContext, BaseAsyncRequestResult baseResult)
        {
            StepReturn result = new StepReturn("PostInitialize", EnumStepState.Complete);
            result.ReturnValue = ScriptMessage;

            engineContext.SetActivityVariable(VAR_RETURNSTATUS, ScriptStatus.ToString(), true);
            engineContext.SetActivityVariable(VAR_RETURNMESSAGE, ScriptMessage == null ? "" : ScriptMessage, true);

            logger.Info(string.Format("{0} : Status: {1}-{2}", base.getWorkflowLoggingContext(engineContext), Convert.ToInt32(ScriptStatus), ScriptStatus.ToString()));

            switch (ScriptStatus)
            {
                case EnumStatusItem.INQUEUE:
                case EnumStatusItem.IN_PROCESS:
                case EnumStatusItem.ORDERED:
                    logger.Debug(string.Format("{0} : check Status: INQUEUE/IN_PROCESS/ORDERED => Wait", base.getWorkflowLoggingContext(engineContext)));
                    result.StepState = EnumStepState.Wait;
                    break;


                // all finished continue with workflow
                case EnumStatusItem.DONE:
                    base.finishEngineAlert(engineContext);
                    logger.Debug(string.Format("{0} : check Status: DONE => Complete", base.getWorkflowLoggingContext(engineContext)));
                    result.StepState = EnumStepState.Complete;
                    break;

                case EnumStatusItem.DONE_WITH_ERRORS:
                    base.finishEngineAlert(engineContext);
                    logger.Debug(string.Format("{0} : check Status: DONE_WITH_ERRORS => ErrorToHandle", base.getWorkflowLoggingContext(engineContext)));
                    result.StepState = EnumStepState.ErrorToHandle;
                    result.ReturnValue = this.ScriptMessage ?? string.Empty;
                    break;

                case EnumStatusItem.ERROR:
                case EnumStatusItem.NOT_DEFINED:
                case EnumStatusItem.OK: //also this case must not be set at this time (at leas queued)
                    base.finishEngineAlert(engineContext);
                    logger.Debug(string.Format("{0} : check Status: ERROR/NOT_DEFINED/OK => ErrorToHandle", base.getWorkflowLoggingContext(engineContext)));
                    result.StepState = EnumStepState.ErrorToHandle;
                    result.ReturnValue = this.ScriptMessage ?? string.Empty;
                    break;

                default:
                    base.finishEngineAlert(engineContext);
                    logger.Debug(string.Format("{0} : check Status: UNKNOWN STATUS => ErrorToHandle", base.getWorkflowLoggingContext(engineContext)));
                    result.StepState = EnumStepState.ErrorToHandle;
                    result.ReturnValue = this.ScriptMessage ?? string.Empty;
                    break;
            }

            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            result.ReturnValue = ScriptMessage;
            return result;

        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            // create Activity Variable

            // setup return object 
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            result.ReturnValue = ScriptMessage;
            return result;
        }

        /// <summary>
        /// reading actual status from ScriptRun and return true. Status is converted to Activity StepState in Run() Method
        /// </summary>
        /// <param name="AsyncRequestID"></param>
        /// <param name="AsyncRequestResult"></param>
        /// <returns></returns>
        public override bool isResultAvailable(string AsyncRequestID, out BaseAsyncRequestResult AsyncRequestResult)
        {
            bool result = true;

            string linkedInstance = this.engineContext.CurrenActivity.WaitInstanceID;
            if (String.IsNullOrWhiteSpace(linkedInstance))
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "No linked instance found for ITAutomationActivityWait");
            }

            string linkedInstanceNr = "";

            try
            {
                linkedInstanceNr = linkedInstance.Substring(0, linkedInstance.IndexOf('.'));
                if (String.IsNullOrEmpty(linkedInstanceNr)) throw new Exception();
            }
            catch (Exception)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "No linked instance nr available " + linkedInstance + "found for ITAutomationActivityWait");
            }

            //logger.Debug(base.getWorkflowLoggingContext(engineContext) + " linkedInstanceNr : " + linkedInstanceNr + "." + VAR_SCRIPTRUNID);
            Variable tmp = engineContext.GetWorkflowVariable(linkedInstanceNr + "." + VAR_SCRIPTRUNID);
            logger.Debug(base.getWorkflowLoggingContext(engineContext) + " linkedInstanceNr : " + linkedInstanceNr + "." + VAR_SCRIPTRUNID + " value = : " + tmp.GetStringValue());

            ScriptRunID = tmp.GetStringValue();

            if (String.IsNullOrWhiteSpace(ScriptRunID))
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "No Value in Variable for ScriptRunID in linked instance " + linkedInstance + " found for ITAutomationActivityWait");
            }
            int intScriptRunID = 0;
            intScriptRunID = Convert.ToInt32(ScriptRunID);
            if (intScriptRunID == 0)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, "Value in Variable for ScriptRunID " + ScriptRunID + " results as 0  for " + linkedInstance + " found for ITAutomationActivityWait");
            }

            //ask Webservice whether Result is Available

            ResultObjectItem serviceResult = null;

            // Do call and WaitItem within transaction
            try
            {
                serviceResult = ServiceClient.GetScriptRunStatus(intScriptRunID);
                this.ScriptStatus = serviceResult.Status;
                this.ScriptMessage = serviceResult.ErrorMessage;
                string msg = "<null>";
                if (this.ScriptMessage != null)
                    msg = this.ScriptMessage;
                logger.Debug("AutomationServieCall resulted with CreatedScriptRun ID: " + serviceResult.ScriptRunID + "with ScriptMessag: " + msg);
            }
            catch (Exception ex)
            {
                string msg = "Error calling Result for ITAutomation ScriptRun " + ScriptRunID + " for this Equipment.";
                if (ex.InnerException != null)
                {
                    msg += " inner Exception: " + ex.InnerException.Message;
                }
                throw new BaseException(ErrorCodeHandler.E_WF_ACTIVITY_INITIALIZE, msg, ex);
            }

            logger.Debug("AutomationServieCall resulted with CreatedScriptRun ID: " + serviceResult.ScriptRunID + "with Status: " + serviceResult.Status);

            this.ScriptRunID = Convert.ToString(serviceResult.ScriptRunID);
            ITAutomationAsyncRequestResult itAutomationAsyncRequestResult = new ITAutomationAsyncRequestResult();
            itAutomationAsyncRequestResult.ServiceResult = serviceResult;

            AsyncRequestResult = itAutomationAsyncRequestResult;
            return result;

        }


        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public override StepReturn HandleReminder(EngineContext engineContext, BaseAsyncRequestResult baseResult, bool resultAvailable)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }
    }
}
