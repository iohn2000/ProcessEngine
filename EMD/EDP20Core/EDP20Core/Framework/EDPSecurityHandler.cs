using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.DB;

namespace Kapsch.IS.EDP.Core.Framework
{
    public class EDPSecurityHandler
    {
        public const string SECURITYFORM_EMD100 = "EMD100";
        public const string SECURITYFORM_EMD101 = "EMD101";
        public const string SECURITYFORM_EMD102 = "EMD102";
        public const string SECURITYFORM_EMD103 = "EMD103";
        public const string SECURITYFORM_EMD104 = "EMD104";
        public const string SECURITYFORM_EMD105 = "EMD105";
        public const string SECURITYFORM_EMD106 = "EMD106";
        public const string SECURITYFORM_EMD110 = "EMD110";
        public const string SECURITYFORM_EMD111 = "EMD111";
        public const string SECURITYFORM_EMD112 = "EMD112";
        public const string SECURITYFORM_ADVANCEDSEARCH = "ADVANCEDSEARCH";
        public const string SECURITYFORM_OFFBOARDING = "OFFBOARDING";
        public const string SECURITYFORM_PICTUREMANAGER = "PICTUREMANAGER";
        public const string SECURITYFORM_COSTCENTERMANAGEMENT = "COSTCENTERMANAGEMENT";
        public const string SECURITYFORM_ORGUNITMANAGEMENT = "ORGUNITMANAGEMENT";
        public const string SECURITYFORM_MAINEMPLOYMENTMANAGER = "MAINEMPLOYMENTMANAGER";
        public const string SECURITYFORM_ADMINEMD = "ADMINEMD";
        public const string SECURITYFORM_CONTACTMANAGER = "CONTACTMANAGER";
        public const string SECURITYFORM_CHANGE = "CHANGE";
        public const string SECURITYFORM_ONBOARDING = "ONBOARDING";
        public const string SECURITYFORM_ADMINORGUNITMANAGEMENT = "ADMINORGUNITMANAGEMENT";

        /// <summary>
        /// check if userid has specific SecurityRole
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool CheckForSecurityRole(String userID) {
            return true;
        }

        /// <summary>
        /// check if user has specific permission in Access-Table
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool CheckForAccessPermission(string permission, string userId, string e_guid = "")
        {
            AccessHandler ah = new AccessHandler();
            string where = "UserId = \"" + userId + "\" && Form = \"" + permission + "\"";
            if (e_guid != "")
                where += " && E_Guid= \"" + e_guid + "\"";


            List<IEMDObject<EMDAccess>> listAccesses =  ah.GetObjects<EMDAccess, Access>(where);
            if (listAccesses.Count > 0)
                return true;
            else
            {
                return false;
            }
        }

        /// <summary>
        /// check for specific entity permission in security tables
        /// (Future USE)
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="action"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool CheckForEntityPermission(String prefix, String action, String userid)
        {
            return true;
        }

    }
}
