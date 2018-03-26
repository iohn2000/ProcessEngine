using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class ManageEquipmentDefinitionOwnerModel
    {
        public string EquipmentDefinitionGuid { get; set; }

        public string EquipmentDefinitionName { get; set; }

        public IList<TextValueModel> AvailableOwners { get; set; }

        public IList<TextValueModel> ConfiguredOwners { get; set; }

        public SelectionAddModel AvailableOwnersSelection { get; set; }

        public ManageEquipmentDefinitionOwnerModel()
        {
            AvailableOwners = new List<TextValueModel>();
            ConfiguredOwners = new List<TextValueModel>();
            AvailableOwnersSelection = new SelectionAddModel();
        }
    }
}