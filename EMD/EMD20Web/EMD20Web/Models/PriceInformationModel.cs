using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    /// <summary>
    /// Class that hold's the basic price information for an equipmentDefinition or an equipmentDefinitionPrice
    /// </summary>
    public class PriceInformationModel
    {
        /// <summary>
        /// The price
        /// </summary>
        public Decimal Price { get; set; }
        /// <summary>
        /// The time period the equipment is billed <see cref="EnumEquipmentDefinitionPriceBillingPeriod"/>
        /// </summary>
        public int BillingPeriod { get; set; }
        /// <summary>
        /// A date on that the future price will get active
        /// </summary>
        public DateTime? ActiveFrom { get; set; }

        public PriceInformationModel(Decimal price, int billingPeriod)
        {
            this.Price = price;
            this.BillingPeriod = billingPeriod;
            this.ActiveFrom = null;
        }

        public PriceInformationModel(Decimal price, int billingPeriod, DateTime activeFrom)
        {
            this.Price = price;
            this.BillingPeriod = billingPeriod;
            this.ActiveFrom = activeFrom;
        }

        /// <summary>
        /// Returns the name of the enum EnumEquipmentDefinitionPriceBillingPeriod
        /// </summary>
        public string BillingPeriodName
        {
            get { return System.Enum.GetName(typeof(EnumEquipmentDefinitionPriceBillingPeriod), BillingPeriod); }

        }
        /// <summary>
        /// Returns the price information including price, billingperiod and currency
        /// </summary>
        /// <param name="priceInformationModel"></param>
        /// <returns></returns>
        public static string PriceInformation(PriceInformationModel priceInformationModel)
        {
                if (priceInformationModel.BillingPeriod != 0)
                {
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    NumberFormatInfo numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
                    numberFormatInfo.CurrencySymbol = "EUR";
                    numberFormatInfo.CurrencyDecimalSeparator = ",";
                    string formattedPrice = priceInformationModel.Price.ToString("C", numberFormatInfo);
                    return string.Format("{0} / {1}", formattedPrice, priceInformationModel.BillingPeriodName);
                }

                else
                    return string.Empty;
        }

        /// <summary>
        /// Returns the price information for a future price - including price, billingperiod, currency and acticeFrom date
        /// </summary>
        /// <param name="priceInformationModel"></param>
        /// <returns></returns>
        public static string FuturePriceInformation(PriceInformationModel priceInformationModel)
        {
                if (priceInformationModel.BillingPeriod != 0)
                {
                    string formattedPrice = PriceInformationModel.PriceInformation(priceInformationModel);
                    if (priceInformationModel.ActiveFrom == null)
                    {
                        return formattedPrice;
                    }
                    else
                    {
                        return string.Format("{0}  (from: {1})", formattedPrice, Convert.ToDateTime(priceInformationModel.ActiveFrom).ToString("dd.MM.yyyy"));
                    }
                }
                else
                    return string.Empty;
        }
    }
}