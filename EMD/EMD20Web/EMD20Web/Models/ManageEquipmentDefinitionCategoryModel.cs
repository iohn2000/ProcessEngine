using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class ManageEquipmentDefinitionCategoryModel
    {
        public string EquipmentDefinitionGuid { get; set; }

        public string EquipmentDefinitionName { get; set; }

        public IList<TextValueModel> AvailableCategories { get; set; }

        public IList<TextValueModel> ConfiguredCategories { get; set; }

        public ManageEquipmentDefinitionCategoryModel()
        {
            AvailableCategories = new List<TextValueModel>();
            ConfiguredCategories = new List<TextValueModel>();
        }
    }
}