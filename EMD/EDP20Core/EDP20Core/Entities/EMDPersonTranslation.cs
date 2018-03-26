using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDPersonTranslation : EMDObject<EMDPersonTranslation>
    {
        public override String Prefix { get { return "PETR"; } }

        public EMDPersonTranslation(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        {}

        public EMDPersonTranslation()
        {}
        public string PT_P_Guid { get; set; }
        public string PT_FamilyName { get; set; }
        public string PT_FirstName { get; set; }
        public string PT_Title1 { get; set; }
        public string PT_Title2 { get; set; }
        public string PT_128_FamilyName { get; set; }
        public string PT_128_FirstName { get; set; }
        
    }
}
