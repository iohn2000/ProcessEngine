using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDGroup : EMDObject<EMDGroup>
    {
        public string E_Guid { get; set; }
        public int G_ID { get; set; }
        public int E_ID { get; set; }
        public string Name { get; set; }

        public override String Prefix { get { return "GROU"; } }
        
        public EMDGroup(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDGroup()
        { }
    }
}
