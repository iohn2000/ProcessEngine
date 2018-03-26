using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Defines the possible values for the clientReferenceSystemForPrice of an equipmentDefinitionPrice
    /// </summary>
    public enum EnumClientReferenceSystemForPrice
    {
        [ClientReferenceSystemPriceAttribute(EnumEquipmentDefinitionPriceBillingPeriod.Month)]
        KBCAccountingItem = 10
    }

    public class ClientReferenceSystemPriceAttribute : Attribute
    {
        public EnumEquipmentDefinitionPriceBillingPeriod Period { get; private set; }

        public ClientReferenceSystemPriceAttribute(EnumEquipmentDefinitionPriceBillingPeriod period)
        {
            Period = period;
        }

        public static ClientReferenceSystemPriceAttribute GetAttribute(EnumClientReferenceSystemForPrice clientReferenceSystemForPrice)
        {
            MemberInfo memberInfo = typeof(EnumClientReferenceSystemForPrice).GetMember(clientReferenceSystemForPrice.ToString())
                                             .FirstOrDefault();

            if (memberInfo != null)
            {
                ClientReferenceSystemPriceAttribute attribute = (ClientReferenceSystemPriceAttribute)
                             memberInfo.GetCustomAttributes(typeof(ClientReferenceSystemPriceAttribute), false)
                                       .FirstOrDefault();
                return attribute;
            }

            return null;

            //    return (ClientReferenceSystemPriceAttribute)Attribute.GetCustomAttributes(typeof(ClientReferenceSystemPriceAttribute),false).FirstOrDefault();
        }
    }
}
