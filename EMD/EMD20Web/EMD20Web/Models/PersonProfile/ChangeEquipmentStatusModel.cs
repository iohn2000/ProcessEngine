using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models.PersonProfile
{
    public class ChangeEquipmentStatusModel : BaseModel
    {


        public string Obre_Guid { get; set; }

        public string Status { get; set; }

        public List<TextValueModel> AvailableStatus { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }
}