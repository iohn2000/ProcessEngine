using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// uses iterating linemanagersearch to find an empl guid
    /// SearchResponsibleRoleForEmployment(string emplGuid, string roleID)
    /// </summary>
    public class SearchResponsibleRoleForEmployment : EntityFunction<SearchResponsibleRoleForEmployment>
    {
        /// <summary>
        /// implementation of Call for NullFunction
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override SearchResponsibleRoleForEmployment Worker()
        {

            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.SearchResponsibleRoleForEmployment(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
