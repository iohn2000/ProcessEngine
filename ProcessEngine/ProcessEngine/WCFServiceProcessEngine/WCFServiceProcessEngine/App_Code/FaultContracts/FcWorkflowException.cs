using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.WsProcessEngine.FaultContracts
{
    [DataContract]
    public class FcWorkflowException : FcBaseException
    {
        [DataMember]
        public List<WorkflowErrorItem> ErrorItems { get; set; }

        [DataMember]
        public int ErrorType { get; set; }

    }
}
