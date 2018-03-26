using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IRoleManager
        : IBaseManager
    {
        EMDRole Get(string guid);
        EMDRole GetRoleById(int Role_Id);
        EMDRole Delete(string guid);
        
    }
}
