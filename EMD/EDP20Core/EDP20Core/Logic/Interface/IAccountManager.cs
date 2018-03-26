using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IAccountManager
        : IBaseManager
    {
        EMDAccount Get(string guid);

        EMDAccount Delete(string guid);

        List<EMDAccount> GetAccounts();

        void Update(EMDAccount emdAccount);

        EMDAccount Create(EMDAccount emdAccount);

        bool IsAccountIdAvailable(string accoGuid, string costCenterId, string enteGuid);
    }
}
