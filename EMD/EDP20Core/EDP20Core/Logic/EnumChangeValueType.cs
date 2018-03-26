using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public enum EnumChangeValueType
    {
        NewEmpl = 0,
        Enterprise = 1,
        EmploymentType = 2,
        OrgUnit = 3,
        Costcenter = 4,
        Location = 5,
        Pause = 6,
        /// <summary>
        /// means the equipmentprocesses must run
        /// </summary>
        EquipmentProc = 7
    }
}
