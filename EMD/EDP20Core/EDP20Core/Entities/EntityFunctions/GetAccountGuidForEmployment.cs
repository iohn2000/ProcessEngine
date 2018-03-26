using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// find account (cost center) guid for given employment
    /// string effectedPersonEmployment, string emplGuid
    /// </summary>
    public class GetAccountGuidForEmployment : EntityFunction<GetAccountGuidForEmployment>
    {
        /// <summary>
        /// implementation of Call
        /// </summary>
        /// <returns></returns>
        protected override GetAccountGuidForEmployment Worker()
        {
            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.GetAccountGuidForEmployment(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
