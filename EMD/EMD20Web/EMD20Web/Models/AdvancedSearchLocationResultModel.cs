using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AdvancedSearchLocationResultModel : BaseModel
    {
        public string Guid { get; set; }
        public string CTY_Guid { get; set; }
        public Nullable<int> EL_ID { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public override String CanManagePermissionString { get { return SecurityPermission.LocationManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.LocationManager_View; } }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }
}