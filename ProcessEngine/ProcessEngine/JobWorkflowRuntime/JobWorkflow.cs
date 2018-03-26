using Kapsch.IS.ProcessEngine;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Quartz;
using System;
using System.IO;
using System.Reflection;

namespace Kapsch.IS
{
    [PersistJobDataAfterExecution, DisallowConcurrentExecution]
    public class JobWorkflow : IJob
    {
        private static JobWorkflowRuntime wfJobInstance;

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                wfJobInstance = JobWorkflowRuntime.Instance;
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_XML_GENERALERROR, "The instance of the class JobWorkflowRuntime could not be created", ex);
            }
            try
            {
                wfJobInstance.Execute(context);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_SCHEDULING_JOBFETCHER_PROBLEM, "The Execute method of the class JobWorkflowRuntime has thrown an error", ex);
            }
        }
    }

    [PersistJobDataAfterExecution, DisallowConcurrentExecution]
    public class JobWorkflowRuntime : IJob
    {
        private IEDPLogger logger = EDPLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static JobWorkflowRuntime instance;

        public static JobWorkflowRuntime Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new JobWorkflowRuntime();
                }
                return instance;
            }
        }
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                // this job will be fired rather often, so no Info-Log. (only Debug, Error, Warning)
                try
                {
                    this.Init(context);
                }
                catch (Exception exc)
                {
                    BaseException bexc = new BaseException(ErrorCodeHandler.E_WF_GENERAL, exc);
                    //should not throw exception inside job so handle this error
                    this.handleError("Exception caught while initializing JobFetcher", bexc); ;
                }
                try
                {
                    this.Run(context);
                }
                catch (Exception exc)
                {
                    BaseException bexc = new BaseException(ErrorCodeHandler.E_WF_GENERAL, exc);
                    //should not throw exception inside job so handle this error
                    this.handleError("Exception caught while running JobFetcher", bexc); ;
                }

                // past your strange ideas HERE

            }
            catch (Exception exc)
            {
                BaseException bexc = new BaseException(ErrorCodeHandler.E_WF_GENERAL, exc);
                //should not throw exception inside job so handle this error
                this.handleError("Exception caught while fetching Jobs", bexc);
            }
        }

        private void Run(IJobExecutionContext context)
        {
            Runtime run = new Runtime();
            string returnMsg = run.RunEngine();
            logger.Debug(returnMsg);
        }

        private void Init(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Handles errors on this level. </summary>
        ///
        /// <remarks>   Mayerr, 17.09.2014. </remarks>
        ///
        /// <param name="message">  The message. </param>
        /// <param name="exc">      Details of the exception. </param>
        #endregion
        private void handleError(String message, BaseException exc)
        {
            // This Error has to be logged to Eventlog so just create an error-Log-Entry
            IEDPLogger logger = EDPLogger.GetLogger();
            logger.Error(message, exc);

            //job should be automatically restarted next time!!!
            return;
        }

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   Handles errors on this level. </summary>
        ///
        /// <remarks>   Mayerr, 17.09.2014. </remarks>
        ///
        /// <param name="exc">  Details of the exception. </param>
        #endregion
        private void handleError(BaseException exc)
        {
            handleError(exc.Message, exc);// This Error has to be logged to Eventlog so just create an error-Log-Entry
            return;
        }
    }
}
