using Kapsch.IS.EDP.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.DataAccess.Interface
{
    public interface IExternalPriceInfo
    {
        ExternalPriceInfo GetPrice(string idReference);

        List<ExternalPriceInfo> GetPrices();
    }
}
