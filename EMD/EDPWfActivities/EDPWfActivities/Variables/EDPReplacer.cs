using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.ProcessEngine.Variables;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.WFActivity.Variables
{
    public class EDPReplacer : Replacer<EDPReplacer>
    {

        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public EDPReplacer(string value) : base(value)
        {
        }

        public override EDPReplacer Replace()
        {
            EntityQuery entityQuery = new EntityQuery();
            Type stringType = "".GetType();
            this.ProcessedValue = entityQuery.QueryMixedString(this.Value, out stringType).ToString();

            return this;
        }

    }
}
