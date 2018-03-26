using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class UserDomainManager
        : BaseManager
        , IUserDomainManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public UserDomainManager()
            : base()
        {
        }

        public UserDomainManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public UserDomainManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public UserDomainManager(CoreTransaction transaction, string guid_ModfifiedBy, string modifyComment = null)
            : base(transaction, guid_ModfifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDUserDomain Get(string guid)
        {
            UserDomainHandler userDomainHandler = new UserDomainHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return (EMDUserDomain)userDomainHandler.GetObject<EMDUserDomain>(guid);
        }

        public List<EMDUserDomain> GetUserDomains()
        {
            
            UserDomainHandler userDomainHandler = new UserDomainHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);


            List<EMDUserDomain> emdUsers = userDomainHandler.GetObjects<EMDUserDomain, DB.UserDomain>().Cast<EMDUserDomain>().ToList();

            return emdUsers;
        }

        public List<EMDUserDomain> GetUserDomains(string enterpriseGuid)
        {
            throw new NotImplementedException();
        }
    }
}
