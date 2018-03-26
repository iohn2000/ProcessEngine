using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.Entities;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IPersonManager
        : IBaseManager
    {
        EMDPerson Get(string guid);

        EMDPerson GetPersonByEmployment(string employmentGuid);

        string getFullDisplayNameWithUserId(EMDPerson pers);

        string getFullDisplayNameWithUserIdAndPersNr(EMDEmployment empl);

        string getFullDisplayNameWithUserIdAndPersNr(string empl_guid);

        EMDPerson Delete(string guid);

        EMDPerson GetPersonByUserId(String UserId);

        bool IsItSelf(string userId, string pers_guid);

    }
}
