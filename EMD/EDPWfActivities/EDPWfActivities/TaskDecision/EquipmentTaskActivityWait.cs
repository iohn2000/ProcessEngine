using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class EquipmentTaskActivityWait : BaseEDPAsyncActivity, IActivityValidator, IProcessStep
    {
        public EquipmentTaskActivityWait() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public override bool isResultAvailable(string AsyncRequestID, out BaseAsyncRequestResult AsyncRequestResult)
        {
            throw new NotImplementedException();
        }

        public override StepReturn PostInitialize(EngineContext engineContext, BaseAsyncRequestResult baseResult)
        {
            throw new NotImplementedException();
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            throw new NotImplementedException();
        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            throw new NotImplementedException();
        }


        #region validate
        public string Validate(XElement activity)
        {
            throw new NotImplementedException();
        }

        public string Validate(string activityXml)
        {
            throw new NotImplementedException();
        }

        public override StepReturn HandleReminder(EngineContext engineContext, BaseAsyncRequestResult baseResult, bool resultAvailable)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }
        #endregion
    }
}
