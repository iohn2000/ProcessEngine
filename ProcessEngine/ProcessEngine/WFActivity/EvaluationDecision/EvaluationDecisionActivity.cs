using FluentValidation.Results;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Xml.Linq;

namespace Kapsch.IS.ProcessEngine.WFActivity.EvaluationDecision
{
    /*
    <properties>
        <property name="condition" input />
        <property name="result" output />
    </properties>          
    */
    public class EvaluationDecisionActivity : SimpleBaseActivity, IProcessStep, IActivityValidator
    {

        public EvaluationDecisionActivity() :  base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

        }
        public override StepReturn Run(EngineContext engineContext)
        {
            StepReturn ret = new StepReturn("", EnumStepState.Complete);

            if (base.isDemoModeOn(engineContext))
            {
                Variable varDemoResult = this.GetProcessedActivityVariable(engineContext, "demoResult", false);
                if (varDemoResult != null) 
                {
                    engineContext.SetActivityVariable("result",varDemoResult.VarValue);
                    return ret;
                }
            }

            try
            {
                string conditionEvaluated = null;
                Variable v = this.GetProcessedActivityVariable(engineContext, "condition", false);
                if (v != null)
                    conditionEvaluated = v.GetStringValue();

                if (conditionEvaluated != null)
                {
                    engineContext.SetActivityVariable("result", conditionEvaluated); // write result back into variables list
                }
            }
            catch (Exception ex)
            {
                ret.ReturnValue = "";
                ret.StepState = EnumStepState.ErrorToHandle;
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "Error in DecisionActivity.Execute.", ex);
            }

            return ret;
        }

        public string Validate(XElement activity)
        {
            Activity a = new Activity(activity);


            EvaluationDecisionActivityValidator validator = new EvaluationDecisionActivityValidator();
            ValidationResult results = validator.Validate(a);

            if (!results.IsValid)
            {
                foreach (var failure in results.Errors)
                {
                    //Console.WriteLine("Property " + failure.PropertyName + " failed validation. Error was: " + failure.ErrorMessage);
                }
            }
            else
            {
                //Console.WriteLine("alles super");
            }
            return "";
        }

        public string Validate(string activityXml)
        {
            return this.Validate(XElement.Parse(activityXml));
        }
    }
}
