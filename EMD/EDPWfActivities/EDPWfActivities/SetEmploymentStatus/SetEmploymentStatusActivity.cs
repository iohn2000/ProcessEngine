using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.SetEmploymentStatus
{
    public class SetEmploymentStatusActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {

        public SetEmploymentStatusActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
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
            logger.Debug(string.Format("{0} : Run() started", getWorkflowLoggingContext(engineContext)));
            // setup return object 
            StepReturn ret = new StepReturn("ok", EnumStepState.Complete);

            // get a value from current activity variable

            try
            {
                Variable emplGuid = base.GetProcessedActivityVariable(engineContext, "emplGuid", true);
                Variable status = base.GetProcessedActivityVariable(engineContext, "status", true);
                if (emplGuid.VarValue != null && !string.IsNullOrWhiteSpace(status.VarValue))
                {
                    EmploymentHandler emplH = new EmploymentHandler();
                    EMDEmployment empl = (EMDEmployment) emplH.GetObject<EMDEmployment>(emplGuid.VarValue);
                    empl.Status = Convert.ToByte(status.VarValue);
                    emplH.UpdateObject(empl, historize: true);
                    engineContext.SetActivityVariable("returnStatus", "ok");
                    ret.ReturnValue = "Set EmploymentStatus for " + emplGuid.VarValue + " to " + status.VarValue;
                }                
            }
            catch (Exception ex)
            {
                engineContext.SetActivityVariable("returnStatus", "error");
                return base.logErrorAndReturnStepState(engineContext,ex,"Error in set employment status Activity",EnumStepState.ErrorToHandle);
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
