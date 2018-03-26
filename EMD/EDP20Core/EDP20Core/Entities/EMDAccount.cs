using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDAccount : EMDObject<EMDAccount>
    {
        public int AC_ID { get; set; }
        public int E_ID { get; set; }
        public string E_Guid { get; set; }
        public string KstID { get; set; }
        public string Name { get; set; }
        public Nullable<int> MainOrgUnit { get; set; }
        public string Responsible { get; set; }
        public Nullable<int> Responsible_EP_ID { get; set; }
        public override String Prefix { get { return "ACCO"; } }
        
        public EMDAccount(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDAccount()
        { }
    }    
}
