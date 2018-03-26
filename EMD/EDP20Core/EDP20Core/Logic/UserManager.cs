using Kapsch.IS.EDP.Core.Logic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.Logging;
using System.Reflection;
using Kapsch.IS.Util.ErrorHandling;
using System.Data.Entity;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using System.Collections;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class UserManager
        : BaseManager
        , IUserManager
    {
        protected internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public UserManager()
            : base()
        {
        }

        public UserManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public UserManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public UserManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public EMDUser Get(string guid)
        {
            UserHandler userHandler = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            return (EMDUser)userHandler.GetObject<EMDUser>(guid);
        }

        public EMDUser GetUserByUserName(string userName)
        {
            UserHandler userHandler = new UserHandler(this.Transaction);

            List<EMDUser> users = userHandler.GetObjects<EMDUser, User>(string.Format("Username like \"{0}\"", userName)).Cast<EMDUser>().ToList();

            if (users.Count == 1)
            {
                return users[0];
            }
            else
            {
                return null;
            }
        }


        public EMDUser Delete(string guid)
        {
            UserHandler userHandler = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDUser emdUser = Get(guid);
            if (emdUser != null)
            {
                Hashtable hashTable = userHandler.GetRelatedEntities(guid);

                if (hashTable.Count == 0)
                {
                    return (EMDUser)userHandler.DeleteObject<EMDUser>(emdUser);
                }
                else
                {
                    throw new RelatedEntitiesException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There were {0} related entities found.", hashTable.Count), hashTable);
                }
            }
            else
            {
                throw new EntityNotFoundException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("The user with guid: {0} was not found.", guid));
            }
        }

        public List<EMDUser> GetEmploymentUsers(string employmentGuid, bool deliverHistorical = false)
        {

            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            UserHandler userHandler = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            if (deliverHistorical)
            {
                userHandler.Historical = true;
                userHandler.DeliverInActive = true;
            }

            //EMDEmployment emdEmployment = (EMDEmployment)employmentHandler.GetObject<EMDEmployment>(employmentGuid);
            List<EMDUser> emdUsers = userHandler.GetObjects<EMDUser, DB.User>("EMPL_Guid=\"" + employmentGuid + "\"").Cast<EMDUser>().ToList();


            return emdUsers;
        }

        /// <summary>
        /// Disabling all users of a given employment, by setting the status to Reserved
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns></returns>
        public List<EMDUser> DisableUsersOnEmployment(string emplGuid)
        {

            UserHandler userHandler = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);


            // delete users
            List<EMDUser> result = userHandler.GetObjects<EMDUser, User>("EMPL_Guid =  \"" + emplGuid + "\"").Cast<EMDUser>().ToList();
            foreach (EMDUser user in result)
            {

                user.Status = (int)EnumUserStatus.Reserverd;
                userHandler.UpdateObject(user);
            }

            return result;
        }

        public void Update(EMDUser emdUser)
        {
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            userH.UpdateDBObject(emdUser);
        }

        public void Update(EMDUser emdUser, bool historize, bool checkActiveTo = true)
        {
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            userH.UpdateObject(emdUser, historize, checkActiveTo);
        }

        public EMDUser Create(EMDUser emdUser)
        {
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            if (!UserExists(emdUser))
            {
                return (EMDUser)userH.CreateObject(emdUser);
            }

            throw new UserException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There is already a User with the name: {0} in USDO_Guid: {1}", emdUser.Username, emdUser.USDO_Guid), EnumUserExceptionType.UserExists);
        }

        public EMDUser Create(EMDUser emdUser, bool datesAreSet)
        {
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            if (!UserExists(emdUser))
            {
                return (EMDUser)userH.CreateObject(emdUser, datesAreSet: datesAreSet);
            }

            throw new UserException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There is already a User with the name: {0} in USDO_Guid: {1}", emdUser.Username, emdUser.USDO_Guid), EnumUserExceptionType.UserExists);
        }

        public EMDUser Create(EMDUser emdUser, bool datesAreSet, CoreTransaction transaction)
        {
            if (transaction == null)
                return Create(emdUser, datesAreSet);

            if (!UserExists(emdUser, transaction))
            {
                UserHandler userHandler = new UserHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);
                return (EMDUser)userHandler.CreateObject(emdUser, datesAreSet: datesAreSet);
            }

            throw new UserException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("There is already a User with the name: {0} in USDO_Guid: {1}", emdUser.Username, emdUser.USDO_Guid), EnumUserExceptionType.UserExists);
        }

        public bool UserExists(EMDUser emdUser)
        {
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            IEnumerable<IEMDObject<EMDUser>> users = null;
            EMD_Entities emdEntities = new EMD_Entities();
            if (emdUser != null)
            {
                if (emdUser.USDO_Guid != null)
                {
                    /*users = (from user in emdEntities.User
                             where user.Username.Equals(emdUser.Username, StringComparison.CurrentCultureIgnoreCase) && user.USDO_Guid.Equals(emdUser.USDO_Guid)
                             select user).ToList();*/

                    users = userH.GetObjects<EMDUser, User>().Where(item => string.Compare((item as EMDUser).Username, emdUser.Username, true) == 0 && (item as EMDUser).USDO_Guid.Equals(emdUser.USDO_Guid));
                }
                else
                {
                    /*users = (from user in emdEntities.User
                             where user.Username.Equals(emdUser.Username, StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(emdUser.USDO_Guid)
                             select user).ToList();*/
                    users = userH.GetObjects<EMDUser, User>("Username == \"" + emdUser.Username + "\"");
                    users = users?.Where(item => string.IsNullOrEmpty((item as EMDUser).USDO_Guid));
                }
            }

            if (users == null || users.Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool UserExists(EMDUser emdUser, CoreTransaction transaction)
        {
            UserHandler userH = new UserHandler(transaction, this.Guid_ModifiedBy, this.ModifyComment);
            IEnumerable<IEMDObject<EMDUser>> users = null;
            EMD_Entities emdEntities = new EMD_Entities();
            if (emdUser != null)
            {
                if (emdUser.USDO_Guid != null)
                {
                    /*users = (from user in emdEntities.User
                             where user.Username.Equals(emdUser.Username, StringComparison.CurrentCultureIgnoreCase) && user.USDO_Guid.Equals(emdUser.USDO_Guid)
                             select user).ToList();*/

                    users = userH.GetObjects<EMDUser, User>().Where(item => string.Compare((item as EMDUser).Username, emdUser.Username, true) == 0 && (item as EMDUser).USDO_Guid.Equals(emdUser.USDO_Guid));
                }
                else
                {
                    /*users = (from user in emdEntities.User
                             where user.Username.Equals(emdUser.Username, StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(emdUser.USDO_Guid)
                             select user).ToList();*/
                    users = userH.GetObjects<EMDUser, User>("Username == \"" + emdUser.Username + "\"");
                    users = users?.Where(item => string.IsNullOrEmpty((item as EMDUser).USDO_Guid));
                }
            }

            if (users == null || users.Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }




        /// <summary>
        /// creates 3 standard users main, 00 and 99
        /// only if the employment is the first employment for this person
        /// if not first employment no need to reserve username b/c already there
        /// Throws BaseException
        /// </summary>
        /// <param name="empl"></param>
        /// <param name="usdo_guid"></param>
        /// <returns>User from Main-Account</returns>
        public EMDUser CreateNewUsersToEmployment(EMDEmployment empl, string usdo_guid, EMDEmploymentType employmentType)
        {
            string prefix = string.Empty;
            if (employmentType.ET_ID == 11)
            {
                prefix = "A_";
            }

            EMDUser mainAccount = null;

            EmploymentHandler emplH = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            emplH.Historical = true;
            emplH.DeliverInActive = true;

            PersonHandler persH = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            UserManager userManager = new UserManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDPerson pers = (EMDPerson)persH.GetObject<EMDPerson>(empl.P_Guid);

            // get all employments inclusive historical to get 

            List<EMDEmployment> emdEmployments = emplH.GetObjects<EMDEmployment, Employment>(string.Format("P_GUID == \"{0}\"", pers.Guid)).Cast<EMDEmployment>().ToList();
            if (emdEmployments != null && emdEmployments.Count > 0)
            {
                List<EMDEmployment> foundOtherEmployments = (from a in emdEmployments
                                                             where a.Guid != empl.Guid
                                                             select a).ToList();  // emdEmployments.Select(a => a.Guid != empl.Guid).Cast<EMDEmployment>().ToList();
                if (foundOtherEmployments.Count > 0)
                {
                    bool hasUsers = false;
                    // the check is necessary if there are only deleted (Removed=new logic OR Deleted=Migration Logic) employments,
                    // because users can't be deleted in EDP or Active Directory
                    // check if there is already a userid in the userTable, which is configured on the person 
                    if (!string.IsNullOrWhiteSpace(pers.UserID))
                    {
                        int deactiveEmployments = (from a in foundOtherEmployments where (a.LastDay < DateTime.Now || a.ActiveTo < DateTime.Now || a.ValidTo < DateTime.Now) select a).Count();

                        foreach (EMDEmployment otherEmployment in foundOtherEmployments)
                        {
                            List<EMDUser> users = userManager.GetEmploymentUsers(otherEmployment.Guid);


                            EMDUser main = (from u in users where pers.UserID == u.Username select u).FirstOrDefault();
                            if (main != null && deactiveEmployments == foundOtherEmployments.Count)
                            {
                                // move all users to new employment
                                foreach (EMDUser user in users)
                                {
                                    user.EMPL_Guid = empl.Guid;
                                    userManager.Update(user);
                                }

                                return main;
                            }
                        }
                    }

                }
            }

            List<Tuple<string, int>> listofUserIDs = new List<Tuple<string, int>>();


            try
            {
                // go ahead and reserve new usernames
                listofUserIDs = userH.CreateUserIDProposalForPerson(pers.C128_FamilyName, pers.C128_FirstName, prefix);


                foreach (var userid in listofUserIDs)
                {
                    // item1 = username, item2 = userStatus
                    EMDUser user = userH.CreateUserID(empl, userid.Item1, usdo_guid, userid.Item2, EnumUserType.ADUserLimitedAccount);
                    if (!user.Username.StartsWith("00", StringComparison.CurrentCultureIgnoreCase) && !user.Username.StartsWith("99", StringComparison.CurrentCultureIgnoreCase))
                    {
                        mainAccount = user;
                    }
                }

            }
            catch (Exception ex)
            {
                string msg = "error creating usernames for EMPL:" + empl.Guid + " and Pers :" + empl.P_Guid;
                logger.Error(msg, ex);
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, msg, ex);
            }

            return mainAccount;
        }


        public void MoveUserFullAccountToMainEmployment(string persGuid)
        {
            EmploymentManager employmentManager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            UserManager userManager = new UserManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonManager personManager = new PersonManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDEmployment mainEmployment = employmentManager.GetMainEploymentForPerson(persGuid);
            List<EMDEmployment> employments = employmentManager.GetEmploymentsForPerson(persGuid, true);

            foreach (EMDEmployment employment in employments)
            {
                if (employment.Guid != mainEmployment.Guid)
                {
                    List<EMDUser> users = GetEmploymentUsers(employment.Guid);
                    EMDUser userFullAccount = users.Find(a => a.UserType == (byte)EnumUserType.ADUserFullAccount);

                    if (userFullAccount != null)
                    {
                        userFullAccount.EMPL_Guid = mainEmployment.Guid;
                        userManager.Update(userFullAccount);

                        EMDPerson person = personManager.Get(persGuid);
                        person.USER_GUID = userFullAccount.Guid;
                        person.UserID = userFullAccount.Username;
                        personHandler.UpdateObject(person);
                        break;
                    }

                }

            }
        }


        /// <summary>
        /// Search user in all employments and set the persons main user information
        /// </summary>
        /// <param name="person"></param>
        public void SetMainEmploymentUserOnPerson(EMDPerson person)
        {
            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EmploymentManager employmentManager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            ObjectFlagManager flagManager = new ObjectFlagManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            EMDEmployment employmenent = employmentManager.GetMainEploymentForPerson(person.Guid);

            List<EMDUser> users = GetEmploymentUsers(employmenent.Guid);

            foreach (EMDUser user in users)
            {
                if (user.UserType == (byte)EnumUserType.ADUserFullAccount)
                {
                    person.USER_GUID = user.Guid;
                    person.UserID = user.Username;
                    personHandler.UpdateObject(person);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the employmentGuid for the main-user from oldEmployment to newEmployment
        /// Updates also the mainAccount-User Guid on person
        /// </summary>
        /// <param name="employmentGuidOld"></param>
        /// <param name="employmentGuidNew"></param>
        public void MoveEmploymentUserAndSynchronizePerson(string employmentGuidOld, string employmentGuidNew)
        {
            DateTime now = DateTime.Now;
            bool hasTransaction = this.Transaction != null;

            EmploymentHandler employmentHandler = new EmploymentHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            ObjectFlagManager flagManager = new ObjectFlagManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonHandler personHandler = new PersonHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            UserHandler userHandler = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            PersonManager personManager = new PersonManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);

            List<EMDUser> users = GetEmploymentUsers(employmentGuidOld);
            EMDEmployment emdEmployment = (EMDEmployment)employmentHandler.GetObject<EMDEmployment>(employmentGuidOld);
            EMDPerson emdPerson = personManager.Get(emdEmployment.P_Guid);
            // bool isMainEmployment = flagManager.IsMainEmployment(employmentGuidOld);

            // set only the main user to the new empoymentGuid
            foreach (EMDUser user in users)
            {
                if (emdPerson.USER_GUID == user.Guid)
                {
                    // udpate the employment Guid on the user (Synchronization)             
                    user.EMPL_Guid = employmentGuidNew;
                    userHandler.UpdateDBObject(user);

                    // Update UserGuid on Person
                    emdPerson.USER_GUID = user.Guid;
                    emdPerson.UserID = user.Username;
                    personHandler.UpdateDBObject(emdPerson);
                }

            }

        }

        public List<Tuple<string, int>> CreateUserIDProposalForPerson(string familyName, string firstName)
        {
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return userH.CreateUserIDProposalForPerson(familyName, firstName);
        }

        public List<Tuple<string, int>> CreateUserIDProposalForPerson(string familyName, string firstName, string prefix)
        {
            UserHandler userH = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            return userH.CreateUserIDProposalForPerson(familyName, firstName, prefix);
        }

        public List<EMDUser> GetEmploymentUsers(string employmentGuid)
        {
            return GetEmploymentUsers(employmentGuid, false);
        }

        public List<EMDUser> GetHistoricalEmploymentUsers(string employmentGuid)
        {
            return GetEmploymentUsers(employmentGuid, true);
        }

        /// <summary>
        /// Returns a available main username for a given familyName-firstName combination for a employmentType
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="firstName"></param>
        /// <param name="employmentType">optional</param>
        /// <returns></returns>
        public string GetNewMainUserName(string familyName, string firstName, EMDEmploymentType employmentType = null)
        {
            string prefix = string.Empty;
            if (employmentType != null && employmentType.ET_ID == 11)
            {
                prefix = "A_";
            }

            return new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment).CreateMainUserIDLogic(familyName, firstName, prefix);
        }

        /// <summary>
        /// Creates a username for an employment in a given userdomain with status reserved
        /// </summary>
        /// <param name="empl"></param>
        /// <param name="userName"></param>
        /// <param name="usdo_guid"></param>
        /// <param name="usrStatus"></param>
        /// <returns></returns>
        public EMDUser CreateMainUserName(EMDEmployment empl, string userName, string usdo_guid)
        {
            EMDUser user = new UserHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment).CreateUserID(empl, userName, usdo_guid, (byte)EnumUserStatus.Reserverd, EnumUserType.ADUserFullAccount);
            return user;
        }

        /// <summary>
        /// Sets a the userGuid on the person as new main user
        /// the new user-status is set to "InUse"
        /// the old user-status is set to "Reserverd"
        /// If the new user has no 00 or 99 users, these users will be created with status "Reserved"
        /// </summary>
        /// <param name="userGuid">Guid of the new main-user Guid</param>
        /// <param name="personGuid">Guid of the target person</param>
        /// <returns>the updated person entity, with the new userid and userguid</returns>
        public EMDPerson SetPersonMainUser(string userGuid, string personGuid)
        {
            EMDPerson person = new PersonManager(this.Transaction).Get(personGuid);
            EMDUser oldUser = null;
            EMDUser newUser = null;


            if (person == null)
            {
                throw new Exception(string.Format("No person found for personGuid: {0}", personGuid));
            }

            oldUser = new UserManager(this.Transaction).Get(person.USER_GUID);
            newUser = new UserManager(this.Transaction).Get(userGuid);

            List<EMDUser> existingNewUsers = new UserManager(this.Transaction).GetEmploymentUsers(newUser.EMPL_Guid);

            string existingUserGuid = person.USER_GUID;

            if (userGuid == existingUserGuid)
            {
                throw new Exception(string.Format("The existing userGuid is the same as the new userGuid: {0}", userGuid));
            }

            bool useTransaction = this.Transaction == null;
            CoreTransaction transaction = this.Transaction == null ? new CoreTransaction() : this.Transaction;

            UserHandler userHandler = new UserHandler(transaction);
            PersonHandler personHandler = new PersonHandler(transaction);

            try
            {
                if (useTransaction)
                {
                    transaction.Begin();
                }

                // update new user status
                if (newUser.Status != (byte)EnumUserStatus.InUse)
                {
                    newUser.Status = (byte)EnumUserStatus.InUse;
                    userHandler.UpdateObject(newUser);
                }

                if (!newUser.Username.Contains("00") && !newUser.Username.Contains("99"))
                {
                    // check and create new 99 and 00 users
                    EMDUser user99 = existingNewUsers.Find(a => a.Username == string.Format("99{0}", newUser.Username));
                    if (user99 == null)
                    {
                        user99 = new EMDUser()
                        {
                            Username = string.Format("99{0}", newUser.Username),
                            Status = (byte)EnumUserStatus.Reserverd,
                            EMPL_Guid = newUser.EMPL_Guid,
                            USDO_Guid = newUser.USDO_Guid,
                            UserType = (byte)EnumUserType.ADUserLimitedAccount
                        };

                        userHandler.CreateObject(user99);
                    }

                    EMDUser user00 = existingNewUsers.Find(a => a.Username == string.Format("00{0}", newUser.Username));
                    if (user00 == null)
                    {
                        user00 = new EMDUser()
                        {
                            Username = string.Format("00{0}", newUser.Username),
                            Status = (byte)EnumUserStatus.Reserverd,
                            EMPL_Guid = newUser.EMPL_Guid,
                            USDO_Guid = newUser.USDO_Guid,
                            UserType = (byte)EnumUserType.ADUserLimitedAccount
                        };

                        userHandler.CreateObject(user00);
                    }
                }

                // update old user status
                if (oldUser != null)
                {
                    oldUser.Status = (byte)EnumUserStatus.Reserverd;
                    userHandler.UpdateObject(oldUser);
                }

                // update main user on person
                person.USER_GUID = newUser.Guid;
                person.UserID = newUser.Username;
                person = (EMDPerson)personHandler.UpdateObject(person);

                if (useTransaction)
                {
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                if (useTransaction)
                {
                    transaction.Rollback();
                }
                person = null;

                throw new Exception("Set person main user failed", ex);
            }

            return person;
        }
    }
}
