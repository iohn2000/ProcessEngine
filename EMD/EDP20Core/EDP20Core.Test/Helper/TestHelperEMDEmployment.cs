using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.Entities;

namespace EDP20Core.Test.Helper
{
    class TestHelperEMDEmployment
    {
        // Test employment
        public static EMDEmployment EMDEmployment
        {
            get
            {
                EMDEmployment empl = new EMDEmployment();
                empl.Entry = DateTime.Now;
                empl.FirstWorkDay = DateTime.Now;
                empl.LastDay = DateTime.Now.AddYears(10);
                empl.Exit = DateTime.Now.AddYears(10);
                empl.PersNr = "1111";

                return empl;
            }
        }
    }
}
