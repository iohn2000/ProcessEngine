using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDAccess : EMDObject<EMDAccess>
    {
        public string UserId { get; set; }
        public string E_Guid { get; set; }
        public string Form { get; set; }
        public Nullable<int> AdminDPW { get; set; }
        public Nullable<int> AdminNonDPW { get; set; }
        public string Note { get; set; }
        
        public override String Prefix { get { return "ACCE"; } }

        public EMDAccess(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDAccess()
        { }
    }
}
