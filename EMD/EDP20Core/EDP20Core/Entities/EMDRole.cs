using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDRole : EMDObject<EMDRole>
    {
        public string Guid_Parent { get; set; }
        public string Guid_Root { get; set; }
        public int R_ID { get; set; }
        public int ID_Parent { get; set; }
        public int ID_Root { get; set; }
        public Nullable<int> GroupNr { get; set; }
        public string Name { get; set; }
        public string URL_Icon { get; set; }
        public Nullable<short> Priority { get; set; }
        public string DescriptionID { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string Key3 { get; set; }

        public bool IsSecurity { get; set; }

        public int Level { get; set; }

        public override String Prefix { get { return "ROLE"; } }
        
        public EMDRole(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDRole()
        { }

    }
}

