using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IGroupManager
    {
        List<EMDGroup> GetGroups(string guidEnterprise);

        List<EMDGroup> GetAssignedCostCenterGroups(string guidCostCenter);

        List<EMDGroup> GetCostCenterGroups();
    }
}
