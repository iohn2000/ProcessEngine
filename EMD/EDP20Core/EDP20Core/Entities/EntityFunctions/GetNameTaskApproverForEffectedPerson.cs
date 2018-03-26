using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// @@func to get full name for approver
    /// string effectedPersonEmployment, string approverCode
    /// </summary>
    public class GetNameTaskApproverForEffectedPerson : EntityFunction<GetNameTaskApproverForEffectedPerson>
    {
        /// <summary>
        /// get full name for approver
        /// 
        /// </summary>
        /// <returns></returns>
        protected override GetNameTaskApproverForEffectedPerson Worker()
        {
            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.GetNameTaskApproverForEffectedPerson(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
