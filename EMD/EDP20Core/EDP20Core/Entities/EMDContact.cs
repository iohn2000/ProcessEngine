using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDContact : EMDObject<EMDContact>
    {
        public string CT_Guid { get; set; }
        public string EP_Guid { get; set; }
        public string P_Guid { get; set; }
        public string E_Guid { get; set; }
        public string L_Guid { get; set; }
        public int C_ID { get; set; }
        public Nullable<int> C_EP_ID { get; set; }
        public Nullable<int> C_E_ID { get; set; }
        public Nullable<int> C_P_ID { get; set; }
        public Nullable<int> C_L_ID { get; set; }
        public int C_CT_ID { get; set; }
        public string Details { get; set; }
        public string Text { get; set; }
        public string Note { get; set; }
        public Nullable<short> Priority { get; set; }
        public bool VisiblePhone { get; set; }
        public bool VisibleKatce { get; set; }
        public bool ACDDisplay { get; set; }

        public override String Prefix { get { return "CONT"; } }
        
        public EMDContact(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDContact()
        { }
    }
}
