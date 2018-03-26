using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ReflectionHelper;
using System.Collections;
using Kapsch.IS.EDP.Core.Framework.Exceptions;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class PersonHandler : EMDObjectHandler
    {
        public PersonHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public PersonHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public PersonHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public PersonHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override IEMDObject<T> CreateObject<T>(IEMDObject<T> emdObject, string guid = null, bool datesAreSet = false)
        {
            EMDPerson pers = emdObject as EMDPerson;
            if (pers == null) throw new ArgumentException("Argument is not a EMDPerson");

            if (!this.IsC128Confirm(pers)) throw new FormatException("One or more C128-Fields contain strings with non-ascii characters");
            return base.CreateObject<T>(emdObject, guid, datesAreSet);
        }

        /// <summary>
        /// Overwrite Delete, because Deletion of persons should be only allowed, if there are no employments linked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="emdObject"></param>
        /// <param name="historize"></param>
        /// <param name="ignoreDependency"></param>
        /// <param name="dueDate"></param>
        /// <returns></returns>
        public override IEMDObject<T> DeleteObject<T>(IEMDObject<T> emdObject, bool historize = true, bool ignoreDependency = false, DateTime? dueDate = null)
        {
            EMDPerson pers = emdObject as EMDPerson;
            if (pers == null) throw new ArgumentException("Argument is not a EMDPerson");

            EmploymentHandler emplHandler = new EmploymentHandler(this.Transaction);
            int employments = emplHandler.GetEmploymentCountForPerson(pers.Guid);

            if (employments > 0)
            {
                Hashtable relations = new Hashtable();
                relations.Add("EMDEmployment", employments);
                throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related employments found.", employments), relations);
            }

            return base.DeleteObject(emdObject, historize, ignoreDependency, dueDate);
        }

        // Only for Migration
        [Obsolete]
        public IEMDObject<T> CreateObjectForMigration<T>(IEMDObject<T> emdObject, string guid = null, bool datesAreSet = false)
        {
            return base.CreateObject<T>(emdObject, guid, datesAreSet);
        }
        public override IEMDObject<T> UpdateObject<T>(IEMDObject<T> emdObject, bool historize = true, bool checkActiveTo = true, bool allowChangeActive = false)
        {
            EMDPerson pers = emdObject as EMDPerson;
            if (pers == null) throw new ArgumentException("Argument is not a EMDPerson");

            if (!this.IsC128Confirm(pers)) throw new FormatException("One or more C128-Fields contain strings with non-ascii characters");
            return base.UpdateObject<T>(emdObject, historize, checkActiveTo, allowChangeActive);
        }

        // Only for Migration
        [Obsolete]
        public IEMDObject<T> UpdateObjectForMigration<T>(IEMDObject<T> emdObject, bool historize = true, bool checkActiveTo = true)
        {
            return base.UpdateObject<T>(emdObject, historize, checkActiveTo);
        }

        /// <param name="pers"></param>
        /// <returns>True if all C128-Fields are actual C128 strings, else false,
        /// throws NullReferenceException if at least one of the C128 strings is null</returns>
        public bool IsC128Confirm(EMDPerson pers)
        {
            try
            {

                if (TextAndNamingHelper.IsC128String(pers.C128_DegreePrefix ?? "") //not required -> null to empty string so test doesn't fail
                    && TextAndNamingHelper.IsC128String(pers.C128_DegreeSuffix ?? "") //not required -> null to empty string so test doesn't fail
                    && TextAndNamingHelper.IsC128String(pers.C128_FamilyName)         //required -> fails if null
                    && TextAndNamingHelper.IsC128String(pers.C128_FirstName))         //required -> fails if null
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("One or more of the C128-Fields is null", ex);
            }
        }

        public override Type GetDBObjectType()
        {
            return new Person().GetType();
        }

        internal override T CreateDataFromDBObject<T, S>(S dbObject, IPropertyCopier<S, T> propCopier)
        {
            if (dbObject == null)
                return default(T);
            else
            {
                T emdObject = new T();
                if (emdObject is EMDPerson && dbObject is Person)
                {
                    Person dbPerson = dbObject as Person;
                    EMDPerson emdPerson = emdObject as EMDPerson;

                    emdPerson.Guid = dbPerson.Guid;
                    emdPerson.HistoryGuid = dbPerson.HistoryGuid;
                    emdPerson.ValidFrom = dbPerson.ValidFrom;
                    emdPerson.ValidTo = dbPerson.ValidTo;
                    emdPerson.Created = dbPerson.Created;
                    emdPerson.Modified = dbPerson.Modified;
                    emdPerson.ActiveFrom = dbPerson.ActiveFrom;
                    emdPerson.ActiveTo = dbPerson.ActiveTo;

                    emdPerson.P_ID = dbPerson.P_ID;
                    emdPerson.FamilyName = dbPerson.FamilyName;
                    emdPerson.FirstName = dbPerson.FirstName;
                    emdPerson.Synonyms = dbPerson.Synonyms;
                    emdPerson.Sex = dbPerson.Sex;
                    emdPerson.DegreePrefix = dbPerson.DegreePrefix;
                    emdPerson.DegreeSuffix = dbPerson.DegreeSuffix;
                    emdPerson.C128_FamilyName = dbPerson.C128_FamilyName;
                    emdPerson.C128_FirstName = dbPerson.C128_FirstName;
                    emdPerson.C128_DegreePrefix = dbPerson.C128_DegreePrefix;
                    emdPerson.C128_DegreeSuffix = dbPerson.C128_DegreeSuffix;
                    emdPerson.UserID = dbPerson.UserID;
                    emdPerson.MainMail = dbPerson.MainMail;
                    emdPerson.UnixID = dbPerson.UnixID;
                    emdPerson.Language = dbPerson.Language;
                    emdPerson.Display_FamilyName = dbPerson.Display_FamilyName;
                    emdPerson.Display_FirstName = dbPerson.Display_FirstName;
                    emdPerson.AD_Picture_UpdDT = dbPerson.AD_Picture_UpdDT;
                    emdPerson.USER_GUID = dbPerson.USER_GUID;
                    emdPerson.Guid_ModifiedBy = dbPerson.Guid_ModifiedBy;
                    emdPerson.ModifyComment = dbPerson.ModifyComment;
                }
                else
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Given object can't be copied by this Handler");
                return emdObject;
            }
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null)
                return null;

            Person person = (Person)dbObject;
            EMDPerson emdObject = new EMDPerson(person.Guid, person.Created, person.Modified);

            if (dbObject is Kapsch.IS.EDP.Core.DB.Person)
            {
                emdObject.Guid = person.Guid;
                emdObject.HistoryGuid = person.HistoryGuid;
                emdObject.ValidFrom = person.ValidFrom;
                emdObject.ValidTo = person.ValidTo;
                emdObject.Created = person.Created;
                emdObject.Modified = person.Modified;
                emdObject.P_ID = person.P_ID;
                emdObject.FamilyName = person.FamilyName;
                emdObject.FirstName = person.FirstName;
                emdObject.Synonyms = person.Synonyms;
                emdObject.Sex = person.Sex;
                emdObject.DegreePrefix = person.DegreePrefix;
                emdObject.DegreeSuffix = person.DegreeSuffix;
                emdObject.C128_FamilyName = person.C128_FamilyName;
                emdObject.C128_FirstName = person.C128_FirstName;
                emdObject.C128_DegreePrefix = person.C128_DegreePrefix;
                emdObject.C128_DegreeSuffix = person.C128_DegreeSuffix;
                emdObject.UserID = person.UserID;
                emdObject.MainMail = person.MainMail;
                emdObject.UnixID = person.UnixID;
                emdObject.Language = person.Language;
                emdObject.Display_FamilyName = person.Display_FamilyName;
                emdObject.Display_FirstName = person.Display_FirstName;
                emdObject.AD_Picture_UpdDT = person.AD_Picture_UpdDT;
                emdObject.ActiveFrom = person.ActiveFrom;
                emdObject.ActiveTo = person.ActiveTo;
                emdObject.USER_GUID = person.USER_GUID;
                emdObject.Guid_ModifiedBy = person.Guid_ModifiedBy;
                emdObject.ModifyComment = person.ModifyComment;
            }
            else
                ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);

            emdObject.SetValidityStatus();

            return (IEMDObject<T>)emdObject;
        }

        [Obsolete]
        public IEMDObject<EMDPerson> GetPersonByP_Id(int p_Id)
        {
            // Errorcases: return null of no person is found
            // throw exception if there are more persons found than only one.

            List<IEMDObject<EMDPerson>> personlist = (List<IEMDObject<EMDPerson>>)GetObjects<EMDPerson, Person>("P_ID = " + p_Id.ToString(), null).ToList();

            if (personlist.Count > 1) throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Consistencyproblem: more than one person found with same P_ID");
            if (personlist.Count == 0)
            {
                return null;
            }
            return personlist.First();
        }

        public List<String> GetDistinctPersonSurNames()
        {
            List<String> listPersonSurName = new List<String>();

            IQueryable<String> query = (from item in transaction.dbContext.Person select item.FamilyName.ToString()).Distinct();
            //calculatePagingValues<Person>(query, null);          

            listPersonSurName = query.ToList();

            return listPersonSurName;
        }

        public int GetNextFreePIDForPerson()
        {
            bool puffer = this.Historical;

            //this call does not make sense non-Historizabel since persons can't "timeout" but is done for safety
            this.Historical = true;
            IQueryable<int> query = (from item in transaction.dbContext.Person where item.P_ID < 99999 orderby item.P_ID descending select item.P_ID);

            List<int> result = query.ToList();

            //set back to the stored value
            this.Historical = puffer;
            //return the + 1 added new Int.
            return result.FirstOrDefault() + 1;
        }

        /// <summary>
        /// Creates a new free e-Mailadress for a person by format:
        /// firstname.familyname.et_EmailSign@domain
        /// if this adress already exists familyname will be reduced by a character and replaced with a number
        /// </summary>
        /// <param name="person"></param>
        /// <param name="index"></param>
        /// <param name="emailType"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public String CreateMainMailProposalForPerson(EMDPerson person, String emailType = "", String domain = "kapsch.net")
        {
            int i = 1;
            person.MainMail = _CreateMainMailProposalForPerson(person.C128_FirstName, person.C128_FamilyName, ref i, emailType, domain);
            return person.MainMail;
        }

        /// <summary>
        /// Creates a new free e-Mailadress for a person by format:
        /// firstname.familyname.et_EmailSign@domain
        /// if this adress already exists familyname will be reduced by a character and replaced with a number
        /// </summary>
        /// <param name="firstname"></param>
        /// <param name="familyname"></param>
        /// <param name="index"></param>
        /// <param name="emailType"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        private String _CreateMainMailProposalForPerson(String firstname, String familyname, ref int index, String emailType = "", String domain = "kapsch.net")
        {
            String resultEMail = "";
            if (index == 2)
            {
                familyname += index.ToString();
            }
            else if (index > 2)
            {
                familyname = familyname.Substring(0, familyname.Length - 1) + index.ToString();
            }
            else if (index > 100)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "No free emailadress possible for this person");
            }

            if (emailType != "")
                resultEMail = firstname + "." + familyname + "." + emailType + "@" + domain;
            else
                resultEMail = firstname + "." + familyname + "@" + domain;

            //check whether this MailAdress Already exists and call again if so.

            Person person = (from per in transaction.dbContext.Person where per.MainMail.Trim() == resultEMail select per).FirstOrDefault();
            if (person == null)
            {
                return resultEMail;
            }
            else
            {
                index += 1;
                return _CreateMainMailProposalForPerson(firstname, familyname, ref index, emailType, domain);
            }
        }        

        internal override void MapDataToDBObject<T>(ref object dbObject, ref IEMDObject<T> emdObject)
        {
            if (dbObject is Kapsch.IS.EDP.Core.DB.Person && emdObject is Kapsch.IS.EDP.Core.Entities.EMDPerson)
            {
                Kapsch.IS.EDP.Core.DB.Person dbPerson = dbObject as Kapsch.IS.EDP.Core.DB.Person;
                Kapsch.IS.EDP.Core.Entities.EMDPerson emdPerson = emdObject as Kapsch.IS.EDP.Core.Entities.EMDPerson;

                dbPerson.Guid = emdPerson.Guid;
                dbPerson.HistoryGuid = emdPerson.HistoryGuid;
                dbPerson.ValidFrom = emdPerson.ValidFrom;
                dbPerson.ValidTo = emdPerson.ValidTo;
                dbPerson.Created = emdPerson.Created;
                dbPerson.Modified = emdPerson.Modified;
                dbPerson.P_ID = emdPerson.P_ID;
                dbPerson.FamilyName = emdPerson.FamilyName;
                dbPerson.FirstName = emdPerson.FirstName;
                dbPerson.Synonyms = emdPerson.Synonyms;
                dbPerson.Sex = emdPerson.Sex;
                dbPerson.DegreePrefix = emdPerson.DegreePrefix;
                dbPerson.DegreeSuffix = emdPerson.DegreeSuffix;
                dbPerson.C128_FamilyName = emdPerson.C128_FamilyName;
                dbPerson.C128_FirstName = emdPerson.C128_FirstName;
                dbPerson.C128_DegreePrefix = emdPerson.C128_DegreePrefix;
                dbPerson.C128_DegreeSuffix = emdPerson.C128_DegreeSuffix;
                dbPerson.UserID = emdPerson.UserID;
                dbPerson.MainMail = emdPerson.MainMail;
                dbPerson.UnixID = emdPerson.UnixID;
                dbPerson.Language = emdPerson.Language;
                dbPerson.Display_FamilyName = emdPerson.Display_FamilyName;
                dbPerson.Display_FirstName = emdPerson.Display_FirstName;
                dbPerson.AD_Picture_UpdDT = emdPerson.AD_Picture_UpdDT;
                dbPerson.ActiveFrom = emdPerson.ActiveFrom;
                dbPerson.ActiveTo = emdPerson.ActiveTo;
                dbPerson.USER_GUID = emdPerson.USER_GUID;
                dbPerson.Guid_ModifiedBy = emdPerson.Guid_ModifiedBy;
                dbPerson.ModifyComment = emdPerson.ModifyComment;
            }
            else
                base.MapDataToDBObject<T>(ref dbObject, ref emdObject);
        }
    }
}
