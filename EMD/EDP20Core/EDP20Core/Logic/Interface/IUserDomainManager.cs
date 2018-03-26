using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IUserDomainManager
        : IBaseManager
    {
        EMDUserDomain Get(string guid);

        List<EMDUserDomain> GetUserDomains();

        List<EMDUserDomain> GetUserDomains(string enterpriseGuid);
    }
}
