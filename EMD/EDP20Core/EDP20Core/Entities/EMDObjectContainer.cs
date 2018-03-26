using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDObjectContainer : EMDObject<EMDObjectContainer>
    {
        public string ObjectKey { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public override String Prefix { get { return "OBCO"; } }
        
        public EMDObjectContainer(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDObjectContainer()
        { }
    }
}
