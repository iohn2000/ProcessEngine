using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.Serialiser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Kapsch.IS.EDP.Core.Logic
{

    /// <summary>
    /// Provides methods for handling <see cref="EMDContact"/>.
    /// </summary>
    public class ContactManager
        : BaseManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public ContactManager()
            : base()
        {
        }

        public ContactManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public ContactManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public ContactManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public XElement SerializeToXml(EMDContact cont)
        {
            XElement contact = new XElement("EMDContact");

            try
            {
                String xStr = XmlSerialiserHelper.SerialiseIntoXmlString(cont);
                contact = XElement.Parse(xStr);
            }
            catch (Exception exc)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Could not serialize given Contact", exc);
            }

            return contact;
        }

        public EMDContact DeSerializeFromXml(XElement x)
        {
            try
            {
                String xStr = x.ToString();
                EMDContact cont = XmlSerialiserHelper.DeserialiseFromXml<EMDContact>(xStr);
                return cont;
            }
            catch (Exception exc)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Could not deserialize given Contact", exc);
            }
        }

        private ContactHandler handler = new ContactHandler();

        public EMDContact Get(string guid)
        {
            using (ContactHandler handler = new ContactHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment))
            {
                return (EMDContact)handler.GetObject<EMDContact>(guid);
            }
        }

        /// <summary>
        /// Reads contacts for an employment from an XML-String and writes them to the database.
        /// </summary>
        /// <param name="emplGuid">GUID of the employment of the contacts.</param>
        /// <param name="xmlContacts">XML-String holding the Contacts for the given employment.</param>
        /// <returns>List of new EMDContacts, null for every item that failed, for detailed information see logfile</returns>
        /// <exception cref="BaseException">Is thrown with the error-code <see cref="ErrorCodeHandler.E_EDP_BUSINESS_LOGIK"/> if either the XML could not be parsed, or one of the contacts could not be 
        /// written to the database. When this occurs a Rollback is performed an none of the contacts is written to the database.</exception>
        public List<EMDContact> AddContactsToEmploymentFromXmlString(string emplGuid, string xmlContacts)
        {
            //  TransactionHandler ta = Framework.TransactionHandler.Instance;
            CoreTransaction cta = new CoreTransaction();
            List<EMDContact> newContacts = new List<EMDContact>();

            XDocument xmlC;
            IEnumerable<XElement> contacts;

            try
            {
                xmlC = XDocument.Parse(xmlContacts);
                contacts = xmlC.XPathSelectElements("/Contacts/EMDContact");
            }
            catch (Exception ex)
            {
                string msg = "error created contact info for employment :" + emplGuid + " contact xml was :" + Environment.NewLine + xmlContacts;
                this.logger.Error(msg, ex);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }

            if (contacts != null && contacts.Count() > 0)
            {
                cta.Begin();
                try
                {

                    ContactHandler contaH = new ContactHandler(cta, this.Guid_ModifiedBy, this.ModifyComment);


                    foreach (var contact in contacts)
                    {
                        EMDContact c = null;
                        c = this.DeSerializeFromXml(contact);
                        c.EP_Guid = emplGuid;
                        c = (EMDContact)contaH.CreateObject(c);
                        newContacts.Add(c);
                    }
                    //finish this transaction
                    cta.Commit();
                }

                catch (Exception ex)
                {
                    cta.Rollback();
                    string msg = "error created contact info for employment :" + emplGuid + " contact xml was :" + Environment.NewLine + xmlContacts;
                    this.logger.Error(msg, ex);
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
                }
            }
            return newContacts;
        }

        public EMDContact CreateContactRoom(EMDContact Room)
        {
            ContactTypeHandler contTypeHandler = new ContactTypeHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            Room.C_CT_ID = ContactTypeHandler.ROOM;
            Room.CT_Guid = contTypeHandler.GetGuidFromId(Room.C_CT_ID);
            Room.ACDDisplay = false;
            Room.VisibleKatce = true;
            Room.VisiblePhone = true;
            return Room;
        }

        public EMDContact CreateContactDirectDial(EMDContact Directdial)
        {
            ContactTypeHandler contTypeHandler = new ContactTypeHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            Directdial.C_CT_ID = ContactTypeHandler.DIRECTDIAL;
            Directdial.CT_Guid = contTypeHandler.GetGuidFromId(Directdial.C_CT_ID);
            Directdial.ACDDisplay = false;
            Directdial.VisibleKatce = true;
            Directdial.VisiblePhone = true;
            return Directdial;
        }

        public EMDContact CreateContactPhone(EMDContact Phone)
        {
            ContactTypeHandler contTypeHandler = new ContactTypeHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            Phone.C_CT_ID = ContactTypeHandler.PHONE;
            Phone.CT_Guid = contTypeHandler.GetGuidFromId(Phone.C_CT_ID);
            Phone.ACDDisplay = false;
            Phone.VisibleKatce = true;
            Phone.VisiblePhone = true;
            return Phone;
        }

        public EMDContact CreateContactMobile(EMDContact Mobile)
        {
            ContactTypeHandler contTypeHandler = new ContactTypeHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            Mobile.C_CT_ID = ContactTypeHandler.MOBILE;
            Mobile.CT_Guid = contTypeHandler.GetGuidFromId(Mobile.C_CT_ID);
            Mobile.ACDDisplay = false;
            Mobile.VisibleKatce = true;
            Mobile.VisiblePhone = true;
            return Mobile;
        }

        public EMDContact CreateContactJobtitle(EMDContact Jobtitle)
        {
            ContactTypeHandler contTypeHandler = new ContactTypeHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            Jobtitle.C_CT_ID = ContactTypeHandler.JOBTITLE;
            Jobtitle.CT_Guid = contTypeHandler.GetGuidFromId(Jobtitle.C_CT_ID);
            Jobtitle.ACDDisplay = false;
            Jobtitle.VisibleKatce = true;
            Jobtitle.VisiblePhone = true;
            return Jobtitle;
        }

        /// <summary>
        /// Writes the given EMDContact to the database. If there are already Contacts of the specific ContactType one of the following cases occurs:
        /// <ul>
        /// <li><b>more than or equal to two:</b> An <see cref="BaseException"/> is thrown.</li>
        /// <li><b>exactly one:</b> The <see cref="EMDObject{T}.ActiveTo"/> of the earlier (whether it is the currently written or the existing contact) is set to the later contacts <see cref="EMDObject{T}.ActiveFrom"/> and the new contact is written to the database.
        /// In case the existing contact is the earlier one ist is updated in the database.</li>
        /// <li><b>no other contact:</b>The given contact is written to the database.</li>
        /// </ul>
        /// </summary>
        /// <param name="cont">Contact to write. Mustn't be null and the <see cref="EMDContact.EP_Guid"/> has to be set.</param>
        /// <returns>The newly written contact, now containing a GUID and some other auto-filled properties.</returns>
        /// <exception cref="BaseException">Is thrown with error-code <see cref="ErrorCodeHandler.E_EDP_BUSINESS_LOGIK"/> if there are to many contacts of the specific ContactType for this employment.</exception>
        /// <seealso cref="EMDObjectHandler.CreateObject{T}(IEMDObject{T}, string, bool)"/>
        public EMDContact WriteOrModifyContact(EMDContact cont)
        {
            using (EmploymentHandler emplHandler = new EmploymentHandler(base.Transaction, base.Guid_ModifiedBy, base.ModifyComment))
            using (ContactHandler contHandler = new ContactHandler(base.Transaction, base.Guid_ModifiedBy, base.ModifyComment))

            {
                cont.FillEmptyDates();

                EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(cont.EP_Guid);
                IEnumerable<EMDContact> emplContacts = contHandler.GetObjectsForEmployment(empl.Guid, futurize: true).Cast<EMDContact>(); //Handler ändern

                List<EMDContact> otherContacts = emplContacts.Where(eCont => eCont.CT_Guid == cont.CT_Guid).ToList();
                EMDContact current = null;

                if (otherContacts.Count() >= 2)
                {
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Found to much contacts for this contact-type and employment."); //+guids
                }
                else if (otherContacts.Count() == 1)
                {
                    EMDContact other = otherContacts.First();
                    if (other.ActiveFrom > cont.ActiveFrom)
                    {
                        cont.ActiveTo = other.ActiveFrom;
                    }
                    else
                    {
                        current = other;
                    }
                }
                if (current != null)
                {
                    contHandler.DeleteObject(current, dueDate: cont.ActiveFrom);
                }

                contHandler.CreateObject(cont, datesAreSet: true);
            }
            return cont;
        }

        /// <summary>
        /// Updates the given EMDContact in the database and also adapts the current active Contact if the updated contact is active in the future.
        /// </summary>
        /// <param name="cont">Contact to update. Mustn't be null.</param>
        /// <returns>Updated EMDContact</returns>
        /// <exception cref="BaseException">Is thrown with error-code <see cref="ErrorCodeHandler.E_EDP_BUSINESS_LOGIK"/> if there is more than one additional EMDContact of the specific ContactType for the employment.</exception>
        /// <seealso cref="EMDObjectHandler.UpdateObject{T}(IEMDObject{T}, bool, bool, bool))"/>
        public EMDContact Update(EMDContact cont)
        {
            using (ContactHandler contH = new ContactHandler())
            {
                if (cont.ActiveFrom > DateTime.Now)
                {
                    //Find currently active contact with specific ContactType
                    try
                    {
                        EMDContact activeCont = contH.GetObjectsForEmployment(cont.EP_Guid)
                                                                        .Cast<EMDContact>()
                                                                        .Where(c => c.CT_Guid == cont.CT_Guid
                                                                                 && c.Guid != cont.Guid)
                                                                        .SingleOrDefault();

                        if (activeCont != null && !DateTimeHelper.IsDateTimeEqual(activeCont.ActiveTo, cont.ActiveFrom))
                        {
                            //Change ActiveTo
                            contH.DeleteObject(activeCont, dueDate: cont.ActiveFrom);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The employment (" + cont.EP_Guid + ") has more than one additional contacts of type " + cont.CT_Guid, ex);
                    }
                }
                return contH.UpdateObject(cont, allowChangeActive: true) as EMDContact;
            }
        }

        public EMDContact GetContactByContactType(string empl_guid, int contactType_Id, bool isFuture = false)
        {
            ContactHandler contactHandler = new ContactHandler(this.Guid_ModifiedBy, this.ModifyComment);
            List<EMDContact> contacts = null;

            if (isFuture)
            {
                DateTime today = DateTime.Now;
                contacts = contactHandler.GetActiveObjectsInInterval<EMDContact, Contact>(new DateTime(today.Year, today.Month, today.Day).AddDays(1), EMDContact.INFINITY, "EP_Guid = \"" + empl_guid + "\" and C_CT_ID = " + contactType_Id).FindAll(a => a.ActiveFrom > DateTime.Now);
            }
            else
            {
                contacts = contactHandler.GetObjects<EMDContact, Contact>("EP_Guid = \"" + empl_guid + "\" and C_CT_ID = " + contactType_Id).Cast<EMDContact>().ToList();
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

        public EMDContact GetContactByContactType(string empl_guid, string contactTypeGuid)
        {
            ContactHandler contactHandler = new ContactHandler(this.Guid_ModifiedBy, this.ModifyComment);

            List<IEMDObject<EMDContact>> contacts = contactHandler.GetObjects<EMDContact, Contact>("EP_Guid = \"" + empl_guid + "\" and CT_Guid = \"" + contactTypeGuid + "\"");

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

        /// <summary>
        /// get contact text by contact type guid
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

        /// <summary>
        /// Deletes the contact with the given <paramref name="guid"/> and updates the <see cref="EMDObject{T}.ActiveTo"/> from an earlier contact to <see cref="EMDObject{T}.INFINITY"/> if one exists.
        /// </summary>
        /// <param name="guid">GUID of the contact to delete.</param>
        /// <returns>The deleted <see cref="EMDContact"/>.</returns>
        public EMDContact Delete(string guid)
        {
            using (ContactHandler contH = new ContactHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment))
            {
                EMDContact emdContact = Get(guid);
                if (emdContact != null)
                {
                    Hashtable hashTable = contH.GetRelatedEntities(guid);

                    if (hashTable.Count == 0)
                    {
                        if (emdContact.ActiveFrom > DateTime.Now)
                        {
                            EMDContact other = contH.GetObjectsForEmployment(emdContact.EP_Guid, true).Cast<EMDContact>()
                                                      .Where(c => c.CT_Guid == emdContact.CT_Guid && c.Guid != emdContact.Guid) //Same contact-type but not the contact currently deleted
                                                      .SingleOrDefault(); //There are only zero or one additional contacts allowed

                            if (other.ActiveFrom < emdContact.ActiveFrom) //other is earlier than emdContact
                            {
                                other.ActiveTo = EMDContact.INFINITY;
                                contH.UpdateObject(other, allowChangeActive: true);
                            }
                        }
                        return (EMDContact)contH.DeleteObject<EMDContact>(emdContact);
                    }
                    else
                    {
                        throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                    }
                }
                else
                {
                    throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The contact with guid: {0} was not found.", guid));
                }
            }
        }
    }
}
