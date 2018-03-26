using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.Entities;

namespace Kapsch.IS.EDP.Core.Logic.Interface
{
    public interface ICountryManagercs
        : IBaseManager
    {
        EMDCountry Get(string guid);

        EMDCountry Delete(string guid);
    }
}
