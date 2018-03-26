using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDAccountGroup : EMDObject<EMDAccountGroup>
    {
        public string AC_Guid { get; set; }
        public string G_Guid { get; set; }
        public int ACG_ID { get; set; }
        public int AC_ID { get; set; }
        public int G_ID { get; set; }
        public string Key { get; set; }

        public override String Prefix { get { return "ACGR"; } }
        
        public EMDAccountGroup(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDAccountGroup()
        { }
    }
}
