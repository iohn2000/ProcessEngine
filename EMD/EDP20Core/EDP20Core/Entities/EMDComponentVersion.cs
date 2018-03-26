using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDComponentVersion : EMDObject<EMDComponentVersion>
    {
        public int CV_ID { get; set; }
        public string Component { get; set; }
        public string VersionNr { get; set; }
        public Nullable<int> BuildNr { get; set; }
        public string HotfixNr { get; set; }

        public override String Prefix { get { return "VERS"; } }
        
        public EMDComponentVersion(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDComponentVersion()
        { }
    }
}
