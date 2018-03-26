using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class ContactHandler : EMDObjectHandler
    {
        public ContactHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public ContactHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public ContactHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public ContactHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new Contact().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Contact cont = (Contact)dbObject;
            EMDContact emdObject = new EMDContact(cont.Guid, cont.Created, cont.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public List<IEMDObject<EMDContact>> GetObjectsForEmployment(String empl_guid, bool futurize = false)
        {
            List<IEMDObject<EMDContact>> contacts = GetActiveObjectsInInterval<EMDContact, Contact>(DateTime.Now, (futurize ? EMDContact.INFINITY : DateTime.Now),
                "EP_Guid=\"" + empl_guid + "\"").Cast<IEMDObject<EMDContact>>().ToList();
            return contacts;
        }

        [Obsolete("User Manager instead")]
        public EMDContact GetContactByContactType(string empl_guid, int contactType_Id, bool isFuture = false)
        {
            List<EMDContact> contacts = null;

            if (isFuture)
            {
                DateTime today = DateTime.Now;
                contacts = GetActiveObjectsInInterval<EMDContact, Contact>(new DateTime(today.Year, today.Month, today.Day).AddDays(1), EMDContact.INFINITY, "EP_Guid = \"" + empl_guid + "\" and C_CT_ID = " + contactType_Id).FindAll(a => a.ActiveFrom > DateTime.Now);
                // contacts = GetActiveObjectsInInterval<EMDContact, Contact>(new DateTime(today.Year, today.Month, today.Day + 1), EMDContact.INFINITY, "EP_Guid = \"" + empl_guid + "\" and C_CT_ID = " + contactType_Id).Cast<EMDContact>().ToList();
            }
            else
            {
                contacts = GetObjects<EMDContact, Contact>("EP_Guid = \"" + empl_guid + "\" and C_CT_ID = " + contactType_Id).Cast<EMDContact>().ToList();
            }

            if (contacts.Count == 1) return (EMDContact)contacts.First();
            else if (contacts.Count > 1)
            {
                IISLogger logger = ISLogger.GetLogger("ContactHandler");
                logger.Warn(String.Format("Data error: more than one contact for EP_Guid {0} + C_CT_ID:{1}", empl_guid, contactType_Id));
                return (EMDContact)contacts[0];
            }
            else
            {
                return null;
            }

        }

        [Obsolete("User Manager instead")]
        public EMDContact GetContactByContactType(string empl_guid, string contactTypeGuid)
        {
            List<IEMDObject<EMDContact>> contacts = GetObjects<EMDContact, Contact>("EP_Guid = \"" + empl_guid + "\" and CT_Guid = \"" + contactTypeGuid + "\"");

            if (contacts.Count == 1)
                return (EMDContact)contacts.First();
            else if (contacts.Count > 1)
            {
                IISLogger logger = ISLogger.GetLogger("ContactHandler");
                logger.Error(String.Format("Data error: more than one contact for EP_Guid {0} + C_CT_ID:{1}", empl_guid, contactTypeGuid));
                return (EMDContact)contacts[0];
            }
            else
                return null;
        }

        [Obsolete("User Manager instead")]
        /// <summary>
        /// @@ function to get contact text by contact type guid
        /// </summary>
        /// <param name="EP_Guid"></param>
        /// <param name="ContactType_Guid"></param>
        /// <param name="isFuture">optional parameter: default false | set param to get a future entry</param>
        /// <returns></returns>
        public string GetContactStringByContactType(string EP_Guid, int ContactType_Guid, bool isFuture = false)
        {
            EMDContact contact = this.GetContactByContactType(EP_Guid, ContactType_Guid, isFuture);
            if (contact != null)
                return contact.Text;
            else
                return "";
        }
    }
}