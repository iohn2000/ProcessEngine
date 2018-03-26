using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic.Filter
{
    public class RowRuleSet
    {
        public string Criteria { get; set; }
        public string RowBoolean { get; set; }

        public RowRuleSet(string criteria, string rowBoolean)
        {
            this.Criteria = criteria;
            this.RowBoolean = rowBoolean;
        }
    }
}
