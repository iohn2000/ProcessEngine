using Kapsch.IS.ProcessEngine.Shared.Enums;

namespace Kapsch.IS.ProcessEngine.WFActivity.Start
{
    public class StartActivity : SimpleBaseActivity
    {
        public StartActivity() : base (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn Run(EngineContext engineContext)
        {

            StepReturn ret = new StepReturn("", EnumStepState.Complete);
            logger.Info(base.getWorkflowLoggingContext(engineContext) + " : Start Activity processed");
            return ret;
        }
    }
}
