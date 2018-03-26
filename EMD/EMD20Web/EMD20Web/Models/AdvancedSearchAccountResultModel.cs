using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AdvancedSearchAccountResultModel : BaseModel
    {
        public string Guid { get; set; }
        public string E_Guid { get; set; }
        public string KstID { get; set; }
        public string Name { get; set; }
        public Nullable<int> MainOrgUnit { get; set; }
        public string Responsible { get; set; }
        [Display(Name = "Responsible Name")]
        public string ResponsibleName { get; set; }
        public Nullable<int> Responsible_EP_ID { get; set; }
        
        public override String CanManagePermissionString { get { return SecurityPermission.CostCenterManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.CostCenterManager_View; } }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }
}