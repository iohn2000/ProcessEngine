using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.SetEnterpriseLocationStatus
{
    public class SetEnterpriseLocationStatusActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {

        public SetEnterpriseLocationStatusActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("ok", EnumStepState.Complete);
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            // setup return object 
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);

            // get a value from current activity variable

            try
            {
                Variable emplGuid = base.GetProcessedActivityVariable(engineContext, "enloGuid");
                Variable status = base.GetProcessedActivityVariable(engineContext, "status");
                if (emplGuid.VarValue != null && !string.IsNullOrWhiteSpace(status.VarValue))
                {
                    EnterpriseLocationHandler enloH = new EnterpriseLocationHandler();
                    EMDEnterpriseLocation enlo = (EMDEnterpriseLocation) enloH.GetObject<EMDEnterpriseLocation>(emplGuid.VarValue);
                    enlo.Status = Convert.ToByte(status.VarValue);
                    enloH.UpdateObject(enlo, historize: true);
                    engineContext.SetActivityVariable("returnStatus", "ok");
                }
            }
            catch (Exception ex)
            {
                engineContext.SetActivityVariable("returnStatus", "error");
                return this.logErrorAndReturnStepState(engineContext,ex,"Error in set enlo status Activity",EnumStepState.ErrorToHandle);
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
