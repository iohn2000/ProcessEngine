using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class OffboardingModel
    {
        public string EmploymentGuid { get; set; }

        public List<EquipmentInstanceModel> EquipmentInstanceModels { get; set; }

        [Required, Display(Name = "Until Date")]
        public DateTime? ExitDate { get; set; }

        [Required, Display(Name = "Last Day")]
        public DateTime? LastDay { get; set; }

        [Display(Name = "Navision Number")]
        public string ResourceNumber { get; set; }


        public OffboardingModel()
        {

        }
    }
}