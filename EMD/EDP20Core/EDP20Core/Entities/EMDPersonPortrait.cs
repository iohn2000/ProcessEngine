using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDPersonPortrait : EMDObject<EMDPersonPortrait>
    {
        public string P_Guid { get; set; }
        public int PP_ID { get; set; }
        public string GUI_Hash { get; set; }
        public string AD_Hash { get; set; }
        public int P_ID { get; set; }

        public override String Prefix { get { return "PEPO"; } }

        public EMDPersonPortrait(string guid, DateTime created, DateTime? modified) 
            : base(guid, created, modified)
        { }

        public EMDPersonPortrait()
        { }
    }
}
