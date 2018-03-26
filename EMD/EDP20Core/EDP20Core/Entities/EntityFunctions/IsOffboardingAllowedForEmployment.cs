using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// checks if employment can be offboarded.
    /// IsOffboardingAllowedForEmployment(string emplGuid, string parameterNotNeeded)
    /// </summary>
    public class IsOffboardingAllowedForEmployment : EntityFunction<IsOffboardingAllowedForEmployment>
    {
        /// <summary>
        /// implementation of Call for NullFunction
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override IsOffboardingAllowedForEmployment Worker()
        {
            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.IsOffboardingAllowedForEmployment(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
