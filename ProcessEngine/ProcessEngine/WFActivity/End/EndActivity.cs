using Kapsch.IS.ProcessEngine.Shared.Enums;

namespace Kapsch.IS.ProcessEngine.WFActivity.End
{
    public class EndActivity : SimpleBaseActivity
    {
        public EndActivity() : base (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn Run(EngineContext engineContext)
        {

            StepReturn ret = new StepReturn("", EnumStepState.Complete);
            logger.Info(base.getWorkflowLoggingContext(engineContext) + " : End Activity processed");
            return ret;
        }
    }
}
