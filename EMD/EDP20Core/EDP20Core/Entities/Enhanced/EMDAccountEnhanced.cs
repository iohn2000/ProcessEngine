using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.Enhanced
{
    public class EMDAccountEnhanced : EMDAccount
    {
        public string ResponsibleName { get; set; }
        public string ResponsibleEmplGuid { get; set; }
        public string ResponsiblePersGuid { get; set; }

    }
}
