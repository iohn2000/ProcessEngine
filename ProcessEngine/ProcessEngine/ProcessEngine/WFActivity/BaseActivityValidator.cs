using FluentValidation;

namespace Kapsch.IS.ProcessEngine.WFActivity
{
    public class BaseActivityValidator<T> : AbstractValidator<T> where T :Activity 
    {
        public BaseActivityValidator()
        {
            RuleFor(a => a.Id)
                .NotEmpty();

            RuleFor(a => a.Instance)
                .NotEmpty();

            RuleFor(a => a.Nr)
                .NotEmpty();

            RuleFor(a => a.Name)
                .NotEmpty();

            RuleFor(a => a.GetAllActivityProperties)
                .SetCollectionValidator(new BasePropertyValidator());

            RuleFor(a => a.GetAllTransitions)
                .SetCollectionValidator(new BaseTransitionValidator());
           
        }
    }

    public class BasePropertyValidator : AbstractValidator<ActivityProperty>
    {
        public BasePropertyValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty();
        }
    }

    public class BaseTransitionValidator : AbstractValidator<Transition>
    {
        public BaseTransitionValidator()
        {
            RuleFor(t => t.ToActivityID)
                .NotEmpty();
            
        }
    }
}
