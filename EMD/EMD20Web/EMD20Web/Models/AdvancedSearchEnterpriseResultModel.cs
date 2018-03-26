using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AdvancedSearchEnterpriseResultModel : BaseModel
    {
        public string Guid { get; set; }
        public string NameShort { get; set; }
        public string NameLong { get; set; }
        public int?  O_ID_Dis { get; set; }
        public string Guid_Parent { get; set; }
        public string Guid_Root { get; set; }
        public int E_ID { get; set; }
        public int E_ID_Parent { get; set; }
        public int E_ID_Root { get; set; }
        //public bool CanViewAdvancedSearchList { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.EnterpriseManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.EnterpriseManager_View; } }

        public int E_ID_new { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
         
            //this.CanViewAdvancedSearchList = secUser.hasPermission(SecurityPermission.AdvancedSearch_View_Enterprise_ViewDetail, new SecurityUserParameterFlags(checkPlainPermisson: true));
        }

    }
}