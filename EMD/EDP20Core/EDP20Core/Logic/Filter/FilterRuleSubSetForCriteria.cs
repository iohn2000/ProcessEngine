using System.Collections.Generic;

namespace Kapsch.IS.EDP.Core.Logic.Filter
{
    public class FilterRuleSubSetForCriteria
    {
        public EnumFilterCriteria Criteria { get; set; }
        public string FilterAction { get; set; }
        public List<string> ObjectGUIDs { get; set; }
        public bool EnteIsInherited { get; set; }

        public FilterRuleSubSetForCriteria()
        {
        }

        public FilterRuleSubSetForCriteria(EnumFilterCriteria criteria, string filterAction, List<string> objGUIDs, bool enteIsInherited = false)
        {
            this.Criteria = criteria;
            this.FilterAction = filterAction;
            this.ObjectGUIDs = objGUIDs;
            this.EnteIsInherited = enteIsInherited;
        }
    }


}
