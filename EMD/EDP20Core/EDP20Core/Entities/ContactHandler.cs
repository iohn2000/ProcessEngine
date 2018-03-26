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
    public class ContactHandler : EMDObjectHandler
    {

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

        public List<IEMDObject<EMDContact>> GetObjectsForEmployment(String empl_guid)
        {
            List<IEMDObject<EMDContact>> contacts = GetObjects<EMDContact, Contact>("EP_Guid = \"" + empl_guid + "\"");
            return contacts;
        }

        public EMDContact GetContactByContactType(string empl_guid, int contactType_Id)
        {
            List<IEMDObject<EMDContact>> contacts = GetObjects<EMDContact, Contact>("EP_Guid = \"" + empl_guid + "\" and C_CT_ID = " + contactType_Id);

            if (contacts.Count == 1) return (EMDContact)contacts.First();
            else if (contacts.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, String.Format("Datenfehler: mehr als ein Contact für EP_Guid {0} + C_CT_ID:{1}", empl_guid, contactType_Id));
                //TODO Datenfehler exception werfen
            } else
            {
                return null;
            }

        }

        //public EMDContact GetContactByContactType(string EP_Guid, string ContactType_Guid)
        //{
        //    DB.EMD_DataBase dbContext = new DB.EMD_DataBase();

        //    DB.Contact dbContact = (from item in dbContext.Contact.Where(this.ValidClause) where item.EP_Guid == EP_Guid && item.CT_Guid == ContactType_Guid select item).FirstOrDefault();

        //    return ReadDBContact(dbContact);
        //}

        public string GetContactStringByContactType(string EP_Guid, int ContactType_Guid)
        {
            EMDContact contact = GetContactByContactType(EP_Guid, ContactType_Guid);
            if (contact != null)
                return contact.Text;
            else
                return "";
        }

        //public string GetContactStringByContactType(string EP_Guid, int ContactType_Id)
        //{
        //    EMDContact contact = GetContactByContactType(EP_Guid, ContactType_Id);
        //    if (contact != null)
        //        return contact.Text;
        //    else
        //        return "";
        //}

        //private List<IEMDObject> ReadDBContacts(List<DB.Contact> dbContacts)
        //{
        //    List<IEMDObject> ListContacts = new List<IEMDObject>();
        //    foreach (DB.Contact item in dbContacts)
        //    {
        //        EMDContact contact = this.ReadDBContact(item);
        //        ListContacts.Add(contact);
        //    }
        //    return ListContacts;
        //}

        //private EMDContact ReadDBContact(DB.Contact dbContact)
        //{
        //    if (dbContact == null)
        //        return null;

        //    EMDContact contact = new EMDContact(dbContact.Guid, dbContact.Created, dbContact.Modified);

        //    contact.ACDDisplay = dbContact.ACDDisplay;
        //    contact.CT_Guid = dbContact.CT_Guid;
        //    contact.C_CT_ID = dbContact.C_CT_ID;
        //    contact.C_EP_ID = dbContact.C_EP_ID;
        //    contact.C_E_ID = dbContact.C_E_ID;
        //    contact.C_ID = dbContact.C_ID;
        //    contact.C_L_ID = dbContact.C_L_ID;
        //    contact.C_P_ID = dbContact.C_P_ID;
        //    contact.Details = dbContact.Details;
        //    contact.EP_Guid = dbContact.EP_Guid;
        //    contact.E_Guid = dbContact.E_Guid;
        //    contact.Guid = dbContact.Guid;
        //    contact.L_Guid = dbContact.L_Guid;
        //    contact.Note = dbContact.Note;
        //    contact.Priority = dbContact.Priority;
        //    contact.P_Guid = dbContact.P_Guid;
        //    contact.Text = dbContact.Text;
        //    contact.VisibleKatce = dbContact.VisibleKatce;
        //    contact.VisiblePhone = dbContact.VisiblePhone;
        //    contact.ValidFrom = dbContact.ValidFrom;
        //    contact.ValidTo = dbContact.ValidTo;
        //    contact.SetStatus();
        //    return contact;
        //}

        //public override void UpdateDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDContact contact = (EMDContact)Object;
        //    Contact dbObject = (from item in transaction.dbContext.Contact where item.Guid == contact.Guid select item).FirstOrDefault();
        //    if (dbObject != null)
        //    {
        //        MapDataToDBObject(ref dbObject, ref contact);

        //        //finally write to db
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}

        //private void MapDataToDBObject(ref Contact dbObject, ref EMDContact contact)
        //{
        //    dbObject.Guid = contact.Guid; 

        //    dbObject.ValidFrom = contact.ValidFrom; 
        //    dbObject.ValidTo = contact.ValidTo;
        //    dbObject.Modified = contact.Modified;
        //    dbObject.Created = contact.Created;

        //    dbObject.ACDDisplay = contact.ACDDisplay;
        //    dbObject.CT_Guid = contact.CT_Guid;
        //    dbObject.C_CT_ID = contact.C_CT_ID;
        //    dbObject.C_EP_ID = contact.C_EP_ID;
        //    dbObject.C_E_ID = contact.C_E_ID;
        //    dbObject.C_ID = contact.C_ID;
        //    dbObject.C_L_ID = contact.C_L_ID;
        //    dbObject.C_P_ID = contact.C_P_ID;
        //    dbObject.Details = contact.Details;
        //    dbObject.EP_Guid = contact.EP_Guid;
        //    dbObject.E_Guid = contact.E_Guid;
        //    dbObject.Guid = contact.Guid;
        //    dbObject.L_Guid = contact.L_Guid;
        //    dbObject.Note = contact.Note;
        //    dbObject.Priority = contact.Priority;
        //    dbObject.P_Guid = contact.P_Guid;
        //    dbObject.Text = contact.Text;
        //    dbObject.VisibleKatce = contact.VisibleKatce;
        //    dbObject.VisiblePhone = contact.VisiblePhone;
        //}

        //public override void InsertDBObject(IEMDObject Object, Framework.CoreTransaction transaction)
        //{
        //    EMDContact contact = (EMDContact)Object;
        //    Contact dbObject = (from item in transaction.dbContext.Contact where item.Guid == contact.Guid select item).FirstOrDefault();
        //    if (dbObject == null)
        //    {
        //        dbObject = new Contact();

        //        MapDataToDBObject(ref dbObject, ref contact);

        //        //finally write to db
        //        transaction.dbContext.Contact.Add(dbObject);
        //        transaction.saveChanges();
        //    }
        //    else
        //    {
        //        //throw new CoreException createAlreadyExistingObject
        //    }
        //}


        //public EMDContact CreateDefault(string Text, string Note, string CT_Guid, string EMPL_Guid = null, string E_Guid = null, string L_Guid = null)
        //{
        //    EMDContact con = new EMDContact();
        //    con.ACDDisplay = false;
        //    con.CT_Guid = CT_Guid;
        //    con.C_CT_ID = 0;
        //    con.C_EP_ID = 0;
        //    con.C_E_ID = 0;
        //    con.C_ID = 0;
        //    con.C_L_ID = 0;
        //    con.C_P_ID = 0;
        //    //con.Details = 
        //    con.EP_Guid = EMPL_Guid;
        //    con.E_Guid = E_Guid;
        //    con.L_Guid = L_Guid;
        //    con.Note = Note;
        //    con.Priority = 1;
        //    con.P_Guid = null;
        //    con.Text = Text;
        //    con.VisibleKatce = true;
        //    con.VisiblePhone = true;
        //    return con;
        //}
    }
}