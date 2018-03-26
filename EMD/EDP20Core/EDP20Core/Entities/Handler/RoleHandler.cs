using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class RoleHandler : EMDObjectHandler
    {
        private List<EMDRole> tmpRoleList;
        private int levelsDown = -1;

        public RoleHandler()
            : base(MethodBase.GetCurrentMethod().DeclaringType)
        {
        }

        public RoleHandler(CoreTransaction transaction)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction)
        {
        }

        public RoleHandler(string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, guid_ModifiedBy, modifyComment)
        {
        }

        public RoleHandler(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(MethodBase.GetCurrentMethod().DeclaringType, transaction, guid_ModifiedBy, modifyComment)
        {
        }

        public override Type GetDBObjectType()
        {
            return new Role().GetType();
        }

        internal override IEMDObject<T> CreateDataFromDBObject<T>(object dbObject)
        {
            if (dbObject == null) return null;
            Role role = (Role)dbObject;
            EMDRole emdObject = new EMDRole(role.Guid, role.Created, role.Modified);
            ReflectionHelper.CopyProperties(ref dbObject, ref emdObject);
            emdObject.SetValidityStatus();
            return (IEMDObject<T>)emdObject;
        }

        public const int DISCIPLINALROLES = 10000;
        public const int PERSON = 10100;
        public const int TEAMLEADER = 10400;
        public const int LINEMANAGER = 10500;
        public const int SECURITYROLE_EMD_ROLES = 1000;
        public const int SECURITYROLE_COMPANY_PRIME = 1100;
        public const int SECURITYROLE_READ_GENERAL_PRIME = 1001;
        public const int SECURITYROLE_RESPONSIBLE = 1200;
        public const int SECURITYROLE_HELPDESK = 1300;
        public const int SECURITYROLE_ADVANCEDSEARCH = 1400;
        public const int SECURITYROLE_PICTUREMANAGER = 1500;
        public const int SECURITYROLE_ORGUNITMANAGER = 1600;
        public const int SECURITYROLE_MAINEMPLOYMENTMANAGER = 1700;
        public const int SECURITYROLE_CONTACTMANAGER = 1800;
        public const int SECURITYROLE_USERIDMANAGER = 1900;

        /// <summary>
        /// return a Role with given (old)Role ID
        /// </summary>
        /// <param name="Role_Id">the role id</param>
        /// <returns>EMDRole Object or null</returns>
        public EMDRole GetRoleById(int Role_Id)
        {
            List<IEMDObject<EMDRole>> roles = GetObjects<EMDRole, Role>("R_ID = " + Role_Id);

            if (roles.Count == 0)
                return null;
            else if (roles.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, string.Format("Consistencyproblem: more than one role found for R_ID {0}", Role_Id));
            }
            else
            {
                return (EMDRole)roles.First();
            }
        }



        public List<EMDRole> GetAllRootRoles()
        {
            this.tmpRoleList = new List<EMDRole>();

            try
            {
                return GetObjects<EMDRole, Role>("Guid_Parent = Guid && Guid_Root = Guid").Cast<EMDRole>().ToList();
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error trying read enterprise tree.", ex);
            }

        }

        public List<EMDRole> GetAllSubRolesFromParent(string roleParentGuid, int levelsDown = -1)
        {
            this.tmpRoleList = new List<EMDRole>();
            this.levelsDown = levelsDown;
            try
            {
                this.getAllSubRoles(roleParentGuid);
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error trying read enterprise tree.", ex);
            }
            return this.tmpRoleList;
        }

        private void getAllSubRoles(string roleParentGuid, int currentLevel = 0)
        {
            if ((this.levelsDown > -1) && (currentLevel > this.levelsDown))
                return;

            // add enterprise
            this.tmpRoleList.Add((EMDRole)GetObject<EMDRole>(roleParentGuid));

            // get all children
            List<IEMDObject<EMDRole>> roleChildList = GetObjects<EMDRole, Role>("Guid_Parent = \"" + roleParentGuid + "\"");
            if (roleChildList.Count > 0)
            {
                currentLevel++;
                foreach (var child in roleChildList)
                {
                    ((EMDRole)child).Level = currentLevel;
                    this.getAllSubRoles(child.Guid, currentLevel);
                }
            }
        }
  
    }
}