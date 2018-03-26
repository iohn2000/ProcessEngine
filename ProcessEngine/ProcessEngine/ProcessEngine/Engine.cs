using Ciloci.Flee;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.DLLConfiguration;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   The Engine can process an activity (step) of a specific
    ///             instance of a workflow definition. </summary>
    ///
    /// <remarks>   Fleckj, 06.02.2015. </remarks>
    #endregion
    public class Engine
    {
        private IEDPLogger logger = EDPLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// ErrorRunMode workflow has been restartet AFTER an error2handle und runs all thw way down to activty that causes error
        /// As soon as this activity is reached ErrorRunMode is false again.
        /// Immediatley after an activity causes an error the ErrorRunMode is still false. 
        /// </summary>
        public bool ErrorRunMode { get; set; }
        /// <summary>
        /// if an acitivty causes an error2handle or error2stop then this is set to true
        /// the difference to ErrorRunMode is that while still executin the current Run (including the ErrorTransistion Path) workflow is not in ErrorRunMode
        /// </summary>
        public bool workflowInErrorMode = false;
        public bool workflowInPauseMode = false;
        private EnumStepState errorStepState;
        private WorkflowModel workflowModel;
        public string activityInstanceNamneThatCausedError = null;

        private string instanceID;
        private bool HasErrorPathAlreadyBeenCreated = false;
        private Dictionary<string, object> dicDLLCache = new Dictionary<string, object>();
        private Dictionary<string, MethodInfo> dicMethodCache = new Dictionary<string, MethodInfo>();
        private DatabaseAccess dataBaseLayer;
        private Guid uniqueRuntimeID;

        private EngineAlert engineAlert;

        public static string Prefix_WorkflowDefinition = "WODE";
        public static string Prefix_WorkflowInstance = "WOIN";


        public WorkflowModel WorkflowModel { get { return this.workflowModel; } }
        public EnumWorkflowInstanceStatus WorfklowStatus
        {
            get { return this.workflowModel.GetStatus(); }
            set { this.workflowModel.SetStatus(value); }
        }

        public string WorkflowDefinitionName { get; set; }

        public Engine(string instanceID, string instanceXml, Guid runtimeGuid, EngineAlert alert)
        {
            string ctx = "";
            try
            {
                ctx = DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), "not set yet", instanceID, "no current AC yet");
            }
            catch (Exception)
            {
                ctx = "[Error building engine context]";
            }

            this.dataBaseLayer = new DatabaseAccess();

            this.workflowModel = new WorkflowModel();
            this.instanceID = instanceID;
            this.workflowModel.LoadModelXml(instanceXml);
            this.uniqueRuntimeID = runtimeGuid;
            this.engineAlert = alert;
            //
            // set runtime and engine to ErrorRun modus
            // meaning : start at start activity again and run through workflow until activity that causes pause/error is reached
            //           do not execute and activities on the way and only follow XOR paths as done in previous run
            //           this is necessary to build up recursions again!
            //
            if (this.WorfklowStatus == EnumWorkflowInstanceStatus.Paused ||
                 this.WorfklowStatus == EnumWorkflowInstanceStatus.Error)
            {
                this.ErrorRunMode = true;
            }
            //
            // load all activity DLLs into cache
            //
            logger.Debug(string.Format("{0} calling ConfigurationManager.GetSection('ActivityDLLsConfig')", ctx));
            ActivityDLLConfigurationSection dllConfig;
            dllConfig = ConfigurationManager.GetSection("ActivityDLLsConfig") as ActivityDLLConfigurationSection;

            logger.Debug(string.Format("{0} START calling ActivityDLLCacheHandler.ReloadActivityCache(dllConfig)", ctx));
            ActivityDLLCacheHandler acH = new ActivityDLLCacheHandler();
            Tuple<Dictionary<string, object>, Dictionary<string, MethodInfo>> returnValue = acH.ReloadActivityCache(dllConfig);
            logger.Debug(string.Format("{0} END calling ActivityDLLCacheHandler.ReloadActivityCache(dllConfig)", ctx));

            this.dicDLLCache = returnValue.Item1;
            this.dicMethodCache = returnValue.Item2;
        }

        /// <summary>
        /// return xml from workflomodel
        /// </summary>
        /// <returns>workflow model xml</returns>
        public string GetProcessedXml()
        {
            return this.workflowModel.wfModel.ToString(SaveOptions.None);
        }

        private EnumStepState processFigure(Activity currentActivity, Transition currentTransition, SortedList<string, Variable> workflowVariables, EngineContext engineContext = null)
        {
            StepReturn stepResult;
            ExecutionIteration nextExecIteration = null;

            string ctx = "";
            try
            {
                ctx = DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), this.WorkflowDefinitionName, this.instanceID, currentActivity.Instance);
            }
            catch (Exception)
            {
                ctx = "[Error building engine context]";
            }

            if (this.workflowInErrorMode || this.ErrorRunMode)
            {
                // in error mode make sure currentActivity stays at activity that has cause the error
                // ergo : do nothing
                logger.Debug(string.Format("{0} In processFigure. Workflow in ErrorMode. currentActivity will not be updated: '{1}'.", ctx, currentActivity.Instance));
            }
            else
            {
                // save to db what the current activity is
                string msg = string.Format("{0} : before UpdateInstanceCurrentActivity ", ctx);
                logger.Debug(msg);

                this.dataBaseLayer.UpdateInstanceCurrentActivity(this.instanceID, currentActivity.Instance);

                msg = string.Format("{0} : after UpdateInstanceCurrentActivity ", ctx);
                logger.Debug(msg);
            }
            //
            // we have reached activity that cause pause mode --> return to normal workflow modus
            //
            if (this.activityInstanceNamneThatCausedError == currentActivity.Instance)
            {
                this.ErrorRunMode = false;
                this.workflowInPauseMode = false;
            }


            if (!this.ErrorRunMode)
            {
                //
                // create execution iteration
                //
                nextExecIteration = this.setupExecutionIteration(currentActivity);
                //
                // prepare engine context
                //
                engineContext = this.prepareEngineContext(currentActivity, currentTransition, workflowVariables, engineContext, nextExecIteration);


                engineContext.stopWatch.Reset();
                engineContext.stopWatch.Start();
                //
                //  execute activity
                //
                stepResult = this.executeStep(engineContext);
                ctx = this.UpdateLogContext(currentActivity, engineContext);
                logger.Debug(string.Format("{0} AFTER executeStep(engineContext)", ctx));
                //
                // if an error occured do the error path only; no other transistions
                //
                string resultValue = stepResult.ReturnValue;

                if (stepResult.StepState == EnumStepState.ErrorStop | stepResult.StepState == EnumStepState.ErrorToHandle)
                {
                    resultValue = "error";
                    // set error mode to true
                    this.workflowInErrorMode = true;
                    // remeber the activity that caused error
                    this.activityInstanceNamneThatCausedError = currentActivity.Instance;
                    // remeber error step state
                    this.errorStepState = stepResult.StepState;
                    // add errortype to execution iteration info
                    nextExecIteration.AddMessage("errorType", stepResult.StepState.ToString());
                    nextExecIteration.AddMessage("errorMsg", stepResult.ReturnValue);
                    engineContext.LastStepReturn = stepResult;
                }


                // write changes made by ActivityStep back to worfklow xml (not DB)
                this.workflowModel.UpdateWorkflowVariablesInXmlModel(workflowVariables);

                nextExecIteration.ReturnValue = resultValue;

                nextExecIteration.SetProperty("stepExecStatus", stepResult.StepState.ToString());

                //
                // if activity needs to wait (e.g. polling) return with Wait status
                //
                if (stepResult.StepState == EnumStepState.Wait)
                {
                    return stepResult.StepState;
                }

                if (stepResult.StepState == EnumStepState.Paused)
                {
                    // remeber the activity that caused error
                    this.activityInstanceNamneThatCausedError = currentActivity.Instance;
                    // remeber error step state
                    this.errorStepState = stepResult.StepState;
                    // add errortype to execution iteration info
                    nextExecIteration.AddMessage("errorType", stepResult.StepState.ToString());
                    this.workflowInPauseMode = true;
                    return stepResult.StepState;
                }
            }
            else
            {
                // in case of Errormode enginecontext has not been created
                engineContext = this.prepareEngineContext(currentActivity, currentTransition, workflowVariables, engineContext, nextExecIteration);

                // ErrorRunMode --> do not execute activities
                stepResult = new StepReturn("", EnumStepState.Complete);
            }
            //
            //--------------------------------

            //
            // do not follow any outgoing transistions if in pause mode
            // except when in ErrorRunMode
            //
            if (this.workflowInPauseMode && this.ErrorRunMode == false)
            {
                return stepResult.StepState;
            }

            List<Transition> outGoingTrans;
            int countOutGoingTrans;
            EnumStepState _stepState = EnumStepState.NotCompleted;
            bool isThereWaitSteps = false;


            outGoingTrans = currentActivity.GetOutgoingTransistions();
            countOutGoingTrans = outGoingTrans.Count;
            logger.Debug(string.Format("{0} currentActivity.GetOutgoingTransistions() count = {1}",
                this.UpdateLogContext(currentActivity, engineContext), countOutGoingTrans.ToString()));

            // if worklow is in error mode then use ErrorTransistion if configured
            // if nothing is configured then use default error activity
            if (!this.HasErrorPathAlreadyBeenCreated && this.workflowInErrorMode && currentActivity.GetConfiguredErrorTransistionCount() == 0)
            {
                ctx = this.UpdateLogContext(currentActivity, engineContext);
                logger.Debug(string.Format("{0} create an error activity and transistion dynamically to errorActivty", ctx));
                // get error activity
                try
                {
                    // create error activity (via name from DB) if doesnt exist


                    string errorActivityInstance = this.WorkflowModel.ErrorActivityInstance; // returns an INSTANCE name
                    logger.Debug(string.Format("{0} in workflow configured errorActivityInstance = '{1}'", ctx, errorActivityInstance));

                    //logger.Debug(string.Format("{0}", ctx));

                    Activity errorAC;
                    if (!string.IsNullOrWhiteSpace(errorActivityInstance))
                    {
                        errorAC = this.WorkflowModel.GetActivityFromInstance(errorActivityInstance);
                        logger.Debug(string.Format("{0} WorkflowModel.GetActivityFromInstance() = {1}", ctx, errorAC == null ? "null" : errorAC.Instance));
                    }
                    else
                    {
                        // no errorActivity configure in WF --> get default activitr
                        // careful there is a DEFINITION name configured in app config
                        string defaultErrActivityDefinition = this.WorkflowModel.DefaultErrorActivityDefinition;
                        WFEActivityDefinition acDef = this.dataBaseLayer.GetActivityDefinitionTemplate(defaultErrActivityDefinition);
                        XElement xEl = XElement.Parse(acDef.WFAD_ConfigTemplate);
                        Activity dynErrorAC = new Activity(xEl);
                        this.WorkflowModel.AddActivity(dynErrorAC);
                        //logger.Debug(string.Format("{0} AFTER adding dynamic error activity. WFModel is now:\r\n{1}", ctx, this.WorkflowModel.wfModel.ToString()));

                        errorAC = this.WorkflowModel.GetActivityFromInstance(dynErrorAC.Instance);
                        logger.Debug(string.Format("{0} AFTER finding new dynamic err activity. GetActivityFromInstance() = '{1}'",
                            ctx, errorAC == null ? "null" : errorAC.Instance));
                    }

                    // create an error transistion to new activity
                    currentActivity.CreateErrorTransitionToActivity(errorAC.Instance);

                    // recalculate outgoing transistions
                    outGoingTrans = currentActivity.GetOutgoingTransistions();
                    countOutGoingTrans = outGoingTrans.Count;
                    logger.Debug(string.Format("{0} AFTER added error activity: currentActivity.GetOutgoingTransistions() count = {1}",
                        this.UpdateLogContext(currentActivity, engineContext), countOutGoingTrans.ToString()));
                    this.HasErrorPathAlreadyBeenCreated = true;
                }
                catch (Exception ex)
                {
                    logger.Debug(
                        string.Format("{0} error trying to create a transistion dynamically to errorActivty.", this.UpdateLogContext(currentActivity, engineContext)), ex);
                }
            }

            for (int k = 0; k < countOutGoingTrans; k++)
            {
                Transition currentOutTrans = outGoingTrans[k];

                // only follow transtions if :
                //  - transition is errorTransition and workflow in error mode
                //  - transition is normal and not error mode
                bool isNormalTransition = !currentOutTrans.IsErrorTransistion;
                if (currentOutTrans.IsErrorTransistion && !this.workflowInErrorMode)
                    continue;

                if (isNormalTransition && this.workflowInErrorMode)
                    continue;


                Activity targetActivity = currentOutTrans.GetTargetActivity();

                //
                // ErrorRunMode :
                // only follow transistion if it has been followed in previous run
                //
                if (this.ErrorRunMode)
                {
                    // get last execution iteration to check if transistion has ben executed (=1)
                    bool transistionHasBeenProcessed = currentOutTrans.IsProcessed();
                    logger.Debug(string.Format("{0} transistion:'{1}' HasBeenProcessed = '{2}'", 
                        this.UpdateLogContext(currentActivity, engineContext), 
                        currentOutTrans.Condition,
                        transistionHasBeenProcessed)
                       );

                    if (transistionHasBeenProcessed)
                    {
                        // transistion has been processed before so follow it but dont change anything
                        _stepState = this.processFigure(targetActivity, currentOutTrans, workflowVariables, engineContext);
                    }
                }
                //
                // normal modus -> calculate condition result
                //
                else
                {
                    try
                    {
                        this.calcConditionAndExecuteNextActivity(workflowVariables, engineContext, ref _stepState, ref isThereWaitSteps, currentOutTrans, targetActivity);
                    }
                    catch (BaseException bEx)
                    {
                        logger.Error(
                            string.Format("{0} Error in calcConditionAndExecuteNextActivity().", this.UpdateLogContext(currentActivity, engineContext)),
                            bEx
                            );

                        return EnumStepState.ErrorToHandle;
                    }
                    logger.Debug(string.Format("{0} END calcConditionAndExecuteNextActivity()", this.UpdateLogContext(currentActivity, engineContext)));
                } //end is ErrorRunMode
            }//end foreach outgoing

            if (this.workflowInErrorMode)
            {
                return this.errorStepState;
            }

            if (isThereWaitSteps) return EnumStepState.Wait;

            return _stepState;
        }

        private string UpdateLogContext(Activity currentActivity, EngineContext engineContext)
        {
            try
            {
                long elapsed = -1;
                try
                {
                    elapsed = engineContext.stopWatch.ElapsedMilliseconds;
                }
                catch (Exception)
                {
                }
                return DataHelper.BuildLogContextPrefix(
                        this.uniqueRuntimeID != null ? this.uniqueRuntimeID.ToString() : "n/a",
                        this.WorkflowDefinitionName,
                        this.instanceID,
                        currentActivity != null ? currentActivity.Instance : "n/a",
                        elapsed);
            }
            catch (Exception ex)
            {
                return "[error building context] " + ex.Message;
            }
        }

        /// <summary>
        /// writes a summery of all workflow variables into audit log
        /// </summary>
        public void WriteAllVariablesIntoAuditLog()
        {
            try
            {
                string summary = "";

                foreach (var wfVar in this.WorkflowModel.GetPunktVariables("[starts-with(@name,'0.')]"))
                {
                    string line = string.Format("Name:{0} Value='{1}' - [{2}, {3}]", wfVar.Name, wfVar.VarValue, wfVar.DataType.ToString(), wfVar.Direction.ToString());
                    summary += line + Environment.NewLine;
                }

                string wfdName = "not found";
                var wfd = this.dataBaseLayer.GetWorkflowDefinitionFromInstanceGuid(this.instanceID);
                if (wfd != null)
                    wfdName = "Workflow Definition :" + wfd.WFD_Name;

                AuditLog.WebService.AuditLogServiceClient auditLogService = new AuditLog.WebService.AuditLogServiceClient();
                auditLogService.WriteEntry(2, "EDP-ProcessEngine", null, wfdName, summary, this.instanceID);
            }
            catch (Exception ex)
            {
                // die without disturbing wht process engine
                logger.Warn("Error trying to write all workflow variables to audit log", ex);
            }
        }

        private void calcConditionAndExecuteNextActivity(SortedList<string, Variable> workflowVariables, EngineContext engineContext, ref EnumStepState _stepState, ref bool isThereWaitSteps, Transition currentOutTrans, Activity targetActivity)
        {
            Ciloci.Flee.IDynamicExpression wf_Expresssion;
            ExpressionContext context = new ExpressionContext();

            string ctx = "";
            try
            {
                ctx = DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), this.WorkflowDefinitionName, engineContext.WorkflowModel.InstanceID, engineContext.CurrenActivity.Instance);
            }
            catch (Exception)
            {
                ctx = "[Error building engine context]";
            }


            string condition = string.Empty;
            string evaluatedCondition = string.Empty;
            try
            {
                if (engineContext != null)
                {
                    if (engineContext.WorkflowModel != null)
                    {
                ExpressionEvaluator eva = new ExpressionEvaluator(engineContext.WorkflowModel);
                condition = currentOutTrans.Condition;
                        logger.Debug(string.Format("{0} Condition for next execution Step 1: condition='{1}'", ctx, condition ?? "null"));

                //evaluate condition
                if (!string.IsNullOrWhiteSpace(condition))
                {
                    object returnValue = eva.Evaluate(condition);
                    logger.Debug(string.Format("{0} Condition for next execution Step 2: returnValue='{1}'", ctx, returnValue));

                    if (returnValue != null)
                    {
                        wf_Expresssion = context.CompileDynamic(returnValue.ToString());
                        returnValue = wf_Expresssion.Evaluate();
                        logger.Debug(string.Format("{0} Condition for next execution Step 3: returnValue='{1}'", ctx, returnValue));
                    }
                    evaluatedCondition = returnValue.ToString().ToLowerInvariant();
                        }
                    }
                    else
                    {
                        throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, string.Format("{0} engineContext.WorkflowModel is null", ctx));
                    }
                }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, string.Format("{0} engineContext is null", ctx));
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0} Error evaluating outgoing transistion condition : '{1}'", ctx, condition);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, msg, ex);
            }

            logger.Debug("Condition for next execution Step 3: " + evaluatedCondition);

            // condition met so execute target activity
            if (string.IsNullOrWhiteSpace(condition) || evaluatedCondition == "true")
            {
                currentOutTrans.GetNextExecutionIteration().SetProperty("processed", "1");

                _stepState = this.processFigure(targetActivity, currentOutTrans, workflowVariables, engineContext);

                if (_stepState == EnumStepState.Wait) isThereWaitSteps = true;
            }
            else
            {
                // condition for Transition not met
                currentOutTrans.GetNextExecutionIteration().SetProperty("processed", "0");
            }
        }

        public void CompleteWaitingStep(Activity ac, int interationID)
        {
            ExecutionIteration iteration = ac.GetExecutionIteration(interationID);
            iteration.SetProperty("stepExecStatus", EnumStepState.Complete.ToString());
            iteration.AddMessage("endDateTime", DateTime.Now.ToString());
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Process from activity. </summary>
        ///
        /// <remarks>   Fleckj, 23.02.2015. </remarks>
        ///
        /// <param name="currentActivity">  The current activity. </param>
        /// <param name="returnValue">      The return value. </param>
        /// <param name="inputParameters">  Variables to add to workflow </param>
        #endregion
        public void ProcessFromActivity(Activity currentActivity, string returnValue, string inputParameters = null)
        {

            bool flatStepError = false;
            this.WorfklowStatus = EnumWorkflowInstanceStatus.Executing;

            // fill values from inputVariables into xml variables
            this.workflowModel.AddInputVariables(inputParameters);

            SortedList<string, Variable> workflowVariables = new SortedList<string, Variable>(StringComparer.InvariantCultureIgnoreCase);
            //workflowVariables = this.workflowModel.GetVariables();

            Transition outgTrans = null;
            List<Transition> allIncoming = currentActivity.GetIncomingTransistions();
            if (allIncoming.Count == 1)
                outgTrans = allIncoming[0];
            else if (allIncoming.Count > 1)
            {
                // must be a "zusamennwarten" activity
            }

            //
            // start process activity (recursive)
            //
            EnumStepState returnStepState = this.processFigure(currentActivity, outgTrans, workflowVariables, null);

            if (returnStepState == EnumStepState.Timeout || returnStepState == EnumStepState.Paused)
            {
                this.workflowModel.SetStatus(EnumWorkflowInstanceStatus.Paused);
                return;
            }

            if (returnStepState == EnumStepState.ErrorToHandle)
            {
                this.workflowModel.SetStatus(EnumWorkflowInstanceStatus.Error);
                return;
            }

            if (returnStepState == EnumStepState.ErrorStop)
            {
                //this.workflowModel.SetStatus(EnumWorkflowInstanceStatus.Aborted);
                this.workflowModel.SetStatus(EnumWorkflowInstanceStatus.StopError);
                return;
            }

            if (this.workflowModel.HasWaitingSteps())
            {
                this.workflowModel.SetStatus(EnumWorkflowInstanceStatus.Sleeping);
            }
            else
            {
                this.workflowModel.SetStatus(EnumWorkflowInstanceStatus.Finish);
            }
        }

        /// <summary>
        /// if wf is a subwf create an alert for parent to continue
        /// </summary>
        public void HandleSubworkflows()
        {
            // if finished and I am a subworflow then alert parent continue
            string wfVarsStr = "";
            string parentWF = this.WorkflowModel.ParentWorkflow;
            string startActivity = this.WorkflowModel.NextActivity; // = subworkflow activity from parent
            if (this.WorfklowStatus == EnumWorkflowInstanceStatus.Finish && !string.IsNullOrWhiteSpace(parentWF))
            {
                //
                // create input params for engine alert
                //
                try
                {
                    WorkflowMessageData msgData = new WorkflowMessageData();
                    msgData.WorkflowVariables.Add(new WorkflowMessageVariable("subWorkflowFinished_" + this.WorkflowModel.InstanceID, "true"));
                    msgData.WorkflowVariables.Add(new WorkflowMessageVariable("subWorkflowInstanceID", this.WorkflowModel.InstanceID));
                    wfVarsStr = XmlSerialiserHelper.SerialiseIntoXmlString(msgData.WorkflowVariables);
                    logger.Debug("Prepared Input Variables for workflow:\r\n" + wfVarsStr);

                    // create engineAlert for parentWF 
                    DatabaseAccess db = new DatabaseAccess();
                    db.CreateEngineAlert(startActivity, parentWF, "", wfVarsStr, EnumAlertTypes.Normal);
                }
                catch (Exception ex)
                {
                    wfVarsStr = "";
                }
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   build a list of all variables fron all activities </summary>
        ///
        /// <remarks>   Fleckj, 17.11.2015. </remarks>
        ///
        /// <param name="activity">     The activity. </param>
        /// <param name="inputVars">    The input variables. </param>
        ///
        /// <returns>   The extended attributes from activity. </returns>
        #endregion
        private SortedList<string, Variable> buildListOfAllWorkflowVariables(string inputVars)
        {
            SortedList<string, Variable> activityVariables = new SortedList<string, Variable>(StringComparer.InvariantCultureIgnoreCase);

            IEnumerable<XElement> allActivityVariables;
            allActivityVariables = this.workflowModel.wfModel.XPathSelectElements("//property");

            return this.mergeInputAndExtendedAttributs(activityVariables, inputVars);
        }
        private SortedList<string, Variable> mergeInputAndExtendedAttributs(SortedList<string, Variable> extendedAttributes, string inputVars)
        {
            #region
            //<hashtable>
            //  <item key="ID" value="37-2626f3f43f63" />
            //  <item key="ins" value="196-2503e9e9bd31" />
            //</hashtable>
            #endregion

            foreach (XElement xe in XDocument.Parse(inputVars).Descendants().Where<XElement>((XElement xx) => { return xx.Name.LocalName == "item"; }))
            {
                if (xe.Attribute("key") != null && !string.IsNullOrWhiteSpace(xe.Attribute("key").Value))
                {
                    Variable v;
                    string key = xe.Attribute("key").Value;
                    string vval = xe.Attribute("value").Value;
                    if (extendedAttributes.TryGetValue(key, out v))
                        v.VarValue = vval; //already a key there -> update the value
                    else
                        extendedAttributes.Add(key, new Variable(key, "", EnumVariablesDataType.stringType, ActivityProperty.ConvertToType("ouput")));
                }
            }
            return extendedAttributes;
        }
        /// <summary>
        /// throws BaseException in case Activtiy or function itself has an exception
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        private StepReturn executeStep(EngineContext engineContext)
        {
            string activityName = engineContext.CurrenActivity.Id;
            object oBaseObject = null;
            MethodInfo oMethod = null;

            string ctx = "";
            try
            {
                ctx = DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), this.WorkflowDefinitionName, this.instanceID, activityName);
            }
            catch (Exception)
            {
                ctx = "[Error building engine context]";
            }

            try
            {
                if (!this.dicDLLCache.TryGetValue(activityName.Trim(), out oBaseObject))
                {
                    string msg = string.Format("{0} Cannot find activity {1} in dicDLLCache", this.GetContextForLogging(engineContext), activityName);
                    logger.Error(msg);
                    StringWriter sw = new StringWriter();
                    ObjectDumper.Write(this.dicDLLCache, 5, sw);
                    logger.Debug(sw.ToString());
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, msg);
                }

                if (!this.dicMethodCache.TryGetValue(activityName.Trim(), out oMethod))
                {
                    string msg = string.Format("{0} Cannot find activity {1} in dicMethodCache", this.GetContextForLogging(engineContext), activityName);
                    logger.Error(msg);
                    StringWriter sw = new StringWriter();
                    ObjectDumper.Write(this.dicDLLCache, 5, sw);
                    logger.Debug(sw.ToString());
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, msg);
                }
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE,
                    string.Format("General Error in ProcessEngine just before invoking Execute() function for activity : '{0}'", activityName), ex);
            }
            //
            // invode the method Execute(EngineContext)
            //
            object[] arguments = new object[] { engineContext };
            StepReturn stepResult;
            try
            {
                logger.Debug(string.Format("{0} START oMethod.Invoke() about to start activity : {1}", ctx, activityName));
                stepResult = (StepReturn) oMethod.Invoke(oBaseObject, BindingFlags.Public, null, arguments, CultureInfo.CurrentCulture);

                logger.Debug(string.Format("{0} END oMethod.Invoke()", ctx));

                if (stepResult.StepState == EnumStepState.ErrorStop)
                {
                    //stepResult.ReturnValue = "error";
                }
            }
            catch (TargetInvocationException ex)
            {
                // see http://stackoverflow.com/questions/2658908/why-is-targetinvocationexception-treated-as-uncaught-by-the-ide

                // ex.InnerException.GetType() == typeof(BaseException)
                if (ex.InnerException is BaseException) // http://stackoverflow.com/questions/983030/type-checking-typeof-gettype-or-is
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE,
                        string.Format("General Error in ProcessEngine invoking Execute() function for activity : '{0}'", activityName), ex.InnerException);
                }

            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE,
                    string.Format("General Error in ProcessEngine invoking Execute() function for activity : '{0}'", activityName), ex);
            }

            return stepResult;
        }

        private ExecutionIteration setupExecutionIteration(Activity currentActivity)
        {
            ExecutionIteration nextExecIteration = currentActivity.GetNextExecutionIteration();
            int iterNr = int.Parse(nextExecIteration.IterationNumber);
            nextExecIteration.AddMessage("startDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            int nextExecutionId = this.workflowModel.GetNextExecutionId();
            nextExecIteration.SetProperty("id", nextExecutionId.ToString());
            return nextExecIteration;
        }

        private EngineContext prepareEngineContext(Activity currentActivity, Transition currentTransition, SortedList<string, Variable> workflowVariables, EngineContext engineContext, ExecutionIteration nextExecIteration)
        {
            if (engineContext != null)
            {
                engineContext.EngineAlert = engineAlert;
                engineContext.ExecutionIteration = nextExecIteration;
                engineContext.CurrenActivity = currentActivity;
                engineContext.CurrentTransistion = currentTransition;
                engineContext.SetListofWorkflowVariables(workflowVariables);
                engineContext.WorkflowDefinitionName = this.WorkflowDefinitionName;
            }
            else
            {
                engineContext = new EngineContext(this.instanceID, this.workflowModel, nextExecIteration,
                    currentActivity, currentTransition, workflowVariables, this.WorfklowStatus, this.uniqueRuntimeID);
                engineContext.EngineAlert = engineAlert;
                engineContext.WorkflowDefinitionName = this.WorkflowDefinitionName;
            }

            return engineContext;
        }

        private void writeVariablesLog(SortedList<string, Variable> workflowVariables, ExecutionIteration nextExecIteration)
        {
            string logVars = ConfigurationManager.AppSettings["ExecutionIteration.LogVariables"];
            if (logVars != null && logVars.ToLowerInvariant() == "true")
                nextExecIteration.UpdateVariables(workflowVariables);
        }

        private string GetContextForLogging(EngineContext engineContext)
        {
            string loggingContext = "";

            if (engineContext != null)
            {
                try
                {
                    // 
                    loggingContext = string.Format("[RunTimeGuid:{0}; WODE:{1}; WOIN:{2}; ActivityInstance:'{3}']",
                       engineContext.uniqueRunTimeID,
                       engineContext.WorkflowDefinitionName,
                                       engineContext.WorkflowModel.InstanceID,
                       engineContext.CurrenActivity.Instance
                                   );
                }
                catch (Exception ex)
                {
                    return "Error building logging context.";
                }
            }
            else
                return "No logging context available.";


            return loggingContext;
        }
    }
}
