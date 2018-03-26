using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    public class GetTaskApproversForEffectedEmployment : EntityFunction<GetTaskApproversForEffectedEmployment>
    {
        /// <summary>
        /// @@func to get comma separated list of EmploymentIDs for approvers
        /// string effectedPersonEmployment, string approverCode        
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override GetTaskApproversForEffectedEmployment Worker()
        {

            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.GetTaskApproversForEffectedEmployment(this.EntityGuid, this.Parameter);
            return this;
        }
    }
}
