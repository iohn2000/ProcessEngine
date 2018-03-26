using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.WsProcessEngine.Entities
{
    [DataContract]
    public class WorkflowItem
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Definition { get; set; }
        [DataMember]
        public int ActiveProcesses { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string CheckedOutBy { get; set; }
        [DataMember]
        public System.DateTime Created { get; set; }
        [DataMember]
        public System.DateTime ValidFrom { get; set; }
        [DataMember]
        public System.DateTime? ValidTo { get; set; }



    }


}
