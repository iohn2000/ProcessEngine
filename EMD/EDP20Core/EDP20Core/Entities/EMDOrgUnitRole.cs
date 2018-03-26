using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDOrgUnitRole : EMDObject<EMDOrgUnitRole>
    {
        public string O_Guid { get; set; }
        public string R_Guid { get; set; }
        public string EP_Guid { get; set; }
        public int OR_ID { get; set; }
        public int EP_ID { get; set; }
        public int O_ID { get; set; }
        public int R_ID { get; set; }
        
        public override String Prefix { get { return "OURO"; } }

        public EMDOrgUnitRole(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDOrgUnitRole()
        { }
    }
}
