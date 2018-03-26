using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEmploymentAccount : EMDObject<EMDEmploymentAccount>
    {
        public string EP_Guid { get; set; }
        public string AC_Guid { get; set; }
        public int EPA_ID { get; set; }
        public int EP_ID { get; set; }
        public int AC_ID { get; set; }
        //public short Main { get; set; }
        public Nullable<short> Percent { get; set; }

        public override String Prefix { get { return "EMAC"; } }
        
        public EMDEmploymentAccount(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEmploymentAccount()
        { }
    }
}
