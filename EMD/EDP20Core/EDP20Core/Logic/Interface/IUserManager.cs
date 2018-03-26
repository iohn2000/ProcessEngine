using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface IUserManager
        : IBaseManager
    {
        EMDUser Get(string guid);

        EMDUser Delete(string guid);

        List<EMDUser> GetEmploymentUsers(string employmentGuid);

        List<EMDUser> GetHistoricalEmploymentUsers(string employmentGuid);

        void Update(EMDUser emdUser);

        void Update(EMDUser emdUser, bool historize, bool checkActiveTo = true);

        EMDUser Create(EMDUser emdUser);

        EMDUser Create(EMDUser emdUser, bool datesAreSet);

        EMDUser Create(EMDUser emdUser, bool datesAreSet, CoreTransaction transaction);

        List<Tuple<string, int>> CreateUserIDProposalForPerson(string familyName, string firstName);

        List<Tuple<string, int>> CreateUserIDProposalForPerson(string familyName, string firstName, string prefix);

        bool UserExists(EMDUser emdUser);
    }
}
