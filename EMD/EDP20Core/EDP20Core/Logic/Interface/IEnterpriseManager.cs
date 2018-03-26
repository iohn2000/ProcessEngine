using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.Entities;


namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IEnterpriseManager
        : IBaseManager
    {
        EMDEnterprise Get(string guid);

        List<EMDEnterprise> GetList();

        List<EMDEnterprise> GetList(string whereClause);

        List<EMDEnterprise> GetAllowedOnboardingList();

        EMDEnterprise Delete(string guid);

        EMDEnterprise GetEnterpriseByOldE_ID(int e_id);

        //void Update(EMDEnterprise emdUser);

        //EMDUser Create(EMDEnterprise emdUser);

        //EMDUser Create(EMDEnterprise emdUser, bool datesAreSet);
    }
}
