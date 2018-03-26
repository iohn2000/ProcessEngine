using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Entities;

namespace Kapsch.IS.EMD.EMD20Web.Models.Location
{
    public class ManageLocationModel : BaseModel
    {
        public override string CanManagePermissionString { get { return SecurityPermission.LocationManager_View_Manage; } }
        public override string CanViewPermissionString { get { return SecurityPermission.LocationManager_View; } }


        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

    }
}