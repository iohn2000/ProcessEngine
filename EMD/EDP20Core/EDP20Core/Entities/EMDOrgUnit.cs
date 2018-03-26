using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDOrgUnit : EMDObject<EMDOrgUnit>
    {
        public string Guid_Parent { get; set; }
        public string Guid_Root { get; set; }
        public int O_ID { get; set; }
        public int ID_Parent { get; set; }
        public int ID_Root { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string Key3 { get; set; }
        //Für die E_Guid sind NULL-Werte in der DB erlaubt da hier nur für Security-Roles auf eine Firma verwiesen wird. Alle anderen Rollen haben hier "NULL" eingetragen
        public string E_Guid { get; set; }
        public bool IsSecurity { get; set; }

        public override String Prefix { get { return "ORGU"; } }
        
        public int Level { get; set; }
        public int Sortorder { get; internal set; }

        public EMDOrgUnit(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDOrgUnit()
        { }
    }
}
