using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDLocation : EMDObject<EMDLocation>
    {
        public string CTY_Guid { get; set; }
        public Nullable<int> EL_ID { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Region { get; set; }

        public override String Prefix { get { return "LOCA"; } }
        
        public EMDLocation(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDLocation()
        { }

    }
}
