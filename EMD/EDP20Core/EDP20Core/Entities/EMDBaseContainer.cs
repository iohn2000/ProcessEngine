using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDBaseContainer : EMDObject<EMDBaseContainer>
    {
        public string OBCOGuid { get; set; }
        public string BACOPrefix { get; set; }
        public override String Prefix { get { return "BACO"; } }
        
        public EMDBaseContainer(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDBaseContainer()
        { }
    }
}
