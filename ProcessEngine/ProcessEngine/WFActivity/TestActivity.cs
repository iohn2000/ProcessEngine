using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.ProcessEngine.WFActivity
{
    public class TestActivity : BaseActivity, IProcessStep
    {
        String ExpectedInitializeState = "";
        String ExpectedRunState = "";
        String ExpectedFinishState = "";

        String InitializeMessage = "";
        String RunMessage = "";
        String FinishMessage = "";

        public TestActivity() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }

        public override StepReturn Finish(EngineContext engineContext)
        {
            try
            {
                StepReturn result = HandleAction(engineContext, "ExpectedFinishState");
                FinishMessage = engineContext.CurrenActivity.Instance + ".Finish() was executed" + engineContext.ExecutionIteration.IterationNumber;
                ExpectedFinishState = result.StepState.ToString();

                engineContext.SetActivityVariable("ExpectedInitializeResult", ExpectedInitializeState, false);
                engineContext.SetActivityVariable("ExpectedRunResult", ExpectedRunState, false);
                engineContext.SetActivityVariable("ExpectedFinishResult", ExpectedFinishState, false);

                engineContext.SetActivityVariable("InitializeMessage", InitializeMessage, false);
                engineContext.SetActivityVariable("RunMessage", RunMessage, false);
                engineContext.SetActivityVariable("FinishMessage", FinishMessage, false);

                return result;
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
        }

        public override StepReturn Initialize(EngineContext engineContext)
        {
            try
            {
                StepReturn result = HandleAction(engineContext, "ExpectedInitializeState");
                InitializeMessage = engineContext.CurrenActivity.Instance + ".Initialize() was executed" + engineContext.ExecutionIteration.IterationNumber;
                ExpectedInitializeState = result.StepState.ToString();
                return result;
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
        }

        public override StepReturn Run(EngineContext engineContext)
        {
            try
            {
                StepReturn result = HandleAction(engineContext, "ExpectedRunState");
                RunMessage = engineContext.CurrenActivity.Instance + ".Run() was executed" + engineContext.ExecutionIteration.IterationNumber;
                ExpectedRunState = result.StepState.ToString();
                return result;
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
        }

        private StepReturn HandleAction(EngineContext engineContext, String Varname)
        {
            StepReturn result = new StepReturn("ok", Shared.Enums.EnumStepState.Complete);
            Variable expectedState;
            try
            {
                expectedState = this.GetProcessedActivityVariable(engineContext, Varname, false);
            }
            catch (Exception ex)
            {
                string errMgs = String.Format("{1} Error calling engineContext.GetProcessedActivityVariable() with Variablename = '{0}'.",
                                    Varname, this.getWorkflowLoggingContext(engineContext));
                logger.Error(errMgs);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMgs, ex);
            }
            if (expectedState != null)
            {
                result.StepState = expectedState.ConvertToEnum<Shared.Enums.EnumStepState>();
            }
            else
            {
                string errMgs = String.Format("{1} Activity did not find expected Variable called '{0}' or an unexpected value",
                    Varname, this.getWorkflowLoggingContext(engineContext));
                logger.Error(errMgs);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, errMgs);
            }
            return result;
        }
    }
}
