using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Filter
{
    public class ColumnRuleSet
    {
        public string FilterExpression { get; set; }
        public string FilterValue { get; set; }
        public string FilterName { get; set; }

        public ColumnRuleSet(string expression, string fValue, string fName)
        {
            this.FilterExpression = expression;
            this.FilterValue = fValue;
            this.FilterName = fName;
        }
    }
}
