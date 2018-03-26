using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// find orgunit guid for given employment and roleID
    /// e.g. what is wolfgang stagl's orgunit he is in role person for the employment x
    /// GetOrgunitGuidforEmployment(string effectedPersonEmployment, string roleID)
    /// </summary>
    public class GetOrgunitGuidforEmployment : EntityFunction<GetOrgunitGuidforEmployment>
    {
        /// <summary>
        /// implementation of Call
        /// </summary>
        /// <returns></returns>
        protected override GetOrgunitGuidforEmployment Worker()
        {

            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.GetOrgunitGuidforEmployment(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
