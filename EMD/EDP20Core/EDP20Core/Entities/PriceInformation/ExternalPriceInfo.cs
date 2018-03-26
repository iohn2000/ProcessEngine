using Kapsch.IS.EDP.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.PriceInformation
{
    public class ExternalPriceInfoExtended : ExternalPriceInfo
    {
        public EnumEquipmentDefinitionPriceBillingPeriod BillingPeriod { get; set; }

        public string BillingPeriodName
        {
            get
            {
                return BillingPeriod.ToString();
            }
        }

        public ExternalPriceInfoExtended Initialize(ExternalPriceInfo externalPriceInfo)
        {

            this.BillingPeriod = EnumEquipmentDefinitionPriceBillingPeriod.Month;
            this.IdClientReference = externalPriceInfo.IdClientReference;
            this.Name = externalPriceInfo.Name;
            this.Price = externalPriceInfo.Price;

            return this;
        }

        public static List<ExternalPriceInfoExtended> Map(List<ExternalPriceInfo> externalPriceInfos)
        {
            List<ExternalPriceInfoExtended> externalPriceInfosExtended = new List<ExternalPriceInfoExtended>();
            foreach (ExternalPriceInfo externalPriceInfo in externalPriceInfos)
            {
                externalPriceInfosExtended.Add(new ExternalPriceInfoExtended().Initialize(externalPriceInfo));
            }

            return externalPriceInfosExtended;
        }

        public override bool Equals(object obj)
        {
            if (obj is ExternalPriceInfoExtended)
            {
                var that = obj as ExternalPriceInfoExtended;
                return this.IdClientReference == that.IdClientReference && this.Name == that.Name && this.Price == that.Price;
            }

            return false;
        }
    }
}
