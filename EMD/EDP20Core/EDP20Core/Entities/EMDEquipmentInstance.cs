using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDEquipmentInstance : EMDObject<EMDObjectRelation>
    {

        private EMDEquipmentDefinition eqde = null;

        public string ObjectRelationGuid { get; set; }
        public string EquipmentName { get; set; }
        public string PackageName { get; set; }
        public int ProcessStatus { get; set; }
        public int PackageStatus { get; set; }

        /// <summary>
        /// Parsed from Data Field in ObjectRelation
        /// </summary>
        public string IdWorkflowInstance { get; set; }
        /// <summary>
        /// Parsed from Data Field in ObjectRelation
        /// </summary>
        public string TechnicalException { get; set; }




        public EMDEquipmentInstance()
        {
        }

        public void SetEquipmentDefinition(EMDEquipmentDefinition e)
        {
            this.eqde = e;
        }
        public EMDEquipmentDefinition GetEquipmentDefinition()
        {
            return this.eqde;
        }
    }
}
