using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class OldRoleModel
    {
        [ScaffoldColumn(false)]
        public string Id { get; set; }

        [Required()]
        public string Name { get; set; }

        [Required()]
        public string Description { get; set; }

        [Required()]
        public int CompanyId { get; set; }

        [Required()]
        public int LocationId { get; set; }

        [Required()]
        public string Flags { get; set; }

        [Required()]
        public string Color { get; set; }

        public OldRoleModel(string id, string name, string description, int companyId, int locationId, string flags, string color)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.CompanyId = companyId;
            this.LocationId = locationId;
            this.Flags = flags;
            this.Color = color;
        }

        public OldRoleModel()
        {
            this.Id = String.Empty;
            this.Name = String.Empty;
            this.Description = String.Empty;
            this.CompanyId = 0;
            this.LocationId = 0;
            this.Flags = String.Empty;
            this.Color = String.Empty;
        }
    }
}