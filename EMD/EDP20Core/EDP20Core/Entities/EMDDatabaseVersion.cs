using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDDatabaseVersion : EMDObject<EMDDatabaseVersion>
    {
        public int ID { get; set; }
        public string CoreVersion { get; set; }
        public System.DateTime VersionDate { get; set; }
        public string Changes { get; set; }
        public int? OrderNumber { get; set; }
        public bool HasErrors { get; set; }
        public override String Prefix { get { return "DAVE"; } }


        public EMDDatabaseVersion(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDDatabaseVersion()
        { }
    }
}
