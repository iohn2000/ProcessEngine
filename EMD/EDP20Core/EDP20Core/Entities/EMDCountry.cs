using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDCountry : EMDObject<EMDCountry>
    {
        public int L_SC_Country { get; set; }
        public int Country1 { get; set; }
        public string Code_A2 { get; set; }
        public string ISO3166_A2 { get; set; }
        public string ISO3166_A3 { get; set; }
        public string ISO3166_N3 { get; set; }
        public string UN_RoadCode { get; set; }
        public string Name { get; set; }
        public string PhoneCode { get; set; }
        public bool EU { get; set; }

        public override String Prefix { get { return "CTRY"; } }
        
        public EMDCountry(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDCountry()
        { }
    }
}
