using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// @@ function get contact via old contact type id
    /// SearchContactItemByCTIDForEmployment(string emplGuid, string contactTypeID)
    /// </summary>

    public class SearchContactItemByCTIDForEmployment : EntityFunction<SearchContactItemByCTIDForEmployment>
    {
        /// <summary>
        /// implementation of Call for NullFunction
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override SearchContactItemByCTIDForEmployment Worker()
        {

            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.SearchContactItemByCTIDForEmployment(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
