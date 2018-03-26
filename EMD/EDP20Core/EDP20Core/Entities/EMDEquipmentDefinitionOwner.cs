using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// An owner of an equipment, can man prices and other extended properties
    /// </summary>
    public class EMDEquipmentDefinitionOwner : EMDObject<EMDEquipmentDefinitionOwner>
    {
        /// <summary>
        /// EquipmentDefinition guid
        /// </summary>
        public string EQDE_Guid { get; set; }
        /// <summary>
        /// Employment guid
        /// </summary>
        public string EP_GUID { get; set; }

        public override String Prefix { get { return "EQDO"; } }

        public EMDEquipmentDefinitionOwner(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEquipmentDefinitionOwner()
        { }
    }
}
