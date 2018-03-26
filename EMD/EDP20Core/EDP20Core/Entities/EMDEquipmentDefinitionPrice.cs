using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Price information for equipment definitions
    /// </summary>
    public class EMDEquipmentDefinitionPrice : EMDObject<EMDEquipmentDefinitionPrice>
    {
        public string EQDE_Guid { get; set; }
        /// <summary>
        /// price of the equipment definition
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Billing period for the equipment definitions <see cref="EnumEquipmentDefinitionPriceBillingPeriod"/>
        /// </summary>
        public int BillingPeriod { get; set; }

        public override String Prefix { get { return "EQDP"; } }

        public EMDEquipmentDefinitionPrice(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDEquipmentDefinitionPrice()
        { }
    }
}
