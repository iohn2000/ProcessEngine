using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.WFActivity.RemoveOnboarding
{
    public class RemoveOnboardingActivity : BaseEDPActivity, IProcessStep, IActivityValidator
    {

        public RemoveOnboardingActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);
            return result;
        }

        public override StepReturn PostInitialize(EngineContext engineContext)
        {
            StepReturn result = new StepReturn("Complete", EnumStepState.Complete);
            return result;
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            // 0.EffectedPersonEmploymentGuid 

            StepReturn ret = new StepReturn("Complete", EnumStepState.Complete);
            try
            {
                Variable emplGuid = base.GetProcessedActivityVariable(engineContext, "EffectedPersonEmploymentGuid");
                if (emplGuid != null)
                {
                    EmploymentManager emplMgr = new EmploymentManager();
                    OffboardingManager offboardingManager = new OffboardingManager();
                    offboardingManager.RemoveEmployment(emplGuid.VarValue);

                    EmploymentHandler emplyHandler = new EmploymentHandler();
                    EMDEmployment employment = emplMgr.GetEmployment(emplGuid.VarValue);
                    emplyHandler.DeleteObject<EMDEmployment>(employment);
                }
                //
                //fill ReturnStatus variable
                //
                engineContext.SetActivityVariable("returnStatus", "Complete");
            }
            catch (Exception ex)
            {
                logger.Error(base.getWorkflowLoggingContext(engineContext) + " : Error in RemoveOnboarding Activity : \r\n", ex);
                ret.ReturnValue = base.getWorkflowLoggingContext(engineContext) + " Error in RemoveOnboarding Activity";
                ret.StepState = EnumStepState.ErrorToHandle;
                engineContext.SetActivityVariable("returnStatus", "Error");
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
