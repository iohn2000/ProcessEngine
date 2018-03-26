using Kapsch.IS.EDP.DataAccess.Entities;
using Kapsch.IS.EDP.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.DataAccess.Mockup
{
    public class MockupExternalPriceInfo : IExternalPriceInfo
    {
        public ExternalPriceInfo GetPrice(string idReference)
        {
            throw new NotImplementedException();
        }

        List<ExternalPriceInfo> IExternalPriceInfo.GetPrices()
        {
            throw new NotImplementedException();
        }
    }
}
