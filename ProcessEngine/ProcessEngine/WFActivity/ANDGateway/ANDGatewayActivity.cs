using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.ProcessEngine.WFActivity.ANDGateway
{
    public class ANDGatewayActivity : SimpleBaseActivity, IProcessStep, IActivityValidator
    {
        public ANDGatewayActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("", EnumStepState.Wait);
            string logLine = "";
            try
            {
                bool allIncomingFinished = engineContext.CurrenActivity.AllInComingTransitionsProcessingCompleted(
                        engineContext.ExecutionIteration.IterationNumber,
                        engineContext.CurrentTransistion);

                logLine = string.Format("{0} : AND Gateway '{1}' ---> '{2}' , allFinished={3}",
                    base.getWorkflowLoggingContext(engineContext),
                    engineContext.CurrentTransistion.FromActivityID,
                    engineContext.CurrentTransistion.ToActivityID,
                    allIncomingFinished.ToString()
                    );

                if (allIncomingFinished)
                {
                    ret.StepState = EnumStepState.Complete;
                    engineContext.ExecutionIteration.AddMessage("message", "synchronise (zusammenwarten) finished");
                    // make sure all iterations from this are not set to complete
                    engineContext.CurrenActivity.SetAllExecutionIterationsToComplete();
                    logger.Info(logLine);
                }
                else
                    logger.Debug(logLine);
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in AND Activity", ex);
                ret.ReturnValue = "AND Activity Error";
                ret.StepState = EnumStepState.ErrorStop;
            }
            return ret;
        }

        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }
    }
}
