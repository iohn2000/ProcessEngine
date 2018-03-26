using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.Enhanced
{
    public class EMDOrgUnitRoleEnhanced : EMDOrgUnitRole
    {
        /// <summary>
        /// matching Orgunit from O_Guid
        /// </summary>
        public string OrgUnitName { get; set; }

        /// <summary>
        /// matching from R_Guid
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// matching Employment EP_Guid
        /// </summary>
        public string EmploymentPersonalId { get; set; }

        /// <summary>
        /// matching Person P_Guid
        /// </summary>
        public string PersonName { get; set; }

        public string EmploymentTypeName { get; set; }
    }
}
