using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using System;

namespace Kapsch.IS.ProcessEngine.WFActivity
{
    /// <summary>
    /// what do all async activities in common ?
    /// </summary>
    public abstract class BaseAsyncActivity : BaseActivity, IProcessStep
    {
        /// <summary>
        /// pass type in for logging
        /// </summary>
        /// <param name="type"></param>
        public BaseAsyncActivity(Type type) : base (type)
        {

        }

        /// <summary>
        /// Standard method to be implemented by any Async Activity for its doing
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="baseResult"></param>
        /// <returns></returns>
        public abstract StepReturn PostInitialize(EngineContext engineContext, BaseAsyncRequestResult baseResult);

        /// <summary>
        /// check if a polling type alert exists for this wf instance and activity id
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="pollingIntervalSeconds"></param>        
        public bool AddAlertIfFirstTime(EngineContext engineContext, Variable pollingIntervalSeconds)
        {
            DatabaseAccess db = new DatabaseAccess();
            bool alertExists = db.PollingAlertExists(engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID);
            if (!alertExists)
            {                
                int pollingInterval = pollingIntervalSeconds.GetIntValue();
                db.CreateEngineAlert(engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID, "", "", EnumAlertTypes.Polling, pollingInterval, null);
            }
            return alertExists;
        }

        public abstract bool isResultAvailable(String AsyncRequestID, out BaseAsyncRequestResult AsyncRequestResult);

        public abstract StepReturn HandleReminder(EngineContext engineContext, BaseAsyncRequestResult baseResult, bool resultAvailable);

        /// <summary>
        /// set Activity Status to Finish 
        /// </summary>
        /// <param name="wI"></param>
        public void finishWaitItem(WFEAsyncWaitItem wI)
        {
            DatabaseAccess db = new DatabaseAccess();
            db.FinishWaitItem(wI.AWI_ID);
        }

        /// <summary>
        /// Set last Polling in EngineAlert to NOW
        /// </summary>
        /// <param name="engineContext"></param>
        public void updateEngineAlertLastPollingToNow(EngineContext engineContext)
        {
            DatabaseAccess db = new DatabaseAccess();
            db.UpdateLastPolling(engineContext.CurrenActivity.Instance, engineContext.WorkflowModel.InstanceID, DateTime.Now);
        }
    }
}
