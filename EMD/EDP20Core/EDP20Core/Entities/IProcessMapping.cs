
using Kapsch.IS.EDP.Core.WF.Message;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public interface IProcessMapping
    {
        bool HasEntities();

        List<KeyValuePair<string, string>> GetEntityList();

        List<WorkflowAction> GetMappingMethods();
    }
}
