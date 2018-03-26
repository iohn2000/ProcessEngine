using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Variables;
using Kapsch.IS.EDP.WFActivity.Variables;

namespace Kapsch.IS.EDP.WFActivity
{
    public abstract class BaseEDPAsyncActivity : BaseAsyncActivity, IEdpFeatures, IProcessStep
    {
        private EdpFeatures edpFeatures;
        private Variable timeOutIntervalDays;
        protected internal WFEAsyncWaitItem waitItemWFE;
        private Variable pollingIntervalSeconds;
        protected internal EngineContext engineContext;


        public BaseEDPAsyncActivity(Type type) : base(type)
        {
            this.edpFeatures = EdpFeatures.GetInstance();
        }

        /// <summary>
        /// calculates workflow variables and additionally resolves all @@ queries from core.
        /// Its recommended to always use this for CORE related activities. (becauee EntityQuery is available)
        /// </summary>
        /// <param name="engineContext"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName)
        {
            return this.GetProcessedActivityVariable(engineContext, propertyName, false);
        }

        public Variable GetProcessedActivityVariable(EngineContext engineContext, string propertyName, bool nullable)
        {
            try
            {
                Variable procEngVar = engineContext.GetActivityVariable(propertyName);

                string processedValue = new WFReplacer(procEngVar.VarValue, engineContext.WorkflowModel).Replace().ProcessedValue;

                processedValue = new EDPReplacer(processedValue).Replace().ProcessedValue;

                processedValue = base.Evaluate(processedValue, engineContext);

                procEngVar.VarValue = processedValue;

                return new Variable(procEngVar.Name, processedValue.ToString(), procEngVar.DataType, procEngVar.Direction);
            }
            catch (BaseException bEx)
            {
                string errMsg = string.Format("{0} BaseException in GetProcessedActivityVariable. Variable Name: {1}", 
                    this.getWorkflowLoggingContext(engineContext), propertyName);
                if (nullable)
                {
                    logger.Warn(errMsg, bEx);
                    return null;
                };
                logger.Error(errMsg, bEx);
                throw bEx;
            }

            catch (Exception ex)
            {
                string errMsg = string.Format("{0} General error in GetProcessedActivityVariable. Variable Name: {1}", 
                    this.getWorkflowLoggingContext(engineContext), propertyName);
                if (nullable)
                {
                    logger.Warn(errMsg, ex);
                    return null;
                };
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
            }
        }

        public override StepReturn Initialize(EngineContext engineContext)
        {
            this.engineContext = engineContext;
            base.updateEngineAlertLastPollingToNow(engineContext);
            logger.Debug(string.Format("{0} : updated Engine Alert.", base.getWorkflowLoggingContext(engineContext)));
            StepReturn result = CheckForTimeout(engineContext);

            if (result != null)
            {
                return result;
            }

            result = new StepReturn("EDP Task not finished.", EnumStepState.Wait);

            try
            {
                //TODO this is an EDP specific implementation which should be changed!! (easy since it is only an int value)
                this.pollingIntervalSeconds = GetProcessedActivityVariable(engineContext, "pollingIntervalSeconds");
            }
            catch (Exception ex)
            {
                return logErrorAndReturnStepState(engineContext, ex, "Error when initializing.", EnumStepState.ErrorToHandle);
            }

            try
            {
                // step 0) first time here create an engine alert with "polling" flag and -schedule and leave
                bool notFirstTime = base.AddAlertIfFirstTime(engineContext, this.pollingIntervalSeconds);

                // only poll from second time onwards (alert already exsits)
                if (notFirstTime)
                {
                    BaseAsyncRequestResult baseResult;
                    bool resultAvailable = isResultAvailable(null, out baseResult);

                    HandleReminder(engineContext, baseResult, resultAvailable);

                    if (resultAvailable)
                    {
                        result = PostInitialize(engineContext, baseResult);
                    }
                    else
                    {
                        logger.Debug(base.getWorkflowLoggingContext(engineContext) + "BaseEDPAsyncActivity called isResultAvailable() for Activtity. resultAvailable = " + resultAvailable.ToString());
                    }
                }
                else
                {
                    logger.Debug(base.getWorkflowLoggingContext(engineContext) + "First Time called => Calling process Wait status");
                    return ProcessWaitStatus(engineContext);
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in BaseEDPAsyncActivityWait Initialize", ex);
                result.ReturnValue = base.getWorkflowLoggingContext(engineContext) + " BaseEDPAsyncActivityWait Error";
                result.StepState = EnumStepState.ErrorToHandle;
                result.DetailedDescription = ex.ToString();
            }

            return result;

        }

        protected internal StepReturn ProcessWaitStatus(EngineContext engineContext)
        {
            StepReturn retStep;
            try
            {
                logger.Debug(base.getWorkflowLoggingContext(engineContext) + "Set Step to Wait for next Polling");
                retStep = new StepReturn("Waiting for Result", EnumStepState.Wait);
                return retStep;
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in WaitActivity when processing wait status.", ex);
                retStep = new StepReturn("", EnumStepState.ErrorToHandle);
                return retStep;
            }
        }

        /// <summary>
        /// check if this AsyncWaitActivity is timeouted. (Wait status lasts to long against WorkflowConfifgurationVariable "timeOutIntervalDays" )
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        public StepReturn CheckForTimeout(EngineContext engineContext)
        {
            this.timeOutIntervalDays = this.GetProcessedActivityVariable(engineContext, "timeOutIntervalDays");

            DateTime Now = DateTime.Now;
            DateTime TimeOut = engineContext.EngineAlert.EA_Created.AddDays(timeOutIntervalDays.GetDoubleValue());
            bool timeouted = Now > TimeOut;

            if (timeouted)
            {
                return new StepReturn("timeout happened", EnumStepState.Timeout);
            }

            return null;
        }
    }


}
