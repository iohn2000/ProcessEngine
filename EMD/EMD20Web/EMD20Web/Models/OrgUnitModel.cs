using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using System.Text;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class OrgUnitModelList : BaseModel
    {
        public List<OrgUnitModel> OrgUnitModels { get; set; }

        public OrgUnitModel ParentOrgUnit { get; set; }

        public OrgUnitModel CurrentOrgUnit { get; set; }

        public Int32 ParentOrgUnitLevel { get; set; }

        public Int32 CurrentOrgUnitLevel { get; set; }

        public Boolean HasParent { get; set; }

        public OrgUnitModelList()
        {
            OrgUnitModels = new List<OrgUnitModel>();
            HasParent = true;
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }

    public class OrgUnitModel : BaseModel
    {
        public OrgUnitModel Parent { get; set; }
        public List<OrgUnitModel> Children { get; set; }

        //public bool HasChildren
        //{
        //    get
        //    {
        //        return Children?.Count > 0;
        //    }
        //}

        public bool HasChildren { get; set; }

        public string Guid { get; set; }


        public int CountAssignedPersons { get; set; }

        public string HistoryGuid { get; set; }

        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        [Display(Name = "Parent")]
        public string Guid_Parent { get; set; }

        public SelectionViewModel ParentSelection { get; set; }


        [Display(Name = "Root")]
        public string Guid_Root { get; set; }
        [Display(Name = "Enterprise")]
        public string E_Guid { get; set; }

        public SelectionViewModel EnterpriseSelection { get; set; }


        [Display(Name = "Enterprise")]
        public string E_ShortName { get; set; }
        public int O_ID { get; set; }
        public int ID_Parent { get; set; }
        public int ID_Root { get; set; }
        [Required]
        public string Name { get; set; }

        [Display(Name = "Name")]
        public string NameLevel
        {
            get
            {
                StringBuilder t = new StringBuilder();

                for (int i = 0; i < Level; i++)
                {
                    t.Append("-");
                }

                t.Append(Name);

                return t.ToString();
            }
        }
        public string Note { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string Key3 { get; set; }
        public bool IsSecurity { get; set; }


        [Display(Name = "Root")]
        public string RootName { get; set; }
        [Display(Name = "Parent")]
        public string ParentName { get; set; }

        public int Level { get; set; }


        public OrgUnitModelList orgUnitModelList { get; set; }
        public override String CanManagePermissionString { get { return SecurityPermission.OrgUnitManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.OrgUnitManager_View; } }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanView = true; //Everyone can view orgunits
            //SecurityUser secUser = SecurityUser.NewSecurityUser(userName);
            //this.CanManage = secUser.IsAllowedOrgUnit(this.Guid, true);
            //this.CanManage = true;
        }

        public static OrgUnitModel Initialize(OrgUnit ou)
        {
            OrgUnitModel oumo = new OrgUnitModel();
            oumo.ActiveFrom = ou.ActiveFrom;
            oumo.ActiveTo = ou.ActiveTo;
            oumo.Created = ou.Created;
            oumo.E_Guid = ou.E_Guid;
            oumo.Guid = ou.Guid;
            oumo.Guid_Parent = ou.Guid_Parent;
            oumo.Guid_Root = ou.Guid_Root;
            oumo.HistoryGuid = ou.HistoryGuid;
            oumo.ID_Parent = ou.ID_Parent;
            oumo.ID_Root = ou.ID_Root;
            oumo.IsSecurity = ou.IsSecurity;
            oumo.Key1 = ou.Key1;
            oumo.Key2 = ou.Key2;
            oumo.Key3 = ou.Key3;
            oumo.Modified = ou.Modified;
            oumo.Name = ou.Name;
            oumo.Note = ou.Note;
            oumo.O_ID = ou.O_ID;
            oumo.ValidFrom = ou.ValidFrom;
            oumo.ValidTo = ou.ValidTo;
            //ReflectionHelper.CopyProperties<OrgUnit,OrgUnitModel>(ref ou, ref oumo);
            return oumo;
        }

        public static OrgUnitModel Initialize(EMDOrgUnit ou)
        {
            OrgUnitModel oumo = new OrgUnitModel();
            oumo.ActiveFrom = ou.ActiveFrom;
            oumo.ActiveTo = ou.ActiveTo;
            oumo.Created = ou.Created;
            oumo.E_Guid = ou.E_Guid;
            oumo.Guid = ou.Guid;
            oumo.Guid_Parent = ou.Guid_Parent;
            oumo.Guid_Root = ou.Guid_Root;
            oumo.HistoryGuid = ou.HistoryGuid;
            oumo.ID_Parent = ou.ID_Parent;
            oumo.ID_Root = ou.ID_Root;
            oumo.IsSecurity = ou.IsSecurity;
            oumo.Key1 = ou.Key1;
            oumo.Key2 = ou.Key2;
            oumo.Key3 = ou.Key3;
            oumo.Modified = ou.Modified;
            oumo.Name = ou.Name;
            oumo.Note = ou.Note;
            oumo.O_ID = ou.O_ID;
            oumo.ValidFrom = ou.ValidFrom;
            oumo.ValidTo = ou.ValidTo;
            oumo.Level = ou.Level;
            //ReflectionHelper.CopyProperties<EMDOrgUnit,OrgUnitModel>(ref ou, ref oumo);
            return oumo;
        }

        public static EMDOrgUnit Update(EMDOrgUnit emdOrgUnit, OrgUnitModel orgUnitModel)
        {
            emdOrgUnit.E_Guid = orgUnitModel.E_Guid;
            emdOrgUnit.Guid_Parent = orgUnitModel.Guid_Parent;
            emdOrgUnit.Guid_Root = orgUnitModel.Guid_Root;
            emdOrgUnit.ID_Parent = orgUnitModel.ID_Parent;
            emdOrgUnit.ID_Root = orgUnitModel.ID_Root;
            emdOrgUnit.Name = orgUnitModel.Name;
            emdOrgUnit.IsSecurity = orgUnitModel.IsSecurity;
            emdOrgUnit.Note = orgUnitModel.Note;
            emdOrgUnit.O_ID = orgUnitModel.O_ID;
            return emdOrgUnit;
        }

        public OrgUnitModel()
        {
            this.Created = DateTime.Now;
            this.EnterpriseSelection = new SelectionViewModel();
            this.ParentSelection = new SelectionViewModel();
        }

        public bool IsAllowedObject(string userId, string guid, bool isEdit = false)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(userId);
            return secUser.IsAllowedOrgUnit(guid, isEdit);
        }



        public static OrgUnitModel Find(OrgUnitModel orgunitModel, string guid)
        {
            return FindOrgunit(orgunitModel, guid);
        }

        public static OrgUnitModel FindOrgunit(OrgUnitModel orgunitModel, string guid)
        {
            if (orgunitModel.Guid == guid)
            {
                return orgunitModel;
            }

            if (orgunitModel.Parent == null)
            {
                return null;
            }

            return FindOrgunit(orgunitModel.Parent, guid);
        }

        public static string GetExtendedName(EMDOrgUnit orgunit, EMDEnterprise enterprise)
        {
            if (orgunit == null)
            {
                return string.Empty;
            }

            if (enterprise != null && !string.IsNullOrWhiteSpace(enterprise.NameShort))
            {
                return string.Format("{0} >> {1}", orgunit.Name, enterprise?.NameShort);
            }



            return orgunit.Name;
        }

    }
}