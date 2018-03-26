using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;


namespace Kapsch.IS.ProcessEngine.WFActivity
{
    /// <summary>
    /// Base class to be implemented by all Callback Activities
    /// 
    ///   <properties>
    ///     <property name = "ClientReferenceID"  direction="input"  dataType="stringType" ></property>
    ///     <property name = "Status"  direction="output"  dataType="stringType" ></property>
    ///     <property name = "Result"  direction="output"  dataType="stringType" ></property>
    ///   </properties>
    /// 
    /// </summary>
    public abstract class BaseCallbackActivity : BaseActivity, IProcessStep
    {
        private readonly string VAR_CLIENTREFERENCEID = "ClientReferenceID";
        private readonly string VAR_AUTOCOMPLETE = "AutoComplete";
        private readonly string VAR_STATUS = "Status";
        private readonly string VAR_RESULT = "Result";

        private bool AutoComplete = false;

        public string CallbackResult;
        public string CallbackStatus;

        public string ClientReferenceID { get; set; }


        public BaseCallbackActivity(Type type) : base(type)
        {

        }


        /// <summary>
        /// check whether this Activity was called the first time
        /// </summary>
        /// <returns>true if activity was started first time</returns>
        /// <remarks>
        /// if the EngineAlert has a positive polling interval this activity was found first time
        /// </remarks>
        public bool CheckIfFirstRun(EngineContext engineContext)
        {
            //somehow fragile: we should store the fact that we run a second time within workflow data e.g in Status Var of this activity.
            string latestEaType = engineContext.EngineAlert.EA_Type;
            int pollingIntervalSeconds = (int)engineContext.EngineAlert.EA_PollingIntervalSeconds;
            logger.Debug(this.getWorkflowLoggingContext(engineContext)+" checking Engine Alert " + engineContext.EngineAlert.EA_ID + " for first run.  pollingIntervalSeconds:" + pollingIntervalSeconds + " type: " + latestEaType);

            //type not callback then no callback alert has been created yet. must be a firstRun
            if (latestEaType.ToLower() != "callback")
                return true; // firstRun = true

            // workflow got startet through a callback type alert
            if (latestEaType.ToLower() == "callback" && pollingIntervalSeconds == -1)
            {
                logger.Warn(base.getWorkflowLoggingContext(engineContext) + " CheckIfFirstRun found case callback and pollingIntervalSeconds = -1");
            }

            return false;
        }

        /// <summary>
        /// Standard method to be implemented by any Async Activity for its doing
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="baseResult"></param>
        /// <returns></returns>
        public abstract StepReturn PostInitialize(EngineContext engineContext);

        public abstract StepReturn PreFinish(EngineContext engineContext);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public override StepReturn Initialize(EngineContext engineContext)
        {

            //read clientreferenceID Variable from Workflow
            string errorMessage = "error trying to get Workflow-variables";
            try
            {
                Variable tmp;
                tmp = this.GetProcessedActivityVariable(engineContext, VAR_CLIENTREFERENCEID, false);
                this.ClientReferenceID = EngineContext.RemoveBorderQuotes(tmp.VarValue); //TODO remove this very grauslich "" behavior!!

                tmp = this.GetProcessedActivityVariable(engineContext, VAR_AUTOCOMPLETE, true);
                if (tmp != null)
                {
                    this.AutoComplete = Boolean.Parse(EngineContext.RemoveBorderQuotes(tmp.VarValue));
                }

            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            try
            {
                bool isFirstRun = this.CheckIfFirstRun(engineContext);

                //check whether this call is first call of the callbackactivity and create an EngineAlert for this callback
                if (isFirstRun)
                {
                    string workflowInstanceID = engineContext.WorkflowModel.InstanceID;

                    DatabaseAccess db = new DatabaseAccess();

                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + "BaseCallbackActivity Initialize: First Call of Callback => create enginealert with pollinginterval -1");

                    db.CreateCallback(workflowInstanceID, this.ClientReferenceID, engineContext.CurrenActivity.Instance);

                    if (!this.AutoComplete)
                        return new StepReturn("CallBack Initialized", Shared.Enums.EnumStepState.Wait);
                    else
                    {
                        logger.Debug(base.getWorkflowLoggingContext(engineContext) + "AutoCallback found => call AutoCallback()");

                        this.AutoCallback(engineContext);
                        engineContext.SetActivityVariable(VAR_RESULT, this.CallbackResult);
                        engineContext.SetActivityVariable(VAR_STATUS, this.CallbackStatus);

                        return PostInitialize(engineContext);
                    }
                }
                //since this is not the first call it must be triggered by the callback => do Postinitialize
                else
                {
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + "BaseCallbackActivity Initialize: Found Callback => call implementation");

                    DatabaseAccess dblayer = new DatabaseAccess();

                    //get external taskdata from AWI_Item
                    AsyncWaitItem awi = dblayer.GetAsyncWaitItem(engineContext.WorkflowModel.InstanceID, engineContext.EngineAlert.EA_StartActivity);

                    //log result if this callback
                    logger.Debug("Callback Result: " + awi.AWI_Config);

                    //convert awi item into XDocument and store the Activity Variables "Result" and "Status"
                    XDocument xdoc = XDocument.Parse(awi.AWI_Config);
                    this.CallbackResult = string.Concat(xdoc.XPathSelectElement("root/item[@name='result']").Nodes());
                    this.CallbackStatus = xdoc.XPathSelectElement("root/item[@name='status']").Value;

                    engineContext.SetActivityVariable(VAR_RESULT, this.CallbackResult);
                    engineContext.SetActivityVariable(VAR_STATUS, this.CallbackStatus);


                    return PostInitialize(engineContext);
                }
            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn stepreturn = this.PreFinish(engineContext);

            string errorMessage = "error trying fnish callback";

            try
            {
                string workflowInstanceID = engineContext.WorkflowModel.InstanceID;
                //de nexte zeun is weu ma genau danach den Alert in da datnbaunk auf "aftercallback" ändan duan. und da finishalert ausn engincontext den oidn daip nimmt
                engineContext.EngineAlert.EA_Type = "AfterCallback"; // weu des im schef scho wurscht woa owa eigantlich gheat do a Enum her. 
                new DatabaseAccess().FinishCallBack(workflowInstanceID, this.ClientReferenceID);
                logger.Debug(this.getWorkflowLoggingContext(engineContext) + " finish Engine Alert " + engineContext.EngineAlert.EA_ID + " set type 'AfterCallback'");

            }
            catch (BaseException bEx)
            {
                return this.logErrorAndReturnStepState(engineContext, bEx, errorMessage + " - " + bEx.Message, EnumStepState.ErrorToHandle);
            }
            catch (Exception ex)
            {
                return this.logErrorAndReturnStepState(engineContext, ex, errorMessage + " - " + ex.Message, EnumStepState.ErrorToHandle);
            }

            return stepreturn;

        }

        internal void AutoCallback(EngineContext engineContext)
        {
            // call a specific method from derivied class to calculate the autofield and autovalue 
            UpdateStatusAndResultForAutoCallback(engineContext);

            DatabaseAccess databaseAccess = new DatabaseAccess();
            databaseAccess.UpdateCallBack(engineContext.WorkflowModel.InstanceID, ClientReferenceID, this.CallbackResult, this.CallbackStatus);
        }

        /// <summary>
        /// Method to create meaningful autocompletion messages
        /// </summary>
        /// <param name="engineContext"></param>
        public abstract void UpdateStatusAndResultForAutoCallback(EngineContext engineContext);
    }
}
