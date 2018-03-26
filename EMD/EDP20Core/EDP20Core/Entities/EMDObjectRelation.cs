using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDObjectRelation : EMDObject<EMDObjectRelation>
    {
        public string Object1 { get; set; }
        public string Object2 { get; set; }
        public string Data { get; set; }
        public string FromTemplateGuid { get; set; }
        public byte Status { get; set; }
        public string ORTYGuid { get; set; }

        public Nullable<System.DateTime> NextValidityCheckDate { get; set; }

        public override String Prefix { get { return "OBRE"; } }

        public EMDObjectRelation(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDObjectRelation()
        { }
    }
}
