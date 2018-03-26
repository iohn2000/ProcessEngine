using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class AccountHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new Account().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Account acco = (Account)dbObject;
            EMDAccount emdObject = new EMDAccount(acco.Guid, acco.Created, acco.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }


        public EMDEmployment GetResponsible(string Account_Guid)
        {
            EMDAccount account = (EMDAccount)this.GetObject<EMDAccount>(Account_Guid);
            EmploymentHandler eh = new EmploymentHandler();
            EMDEmployment employment = (EMDEmployment)eh.GetObject <EMDEmployment>(account.Responsible);
            if (employment != null)
                return employment;
            else
                return null;
        }

        public string GetResponsibleName(string Account_Guid)
        {
            EMDEmployment employment = this.GetResponsible(Account_Guid);
            if (employment != null)
            {
                PersonHandler ph = new PersonHandler();
                EMDPerson person = (EMDPerson)ph.GetObject<EMDPerson>(employment.P_Guid);
                return person.Display_FirstName + " " + person.Display_FamilyName + "(" + person.UserID + ")";
            }
            else
                return String.Empty;
        }
    }
}
