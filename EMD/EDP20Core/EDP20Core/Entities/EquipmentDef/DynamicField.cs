using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EquipmentDef
{
    public class DynamicField
    {
        /// <summary>
        /// The Constant is used to enrich the uniqueness for taks-tags
        /// </summary>
        public const string CONST_DYNPREFIX = "EqDyn_";

        public EnumDynamicFieldEquipment Identifier { get; set; }

        public EnumDynamicFieldType Type { get; set; }

        public string Name { get; set; }

        public bool IsMandatory { get; set; }
    }
}
