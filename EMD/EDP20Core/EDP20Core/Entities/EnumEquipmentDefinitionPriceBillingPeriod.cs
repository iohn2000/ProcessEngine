using System.ComponentModel;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Defines the possible values for the billing period of an equipmentDefinitionPrice
    /// </summary>
    public enum EnumEquipmentDefinitionPriceBillingPeriod
    {
        /// <summary>
        /// EquipmentDefinitionPrice will be billed on a yearly base
        /// </summary>
        [Description("Yearly")]
        Year = 1,
        /// <summary>
        /// EquipmentDefinitionPrice will be billed on a monthly base
        /// </summary>
        [Description("Monthly")]
        Month = 2,
        /// <summary>
        /// EquipmentDefinitionPrice will be billed on a quaterly base
        /// </summary>
        [Description("Quaterly")]
        Quater = 3,
        /// <summary>
        /// EquipmentDefinitionPrice will be billed only once
        /// </summary>
        [Description("Once")]
        Once = 4
    }
}
