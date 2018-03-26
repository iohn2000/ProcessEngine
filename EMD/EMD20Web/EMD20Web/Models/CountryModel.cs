using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

using Kapsch.IS.EDP.Core;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class CountryModel : BaseModel
    {
        [ScaffoldColumn(true)]
        public string Guid { get; set; }
        [Editable(false)]
        public string HistoryGuid { get; set; }

        public System.DateTime? Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }

        public int L_SC_Country { get; set; }
        [Required, Display(Name = "Code A2")]
        public string Code_A2 { get; set; }
        [Required, Display(Name = "ISO 3166 A2")]
        public string ISO3166_A2 { get; set; }
        [Required, Display(Name = "ISO 3166 A3")]
        public string ISO3166_A3 { get; set; }
        [Required, Display(Name = "ISO 3266 N3")]
        public string ISO3166_N3 { get; set; }
        [Required, Display(Name = "UN-Road-Code")]
        public string UN_RoadCode { get; set; }
        [Required, Display(Name = "Name")]
        public string Name { get; set; }
        [Required, Display(Name = "Phone-Code")]
        public string PhoneCode { get; set; }
        [Display(Name = "European Union")]
        public bool EU { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.CountryManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.CountryManager_View; } }

        public CountryModel()
        {
            this.Created = DateTime.Now;
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static CountryModel Initialize(Country cty)
        {
            CountryModel ctyModel = new CountryModel();
            ReflectionHelper.CopyProperties<Country, CountryModel>(ref cty, ref ctyModel);
            return ctyModel;
        }

        public static CountryModel Initialize(EMDCountry cty)
        {
            CountryModel ctyModel = new CountryModel();
            ReflectionHelper.CopyProperties<EMDCountry, CountryModel>(ref cty, ref ctyModel);
            return ctyModel;
        }
    }
}