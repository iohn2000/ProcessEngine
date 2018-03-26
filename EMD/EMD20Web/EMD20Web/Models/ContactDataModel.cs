using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class ContactDataModel : BaseModel
    {
        public string GuidEmployment { get; set; }


        public override String CanManagePermissionString { get { return SecurityPermission.ContactManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.ContactManager_View; } }



        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            if (!string.IsNullOrWhiteSpace(GuidEmployment))
            {
                this.CanManage = this.CanManage && securityUser.IsAllowedEmployment(securityUser.UserId, GuidEmployment);
            }
            
        }
    }
}