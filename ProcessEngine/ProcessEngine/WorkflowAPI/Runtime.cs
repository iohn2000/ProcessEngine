using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Configuration;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   The class Runtime is the main class to run the WF engine.
    ///             Every time the when the function <see cref="RunEngine"/> is called (= heart beat) by a client the WF engine goes looking
    ///             for new activities to work on. Please note: database connection is only opened
    ///             and closed once per heart beat for performance reasons.
    ///             This is also the class to use when working with tasks.</summary>
    ///
    /// <remarks>   Author: Fleckj, 16.03.2015. </remarks>
    #endregion
    public class Runtime
    {
        /// <summary>
        /// max engines running at same time
        /// </summary>
        public const string CONST_MAXPROCESSENGINEPROCESSES = "MaxProcessEngineProcesses";
        /// <summary>   unique id per instance of runtime. needed so that 2 parallel runtimes dont take each others engineAlerts</summary>
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Guid uniqueRuntimeID;
        private string msgString = "RunTimeGuid:{4} EngineAlert processed. WF_ID='{0}', StartActivity='{1}', WorkflowStatus='{2}', AlertStatus='{3}'";
        private bool errorRunMode = false;
        private string workflowDefinitionName = null;

        /// <summary>   Class to access the data layer. 
        ///             The <see cref="DatabaseAccess"/> keeps an open database connection through the lifetime of 
        ///             the function <see cref="RunEngine"/> (heart beat). This is done for performance reasons as a
        ///             heart beat typically has a short life and is called often in short intervalls.</summary>
        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Default constructor to setup a database connection and create a unique ID (per runtime) </summary>
        ///
        /// <remarks>   Fleckj, 16.03.2015. </remarks>
        #endregion
        public Runtime()
        {

        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   The function <see cref="RunEngine"/> is the function to call every time you want the
        ///             WF engine to check for new work to do. Every call is a heart beat. Typically
        ///             this could be something between every second or every minute.
        ///             Note: One heart beat will execute as many steps as possible before a human interaction
        ///             or something to wait for occurs. If there is no "stops" one heart beat would be enough
        ///             to finish a whole workflow. 
        ///            
        ///             </summary>
        /// <exception cref="BaseException">Throws a BaseException in case of errors</exception>
        ///
        /// <remarks>   Fleckj, 16.03.2015. </remarks>
        #endregion
        public string RunEngine()
        {
            int amountProcesses;
            int maxProcesses = 30;
            ProcessInstance processInstance = null;
            Engine engine = null;
            DatabaseAccess db = new DatabaseAccess();
            WFEEngineAlert alert = null;
            try
            {
                // get unique RuntimeID to lock engine alert.
                this.uniqueRuntimeID = Guid.NewGuid();                
                logger.Info(string.Format("{0} Workflow RunEngine() started.", DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString())));

                // get setting for maximum allowed processes.
                try
                {
                    maxProcesses = int.Parse(ConfigurationManager.AppSettings[CONST_MAXPROCESSENGINEPROCESSES].ToString());
                }
                catch (Exception bEx)
                {
                    logger.Warn(string.Format("{0} Error trying to get count for concurrent process engines processes. Taking default value with Max={1}.",
                                            DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString()), maxProcesses), bEx);
                    //nothing to do because we can continue with a default value
                }

                // get actual amount of Engine processes running 
                //TODO: change this db call for parallelization of processengine to several systems, because this number is counted per EngineInstance
                try
                {
                    amountProcesses = db.GetAmountOfProcessEngineProcessesRunning();
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("{0} Error trying to get count for concurrent process engines processes.",
                                  DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString())), ex);
                    return null;
                }

                //check whether it is still safe to start this new process
                if (amountProcesses > maxProcesses)
                {
                    // too much processes locking alerts. process must not be started until this number relaxes. =>  log it and do nothing
                    // TODO: Put this information into Windows Eventlog
                    logger.Warn(string.Format("{0} Maximum amount of concurrent engine alert locks by process reached. Max={1}",
                                                DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString()), amountProcesses)
                                );
                    // do nothing
                    return null;
                }

                logger.Debug(string.Format("{0} Amount of concurrent process engine processes at the moment = {1}; Max = {2}",
                                                DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString()), amountProcesses, maxProcesses)
                                );

                //get top engine alert (the most important one) to be worked on
                try
                {
                    alert = db.GetTopEngineAlertOneTable(this.uniqueRuntimeID);
                }
                catch (BaseException bEx)
                {
                    logger.Error(string.Format("{0} Error trying to get engine alert.",
                        DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString())), bEx);
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("{0} Error trying to get engine alert.",
                        DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString())), ex);
                    return null;
                }

                EngineAlert engineAlert = new EngineAlert();

                //check whether an alert was found. if not exit.
                if (alert == null)
                {
                    logger.Info(string.Format("{0} No new Alert found, nothing to do.", DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString())));
                    return null;
                }

                //alert was found: => work on engine alert
                ReflectionHelper.CopyProperties(ref alert, ref engineAlert);

                logger.Info(string.Format("{0} Working on TopEngineAlert: (WFEEngineAlert): {1} - {2} - {3}",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), "", alert.EA_WFI_ID),
                    alert.EA_ID,
                    alert.EA_Created,
                    alert.EA_Type));

                //get specific workflow definition for this engine Alert
                WFEWorkflowDefinition wfd = db.GetWorkflowDefinitionFromInstanceGuid(alert.EA_WFI_ID);
                if (wfd != null)
                    workflowDefinitionName = wfd.WFD_Name;
                else
                    workflowDefinitionName = "workflow definition name not found";

                logger.Debug(string.Format("{0} For Workflow Instance: '{1}' the Workflow Definition Name is '{2}'",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID),
                    alert.EA_WFI_ID,
                    workflowDefinitionName));

                //create a new ProcessInstance with this engine alert
                processInstance = new ProcessInstance(alert.EA_WFI_ID);

                //create an engine with this workflowInstance 
                logger.Debug(string.Format("{0} START instance.GetInstanceXml().",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));

                string instanceXML = processInstance.GetInstanceXml();

                logger.Debug(string.Format("{0} END Engine instance.GetInstanceXml()",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));

                engine = new Engine(processInstance.InstanceID, instanceXML, this.uniqueRuntimeID, engineAlert);
                engine.WorkflowDefinitionName = workflowDefinitionName;
                logger.Debug(string.Format("{0} Engine instance created.",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));

                //work on workflow-status and decide what to do

                //
                // ABORTED: if workflow status is aborted no restarting possible. wf is dead
                // => finish the engine alert and release it, so engine does not longer work on this instance
                //
                if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Aborted)
                {
                    //finish engine alert
                    db.FinishEngineAlert(alert);

                    //release engine alert from process
                    db.ReleaseEngineAlert(alert);

                    logger.Debug(string.Format("{0} WorfklowStatus is 'EnumWorkflowExecutionStatus.Aborted'. Engine will stop the run.",
                        DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));

                    // exit runtime
                    return null;
                }

                //
                // PAUSED || ERROR: if workflow status is in pause or error mode do not do any polling
                // but it is possible to restart
                // => update last polling to now, finish alert release alert 
                //
                if ((alert.EA_Type.ToLowerInvariant() == "polling") &&
                    (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Paused ||
                     engine.WorfklowStatus == EnumWorkflowInstanceStatus.Error))
                {
                    //update last polling
                    db.UpdateLastPolling(alert.EA_StartActivity, alert.EA_WFI_ID, DateTime.Now);
                    
                    //finish engine alert
                    db.FinishEngineAlert(alert);

                    //release engine alert from process
                    db.ReleaseEngineAlert(alert);

                    logger.Debug(string.Format("{0} Engine Alert found has status 'polling' but WorkflowExecutionstatus is Pause or Error: Engine will stop the run.",
                        DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));

                    // exit runtime
                    return null;
                }
                //
                // FINISH: WF finished, just update status in instance (database)
                // => Finish and Release the alert. => update workflow-instance
                //
                if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Finish)
                {
                    //finish engine alert
                    db.FinishEngineAlert(alert);

                    //release engine alert from process
                    db.ReleaseEngineAlert(alert);

                    //update status of instance in db
                    processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Finish);

                    logger.Debug(string.Format("{0} WF finished, update status in instance xml (database)",
                        DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));
                    return string.Format(msgString, alert.EA_WFI_ID, alert.EA_StartActivity, engine.WorfklowStatus, alert.EA_Status, this.uniqueRuntimeID.ToString());
                }

                // EXECUTING || SLEEPING: work on the instance

                //
                // alert does not point to Start-Activity = > Process From Activity
                //
                else if (alert.EA_StartActivity != "start") // continue processing form a specific activity
                {
                    // find next activity for this instance and process it.
                    this.ProcessFromActivity(processInstance, engine, db, alert);
                }

                //
                // alert does point to Start-Activity => Process From Start
                //
                #region From Start
                else
                {
                    //start process at start activity
                    this.ProcessFromStartActivity(processInstance, engine, db, alert, workflowDefinitionName);
                }
                #endregion

                //
                // run finished - update intance xml
                //
                processInstance.UpdateInstanceXmlContent(engine.GetProcessedXml());

                // store workflow status to Database
                if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Paused)
                {
                    processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Paused);
                }
                else if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Error)
                {
                    processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Error);
                }
                else if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Aborted)
                {
                    processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Aborted);
                }
                else if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.StopError)
                {
                    processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.StopError);
                }
                else if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Finish)
                {
                    processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Finish);

                    // if this wf is a subworkflow create an alert for parent to continue... (refactor)
                    // engine.HandleSubworkflows();

                    // write all variables into audit log
                    engine.WriteAllVariablesIntoAuditLog();

                    // calc time-metrics and log it
                    this.CalcMetrics(db, alert);
                }
                else if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Sleeping)
                {
                    processInstance.UpdateInstanceXmlStatus(engine.WorfklowStatus, engine.WorkflowModel.GetWaitingSteps());
                }
            }
            catch (BaseException bEx)
            {
                logger.Error(string.Format("{0} catched the following exception.",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), this.workflowDefinitionName, alert?.EA_WFI_ID)), bEx);
                processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.StopError);
                throw bEx;
            }
            catch (Exception ex)
            {
                // some error handling
                logger.Error(string.Format("{0} catched the following exception",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), this.workflowDefinitionName, alert?.EA_WFI_ID)), ex);
                processInstance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.StopError);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, "General Error in ProcessEngine.", ex);
            }
            finally
            {
                logger.Info(string.Format("{0} Workflow RunEngine() finished.",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), this.workflowDefinitionName, alert?.EA_WFI_ID)));
                // release engine alert here in case ... ??
            }
            return string.Format(msgString,
                alert?.EA_WFI_ID,
                alert?.EA_StartActivity,
                engine.WorfklowStatus,
                alert?.EA_Status,
                this.uniqueRuntimeID.ToString());
        }

        /// <summary>
        /// calculates time metrics of a workflow (by its last alert) and logs it
        /// </summary>
        /// <param name="db"></param>
        /// <param name="alert"></param>
        private void CalcMetrics(DatabaseAccess db, WFEEngineAlert alert)
        {
                    try
                    {
                        WFEWorkflowInstance wfi = db.GetWorkflowInstance(alert.EA_WFI_ID);
                        string niceTime = "";
                        if (wfi != null)
                        {
                            TimeSpan durchlaufZeit = wfi.WFI_Updated.Value - wfi.WFI_Created.Value;
                            niceTime = string.Format("{0} days, {1} hours {2} minutes {3} seconds",
                                durchlaufZeit.Days.ToString(),
                                durchlaufZeit.Hours.ToString(),
                                durchlaufZeit.Minutes.ToString(),
                                durchlaufZeit.Seconds.ToString());
                        }
                logger.Info(string.Format("{0} Workflow Instance has finished. Duration (runnig time) = [{1}]",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert?.EA_WFI_ID), niceTime)); // durchlauf zeit
                    }
                    catch (Exception ex)
                    {
                        // error writing log
                logger.Error(string.Format("{0} error calculating duration for workflow",
                   DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert?.EA_WFI_ID)), ex);
                    }
                }

        /// <summary>
        /// process a workflow from its start activity
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="engine"></param>
        /// <param name="db"></param>
        /// <param name="alert"></param>
        /// <param name="workflowDefinitionName"></param>
        private void ProcessFromStartActivity(ProcessInstance instance, Engine engine, DatabaseAccess db, WFEEngineAlert alert, string workflowDefinitionName)
        {
            logger.Info(string.Format("{0} New Workflow Instance started.",
                DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));
            instance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Executing);

            Activity startActivity = engine.WorkflowModel.GetStartActivityFromInstance();
            if (startActivity != null)
            {
                engine.ProcessFromActivity(startActivity, alert.EA_CallbackID, alert.EA_InputParameters);
            }
            else
            {
                // could not find start activity
                // stop workflow with error  (Error To Handle) -> possible to restart
                logger.Debug(string.Format("{0} Could not find start activity for workflow instance",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));
                engine.WorfklowStatus = EnumWorkflowInstanceStatus.Error;
                instance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Error);
            }

            if (alert.EA_Type.ToLowerInvariant() == "normal")
            {
                db.FinishEngineAlert(alert);
                db.ReleaseEngineAlert(alert);
            }
            else
            {
                logger.Debug(string.Format("RunTimeGuid:{0} do not deactivate polling type alerts, should never happen here because polling is not a start activity",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));
            }
            }

        /// <summary>
        /// process the workflow from the activity where the alert points to by calling Engine:ProcessFromActivity()
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="engine"></param>
        /// <param name="db"></param>
        /// <param name="alert"></param>
        private void ProcessFromActivity(ProcessInstance instance, Engine engine, DatabaseAccess db, WFEEngineAlert alert)
        {
            Activity activity = engine.WorkflowModel.GetActivityFromInstance(alert.EA_StartActivity);
            if (activity != null)
            {
                // update execution iteration status from prev. run
                string sIteration = activity.GetWaitIteration();
                int iter; if (int.TryParse(sIteration, out iter)) engine.CompleteWaitingStep(activity, iter);

                if (engine.WorfklowStatus == EnumWorkflowInstanceStatus.Paused || engine.WorfklowStatus == EnumWorkflowInstanceStatus.Error)
                {
                    logger.Debug(string.Format("{0} Workflow was in Pause or Error Mode. Start process from start activtiy again.",
                                        DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID)));

                    engine.activityInstanceNamneThatCausedError = alert.EA_StartActivity;
                    Activity startActivity = null;
                    startActivity = engine.WorkflowModel.GetStartActivityFromInstance();
                    engine.ProcessFromActivity(startActivity, alert.EA_CallbackID, alert.EA_InputParameters);
                }
                else
                {
                    logger.Debug(string.Format("{0} Re-Entry into workflow. Start process from activity: {1}",
                                        DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID), activity.Instance));
                    engine.ProcessFromActivity(activity, alert.EA_CallbackID, alert.EA_InputParameters);
            }

            }
            else
            {
                // could not find activity to continue
                // stop workflow with error  (Error To Handle) -> possible to restart
                engine.WorfklowStatus = EnumWorkflowInstanceStatus.Error;
                instance.UpdateInstanceStatus(EnumWorkflowInstanceStatus.Error);
                logger.Debug(string.Format("{0} could not find activity: '{1}' in workflow instance.",
                    DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID, null),
                    alert.EA_StartActivity));
            }

            if (alert.EA_Type == "Normal"||alert.EA_Type=="Callback")
            {
                logger.Debug(string.Format("{0} RunTime call FinishEngineAlert", DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID, null)));
                db.FinishEngineAlert(alert);
                db.ReleaseEngineAlert(alert);
            }
            else if (alert.EA_Type == "Polling")
            {
                // do not finish polling type alerts, but release polling alert, its needed again
                logger.Debug(string.Format("{0} RunTime call ReleaseEngineAlert", DataHelper.BuildLogContextPrefix(this.uniqueRuntimeID.ToString(), workflowDefinitionName, alert.EA_WFI_ID, null)));
                db.ReleaseEngineAlert(alert);
            } else
            {
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, "Unknown EngineAlertType to finish found:" + alert.EA_Type);
            }
        }
    }
}
