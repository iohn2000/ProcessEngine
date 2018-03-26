using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using System.Web.Mvc;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class PackageModel : BaseModel
    {
        //[ScaffoldColumn(true), Key]
        public string Guid { get; set; }
        [Required(), Display(Name = "Name")]
        public string Name { get; set; }
        [Required(), Display(Name = "Description")]
        public string Description { get; set; }

        public String EMPL_Guid { get; set; }
        [Display(Name = "Package_Guid")]
        public String Package_Guid { get; set; }

        #region FilterRule
        public RuleFilterModel RuleFilterModel { get; set; }

        #endregion


        [Display(Name = "Flags")]
        public List<String> Flags { get; set; }

        [Display(Name = "Flags")]
        public String FlagsStr { get; set; }

        [Display(Name = "Base Package Location")]
        public bool Access { get; set; }

        [Display(Name = "Base Package Enterprise")]
        public bool Base { get; set; }

        public String Rule { get; set; }

        [Display(Name = "Status")]
        public String PackageStatusShort { get; set; }

        public String PackageStatusLong { get; set; }

        public int PackageStatusInt { get; set; }
        public override String CanManagePermissionString { get { return SecurityPermission.PackageManagement_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.PackageManagement_View; } }

        //public bool CanAddEquipment { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            //if (this.CanManage)
            //    this.CanAddEquipment = true;

            //SecurityUser secUser = SecurityUser.NewSecurityUser(userName);

            //if (secUser.IsItSelf(this.EMPL_Guid))
            //    this.CanAddEquipment = true;
        }
        public PackageModel()
        {
            RuleFilterModel = new RuleFilterModel();
        }

        public PackageModel(EMDObjectContainer objcont)
        {
            RuleFilterModel = new RuleFilterModel();

            this.Guid = objcont.Guid;
            this.Name = objcont.Name;
            this.Description = objcont.Description;
            this.Rule = String.Empty;

            this.ActiveFrom = objcont.ActiveFrom;
            this.ActiveTo = objcont.ActiveTo;
            this.ValidFrom = objcont.ValidFrom;
            this.ValidTo = objcont.ValidTo;
            //this.FilterGuid = objcont.FilterGuid;            
        }
    }
}