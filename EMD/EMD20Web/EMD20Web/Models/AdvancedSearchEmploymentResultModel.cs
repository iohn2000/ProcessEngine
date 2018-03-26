using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AdvancedSearchEmploymentResultModel : BaseModel
    {
        [StringLength(255), Required(), Display(Name = "Surname")]
        public string FamilyName { get; set; }

        [StringLength(255), Required()]
        public string FirstName { get; set; }

        public string EMPL_Guid { get; set; }

        public string PERS_Guid { get; set; }

        public string PersNr { get; set; }

        public string UserID { get; set; }

        public int P_ID { get; set; }

        public int EP_ID { get; set; }

        public DateTime? Entry { get; set; }

        public DateTime? FirstWorkDay { get; set; }

        public DateTime? LastDay { get; set; }

        public DateTime? Exit { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.Change; } }
        public override String CanViewPermissionString { get { return SecurityPermission.AdvancedSearch_View_Employment_ViewDetail; } }


        public override void InitializeSecurity(SecurityUser securityUser)
        {

            this.IsAdmin = securityUser.IsAdmin;
            //this.CanView = false;
            //this.CanManage = false;
            //this.CanViewAdvancedSearchList = secUser.hasPermission(SecurityPermission.AdvancedSearch_View_Enterprise_ViewDetail, new SecurityUserParameterFlags(checkPlainPermisson: true));
        }
    }
}