using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ContactTypeHandler : EMDObjectHandler
    {
        public override Type GetDBObjectType()
        {
            return new Contact().GetType();
        }
        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Contact coty = (Contact)dbObject;
            EMDContact emdObject = new EMDContact(coty.Guid, coty.Created, coty.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public const int EMAIL = 1;
        public const int PHONE = 2;
        public const int FAX = 3;
        public const int MOBILE = 4;
        public const int EFAX = 5;
        public const int DIRECTDIAL = 6;
        public const int ROOM = 7;
        public const int AGENTKATCE = 8;
        public const int MONITORKATCE = 9;
        public const int ISMDKATCE = 10;
        public const int SERVERKATCE = 11;
        public const int JOBTITLE = 12;
        public const int HIERARCHY = 13;
        public const int DIRECTDIAL2 = 14;
        public const int DIRECTDIALKATCE = 15;
        public const int LOCATIONPHONE = 16;
        public const int LOCATIONFAX = 17;
        public const int LOCATIONEMAIL = 18;
        public const int DIRECTEFAX = 19;


        //public List<EMDContactType> GetContactTypeList(Boolean Valid = true)
        //{
        //    EMD_DataBase dbContext = new EMD_DataBase();
        //    List<ContactType> dbContactTypes;
        //    if (Valid)
        //        dbContactTypes = (from item in dbContext.ContactType.Where(this.ValidClause) select item).ToList();
        //    else
        //        dbContactTypes = (from item in dbContext.ContactType select item).ToList();

        //    return ReadDBContactTypes(dbContactTypes);
        //}

        //private List<EMDContactType> ReadDBContactTypes(List<ContactType> dbContactTypes)
        //{
        //    List<EMDContactType> ListPerson = new List<EMDContactType>();
        //    foreach (ContactType dbContactType in dbContactTypes)
        //    {
        //        EMDContactType person = this.ReadDBContactType(dbContactType);
        //        ListPerson.Add(person);
        //    }
        //    return ListPerson;
        //}

        //private EMDContactType ReadDBContactType(ContactType dbContactType)
        //{
        //    EMDContactType contactType = new EMDContactType(dbContactType.Guid,dbContactType.Created,dbContactType.Modified);

        //    contactType.Guid = dbContactType.Guid;
        //    contactType.CT_ID = dbContactType.CT_ID;
        //    contactType.Description = dbContactType.Description;
        //    contactType.Edit = dbContactType.Edit;
        //    contactType.Name = dbContactType.Name;
        //    contactType.ValidFrom = dbContactType.ValidFrom;
        //    contactType.ValidTo = dbContactType.ValidTo;
        //    contactType.SetStatus();

        //    return contactType;
        //}

    }
}
