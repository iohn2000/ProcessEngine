using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;

namespace Kapsch.IS.EDP.WFActivity
{
    public abstract class SimpleBaseEDPActivity : BaseEDPActivity
    {
        public SimpleBaseEDPActivity(Type type) : base(type)
        {
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            return new StepReturn("ok", EnumStepState.Complete);
        }

        public override StepReturn Initialize(EngineContext engineContext)
        {
            return new StepReturn("ok", EnumStepState.Complete);
        }
    }
}
