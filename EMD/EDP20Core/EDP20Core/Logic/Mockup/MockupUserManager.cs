using Kapsch.IS.EDP.Core.Logic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Logic.Mockup
{
    public class MockupUserManager
        : BaseManager
        , IUserManager
    {
        #region Constructors

        public MockupUserManager()
            : base()
        {
        }

        public MockupUserManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public MockupUserManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public MockupUserManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public List<EMDUser> GetEmploymentUsers(string employmentGuid)
        {
            List<EMDUser> emdUsers = new List<EMDUser>();

            for (int i = 0; i < 10; i++)
            {
                emdUsers.Add(new EMDUser()
                {
                    Guid = string.Format("U_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty)),
                    ActiveFrom = DateTime.Now,
                    ActiveTo = Entities.EMDObject<EMDUser>.INFINITY,
                    Username = string.Format("testuser{0}", i),
                    UserType = (byte)EnumUserType.ADUserLimitedAccount,
                });
            }

            return emdUsers;
        }

        public EMDUser Create(EMDUser emdUser)
        {
            throw new NotImplementedException();
        }

        public void Update(EMDUser emdUser)
        {
            throw new NotImplementedException();
        }

        public void Update(EMDUser emdUser, bool historize, bool checkActiveTo = true)
        {
            throw new NotImplementedException();
        }

        public EMDUser Get(string guid)
        {
            throw new NotImplementedException();
        }

        public List<Tuple<string, int>> CreateUserIDProposalForPerson(string familyName, string firstName)
        {
            throw new NotImplementedException();
        }

        List<Tuple<string, int>> IUserManager.CreateUserIDProposalForPerson(string familyName, string firstName, string prefix)
        {
            throw new NotImplementedException();
        }

        public EMDUser Create(EMDUser emdUser, bool datesAreSet)
        {
            throw new NotImplementedException();
        }

        public EMDUser Create(EMDUser emdUser, bool datesAreSet, CoreTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public EMDUser Delete(string guid)
        {
            throw new NotImplementedException();
        }

        public bool UserExists(EMDUser emdUser)
        {
            throw new NotImplementedException();
        }

        public List<EMDUser> GetHistoricalEmploymentUsers(string employmentGuid)
        {
            throw new NotImplementedException();
        }
    }

}
