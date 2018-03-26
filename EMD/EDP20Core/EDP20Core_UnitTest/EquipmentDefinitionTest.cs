using System;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.Entities.PriceInformation;
using Kapsch.IS.EDP.DataAccess.DB;
using Kapsch.IS.EDP.DataAccess.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class EquipmentDefinitionTest
    {
        [TestMethod]
        public void TestGettingPriceInfos()
        {
            List<ExternalPriceInfo> priceInfos = DatabaseConnection.ExternalPriceInfoKbcAccounting.GetPrices();
            List<ExternalPriceInfoExtended> externalPriceInfosExtended = ExternalPriceInfoExtended.Map(priceInfos);

            Assert.IsTrue(externalPriceInfosExtended.Count > 0);


            ExternalPriceInfo externalPriceInfo = DatabaseConnection.ExternalPriceInfoKbcAccounting.GetPrice(externalPriceInfosExtended[0].IdClientReference);
            ExternalPriceInfoExtended externalPriceInfoExtended = new ExternalPriceInfoExtended().Initialize(externalPriceInfo);

            Assert.AreEqual(externalPriceInfosExtended[0], externalPriceInfoExtended);
        }
    }
}
