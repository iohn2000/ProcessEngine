using FluentValidation;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System.Collections.Generic;

namespace Kapsch.IS.ProcessEngine.WFActivity.AuditLog
{
    public class AuditLogValidator : BaseActivityValidator<Activity>
    {
        public AuditLogValidator()
        {
            RuleFor(a => a.GetAllActivityProperties)
                .Must(delegate (List<ActivityProperty> props)
                {
                    return props.Exists(p => p.Name.Equals("logText"));
                })
                .WithMessage("This activity must contain a property called 'logText'.");

            RuleFor(a => a.GetAllTransitions)
                .Must(delegate (List<Transition> trans)
                {
                    if (trans != null)
                    {

                        bool doesCondExist = trans.Exists(delegate (Transition t)
                        {
                            return ! string.IsNullOrWhiteSpace(t.Condition);
                        });

                        return !doesCondExist;
                    }
                    else
                    {
                        return true;
                    }
                })
                .WithMessage("Only outgoing transistions with no condition are allowed.");

            RuleFor(a => a.GetAllActivityProperties)
                .SetCollectionValidator(new AuditPropertyValidator());
        }
    }

    public class AuditPropertyValidator : AbstractValidator<ActivityProperty>
    {
        public AuditPropertyValidator()
        {
            RuleFor(p => p.Direction)
                .Equal(EnumVariableDirection.input).When(p => p.Name.Equals("logText"))
                .WithMessage("Property 'logText' has wrong direction, must be 'input'");

            RuleFor(p => p.DataType)
                .Equal(EnumVariablesDataType.stringType).When(p => p.Name.Equals("logText"))
                .WithMessage("Property 'logText' be stringType.");

        }
    }
}
