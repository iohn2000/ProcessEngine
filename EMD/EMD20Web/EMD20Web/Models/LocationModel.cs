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
    public class LocationModel : BaseModel
    {
        [ScaffoldColumn(true)]
        public string Guid { get; set; }
        [Editable(false)]
        public string HistoryGuid { get; set; }

        public System.DateTime? Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }

        [Required, Display(Name = "Country")]
        public string CTY_Guid { get; set; }
        [UIHint("String"), Display(Name = "Object")]
        public Nullable<int> EL_ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Street { get; set; }
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public EMDCountry Country { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.LocationManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.LocationManager_View; } }

        public string CountryDisplayName
        {
            get
            {
                if (Country == null && CTY_Guid != null)
                {
                    Country = Manager.CountryManager.Get(CTY_Guid);
                }

                if (Country != null)
                {
                    return Country.Name;
                }
                return null;
            }
        }

        public LocationModel()
        {

        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanView = true; //Everyone can see Locations
        }

        //public static LocationModel copyFromDBObject(EDP.Core.DB.Location loca)
        public static LocationModel Initialize(EDP.Core.DB.Location loca)
        {
            LocationModel locaModel = new LocationModel();
            ReflectionHelper.CopyProperties<EDP.Core.DB.Location, LocationModel>(ref loca, ref locaModel);
            return locaModel;
        }

        //public static LocationModel copyFromObject(EMDLocation loca)
        public static LocationModel Initialize(EMDLocation loca)
        {
            LocationModel locaModel = new LocationModel();
            ReflectionHelper.CopyProperties<EMDLocation, LocationModel>(ref loca, ref locaModel);
            return locaModel;
        }

    }
}