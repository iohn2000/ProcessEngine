using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class ManagePackageEquipmentModel
    {
        public string PackageGuid { get; set; }

        public string PackageName { get; set; }

        public IList<TextValueModel> AvailableEquipments { get; set; }

        public IList<TextValueModel> ConfiguredEquipments { get; set; }

        public ManagePackageEquipmentModel()
        {
            AvailableEquipments = new List<TextValueModel>();
            ConfiguredEquipments = new List<TextValueModel>();
        }
    }
}