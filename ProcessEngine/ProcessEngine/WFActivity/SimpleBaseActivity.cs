using System;

namespace Kapsch.IS.ProcessEngine.WFActivity
{
    /// <summary>
    /// implemented for compatibility with activities out of standard errorhandling directly implementing execute.
    /// </summary>
    public abstract class SimpleBaseActivity : BaseActivity
    {
        public SimpleBaseActivity(Type type) : base(type)
        {
        }

        /// <summary>
        /// do not call!
        /// </summary>

        public override StepReturn Finish(EngineContext engineContext)
        {
            return new StepReturn("ok", Shared.Enums.EnumStepState.Complete);
        }

        public override StepReturn Initialize(EngineContext engineContext)
        {
            return new StepReturn("ok", Shared.Enums.EnumStepState.Complete);
        }        
        
    }
}