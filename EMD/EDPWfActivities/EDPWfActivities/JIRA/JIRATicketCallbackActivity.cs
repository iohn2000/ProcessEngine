using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine;

namespace Kapsch.IS.EDP.WFActivity.JIRA
{
    public class JIRATicketCallbackActivity : BaseEDPCallbackActivity, IProcessStep
    {

        public JIRATicketCallbackActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }
        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        public override StepReturn PreFinish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        public override void UpdateStatusAndResultForAutoCallback(EngineContext engineContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
