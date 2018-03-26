using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface ISecurityActionManager
        : IBaseManager
    {
        EMDSecurityAction Get(string guid);

        EMDSecurityAction Create(EMDSecurityAction emdSecurityAction);

        EMDSecurityAction Update(EMDSecurityAction emdSecurityAction);
    }
}
