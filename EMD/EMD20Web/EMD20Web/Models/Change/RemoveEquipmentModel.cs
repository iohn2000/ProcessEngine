using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models.Change
{
    public class RemoveEquipmentModel : BaseModel
    {
        [Display(Name = "Equipment Name")]
        public string EquipmentName { get; internal set; }

        public string GuidEmployment { get; set; }
        public string GuidObre { get; set; }

        public string GuidEquipmentDefinition { get; set; }

        [Display(Name = "Delete without processes")]
        public bool DoDeleteWithoutProcesses { get; set; }

        [Required, Display(Name = "Target Date")]
        public DateTime? TargetDate { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }
}