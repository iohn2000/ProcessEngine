using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    /// <summary>
    /// SearchContactItemForEmployment 
    /// (string emplGuid, string contactType)
    /// </summary>
    public class SearchContactItemForEmployment : EntityFunction<SearchContactItemForEmployment>
    {
        /// <summary>
        /// implementation of Call for NullFunction
        /// </summary>
        protected override SearchContactItemForEmployment Worker()
        {
            EmploymentHandler emplH = new EmploymentHandler();
            base.Result = emplH.SearchContactItemForEmployment(this.EntityGuid, this.Parameter);

            return this;
        }
    }
}
