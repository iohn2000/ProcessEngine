using System;
using System.Collections.Generic;

namespace Kapsch.IS.EDP.Core.Entities
{
    public interface IFilterRuleHandler
    {
        List<IEMDObject<EMDFilterRule>> GetRuleSetByObjGuid(string objectGuid);

        List<EMDFilterRule> ReadRulesFromDatase(string objectGuid);
    }
}
