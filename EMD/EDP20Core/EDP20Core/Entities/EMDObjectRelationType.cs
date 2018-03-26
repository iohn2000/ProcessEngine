using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDObjectRelationType : EMDObject<EMDObjectRelationType>
    {
        public string RelationName { get; set; }
        public string Object1 { get; set; }
        public string Object2 { get; set; }        

        public override String Prefix { get { return "ORTY"; } }
        
        public EMDObjectRelationType(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDObjectRelationType()
        { }
    }
}
