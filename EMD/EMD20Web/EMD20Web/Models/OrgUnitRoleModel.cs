using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class OrgUnitRoleModel : BaseModel
    {
        [ScaffoldColumn(true)]
        public string Guid { get; set; }
        [Editable(false)]
        public string HistoryGuid { get; set; }
        [UIHint("Date")]

        public System.DateTime? Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }

        [Display(Name = "Orgunit")]
        public string O_Guid { get; set; }
        [Display(Name = "Role")]
        public string R_Guid { get; set; }
        [Display(Name = "Employment")]
        public string EP_Guid { get; set; }
        public int OR_ID { get; set; }
        public int EP_ID { get; set; }
        public int O_ID { get; set; }
        public int R_ID { get; set; }
        [Display(Name = "Orgunit Name")]
        public string O_Name { get; set; }
        [Display(Name = "Role")]
        public string R_Name { get; set; }
        [Display(Name = "PersNr")]
        public string EMPL_PersNr { get; set; }
        [Display(Name = "Person Name")]
        public string PERS_Name { get; set; }

        public bool CanDelete
        {
            get
            {
                if (R_ID != 10100)
                {
                    return true;
                }
                return false;
            }
        }

        public string EmploymentPersonalId { get; set; }
        public string OrgUnitName { get; set; }
        public string PersonName { get; set; }
        public string RoleName { get; set; }
        public string EmploymentTypeName { get; set; }

        //public IEnumerable<RoleModel> availableRoles { get; set; }
        //public IEnumerable<OrgUnitModel> availableOrgUnits { get; set; }

        public IEnumerable<TextValueModel> availableRoles { get; set; }
        public IEnumerable<TextValueModel> availableOrgUnits { get; set; }

        public IEnumerable<TextValueModel> availableEmployments { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.OrgUnitRoleManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.OrgUnitRoleManager_View; } }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanView = true; //Everyone can view Orgunit-Roles
        }

        public static OrgUnitRoleModel Initialize(OrgUnitRole our)
        {
            OrgUnitRoleModel ourmo = new OrgUnitRoleModel();
            ReflectionHelper.CopyProperties(ref our, ref ourmo);
            return ourmo;
        }

        public static OrgUnitRoleModel Initialize(EMDOrgUnitRole emdOrgunitRole)
        {
            OrgUnitRoleModel orgUnitRoleModel = new OrgUnitRoleModel()
            {
                Guid = emdOrgunitRole.Guid,
                ActiveFrom = emdOrgunitRole.ActiveFrom,
                ActiveTo = emdOrgunitRole.ActiveTo,
                ValidFrom = emdOrgunitRole.ValidFrom,
                ValidTo = emdOrgunitRole.ValidTo,
                EP_Guid = emdOrgunitRole.EP_Guid,
                EP_ID = emdOrgunitRole.EP_ID,
                OR_ID = emdOrgunitRole.OR_ID,
                O_Guid = emdOrgunitRole.O_Guid,
                O_ID = emdOrgunitRole.O_ID,
                R_Guid = emdOrgunitRole.R_Guid,
                R_ID = emdOrgunitRole.R_ID
            };



            //ReflectionHelper.CopyProperties(ref our, ref ourmo);
            return orgUnitRoleModel;
        }

        public static OrgUnitRoleModel Initialize(EMDOrgUnitRoleEnhanced emdOrgunitRole)
        {
            OrgUnitRoleModel orgUnitRoleModel = new OrgUnitRoleModel()
            {
                Guid = emdOrgunitRole.Guid,
                ActiveFrom = emdOrgunitRole.ActiveFrom,
                ActiveTo = emdOrgunitRole.ActiveTo,
                ValidFrom = emdOrgunitRole.ValidFrom,
                ValidTo = emdOrgunitRole.ValidTo,
                EP_Guid = emdOrgunitRole.EP_Guid,
                EP_ID = emdOrgunitRole.EP_ID,
                OR_ID = emdOrgunitRole.OR_ID,
                O_Guid = emdOrgunitRole.O_Guid,
                O_ID = emdOrgunitRole.O_ID,
                R_Guid = emdOrgunitRole.R_Guid,
                R_ID = emdOrgunitRole.R_ID,
                O_Name = emdOrgunitRole.OrgUnitName,
                R_Name = emdOrgunitRole.RoleName,
                EMPL_PersNr = emdOrgunitRole.EmploymentPersonalId,
                PERS_Name = emdOrgunitRole.PersonName,
                EmploymentTypeName = emdOrgunitRole.EmploymentTypeName
            };



            //ReflectionHelper.CopyProperties(ref our, ref ourmo);
            return orgUnitRoleModel;
        }

        public bool IsAllowedObject(string userId, string orguGuid, bool isEdit = false)
        {
            //Same as for OrgUnit => as the enterprise maps to the OrgUnit 
            SecurityUser secUser = SecurityUser.NewSecurityUser(userId);
            return secUser.IsAllowedOrgUnit(orguGuid, isEdit);
        }
    }
}