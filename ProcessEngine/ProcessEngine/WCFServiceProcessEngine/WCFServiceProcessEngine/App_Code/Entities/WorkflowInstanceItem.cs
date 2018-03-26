using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;


namespace Kapsch.IS.WsProcessEngine.Entities
{
    /// <summary>
    /// Summary description for WorkflowInstances
    /// </summary>
    [DataContract]
    public class WorkflowInstanceItem
    {
        [DataMember]
        public string InstanceID { get; set; }
        [DataMember]
        public string DefinitionID { get; set; }
        [DataMember]
        public EnumWorkflowInstanceStatus Status { get; set; }
        [DataMember]
        public string Name { get; set; } // name from workflow definition
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string InstanceXML { get; set; }
        [DataMember]
        public string CurrentActivity { get; set; }
        [DataMember]
        public string ParentWorkflowInstanceID { get; set; }
        [DataMember]
        public string NextActivity { get; set; }
        [DataMember]
        public string Version { get; set; } //TODO mitspeichern bei instance
        [DataMember]
        public System.DateTime? Created { get; set; }
        [DataMember]
        public System.DateTime? Updated { get; set; }
        [DataMember]
        public System.DateTime? Finished { get; set; }
        public WorkflowInstanceItem()
        {

        }
    }
}
