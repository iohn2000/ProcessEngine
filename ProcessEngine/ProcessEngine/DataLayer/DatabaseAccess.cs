using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.ProcessEngine.Shared.Interfaces;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;

namespace Kapsch.IS.ProcessEngine.DataLayer
{
    // TODO Open for extension closed for modification -- SOLID think about it
    public class DatabaseAccess : IDataAccess, IDisposable
    {
        //Get logger
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static DateTime validToForever = new DateTime(2229, 12, 31, 23, 59, 59);


        private WorkflowDBContext databaseContextForTransaction = null;
        public WorkflowDBContext DatabaseContextForTransaction { get; }

        public DatabaseAccess()
        {
        }


        #region Worfklow Alerts

        public int GetAmountOfProcessEngineProcessesRunning()
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    int eaCount = (from ea in db.EngineAlerts where ea.EA_LockedByProcess != null select ea.EA_WFI_ID).Distinct().Count();
                    return eaCount;
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(
                    ErrorCodeHandler.E_WF_GENERAL, "Error trying to get count for engine alerts that are locked by process.", ex);
            }
        }

        public void CreateCallback(string workflowInstanceID, string clientReferenceID, string currentActivity)
        {
            try
            {
                    this.CreateEngineAlert(
                        startActivity: currentActivity,
                        workflowInstanceID: workflowInstanceID,
                        callBackID: clientReferenceID,
                        inputParameters: "",
                        alertType: Shared.Enums.EnumAlertTypes.Callback,
                        pollingInterval: -1,
                        lastPollingDate: null);

                    this.CreateWaitItem(
                        instanceID: workflowInstanceID, 
                        activityInstanceID: currentActivity, 
                        waitItemConfig: "", 
                        duedate: DatabaseAccess.validToForever);
            }
            catch (Exception ex)
            {
                throw new BaseException(
                    ErrorCodeHandler.E_WF_GENERAL, "Error trying to create Callback entries (awi, ea).", ex);
            }
        }

        /// <summary>   Gets top engine alert and sets status to "Executing".
        ///             Ideally this should be atomic (it should not be possible to read the same engine alert before status is set. 
        ///             if a workflow is in "paused", "error", or "aborted" status no alert are executed (selected)
        ///             Throws a BaseException.</summary>
        /// <remarks>   Fleckj, 12.03.2015. </remarks>
        ///
        /// <param name="uniquRuntimeId">   Identifier for the uniqu runtime. </param>
        /// <param name="alertType">what type of alert you want to get. e.g. only polling alert. default behaviour is to get oldest (the longest waiting) alert first.
        ///    prio 1 : oldest normal alert
        ///    prio 2 : oldest polling alert, make sure polling intervall is met
        /// </param>
        ///
        /// <returns>   The top engine alert. </returns>
        public WFEEngineAlert GetTopEngineAlertOneTable(Guid uniquRuntimeId, EnumAlertTypes alertType = EnumAlertTypes.Default)
        {
            EngineAlert topEngineAlert = null;
            WFEEngineAlert theOneToReturn = new WFEEngineAlert();

            //using (TransactionScope scope = new TransactionScope(
            //    TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Serializable }))
            //{

            DbContextTransaction transaction = null;
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    using (transaction = db.Database.BeginTransaction())
                    {
                        // lock table
                        //TODO: check whether this sql statement is necessary for locking....
                        db.Database.ExecuteSqlCommand("update EngineAlerts set EA_Status = EA_Status where EA_ID=(select top 1 EA_ID from EngineAlerts)");

                        switch (alertType)
                        {
                            case EnumAlertTypes.Normal:
                                break;
                            case EnumAlertTypes.Polling:
                                break;
                            default:
                            case EnumAlertTypes.Default:
                                try
                                {
                                    //
                                    // get a list of potential engine alerts ordery by LastPolling oldest first
                                    //
                                    IEnumerable<EngineAlert> x = from ea in db.EngineAlerts
                                                                 where (ea.EA_Status == EnumEngineAlertStatus.NotStarted.ToString() | ea.EA_Status == EnumEngineAlertStatus.Polling.ToString()) &   // only take NotStarted and Polling 
                                                                 ea.EA_LockedByProcess == null                                        // only take UNLOCKED alerts
                                                                 orderby ea.EA_LastPolling ascending, ea.EA_Created ascending         // sort so that oldest LastPolling is top
                                                                 select ea;

                                    //iterate through all relevant alerts to foind "the one" top engine alert
                                    for (int idx = 0; idx < x.Count(); idx++)
                                    {
                                        EngineAlert engineAlert = x.ElementAt(idx);

                                        //
                                        // prio 1 : oldest normal alert                            
                                        //
                                        if (engineAlert.EA_Type == "Normal")
                                        {
                                            topEngineAlert = engineAlert;
                                            break;
                                        }

                                        // prio 2 : oldest polling alert, make sure polling interval is met
                                        if (engineAlert.EA_Type == "Polling" || engineAlert.EA_Type == "Callback")
                                        {
                                            DateTime lastRun;

                                            if (engineAlert.EA_PollingIntervalSeconds == -1)
                                            {
                                                //ignore this EA because it waits for a Callback
                                                continue;
                                            }

                                            if (engineAlert.EA_LastPolling == null)
                                                //last polling not set so lets assume it was aons ago...
                                                lastRun = new DateTime(1920, 1, 1, 2, 2, 2);
                                            else
                                                lastRun = engineAlert.EA_LastPolling.Value;

                                            //calculate when this engine alert has to run next...
                                            DateTime nextRun = lastRun.AddSeconds(engineAlert.EA_PollingIntervalSeconds.Value);

                                            //check whether the next run date is met.
                                            bool isDueToRun = DateTime.Now >= nextRun;
                                            if (isDueToRun)
                                            {
                                                topEngineAlert = engineAlert;
                                                break;
                                            }
                                        }
                                    }

                                    //
                                    // we found a suitable engine alert
                                    //
                                    if (topEngineAlert != null)
                                    {
                                        //
                                        // set correct new status for engine alert
                                        //
                                        if (topEngineAlert.EA_Type == "Polling")
                                        {
                                            topEngineAlert.EA_Status = "Polling";
                                        }
                                        else
                                        {
                                            topEngineAlert.EA_Status = "Executing";
                                        }
                                        //
                                        // lock all engine alerts with same workflow instance guid
                                        //
                                        topEngineAlert.EA_LockedByProcess = uniquRuntimeId;

                                        db.SaveChanges(); // needed for EA_Status

                                        db.Database.ExecuteSqlCommand(
                                            string.Format("update EngineAlerts set EA_LockedByProcess='{0}' where EA_WFI_ID like '{1}'", topEngineAlert.EA_LockedByProcess, topEngineAlert.EA_WFI_ID)
                                            );


                                        //
                                        // copy engine alert into suitable class for webservice
                                        //
                                        try
                                        {
                                            ReflectionHelper.CopyProperties(ref topEngineAlert, ref theOneToReturn);
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, "error copying properties from EngineAlert to WFEEngineAlert", ex);
                                        }
                                    }
                                    else // we did not fine a suitable engine alert => nothing to do
                                    {
                                        theOneToReturn = null;
                                    }
                                    transaction.Commit();
                                }
                                catch (BaseException bEx)
                                {
                                    throw bEx;
                                }
                                catch (Exception ex)
                                {
                                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, "Error trying to get next engine alert. (EnumAlertTypes.Default)", ex);
                                }
                                break;
                        }
                    }//end transaction
                }//end db context
            }
            catch (Exception ex)
            {
                try
                {
                    if (transaction != null)
                        transaction.Rollback();
                }
                catch { }
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, "Possible table lock?", ex);
            }
            finally
            {
                // scope.Complete();
            }
            //}//end transaction scope

            return theOneToReturn;
        }

        public string GetNextActivity(string instanceID)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {

                    string wfi = db.Database.SqlQuery<string>("SELECT WFI_NextActivity from WorkflowInstance WHERE WFI_ID = '" + instanceID + "'").FirstOrDefault();

                    //WorkflowInstance wfi = (from w in db.WorkflowInstances
                    //                        where w.WFI_ID == instanceID
                    //                        select w).FirstOrDefault();
                    if (wfi != null)
                    {
                        return wfi;//.WFI_NextActivity; //can be null
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, string.Format("Error trying to read parent workflow : {0}.", instanceID), ex);
            }
        }

        public string GetParentWorkflow(string instanceID)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    string wfi = db.Database.SqlQuery<string>("SELECT WFI_ParentWF from WorkflowInstance WHERE WFI_ID = '" + instanceID + "'").FirstOrDefault();
                    //WorkflowInstance wfi = (from w in db.WorkflowInstances
                    //                        where w.WFI_ID == instanceID
                    //                        select w).FirstOrDefault();
                    if (wfi != null)
                    {
                        return wfi;//.WFI_ParentWF; //can be null
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, string.Format("Error trying to read parent workflow : {0}.", instanceID), ex);
            }
        }

        public void FinishWaitItem(int AWI_ID)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                doFinishWaitItem(AWI_ID, db);
            }
        }
        public void FinishWaitItem(DbContextTransaction transaction, int AWI_ID)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction", "transaction cannot be null. please call StartTransaction()");
            if (this.databaseContextForTransaction == null)
                throw new NullReferenceException("unexpected error. databaseContextForTransaction must not be null. (did you call StartTransaction?)");

            doFinishWaitItem(AWI_ID, this.databaseContextForTransaction);
        }

        public WFEWorkflowDefinition GetWorkflowDefinitionFromInstanceGuid(string instanceID)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    string wfi_wfd_id = db.Database.SqlQuery<string>("SELECT WFI_WFD_ID from WorkflowInstance WHERE WFI_ID = '" + instanceID + "'").FirstOrDefault();
                    //WorkflowInstance wfi = (from w in db.WorkflowInstances where w.WFI_ID == instanceID select w).FirstOrDefault();
                    if (wfi_wfd_id != null)
                    {
                        WorkflowDefinition wfd = (from d in db.WorkflowDefinitions where d.WFD_ID == wfi_wfd_id orderby d.WFD_Version descending select d).FirstOrDefault();
                        if (wfd != null)
                        {
                            WFEWorkflowDefinition wfed = new WFEWorkflowDefinition();
                            ReflectionHelper.CopyProperties(ref wfd, ref wfed);
                            return wfed;
                        }
                        else
                            return null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, string.Format("Error trying to read get workflow definiton : {0}.", instanceID), ex);
            }
        }

        private static void doFinishWaitItem(int AWI_ID, WorkflowDBContext db)
        {
            //find alert and delete 
            AsyncWaitItem awi = (from s1 in db.AsyncWaitItems
                                 where s1.AWI_ID == AWI_ID
                                 select s1).FirstOrDefault();
            if (awi != null)
            {
                awi.AWI_Status = EnumWorkflowInstanceStatus.Finish.ToString();
                awi.AWI_CompletedDate = DateTime.Now;
                db.SaveChanges();
            }
        }

        public int CreateEngineAlert(string startActivity, string workflowInstanceID, string callBackID,
            string inputParameters, EnumAlertTypes alertType, int pollingInterval, DateTime? lastPollingDate)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    EngineAlert alert = new EngineAlert();
                    DateTime jetzt = DateTime.Now;

                    alert.EA_WFI_ID = workflowInstanceID;
                    alert.EA_StartActivity = startActivity;
                    alert.EA_InputParameters = inputParameters;
                    alert.EA_Created = jetzt;
                    alert.EA_Updated = jetzt;
                    alert.EA_Status = EnumEngineAlertStatus.NotStarted.ToString();
                    alert.EA_Type = alertType.ToString();
                    alert.EA_CallbackID = callBackID;
                    alert.EA_PollingIntervalSeconds = pollingInterval;

                    if (lastPollingDate != null)
                        alert.EA_LastPolling = lastPollingDate.Value;

                    db.EngineAlerts.Add(alert);
                    db.SaveChanges();

                    try
                    {
                        StringWriter dump = new StringWriter();
                        ObjectDumper.Write(alert, 1, dump);
                        string msg = string.Format("Engine Alert created. EngineAlert:\r\n{0}", dump.ToString());
                        logger.Info(msg);
                    }
                    catch
                    {
                        logger.Error("Error writing log for CreateEngineAlert");
                    }
                    return alert.EA_ID;
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(
                    ErrorCodeHandler.E_WF_GENERAL,
                    string.Format("Error trying to create an engine alert for Workflow : {0}.", workflowInstanceID),
                    ex);
            }
        }


        public int CreateEngineAlert(string startActivity, string workflowInstanceID, string callbackID,
            string inputParameters, EnumAlertTypes alertType)
        {
            return this.CreateEngineAlert(startActivity, workflowInstanceID, callbackID, inputParameters, alertType, 0, null);
        }


        public List<DocumentTemplate> GetAllDocumentTemplates(string templateCategory)
        {
            using (WorkflowDBContext db = new DataLayer.WorkflowDBContext())
            {
                var allDocs = (from doc in db.DocumentTemplates where doc.TMPL_Category == templateCategory select doc).ToList();
                return allDocs;
            }
        }

        /// <summary>
        /// throws excpetion if not template found or error reading
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public DocumentTemplate GetDocumentTemplateByName(string templateName)
        {
            using (WorkflowDBContext db = new DataLayer.WorkflowDBContext())
            {
                var doctempl = (from doc in db.DocumentTemplates where doc.TMPL_Name == templateName select doc).ToList();

                if (doctempl == null)
                {
                    string errMsg = "error trying to read document template :" + templateName;
                    this.logger.Error(errMsg);
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg);
                }

                if (doctempl.Count() > 0)
                {
                    if (doctempl.Count() > 1)
                    {
                        logger.Warn("found more than 1 document teplate for templatName: " + templateName + " taking first one");
                    }
                    return doctempl[0];
                }
                else
                {
                    string errMsg = "cannot find document template :" + templateName;
                    this.logger.Error(errMsg);
                    throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg);
                }
            }
        }



        public List<DocumentTemplateType> GetAllDocumentTemplateCategories()
        {
            using (WorkflowDBContext db = new DataLayer.WorkflowDBContext())
            {
                List<DocumentTemplateType> allCats = (from docType in db.DocumentTemplateTypes select docType).ToList();
                return allCats;
            }
        }

        public void FinishCallBack(string workflowInstanceID, string clientReferenceID)
        {

            using (WorkflowDBContext db = new DataLayer.WorkflowDBContext())
            {
                EngineAlert engineAlert = null;

                if (String.IsNullOrWhiteSpace(workflowInstanceID) || workflowInstanceID.ToLower() == "null")
                {
                    logger.Warn("CallBack got no WorkflowInstance. Try to get it via ClientreferenceId");
                    engineAlert = (from ea in db.EngineAlerts
                                   where ea.EA_CallbackID == clientReferenceID
                                   select ea).FirstOrDefault();
                }
                else
                {
                    engineAlert = (from ea in db.EngineAlerts
                                   where ea.EA_CallbackID == clientReferenceID &
                                   ea.EA_WFI_ID == workflowInstanceID
                                   select ea).FirstOrDefault();
                }

                if (engineAlert == null)
                {
                    logger.Warn("FinishCallback: Could not find an enginealert to this call. Ignoring! ClientreferenceID: " + clientReferenceID + " workflowinstance: " + workflowInstanceID);
                    //just do ignore this call.
                    return;
                }

                engineAlert.EA_PollingIntervalSeconds = 0;
                engineAlert.EA_Type = "AfterCallback";
                engineAlert.EA_Updated = DateTime.Now;

                db.SaveChanges();
            }
        }

        public void UpdateCallBack(string workflowInstanceID, string clientReferenceID, string callbackResult, string callBackStatus)
        {
            string config = @"<root>
                    <item name = ""result"" >{0}</item> 
                    <item name = ""status"" >{1}</item>
                </root>";



            using (WorkflowDBContext db = new DataLayer.WorkflowDBContext())
            {
                EngineAlert engineAlert = null;

                if (String.IsNullOrWhiteSpace(workflowInstanceID) || workflowInstanceID.ToLower() == "null")
                {
                    logger.Warn("CallBack got no WorkflowInstance. Try to get it via ClientreferenceId");
                    engineAlert = (from ea in db.EngineAlerts
                                   where ea.EA_CallbackID == clientReferenceID
                                   select ea).FirstOrDefault();
                }
                else
                {
                    engineAlert = (from ea in db.EngineAlerts
                                   where ea.EA_CallbackID == clientReferenceID &
                                   ea.EA_WFI_ID == workflowInstanceID
                                   select ea).FirstOrDefault();
                }

                if (engineAlert == null)
                {
                    logger.Warn("UpdateCallBack: Could not find an enginealert to this call. Ignoring! ClientreferenceID: " + clientReferenceID + " workflowinstance: " + workflowInstanceID);
                    //just do ignore this call.
                    return;
                }

                engineAlert.EA_PollingIntervalSeconds = 0;
                engineAlert.EA_Updated = DateTime.Now;

                AsyncWaitItem asyncWaitItem = (from awi in db.AsyncWaitItems
                                               where awi.AWI_InstanceID == engineAlert.EA_WFI_ID &
                                               awi.AWI_ActivityInstanceID == engineAlert.EA_StartActivity
                                               select awi).FirstOrDefault();

                asyncWaitItem.AWI_Config = String.Format(config, callbackResult, callBackStatus);
                asyncWaitItem.AWI_CompletedDate = DateTime.Now;
                asyncWaitItem.AWI_Modified = DateTime.Now;
                asyncWaitItem.AWI_Status = "Finish";

                db.SaveChanges();
                //TODO engineAlert.EA_InputParameters
            }

        }
        public AsyncWaitItem GetAsyncWaitItem(string workflowInstanceID, string engineAlertStartActivity)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {

                AsyncWaitItem asyncWaitItem = (from awi in db.AsyncWaitItems
                                               where awi.AWI_InstanceID == workflowInstanceID &
                                               awi.AWI_ActivityInstanceID == engineAlertStartActivity
                                               select awi).FirstOrDefault();
                return asyncWaitItem;
            }
        }
        public void FinishEngineAlert(string startActivity, string workflowInstanceID)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                this.doFinishEngineAlert(db, startActivity, workflowInstanceID);
            }
        }

        public void AbortEngineAlerts(string workflowInstanceId, EnumEngineAlertStatus engineAlertStatus)
        {
            List<EngineAlert> engineAlerts = null;
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                //find alert and delete 
                engineAlerts = (from s1 in db.EngineAlerts
                                where s1.EA_WFI_ID == workflowInstanceId && (s1.EA_Status == EnumEngineAlertStatus.Executing.ToString() || s1.EA_Status == EnumEngineAlertStatus.NotStarted.ToString() || s1.EA_Status == EnumEngineAlertStatus.Polling.ToString())
                                select s1).ToList();

                if (engineAlerts != null)
                {
                    foreach (EngineAlert engineAlert in engineAlerts)
                    {
                        engineAlert.EA_Status = engineAlertStatus.ToString();
                        engineAlert.EA_Updated = DateTime.Now;

                    }
                }
                db.SaveChanges();
            }

        }

        public void ReleaseEngineAlert(WFEEngineAlert eA)
        {
            logger.Debug("DATABASE ACCESS: ReleaseEngineAlert: " + eA.EA_ID);
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                db.Database.ExecuteSqlCommand(string.Format("update EngineAlerts set EA_LockedByProcess=null, EA_Updated=getdate() where EA_WFI_ID like '{0}'", eA.EA_WFI_ID));
                db.SaveChanges();
            }
        }

        public void FinishEngineAlert(DbContextTransaction transaction, string startActivity, string workflowInstanceID)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction", "transaction cannot be null. please call StartTransaction()");
            if (this.databaseContextForTransaction == null)
                throw new NullReferenceException("unexpected error. databaseContextForTransaction must not be null. (did you call StartTransaction?)");

            this.doFinishEngineAlert(this.databaseContextForTransaction, startActivity, workflowInstanceID);

        }

        private void doFinishEngineAlert(WorkflowDBContext db, string startActivity, string workflowInstanceID)
        {
            //find alert and delete 
            EngineAlert eA = (from s1 in db.EngineAlerts
                              where s1.EA_WFI_ID == workflowInstanceID & s1.EA_StartActivity == startActivity
                              select s1).FirstOrDefault();
            if (eA != null)
            {
                WFEEngineAlert engineAlert = new WFEEngineAlert();
                ReflectionHelper.CopyProperties(ref eA, ref engineAlert);
                this.FinishEngineAlert(engineAlert);
                //eA.EA_Status = EnumEngineAlertStatus.Completed.ToString();
                //eA.EA_Updated = DateTime.Now;
                //eA.EA_LockedByProcess = null; // release lock again
                //db.SaveChanges();
            }
        }

        /// <summary>
        /// Sets the EA_Status to Completed and EA_Updated to now
        /// </summary>
        /// <param name="eA"></param>
        public void FinishEngineAlert(WFEEngineAlert eA)
        {
            logger.Debug("DATABASE ACCESS: FinishEngineAlert: " + eA.EA_ID + " Sets the EA_Status to Completed and EA_Updated to now.");

            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                db.Database.ExecuteSqlCommand(
                    string.Format(
                        "update EngineAlerts set EA_Status='{1}', EA_Updated=getdate() where EA_ID like '{0}'",
                        eA.EA_ID, EnumEngineAlertStatus.Completed.ToString())
                );
                db.SaveChanges();
            }
        }

        #endregion

        #region WorkflowDefinition

        public WFEWorkflowDefinition CreateNewWorkflowDefinition(WFEWorkflowDefinition newWFEDefinition, bool isCheckout)
        {

            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    WorkflowDefinition newWfDef = new WorkflowDefinition();
                    ReflectionHelper.CopyProperties(ref newWFEDefinition, ref newWfDef);


                    // alles von newwfdef ins newDBdef mappen
                    //make sure ID isnt used yet
                    bool alreadyExists = true;

                    if (isCheckout)
                        alreadyExists = false;
                    else
                        alreadyExists = db.WorkflowDefinitions.Where(wf => wf.WFD_Name == newWFEDefinition.WFD_Name).Count() > 0;


                    if (!alreadyExists)
                    {
                        newWfDef.Guid = Guid.NewGuid();
                        newWfDef.WFD_Created = DateTime.Now;
                        if (newWfDef.WFD_ValidFrom == null) newWfDef.WFD_ValidFrom = DateTime.Now;
                        db.WorkflowDefinitions.Add(newWfDef);
                        db.SaveChanges();

                        ReflectionHelper.CopyProperties(ref newWfDef, ref newWFEDefinition);

                        return newWFEDefinition;
                    }
                    else
                    {
                        string errMsg = string.Format("Duplicate Workflow Name not allowed {0}", newWfDef.WFD_Name);
                        throw new DefaultException(errMsg, DefaultErrorType.NameAlreadyExists);
                    }

                }
            }
            catch (DefaultException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Could not create new WorkflowDefinition {0}", newWFEDefinition.WFD_ID);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMsg, ex);
            }
        }

        public WFEWorkflowDefinition CreateNewWorkflowDefinition(WFEWorkflowDefinition newWFEDefinition)
        {
            return this.CreateNewWorkflowDefinition(newWFEDefinition, false);
        }

        public void UpdateWorkflowMetaData(Guid workflowGUID, string name, string description, DateTime? validFrom, DateTime? validTo)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var wf = db.WorkflowDefinitions.Where(p => p.Guid == workflowGUID).Single();

                    if (wf != null)
                    {
                        wf.WFD_Description = description;
                        wf.WFD_Name = name;
                        wf.WFD_ValidFrom = validFrom == null ? DateTime.Now : validFrom;
                        wf.WFD_ValidTo = validTo;
                        db.SaveChanges();
                    }
                    else
                    {
                        string msg = string.Format("Cannot find workflow with Guid = {0}", workflowGUID);
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Could not save workflow definition xml for WF-Guid : {0}", workflowGUID);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMsg, ex);
            }
        }

        public void UpdateWorkflowDefinitionObject(WFEWorkflowDefinition wfeDef)
        {
            try
            {
                WorkflowDefinition wfDef = new WorkflowDefinition();
                ReflectionHelper.CopyProperties(ref wfeDef, ref wfDef);

                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var wf = db.WorkflowDefinitions.Where(p => p.Guid == wfeDef.Guid).Single();

                    if (wf != null)
                    {

                        wf.WFD_CheckedOutBy = wfDef.WFD_CheckedOutBy;
                        wf.WFD_Created = wfDef.WFD_Created;
                        wf.WFD_Definition = wfDef.WFD_Definition;
                        wf.WFD_Description = wfDef.WFD_Description;
                        wf.WFD_ID = wfDef.WFD_ID;
                        wf.WFD_Name = wfDef.WFD_Name;
                        wf.WFD_ValidFrom = wfDef.WFD_ValidFrom;
                        wf.WFD_ValidTo = wfDef.WFD_ValidTo;
                        wf.WFD_Version = wfDef.WFD_Version;

                        db.SaveChanges();

                    }
                    else
                    {
                        string msg = string.Format("Cannot find workflow with Guid = {0}", wfDef.Guid);
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Could not save workflow definition xml for WF-Guid : {0}", wfeDef.Guid);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMsg, ex);
            }
        }

        public void DeletWorkflowByGUID(Guid guid)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var wf = db.WorkflowDefinitions.Where(p => p.Guid == guid).Single();
                    db.WorkflowDefinitions.Remove(wf);
                    db.SaveChanges();
                }
            }
            catch (InvalidOperationException ex)
            {
                string msg = string.Format("Cannot find workflow with Guid = {0}", guid.ToString());
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg, ex);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Could not delete workflow with Guid : {0}", guid.ToString());
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMsg, ex);
            }
        }

        /// <summary>
        /// updates the workflow definition
        /// throws BaseExceptions
        /// </summary>
        /// <param name="workflowGUID"></param>
        /// <param name="workflowXml"></param>
        public void UpdateWorkflowDefinitionXml(Guid workflowGUID, string workflowXml)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var wf = db.WorkflowDefinitions.Where(p => p.Guid == workflowGUID).Single();

                    wf.WFD_Definition = workflowXml;
                    db.SaveChanges();
                }
            }
            catch (InvalidOperationException ex)
            {
                string msg = string.Format("Cannot find workflow with Guid = {0}", workflowGUID);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg, ex);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Could not save workflow definition xml for WF-Guid : {0}", workflowGUID);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMsg, ex);
            }
        }

        /// <summary>
        /// increase workflow version number, sets userGuid
        /// throws Exceptions
        /// </summary>
        /// <param name="workflowGUID"></param>
        /// <param name="userGuid"></param>
        [Obsolete("old do not use anymore", true)]
        private void CheckinWorkflow(Guid workflowGUID, string userGuid)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var wf = db.WorkflowDefinitions.Where(p => p.Guid == workflowGUID).Single();

                    var latestCheckedInWorkflowVersion = this.GetWorkflowDefinition(wf.WFD_ID, false);
                    if (latestCheckedInWorkflowVersion != null)
                    {
                        wf.WFD_Version = latestCheckedInWorkflowVersion.WFD_Version + 1;
                        wf.WFD_CheckedOutBy = userGuid;
                    }
                    else
                    {
                        string msg = string.Format("Cannot find last checked in version of workflow with ID = {0}", wf.WFD_ID);
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Cannot check in version of workflow with GUID = {0}", workflowGUID);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }
        }

        /// <summary>
        /// a list of all rows in the table workflowdefinitions
        /// </summary>
        /// <param name="onlyactive">default = false, define whether last version should be called or all</param>
        /// <returns></returns>
        public List<WFEWorkflowDefinition> GetAllWorkflowDefinitions(bool onlyactive = false)
        {
            List<WFEWorkflowDefinition> wfeDefs = new List<WFEWorkflowDefinition>();
            try
            {
                if (onlyactive) return GetAllActiveWorkflowDefinitions();

                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    IQueryable<WorkflowDefinition> wfDefs = db.WorkflowDefinitions;
                    WorkflowDefinition[] wfDefinitions = wfDefs.ToArray<WorkflowDefinition>();
                    for (int i = 0; i < wfDefinitions.Count(); i++)
                    {
                        WorkflowDefinition wDef = wfDefinitions[i];
                        WFEWorkflowDefinition wfeDef = new WFEWorkflowDefinition();
                        ReflectionHelper.CopyProperties(ref wDef, ref wfeDef);
                        wfeDefs.Add(wfeDef);
                    }
                    return wfeDefs;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error reading workflow definitions", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
        }

        /// <summary>
        /// returns all active WorkflowDefinitions
        /// </summary>
        /// <returns></returns>
        public List<WFEWorkflowDefinition> GetAllActiveWorkflowDefinitions()
        {
            List<WFEWorkflowDefinition> wfeDefs = new List<WFEWorkflowDefinition>();
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var wfdefs = (from item in db.WorkflowDefinitions
                                  where item.WFD_Version ==
                                      (from subItem in db.WorkflowDefinitions
                                       where subItem.WFD_ID == item.WFD_ID
                                       select new { subItem.WFD_Version }
                                      ).Max(p => p.WFD_Version)
                                  select item).ToList();

                    foreach (WorkflowDefinition wfd in wfdefs)
                    {
                        WorkflowDefinition wfdTemp = wfd;
                        WFEWorkflowDefinition wfdWFE = new WFEWorkflowDefinition();
                        ReflectionHelper.CopyProperties(ref wfdTemp, ref wfdWFE);
                        wfeDefs.Add((WFEWorkflowDefinition)wfdWFE);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error reading workflow definitions", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
            return wfeDefs;
        }

        /// <summary>
        /// a list of ids in the table workflowdefinitions
        /// </summary>
        /// <returns></returns>
        public List<String> GetAllWorkflowDefinitionIDs()
        {
            List<String> result = new List<String>();
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    result = (from wfd in db.WorkflowDefinitions select wfd.WFD_ID).Distinct().ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error reading workflow definitions", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
        }

        /// <summary>
        /// gets latest version of workflow if getCheckedoutVersion === false
        /// gets checked out version if getCheckoutVersion === true
        /// throws exception
        /// </summary>
        /// <param name="workflowDefinitionID"></param>
        /// <param name="getCheckedOutVersion">set true if you want to get the version that is currently checked out</param>
        /// <returns>null if nothing found</returns>
        public WFEWorkflowDefinition GetWorkflowDefinition(string workflowDefinitionID, bool getCheckedOutVersion = false)
        {
            WorkflowDefinition wfDef = null;
            WFEWorkflowDefinition wfDefWFE = new WFEWorkflowDefinition();
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {

                    if (getCheckedOutVersion)
                    {
                        wfDef = (from item in db.WorkflowDefinitions
                                 where item.WFD_Version == -1 & item.WFD_ID == workflowDefinitionID
                                 select item).SingleOrDefault();
                    }
                    else
                    {
                        // get latest version that is not checked out (checkout out hast version -1)
                        wfDef = (from item in db.WorkflowDefinitions
                                 where item.WFD_ID == workflowDefinitionID & item.WFD_Version ==
                                     (from subItem in db.WorkflowDefinitions
                                      where subItem.WFD_ID == workflowDefinitionID
                                      select new { subItem.WFD_Version }
                                     ).Max(p => p.WFD_Version)
                                 select item).SingleOrDefault();
                    }
                }
                ReflectionHelper.CopyProperties(ref wfDef, ref wfDefWFE);
                return wfDefWFE;
            }
            catch (Exception ex)
            {
                string v = "Error try to get worklfow definition (ID = " + workflowDefinitionID + " )";
                this.logger.Error(v, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, v, ex);
            }
        }

        public WFEWorkflowDefinition GetWorkflowDefinitionByGuid(Guid theGuid)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var wfdef = db.WorkflowDefinitions.Where(wd => wd.Guid == theGuid).ToList();
                    if (wfdef.Count == 1)
                    {
                        WorkflowDefinition def = wfdef[0];
                        WFEWorkflowDefinition result = new WFEWorkflowDefinition();
                        ReflectionHelper.CopyProperties(ref def, ref result);
                        return result;
                    }
                    else
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Cannot find exaclty one worfklowdefinition for GUID = " + theGuid.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error reading a activity definition (GUID = " + theGuid.ToString() + " )", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
        }

        #endregion

        #region Workflow Instances
        /// <summary>
        /// returns a dictionary(wfi_id,inputParameters) from all active workflow instances
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAllActiveWorkflowInstancesInputParameters()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (WorkflowDBContext dbContext = new WorkflowDBContext())
            {
                var x = from wfi in dbContext.WorkflowInstances
                        join ea in dbContext.EngineAlerts on wfi.WFI_ID equals ea.EA_WFI_ID
                        where ea.EA_StartActivity.Equals("start") &
                            wfi.WFI_Status == EnumWorkflowInstanceStatus.Executing.ToString() &
                            wfi.WFI_Status == EnumWorkflowInstanceStatus.Sleeping.ToString() &
                            wfi.WFI_Status == EnumWorkflowInstanceStatus.Paused.ToString()
                        select new { ea.EA_WFI_ID, ea.EA_InputParameters };
                result = x.ToDictionary(k => k.EA_WFI_ID, v => v.EA_InputParameters);
            }
            return result;
        }
        #endregion

        /// <summary>
        /// stores given WorkflowInstance to DB
        /// </summary>
        /// <param name="workflowInstance"></param>
        public void AddWorkflowInstance(WorkflowInstance workflowInstance)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                db.WorkflowInstances.Add(workflowInstance);
                db.SaveChanges();
            }
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Creates new wf instance./ </summary>
        /// <param name="definitionID"> Name of the new workflow instance. </param>
        /// <param name="instanceID"> Name of the new workflow instance. </param>
        /// <param name="parentWorkflowInstanceID"> if this is a subworkflow being created, write the parent WF  </param>
        /// <param name="nextActivity"> if this is a subworkflow being created, remember next activitry in parent to call when subwf finished </param>
        /// <returns>   the xml of workflow </returns>
        #endregion
        public string CreateNewWorkflowInstance(string definitionID, string instanceID, string parentWorkflowInstanceID = "", string nextActivity = "")
        {
            WFEWorkflowDefinition wdf = this.GetWorkflowDefinition(definitionID, false); // get latest version if workflow that isn't checked out

            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                WorkflowInstance wfi = new WorkflowInstance();
                DateTime jetzt = DateTime.Now;

                //get definition xml via definitionID

                if (wdf != null)
                {
                    wfi.WFI_ID = instanceID;
                    wfi.WFI_WFD_ID = definitionID;
                    wfi.WFI_Xml = wdf.WFD_Definition;
                    //TODO : maybe add the version if definition used to instance ?  wdf.WFD_Version
                    wfi.WFI_Status = EnumWorkflowInstanceStatus.NotStarted.ToString();
                    wfi.WFI_CurrentActivity = "start";
                    wfi.WFI_ParentWF = parentWorkflowInstanceID;
                    wfi.WFI_NextActivity = nextActivity;
                    wfi.WFI_Created = jetzt;
                    wfi.WFI_Updated = jetzt;

                    db.WorkflowInstances.Add(wfi);
                    db.SaveChanges();
                    return wdf.WFD_Definition;
                }
                else
                {
                    string errMsg = string.Format("Could not find Workflow Definition {0}", definitionID);
                    throw new BaseException(ErrorCodeHandler.E_WF_CREATE_INSTANCE, errMsg);
                }
            }
        }

        public string GetInstanceXml(string instanceID)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                string wfiXml = db.Database.SqlQuery<string>("SELECT WFI_Xml from WorkflowInstance WHERE WFI_ID = '" + instanceID + "'").FirstOrDefault();
                return wfiXml;
                //WorkflowInstance wfi = new WorkflowInstance();
                //wfi = db.WorkflowInstances.SingleOrDefault(delegate (WorkflowInstance w) { return w.WFI_ID.Equals(instanceID); });
                //if (wfi != null)
                //{
                //    return wfi.WFI_Xml;
                //}
                //else
                //    return null;
            }
        }

        public WFEWorkflowInstance GetWorkflowInstance(string instanceID)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                //WorkflowInstance wfi = new WorkflowInstance();
                //wfi = db.WorkflowInstances.SingleOrDefault(delegate (WorkflowInstance w) { return w.WFI_ID.Equals(instanceID); });
                WorkflowInstance wfi = db.Database.SqlQuery<WorkflowInstance>("SELECT * from WorkflowInstance WHERE WFI_ID = '" + instanceID + "'").FirstOrDefault();
                if (wfi != null)
                {
                    WFEWorkflowInstance wfei = new WFEWorkflowInstance();
                    ReflectionHelper.CopyProperties(ref wfi, ref wfei);
                    return wfei;
                }
                else
                    return null;
            }
        }

        public List<WorkflowInstance> GetAllWorkflowInstances()
        {
            List<WorkflowInstance> workflowInstances = new List<WorkflowInstance>();
            try
            {
                using (WorkflowDBContext dbContext = new WorkflowDBContext())
                {
                    var x = from wfi in dbContext.WorkflowInstances
                            select new { wfi.WFI_ID, wfi.WFI_WFD_ID, wfi.WFI_Status, wfi.WFI_CurrentActivity, wfi.WFI_ParentWF, wfi.WFI_NextActivity, wfi.WFI_Created, wfi.WFI_Updated, wfi.WFI_Finished, wfi.WFI_ProcessTime };

                    foreach (var item in x)
                    {
                        workflowInstances.Add(new WorkflowInstance()
                        {
                            WFI_ID = item.WFI_ID,
                            WFI_WFD_ID = item.WFI_WFD_ID,
                            WFI_Status = item.WFI_Status,
                            WFI_CurrentActivity = item.WFI_CurrentActivity,
                            WFI_ParentWF = item.WFI_ParentWF,
                            WFI_NextActivity = item.WFI_NextActivity,
                            WFI_Created = item.WFI_Created,
                            WFI_Updated = item.WFI_Updated,
                            WFI_Finished = item.WFI_Finished,
                            WFI_ProcessTime = item.WFI_ProcessTime
                        });
                    }

                    return workflowInstances;
                }
            }
            catch (Exception ex)
            {
                logger.Error("error getting workflow instances", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
        }

        public int GetActiveWorkflowProcesses(string wfDefinition)
        {
            int amount = -1;
            try
            {
                using (WorkflowDBContext dbContext = new WorkflowDBContext())
                {
                    amount = (from wfi in dbContext.WorkflowInstances
                              where (
                                     wfi.WFI_Status == EnumWorkflowInstanceStatus.Executing.ToString() |
                              wfi.WFI_Status == EnumWorkflowInstanceStatus.Sleeping.ToString() |
                                     wfi.WFI_Status == EnumWorkflowInstanceStatus.Paused.ToString()
                                    )
                                    & wfi.WFI_WFD_ID == wfDefinition
                              select wfi.WFI_ID).Count();
                }
            }
            catch (Exception ex)
            {
                logger.Error("error getting processes count", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
            return amount;
        }

        public void UpdateInstanceXml(string instanceID, string instannceXml)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                try
                {
                    string query = "UPDATE WorkflowInstance SET WFI_Xml='{0}', WFI_Updated=getdate() WHERE WFI_ID = '{1}'";
                    db.Database.ExecuteSqlCommand(string.Format(query, instannceXml.Replace("'", "''"), instanceID));
                }
                catch (Exception ex)
                {
                    logger.Error("error updating wf xml. xml was:\r\n" + instannceXml + "\r\n", ex);
                    throw;
                }
            }
        }

        public void UpdateInstanceStatus(string instanceID, EnumWorkflowInstanceStatus newStatus)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                string query = "";

                if (newStatus == EnumWorkflowInstanceStatus.Finish)
                {
                    query = "UPDATE WorkflowInstance SET WFI_Status='{0}', WFI_Updated=getdate(), WFI_Finished=getdate() WHERE WFI_ID = '{1}'";
                }
                else
                {
                    query = "UPDATE WorkflowInstance SET WFI_Status='{0}', WFI_Updated=getdate() WHERE WFI_ID = '{1}'";
                }
                db.Database.ExecuteSqlCommand(string.Format(query, newStatus.ToString(), instanceID));
            }
        }

        /// <summary>
        /// Updates the WorkflowInstance with the current activity and its StartTime (WFI_CurrentActivity, WFI_Updated)
        /// 
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="currentActivityInstanceName"></param>
        public void UpdateInstanceCurrentActivity(string instanceID, string currentActivityInstanceName)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                string query = "UPDATE WorkflowInstance SET WFI_CurrentActivity='{0}', WFI_Updated=getdate() WHERE WFI_ID = '{1}'";
                db.Database.ExecuteSqlCommand(string.Format(query, currentActivityInstanceName, instanceID));
            }
        }

        public bool PollingAlertExists(string activityID, string wfInstanceID)
        {
            bool doesExist = false;
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                EngineAlert aa = db.EngineAlerts.SingleOrDefault(delegate (EngineAlert eA)
                {
                    if (eA.EA_WFI_ID == wfInstanceID
                        & eA.EA_StartActivity == activityID
                        & eA.EA_Type == EnumAlertTypes.Polling.ToString())
                    {
                        return true;
                    }
                    else
                        return false;
                });

                if (aa != null)
                    doesExist = true;
            }
            return doesExist;
        }

        public void UpdateLastPolling(string activityID, string wfInstanceID, DateTime? lastPoll)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                this.UpdateLastPolling(db, activityID, wfInstanceID, lastPoll);
            }
        }

        public void UpdateLastPolling(DbContextTransaction transaction, string activityID, string wfInstanceID, DateTime? lastPoll)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction", "transaction cannot be null. please call StartTransaction()");
            if (this.databaseContextForTransaction == null)
                throw new NullReferenceException("unexpected error. databaseContextForTransaction must not be null. (did you call StartTransaction?)");

            this.UpdateLastPolling(this.databaseContextForTransaction, activityID, wfInstanceID, lastPoll);
        }

        private void UpdateLastPolling(WorkflowDBContext db, string activityID, string wfInstanceID, DateTime? lastPoll)
        {
            //find alert and delete 
            EngineAlert eA = (from s1 in db.EngineAlerts
                              where s1.EA_WFI_ID == wfInstanceID & s1.EA_StartActivity == activityID
                              select s1).FirstOrDefault();
            if (eA != null)
            {
                eA.EA_LastPolling = lastPoll;
                db.SaveChanges();
            }
        }


        public DbContextTransaction StartTransaction()
        {
            this.databaseContextForTransaction = new DataLayer.WorkflowDBContext();
            return this.databaseContextForTransaction.Database.BeginTransaction();
        }

        public void EndTransaction(DbContextTransaction tr)
        {

            if (this.databaseContextForTransaction != null)
            {
                this.databaseContextForTransaction.Dispose();
            }
            tr.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="activityInstanceID"></param>
        /// <param name="waitItemConfig"></param>
        /// <param name="duedate"></param>
        /// <returns></returns>
        public int CreateWaitItem(string instanceID, string activityInstanceID, string waitItemConfig, DateTime? duedate)
        {
            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                AsyncWaitItem async = this.doCreateWaitItem(instanceID, activityInstanceID, waitItemConfig, duedate, db);
                return async.AWI_ID;
            }
        }

        /// <summary>
        /// uses transaction to create waititem, throws exception with detailed err msg. 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="instanceID"></param>
        /// <param name="activityInstanceID"></param>
        /// <param name="waitItemConfig"></param>
        /// <param name="duedate"></param>
        /// <returns></returns>
        public int CreateWaitItem(DbContextTransaction transaction, string instanceID, string activityInstanceID, string waitItemConfig, DateTime? duedate)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction", "transaction cannot be null. please call StartTransaction()");
            if (this.databaseContextForTransaction == null)
                throw new NullReferenceException("unexpected error. databaseContextForTransaction must not be null. (did you call StartTransaction?)");

            AsyncWaitItem async = this.doCreateWaitItem(instanceID, activityInstanceID, waitItemConfig, duedate, this.databaseContextForTransaction);
            return async.AWI_ID;
        }

        private AsyncWaitItem doCreateWaitItem(string instanceID, string activityInstanceID, string waitItemConfig, DateTime? duedate, WorkflowDBContext db)
        {
            try
            {
                AsyncWaitItem async = new AsyncWaitItem();
                async.AWI_InstanceID = instanceID;
                async.AWI_ActivityInstanceID = activityInstanceID;
                async.AWI_Config = waitItemConfig;
                async.AWI_StartDate = DateTime.Now;
                async.AWI_Status = EnumTaskStatus.Wait.ToString();
                async.AWI_DueDate = duedate;
                async.AWI_Modified = DateTime.Now;
                async.AWI_Created = DateTime.Now;
                db.AsyncWaitItems.Add(async);
                db.SaveChanges();
                return async;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("cannot create AsyncWaitItem for workflowinstance: {0} activity: {1}", instanceID, activityInstanceID);
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
            }
        }

        public WFEAsyncWaitItem GetWaitItem(int awiID)
        {
            AsyncWaitItem waitItem = new AsyncWaitItem();
            WFEAsyncWaitItem wfeWaitItem = new WFEAsyncWaitItem();

            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                waitItem = db.AsyncWaitItems.SingleOrDefault(delegate (AsyncWaitItem a)
                {
                    return a.AWI_ID == awiID;
                });
                if (waitItem != null)
                {
                    ReflectionHelper.CopyProperties(ref waitItem, ref wfeWaitItem);
                    return wfeWaitItem;
                }
                else
                    return null;
            }
        }

        public WFEAsyncWaitItem GetWaitItem(string instanceID, string activityInstanceID)
        {
            AsyncWaitItem waitItem = new AsyncWaitItem();
            WFEAsyncWaitItem wfeWaitItem = new WFEAsyncWaitItem();

            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                waitItem = db.AsyncWaitItems.SingleOrDefault(delegate (AsyncWaitItem a)
                {
                    return a.AWI_InstanceID == instanceID & a.AWI_ActivityInstanceID == activityInstanceID;
                });
                if (waitItem != null)
                {
                    ReflectionHelper.CopyProperties(ref waitItem, ref wfeWaitItem);
                    return wfeWaitItem;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// updates AWI_Config and AWI_Duedate only !
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="activityInstanceID"></param>
        /// <param name="waitItem"></param>
        public void UpdateWaitItem(string instanceID, string activityInstanceID, WFEAsyncWaitItem waitItem)
        {
            AsyncWaitItem dbItem = new AsyncWaitItem();
            WFEAsyncWaitItem wfeWaitItem = new WFEAsyncWaitItem();

            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                dbItem = db.AsyncWaitItems.SingleOrDefault(delegate (AsyncWaitItem a)
                {
                    return a.AWI_InstanceID == instanceID & a.AWI_ActivityInstanceID == activityInstanceID;
                });

                dbItem = this.doUpdateWaitItem(dbItem, waitItem);
                db.SaveChanges();
            }
        }
        public void UpdateWaitItem(DbContextTransaction transaction, string instanceID, string activityInstanceID, WFEAsyncWaitItem waitItem)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction", "transaction cannot be null. please call StartTransaction()");
            if (this.databaseContextForTransaction == null)
                throw new NullReferenceException("unexpected error. databaseContextForTransaction must not be null. (did you call StartTransaction?)");

            AsyncWaitItem dbItem = new AsyncWaitItem();
            WFEAsyncWaitItem wfeWaitItem = new WFEAsyncWaitItem();

            dbItem = this.databaseContextForTransaction.AsyncWaitItems.SingleOrDefault(delegate (AsyncWaitItem a)
            {
                return a.AWI_InstanceID == instanceID & a.AWI_ActivityInstanceID == activityInstanceID;
            });

            dbItem = this.doUpdateWaitItem(dbItem, waitItem);
            this.databaseContextForTransaction.SaveChanges();


        }
        public void UpdateWaitItem(WFEAsyncWaitItem waitItem)
        {
            AsyncWaitItem dbItem = new AsyncWaitItem();
            WFEAsyncWaitItem wfeWaitItem = new WFEAsyncWaitItem();

            using (WorkflowDBContext db = new WorkflowDBContext())
            {
                dbItem = db.AsyncWaitItems.SingleOrDefault(delegate (AsyncWaitItem a)
                {
                    return a.AWI_ID == waitItem.AWI_ID;
                });

                dbItem = this.doUpdateWaitItem(dbItem, waitItem);
                db.SaveChanges();
            }
        }

        private AsyncWaitItem doUpdateWaitItem(AsyncWaitItem dbItem, WFEAsyncWaitItem updatedItem)
        {
            if (updatedItem != null)
            {
                dbItem.AWI_Config = updatedItem.AWI_Config;
                dbItem.AWI_DueDate = updatedItem.AWI_DueDate;
                dbItem.AWI_Modified = DateTime.Now;

            }
            return dbItem;
        }

        public string WriteXmlDataForWorkflow(string instanceID, string xmlData)
        {
            return "";
            //using (WorkflowDBContext db = new WorkflowDBContext())
            //{
            //    WorkflowXmlDataRepository xmlRep = new WorkflowXmlDataRepository();

            //    xmlRep.Guid = Guid.NewGuid().ToString("N");
            //    xmlRep.XML = xmlData;
            //    xmlRep.WFI_ID = instanceID;
            //    db.WorkflowXmlDataRepositorys.Add(xmlRep);
            //    db.SaveChanges();
            //    return xmlRep.Guid;
            //}
        }

        /// <summary>
        /// gets all acitivity definitions from databaser
        /// </summary>
        /// <returns></returns>
        public List<WFEActivityDefinition> GetAllActivityDefinitons()
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    List<WFEActivityDefinition> wfeActivityDefs = new List<WFEActivityDefinition>();
                    IQueryable<ActivityDefinition> activityDefs = db.ActivityDefinitions;
                    ActivityDefinition[] aDefs = activityDefs.ToArray<ActivityDefinition>();
                    for (int i = 0; i < aDefs.Count(); i++)
                    {
                        ActivityDefinition aDef = aDefs[i];
                        WFEActivityDefinition def = MapActivity(aDef);
                        wfeActivityDefs.Add(def);
                    }
                    return wfeActivityDefs;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error reading activity definitions", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
        }


        public WFEActivityDefinition GetActivityDefinitionTemplate(string activityDefinitionID)
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    WFEActivityDefinition aDef = new WFEActivityDefinition();
                    var activityDefs = db.ActivityDefinitions.Where(ac => ac.WFAD_ID == activityDefinitionID).ToList();
                    if (activityDefs.Count == 1)
                    {
                        return MapActivity(activityDefs[0]);
                    }
                    else
                        throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Cannot find one activitydefinition template for ID = " + activityDefinitionID);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error reading a activity definition (ID = " + activityDefinitionID + " )", ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, ex);
            }
        }

        private static WFEActivityDefinition MapActivity(ActivityDefinition aDef)
        {
            return new WFEActivityDefinition()
            {
                WFAD_ConfigTemplate = aDef.WFAD_ConfigTemplate,
                WFAD_Created = aDef.WFAD_Created,
                WFAD_HostLoad = aDef.WFAD_HostLoad.Value,
                WFAD_ID = aDef.WFAD_ID,
                WFAD_IsStartActivity = aDef.WFAD_IsStartActivity,
                WFAD_Name = aDef.WFAD_Name,
                WFAD_Type = (EnumActivityType)aDef.WFAD_Type,
                WFAD_ValidFrom = aDef.WFAD_ValidFrom,
                WFAD_ValidTo = aDef.WFAD_ValidTo,
            };
        }
        private static ActivityDefinition MapActivity(WFEActivityDefinition aDef)
        {
            return new ActivityDefinition()
            {
                WFAD_ConfigTemplate = aDef.WFAD_ConfigTemplate,
                WFAD_Created = aDef.WFAD_Created,
                WFAD_HostLoad = aDef.WFAD_HostLoad,
                WFAD_ID = aDef.WFAD_ID,
                WFAD_IsStartActivity = aDef.WFAD_IsStartActivity,
                WFAD_Name = aDef.WFAD_Name,
                WFAD_Type = (int)aDef.WFAD_Type,
                WFAD_ValidFrom = aDef.WFAD_ValidFrom,
                WFAD_ValidTo = aDef.WFAD_ValidTo,
            };
        }

        public int GetAmountOfPollingWaitActivities()
        {
            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    int eaCount = (from ea in db.EngineAlerts where ea.EA_Type == "Polling" && ea.EA_Status != EnumEngineAlertStatus.Completed.ToString() && ea.EA_Status != EnumEngineAlertStatus.Aborted.ToString() select ea.EA_WFI_ID).Distinct().Count();
                    return eaCount;
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(
                    ErrorCodeHandler.E_WF_GENERAL, "Error trying to get count for engine alerts that are locked by process.", ex);
            }
        }




        public WFEActivityResultMessage SaveActivityResultMessageToWorkflowInstance(string workflowInstanceID, string activityInstanceID, string resultMessage)
        {
            if (workflowInstanceID == null)
                throw new ArgumentNullException("workflowInstanceID");
            if (activityInstanceID == null)
                throw new ArgumentNullException("activityInstanceID");
            if (resultMessage == null)
                throw new ArgumentNullException("resultMessage");

            ActivityResultMessage newMesssage = null;
            WFEActivityResultMessage wfeMsg = null;
            try
            {
                newMesssage = new ActivityResultMessage()
                {
                    ARM_ID = Guid.NewGuid(),
                    ARM_Woin = workflowInstanceID,
                    ARM_ActivityInstanceId = activityInstanceID,
                    ARM_ResultMessage = resultMessage,
                    ARM_Created = DateTime.Now
                };
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    db.ActivityResultMessages.Add(newMesssage);
                    db.SaveChanges();
                }
                wfeMsg = new WFEActivityResultMessage()
                {
                    ARM_ActivityInstanceId = newMesssage.ARM_ActivityInstanceId,
                    ARM_Created = newMesssage.ARM_Created,
                    ARM_ID = newMesssage.ARM_ID,
                    ARM_ResultMessage = newMesssage.ARM_ResultMessage,
                    ARM_Woin = newMesssage.ARM_Woin
                };
            }
            catch (DbEntityValidationException ex)
            {
                string errMsg = "DbEntityValidationException:";
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        errMsg += ("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage) + Environment.NewLine;
                    }
                }
                errMsg += string.Format("Error adding activity result messages for woin: {0} activityInstanceID: {1}", workflowInstanceID, activityInstanceID);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMsg, ex);
            }
            catch (Exception ex)
            {
                string message =
                    string.Format("Error adding activity result messages for woin: {0} activityInstanceID: {1}", workflowInstanceID, activityInstanceID);
                logger.Error(message, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, message, ex);
            }


            return wfeMsg;
        }

        public List<WFEActivityResultMessage> GetActivityResultMsgByWoin(string workflowInstanceID)
        {
            List<ActivityResultMessage> result = null;
            List<WFEActivityResultMessage> resultList = null;

            try
            {
                using (WorkflowDBContext db = new WorkflowDBContext())
                {
                    var xx = from arm in db.ActivityResultMessages where arm.ARM_Woin == workflowInstanceID orderby arm.ARM_Created ascending select arm;
                    if (xx != null)
                    {
                        result = xx.ToList();
                    }
                }

                if (result != null)
                {
                    resultList = new List<WFEActivityResultMessage>();
                    foreach (ActivityResultMessage item in result)
                    {
                        WFEActivityResultMessage wfeMsg = new WFEActivityResultMessage()
                        {
                            ARM_ActivityInstanceId = item.ARM_ActivityInstanceId,
                            ARM_Created = item.ARM_Created,
                            ARM_ID = item.ARM_ID,
                            ARM_ResultMessage = item.ARM_ResultMessage,
                            ARM_Woin = item.ARM_Woin
                        };
                        resultList.Add(wfeMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = "Error reading activity result messages for woin: " + workflowInstanceID;
                logger.Error(message, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, message, ex);
            }
            return resultList;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.databaseContextForTransaction != null)
                        this.databaseContextForTransaction.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }




        #endregion

    }
}
