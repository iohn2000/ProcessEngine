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

namespace Kapsch.IS.EMD.EMD20Web.Models
{

    public class RoleModelList : BaseModel
    {
        public List<RoleModel> RoleModels { get; set; }

        public RoleModel ParentRole { get; set; }

        public RoleModel CurrentRole { get; set; }

        public Int32 ParentRoleLevel { get; set; }

        public Int32 CurrentRoleLevel { get; set; }

        public Boolean HasParent { get; set; }

        public RoleModelList()
        {
            RoleModels = new List<RoleModel>();
            HasParent = true;
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }

    //public class RoleViewModel : RoleModel
    //{

    //    public RoleModelList roleModelList { get; set; }

    //    public RoleViewModel()
    //    {
    //        roleModelList = new RoleModelList();
    //    }

    //    public static RoleViewModel copyFromDBObject(Role role)
    //    {
    //        RoleViewModel roleModel = new RoleViewModel();
    //        ReflectionHelper.CopyProperties(ref role, ref roleModel);
    //        return roleModel;
    //    }

    //    public static RoleViewModel copyFromObject(EMDRole role)
    //    {
    //        RoleViewModel roleModel = new RoleViewModel();
    //        ReflectionHelper.CopyProperties(ref role, ref roleModel);
    //        return roleModel;
    //    }
    //}

    public class RoleModel : BaseModel
    {
        [ScaffoldColumn(true)]
        public string Guid { get; set; }
        [Editable(false)]
        public string HistoryGuid { get; set; }

        public System.DateTime? Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }

        [Display(Name = "Parent")]
        public string Guid_Parent { get; set; }
        //[Display(Name = "Root")]
        //public string Guid_Root { get; set; }

        [Display(Name = "Parent")]
        public string Name_Parent { get; set; }
        //[Display(Name = "Root")]
        //public string Name_Root { get; set; }

        [Required(ErrorMessage = "Required Field!"), RegularExpression("^[0-9]*$", ErrorMessage = "Only numeric input is allowed!"), Display(Name = "Role ID")]
        public int R_ID { get; set; }


        [Display(Name = "R_ID_Parent")]
        public int? ID_Parent { get; set; }
        public int? ID_Root { get; set; }
        [Display(Name = "Is Security")]
        public bool IsSecurity { get; set; }
        public Nullable<int> GroupNr { get; set; }
        //[Required(ErrorMessage = "Required Field!")]
        [Required(), Display(Name = "Name")]
        public string Name { get; set; }
        public string URL_Icon { get; set; }
        public Nullable<short> Priority { get; set; }
        public string DescriptionID { get; set; }
        [Display(Name = "Key 1")]
        public string Key1 { get; set; }
        [Display(Name = "Key 2")]
        public string Key2 { get; set; }
        [Display(Name = "Key 3")]
        public string Key3 { get; set; }
        public List<RoleModel> AvailableRoles { get; set; }

        public RoleModelList roleModelList { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.RoleManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.RoleManager_View; } }

        public RoleModel()
        {
            AvailableRoles = new List<RoleModel>();
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static RoleModel Initialize(Role role)
        {
            RoleModel roleMod = new RoleModel();
            ReflectionHelper.CopyProperties(ref role, ref roleMod);
            return roleMod;
        }

        public static RoleModel Initialize(EMDRole role)
        {
            RoleModel roleMod = new RoleModel();
            ReflectionHelper.CopyProperties(ref role, ref roleMod);
            return roleMod;
        }
    }
}