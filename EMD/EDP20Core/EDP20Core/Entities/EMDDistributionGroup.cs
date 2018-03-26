using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDDistributionGroup : EMDObject<EMDDistributionGroup>
    {
        public int DGT_ID { get; set; }
        public string Name { get; set; }
        public override String Prefix { get { return "DIST"; } }
        
        public EMDDistributionGroup(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDDistributionGroup()
        { }
    }
}
