using FluentValidation;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.WFActivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity.Email
{
    public class EmailActivityValidator : BaseActivityValidator<Activity>
    {
        public EmailActivityValidator()
        {
            RuleFor(a => a.GetAllActivityProperties) 
                .Must(delegate (List<ActivityProperty> props)
                {
                    return props.Exists(p => p.Name.Equals("XXX-Example"));
                })
                .WithMessage("This activity must contain a property called 'XXX-Example'.");
        }
    }
}
