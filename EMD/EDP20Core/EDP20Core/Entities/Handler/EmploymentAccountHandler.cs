using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EmploymentAccountHandler : EMDObjectHandler
    {
        public EmploymentAccountHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public EmploymentAccountHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public EmploymentAccountHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public EmploymentAccountHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }
        
        public override Type GetDBObjectType()
        {
            return new EmploymentAccount().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            EmploymentAccount emac = (EmploymentAccount)dbObject;
            EMDEmploymentAccount emdObject = new EMDEmploymentAccount(emac.Guid, emac.Created, emac.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public IEMDObject<EMDEmploymentAccount> GetMainEmploymentAccount(String EP_Guid)
        {
            List<IEMDObject<EMDEmploymentAccount>> employmentAccounts = GetObjects<EMDEmploymentAccount, EmploymentAccount>("EP_Guid = \"" + EP_Guid + "\"");
            List<EMDEmploymentAccount> employmentMainAccounts = new List<EMDEmploymentAccount>();
            ObjectFlagManager obflManager = new ObjectFlagManager();

            foreach (EMDEmploymentAccount emplAccount in employmentAccounts)
            {
                if (obflManager.IsMainAccount(emplAccount.Guid))
                {
                    employmentMainAccounts.Add(emplAccount);
                }
            }

            if (employmentMainAccounts.Count == 0)
                return null;
            else if (employmentMainAccounts.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one Main-Account found for Employment EP_Guid: {0}",EP_Guid));
            }
            else
            {
                return employmentMainAccounts.First();
            }
        }

    }
}
