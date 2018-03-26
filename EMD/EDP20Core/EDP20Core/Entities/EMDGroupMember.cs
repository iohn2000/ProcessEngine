using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDGroupMember : EMDObject<EMDGroupMember>
    {
        public string G_Guid { get; set; }
        public string EP_Guid { get; set; }
        public int GM_ID { get; set; }
        public int G_ID { get; set; }
        public int EP_ID { get; set; }

        public override String Prefix { get { return "GRME"; } }
        
        public EMDGroupMember(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDGroupMember()
        { }
    }    
}
