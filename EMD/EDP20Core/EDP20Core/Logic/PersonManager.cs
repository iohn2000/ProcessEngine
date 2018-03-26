using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Logic.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;

using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class PersonManager
        : BaseManager
        , IPersonManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public PersonManager()
            : base()
        {
        }

        public PersonManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public PersonManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public PersonManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDPerson Get(string guid)
        {
            PersonHandler userHandler = new PersonHandler(this.Transaction);

            return (EMDPerson)userHandler.GetObject<EMDPerson>(guid);
        }

        public EMDPerson GetPersonByEmployment(string employmentGuid)
        {
            EMD_Entities db_Context = new EMD_Entities();

            DateTime now = DateTime.Now;

            EMDPerson foundEmdPerson = null;
            if (this.Transaction == null)
            {
                Person foundPerson = (from pers in db_Context.Person
                                      join empl in db_Context.Employment on pers.Guid equals empl.P_Guid
                                      where
                                       pers.ValidFrom < now && pers.ValidTo > now && pers.ActiveFrom < now && pers.ActiveTo > now &&
                                       empl.ValidFrom < now && empl.ValidTo > now && empl.ActiveFrom < now && empl.ActiveTo > now &&
                                       empl.Guid == employmentGuid
                                      select pers).FirstOrDefault();

                foundEmdPerson = new EMDPerson();
                ReflectionHelper.CopyProperties(ref foundPerson, ref foundEmdPerson);
            }
            else
            {
                EMDEmployment employment = new EmploymentManager(this.Transaction).GetEmployment(employmentGuid);
                PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
                foundEmdPerson = (EMDPerson)personHandler.GetObject<EMDPerson>(employment.P_Guid);
            }



            return foundEmdPerson;
        }

        public string getFullDisplayNameWithUserId(EMDPerson pers)
        {
            string fullDisplayNameWithUserId = String.Empty;
            if (pers != null)
            {
                //if (pers.Display_FamilyName != null)
                //    fullDisplayNameWithUserId += pers.Display_FamilyName + " ";

                //if (pers.Display_FirstName != null)
                //    fullDisplayNameWithUserId += pers.Display_FirstName + " ";
                fullDisplayNameWithUserId = GetFullDisplayName(pers) + " ";

                if (pers.UserID != null)
                    fullDisplayNameWithUserId += "(" + pers.UserID + ")";

            }
            return fullDisplayNameWithUserId;
        }


        /// <summary>
        /// Sets the email address on the person object
        /// </summary>
        /// <param name="emdPers"></param>
        /// <param name="emplGuid"></param>
        /// <param name="transaction"></param>
        /// <param name="personGuidModifiedBy"></param>
        /// <param name="modifyComment"></param>
        /// <param name="isExternalEmail"></param>
        /// <returns>the email-address set on the person</returns>
        public string CreateMainMailForPerson(ref EMDPerson emdPers, EMDEmploymentType effectedEmploymentType, CoreTransaction transaction, string personGuidModifiedBy = null, string modifyComment = null, bool? isExternalEmail = null)
        {
            string mailAddress = string.Empty;


            PersonHandler persH = new PersonHandler(transaction, personGuidModifiedBy ?? this.Guid_ModifiedBy, modifyComment ?? this.ModifyComment);
            // get employmentType for generation E-Mail Adress


            if (emdPers != null)
            {
                if (string.IsNullOrWhiteSpace(emdPers.MainMail))
                {
                    // for e-mailtype there are 2 strings possible: intern or extern
                    // only for extern types there must be created a suffix
                    if ((isExternalEmail == null || isExternalEmail.Value) && !string.IsNullOrWhiteSpace(effectedEmploymentType.EMailSign))
                    {
                        mailAddress = persH.CreateMainMailProposalForPerson(emdPers, effectedEmploymentType.EMailSign);
                    }
                    else
                    {
                        mailAddress = persH.CreateMainMailProposalForPerson(emdPers);
                    }
                }
                else
                {
                    mailAddress = emdPers.MainMail;
                }
            }

            return mailAddress;
        }




        public string getFullDisplayNameWithUserIdAndPersNr(string empl_guid)
        {
            EmploymentManager emplManager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDEmployment empl = emplManager.GetEmployment(empl_guid);
            if (empl != null)
                return getFullDisplayNameWithUserIdAndPersNr(empl);
            else
                return String.Empty;
        }

        public string getFullDisplayNameWithUserIdAndPersNr(EMDEmployment empl)
        {
            PersonHandler persHandler = new PersonHandler(this.Transaction);
            EMDPerson pers = (EMDPerson)persHandler.GetObject<EMDPerson>(empl.P_Guid);
            return getFullDisplayNameWithUserIdAndPersNr(pers, empl);
        }

        public static string getFullDisplayNameWithUserIdAndPersNr(EMDPerson pers, EMDEmployment empl)
        {
            string fullDisplayNameWithUserId = String.Empty;
            if (pers != null)
            {
                //if (pers.Display_FamilyName != null)
                //    fullDisplayNameWithUserId += pers.Display_FamilyName + " ";

                //if (pers.Display_FirstName != null)
                //    fullDisplayNameWithUserId += pers.Display_FirstName + " ";

                fullDisplayNameWithUserId = GetFullDisplayName(pers) + " ";

                if (pers.UserID != null)
                    fullDisplayNameWithUserId += "(" + pers.UserID + ")";

                if (!String.IsNullOrWhiteSpace(empl.EP_ID.ToString()))
                {
                    fullDisplayNameWithUserId += " - " + empl.EP_ID.ToString();
                }
            }
            return fullDisplayNameWithUserId;
        }

        public static string GetFullDisplayName(EMDPerson pers)
        {
            string fullDisplayName = String.Empty;
            if (pers != null)
            {
                if (pers.Display_FamilyName != null)
                    fullDisplayName += pers.Display_FamilyName + " ";

                if (pers.Display_FirstName != null)
                    fullDisplayName += pers.Display_FirstName;

            }
            return fullDisplayName;
        }

        public static string GetFullDisplayName(EMDEmployment empl)
        {
            string fullDisplayName = String.Empty;
            PersonManager persManager = new PersonManager();
            EMDPerson pers = persManager.GetPersonByEmployment(empl.Guid);

            if (pers != null)
            {
                if (pers.Display_FamilyName != null)
                    fullDisplayName += pers.Display_FamilyName + " ";

                if (pers.Display_FirstName != null)
                    fullDisplayName += pers.Display_FirstName + " ";

            }
            return fullDisplayName;
        }

        public EMDPerson Delete(string guid)
        {
            PersonHandler personHandler = new PersonHandler(this.Transaction);
            EMDPerson emdUser = Get(guid);
            if (emdUser != null)
            {
                Hashtable hashTable = personHandler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDPerson)personHandler.DeleteObject<EMDPerson>(emdUser);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The person with guid: {0} was not found.", guid));
            }
        }

        public bool IsItSelf(string userId, string pers_guid)
        {
            EMDPerson viewingUser = (EMDPerson)this.GetPersonByUserId(userId);
            EMDPerson viewedUser = (EMDPerson)this.Get(pers_guid);
            if (viewedUser == null)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No person found for " + pers_guid);
            }
            if (viewingUser == null)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No person found for user-id " + userId);
            }

            if (viewedUser.Guid == viewingUser.Guid)
                return true;
            else
                return false;
        }

        public EMDPerson GetPersonByUserId(String UserId)
        {
            // Errorcases: return null of no person is found
            // throw exception if there are more persons found than only one.
            PersonHandler persHandler = new PersonHandler(this.Transaction);
            List<IEMDObject<EMDPerson>> personlist = (List<IEMDObject<EMDPerson>>)persHandler.GetObjects<EMDPerson, Person>("UserID = \"" + UserId + "\"", null).ToList();

            if (personlist.Count > 1) throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Consistencyproblem: more than one person found with same UserID");
            if (personlist.Count == 0)
            {
                return null;
            }
            return (EMDPerson)personlist.First();
        }

        public List<EMDPerson> GetCreatedBy(String persGuid)
        {
            PersonHandler ph = new PersonHandler();
            ph.Historical = true;
            //List<EMDPerson> persons = ph.GetObjects<EMDPerson,Person>("Created = DATEADD(ss,2,ValidFrom) and Guid_ModifiedBy = \"" + persGuid + "\"").Cast<EMDPerson>().ToList();
            List<EMDPerson> persons = ph.GetObjects<EMDPerson, Person>("Guid_ModifiedBy = \"" + persGuid + "\"").Cast<EMDPerson>().ToList();
            List<EMDPerson> myPersons = new List<EMDPerson>();

            foreach (EMDPerson pers in persons)
            {
                if (pers.Created == pers.ValidFrom.AddSeconds(2))
                {
                    EMDPerson latestPerson = persons.Where(item => item.Guid == pers.HistoryGuid).FirstOrDefault();
                    if (latestPerson != null)
                        myPersons.Add(latestPerson);
                }
            }
            return myPersons;
        }

        public int GetNextFreeP_ID()
        {
            EMD_Entities dbcontext = new EMD_Entities();
            IQueryable<int> query = (from item in dbcontext.Person orderby item.P_ID descending select item.P_ID);
            //Some performance improvement
            IQueryable<int> newQuery = query.Take(1);

            List<int> result = newQuery.ToList();
            //return the + 1 added new Int.
            return result.Single() + 1;
        }

        public EMDPerson Create(EMDPerson person)
        {
            PersonHandler persHandler = new PersonHandler();
            person.P_ID = GetNextFreeP_ID();
            return (EMDPerson)persHandler.CreateObject<EMDPerson>(person);
        }

        /// <summary>
        /// updates the person picture hash
        /// if adhash is not given (picture was uploded new) "updated" or "new" is added as hash to ADHash
        /// </summary>
        /// <param name="persGuid"></param>
        /// <param name="hash"></param>
        /// <param name="adhash"></param>
        public void UpdatePersonPictureHash(String persGuid, String hash, bool adhash = false)
        {
            //Save the PP_AD_Hash in database
            PersonPortraitHandler portraitHandler = new PersonPortraitHandler(this.Guid_ModifiedBy, this.ModifyComment);
            EMDPersonPortrait persPortrait = portraitHandler.GetObjects<EMDPersonPortrait, PersonPortrait>("P_Guid = \"" + persGuid + "\"").Cast<EMDPersonPortrait>().ToList().FirstOrDefault();
            if (persPortrait != null)
            {
                persPortrait.GUI_Hash = hash;
                if (adhash)
                {
                    persPortrait.AD_Hash = hash;
                }
                else persPortrait.AD_Hash = "updated";
                persPortrait = (EMDPersonPortrait)portraitHandler.UpdateObject<EMDPersonPortrait>(persPortrait);
            }
            else
            {
                persPortrait = new EMDPersonPortrait();
                persPortrait.P_Guid = persGuid;
                persPortrait.GUI_Hash = hash;
                if (adhash)
                {
                    persPortrait.AD_Hash = hash;
                }
                else persPortrait.AD_Hash = "new";
                portraitHandler.CreateObject<EMDPersonPortrait>(persPortrait);
            }
        }
    }
}
