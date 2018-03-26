using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.Util.Strings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class UserHandler
        : EMDObjectHandler
        , IUserHandler
    {
        private const int USERID_LENGTH = 8;

        public UserHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public UserHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public UserHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public UserHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new User().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            User user = (User)dbObject;
            EMDUser emdObject = new EMDUser(user.Guid, user.Created, user.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        [Obsolete("old version of create user, links to person! please use new function", true)]
        public string CreateUserIDs(string P_Guid, List<string> userIds)
        {
            //ruf intern das auf was robert grad geschrieben hat. danke
            foreach (string userId in userIds)
            {
                EMDUser newUserID = new EMDUser();
                //newUserID.P_Guid = P_Guid;
                newUserID.Username = userId;
                newUserID.ValidFrom = DateTime.Now;
                newUserID.ValidTo = EMDUser.INFINITY;

                this.CreateObject<EMDUser>(newUserID);
            }

            //schreib die 3 (vielleicht iwann mehr???) ID's in die DB
            // ab hier ...
            if (userIds.Count > 0)
                return userIds.First();
            else
                return null;
        }

        /// <summary>
        /// create an entity User connected to employment
        /// </summary>
        /// <param name="empl"></param>
        /// <param name="userName"></param>
        /// <param name="usdo_guid"></param>
        /// <param name="usrStatus"></param>
        /// <param name="userType">the initial usertype - changed id changeUsertypes is true (default)</param>
        /// <param name="changeUserTypes">the initial usertype - changed id changeUsertypes is true (default)</param>
        public EMDUser CreateUserID(EMDEmployment empl, string userName, string usdo_guid, int usrStatus, EnumUserType userType, bool changeUserTypes = true)
        {
            if (changeUserTypes)
            {
                if (userName.StartsWith("A_", StringComparison.CurrentCultureIgnoreCase))
                {
                    userType = EnumUserType.ADUserLimitedAccount;
                }
                else if (userName.StartsWith("00", StringComparison.CurrentCultureIgnoreCase) || userName.StartsWith("99", StringComparison.CurrentCultureIgnoreCase))
                {
                    userType = EnumUserType.ADUserLimitedAccount;
                }
                else
                {
                    userType = EnumUserType.ADUserFullAccount;
                }
            }


            EMDUser newUser = new EMDUser()
            {
                Username = userName,
                Status = usrStatus,
                EMPL_Guid = empl.Guid,
                USDO_Guid = usdo_guid,
                UserType = (byte)userType
            };

            EMDUser created = (EMDUser)this.CreateObject<EMDUser>(newUser);
            return created;
        }

        /// <summary>
        /// Creates a new user set, but KEEP CARE >> it ignores historical data
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="firstName"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public List<Tuple<string, int>> CreateUserIDProposalForPerson(string familyName, string firstName, string prefix = "")
        {
            List<Tuple<string, int>> results = new List<Tuple<string, int>>();
            String result = CreateMainUserIDLogic(familyName, firstName, prefix);
            String userIdTest = "99" + result;
            String userIdAdmin = "00" + result;
            results.Add(Tuple.Create<string, int>(result, (int)EnumUserStatus.Reserverd));
            if (string.IsNullOrEmpty(prefix))
            {
                results.Add(Tuple.Create<string, int>(userIdTest, (int)EnumUserStatus.Reserverd));
                results.Add(Tuple.Create<string, int>(userIdAdmin, (int)EnumUserStatus.Reserverd));
            }
            return results;
        }

        /// <summary>
        /// Checks if username is already in use
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private bool HasExistingUsername(string username)
        {
            List<User> users = this.Transaction.dbContext.User.SqlQuery(string.Format("SELECT * from [User] where Username like '{0}'", username), new object[] { }).ToList();
            return (users != null && users.Count > 0) || HasExistingOldUsernameInPersonTable(username);

        }

        /// Bugfix: 
        /// <summary>
        /// Checks if the username is already in use in the person table
        /// </summary>
        /// <remarks>
        /// Not all userid's where migrated correctly to the user table. Therefore we must also take a look at the specific view, where old user-Ids available only in the person-table are provided. ViewName: V_OldUserIdsNotInUserTable
        /// </remarks>
        /// <param name="username"></param>
        /// <returns></returns>
        private bool HasExistingOldUsernameInPersonTable(string username)
        {
            using (SqlConnection sqlConnection =  new SqlConnection(this.Transaction.dbContext.Database.Connection.ConnectionString))
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(string.Format("SELECT count(*) from [V_OldUserIdsNotInUserTable] where UserID like '{0}'", username), sqlConnection);
                    if (sqlCommand.Connection.State == System.Data.ConnectionState.Closed || sqlCommand.Connection.State == System.Data.ConnectionState.Broken)
                        sqlCommand.Connection.Open();

                    int foundUserIds = (int)sqlCommand.ExecuteScalar();
                    return foundUserIds > 0;
                }
                catch (Exception ex)
                {
                    throw new BaseException(ErrorCodeHandler.E_DB_GENERAL_ERROR, ex );
                }
            }
        }


        /// <summary>
        /// Returns a new main username for a given familyName-firstName-prefix combination
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="firstName"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public String CreateMainUserIDLogic(string familyName, string firstName, string prefix = "")
        {
            string result = "NONAME";
            int userid_length = USERID_LENGTH;

            if (prefix != String.Empty)
            {
                userid_length = USERID_LENGTH - prefix.Length;
                if (familyName.Length > USERID_LENGTH - 2)
                    result = prefix + familyName.Substring(0, USERID_LENGTH - 2).ToUpper();
                else
                    result = prefix + familyName.ToUpper();
            }
            else
            {
                if (familyName.Length > USERID_LENGTH)
                    result = familyName.Substring(0, USERID_LENGTH).ToUpper();
                else
                    result = familyName.ToUpper();
            }

            //if (!IsSupporter)
            //{
            //    if (familyName.Length > USERID_LENGTH)
            //        result = familyName.Substring(0, USERID_LENGTH).ToUpper();
            //    else
            //        result = familyName.ToUpper();
            //}
            //else
            //{
            //    if (familyName.Length > USERID_LENGTH - 2)
            //        result = "A_" + familyName.Substring(0, USERID_LENGTH - 2).ToUpper();
            //    else
            //        result = "A_" + familyName.ToUpper();
            //}

            PersonManager persManager = new PersonManager();
            bool hasExistingUsername = HasExistingUsername(result);
            if (!hasExistingUsername)
            {
                return result;
            }
            else
            {
                //Erstes, Zweites, Drittes, Zeichen vom Vorname hinzufügen und in DB nachsehen ob es diesen schon gibt
                int index = 0;
                int lenFirstName = firstName.Length;
                while (index < lenFirstName)
                {
                    string dummyResult = result;
                    if (dummyResult.Length < USERID_LENGTH)
                        dummyResult = dummyResult + firstName.Substring(index, 1).ToUpper();
                    else
                        dummyResult = dummyResult.Substring(0, USERID_LENGTH - 1) + firstName.Substring(index, 1).ToUpper();

                    index += 1;
                    hasExistingUsername = HasExistingUsername(dummyResult);
                    if (!hasExistingUsername)
                    {
                        return dummyResult.ToUpper();
                    }
                }

                //Alternative: Hinter den Nachnamen hochzählen
                int i = 1;
                while (i <= 9)
                {
                    string dummyResult = result;
                    if (dummyResult.Length < USERID_LENGTH)
                        dummyResult = dummyResult + i.ToString();
                    else
                        dummyResult = dummyResult.Substring(0, USERID_LENGTH - 1) + i.ToString();

                    i += 1;
                    hasExistingUsername = HasExistingUsername(dummyResult);
                    if (!hasExistingUsername)
                    {
                        return dummyResult.ToUpper();
                    }
                }
            }

            return result.ToUpper();
        }

        #region @@Query
        [Obsolete]
        /// <summary>
        /// @@ func to get name of usertype
        /// </summary>
        /// <param name="userType"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        public string GetUserTypeName(string userGuid, string userType)
        {
            if (string.IsNullOrEmpty(userType))
            {
                return "";
            }
            try
            {
                int userTypeInt = Convert.ToInt32(userType);
                if (Enum.IsDefined(typeof(EnumUserType), userTypeInt))
                {
                    EnumUserType enumUserType = (EnumUserType)userTypeInt;
                    return enumUserType.ToString();
                }
                else
                    throw new Exception();
            }
            catch (Exception ex)
            {
                return "UT_" + userType;
            }
            
        }
        #endregion
    }
}
