using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    public class IsEnterpriseUnderParentAt : EntityFunction<IsEnterpriseUnderParentAt>
    {
        /// <summary>
        /// IsEnterpriseUnderParentAt(string enteParentGuid, string emplChildGuid)
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override IsEnterpriseUnderParentAt Worker()
        {

            EnterpriseHandler enteH = new EnterpriseHandler();
            base.Result = enteH.IsEnterpriseUnderParentAt(this.EntityGuid, this.Parameter);
            return this;
        }
    }
}
