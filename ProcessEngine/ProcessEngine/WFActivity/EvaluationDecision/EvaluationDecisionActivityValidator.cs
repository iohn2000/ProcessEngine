using FluentValidation;
using Kapsch.IS.ProcessEngine.Shared.Enums;

namespace Kapsch.IS.ProcessEngine.WFActivity.EvaluationDecision
{
    public class EvaluationDecisionActivityValidator : BaseActivityValidator<Activity>
    {
        public EvaluationDecisionActivityValidator()
        {

            // condition and result is not always necessary, e.g. if conditions are in the transistions only with variables from somewhere else
            /*
            RuleFor(a => a.GetAllActivityProperties)
                            .Must(delegate (List<ActivityProperty> props)
                            {
                                
                                bool conditionExists = props.Exists(p => p.Name.Equals("condition")); 
                                bool resultExists = props.Exists(p => p.Name.Equals("result"));
                                return conditionExists & resultExists;
                            })
                            .WithMessage("This activity must contain 2 properties called 'condition' and  'result");
            */

            // keep it max. flexible 
            /*
            RuleFor(a => a.GetAllTransitions)
                .Must(delegate (List<Transition> trans)
                {
                    if (trans != null)
                    {
                        bool onlyTwo = trans.Count == 2;
                        bool condTrue = trans.Exists(t => t.Condition.Equals("true"));
                        bool condFalse = trans.Exists(t => t.Condition.Equals("false"));
                        return onlyTwo & condTrue & condFalse;
                    }
                    else
                        return false;

                })
                .WithMessage("Only excately 2 outgoing transistions allowed with condition 'true' and 'false'.");
            */

            RuleFor(a => a.GetAllActivityProperties)
                .SetCollectionValidator(new DecisionPropertyValidator());
        }
    }

    public class DecisionPropertyValidator : AbstractValidator<ActivityProperty>
    {
        public DecisionPropertyValidator()
        {
            RuleFor(p => p.Direction)
                .Equal(EnumVariableDirection.output).When(p => p.Name.Equals("result"))
                .WithMessage("Property result has wrong direction");

            RuleFor(p => p.Direction)
                .Equal(EnumVariableDirection.input).When(p => p.Name.Equals("condition"))
                .WithMessage("Property condition has wrong direction");

            RuleFor(p => p.DataType)
                .Equal(EnumVariablesDataType.stringType).When(p => p.Name.Equals("result") | p.Name.Equals("condition"))
                .WithMessage("Property result and condition must be stringType.");

        }
    }
}
