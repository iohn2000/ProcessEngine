using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;


namespace Kapsch.IS.ProcessEngine.Webservice.Entities
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
        public string Version { get; set; }
        [DataMember]
        public System.DateTime? Created { get; set; }
        [DataMember]
        public System.DateTime? Updated { get; set; }
        [DataMember]
        public System.DateTime? Finished { get; set; }
        public WorkflowInstanceItem()
        {

        }

        public static WorkflowInstanceItem Map(WFEWorkflowInstance wfeWorkflowInstance)
        {
            WorkflowInstanceItem wfInstanceItem = new WorkflowInstanceItem();

            DatabaseAccess db = new DatabaseAccess();

            var wfDef = db.GetWorkflowDefinition(wfeWorkflowInstance.WFI_WFD_ID, false); // get latest version
            if (wfDef != null)
            {
                wfInstanceItem.Name = wfDef.WFD_Name;
                wfInstanceItem.DefinitionID = wfDef.WFD_ID.ToString();
                wfInstanceItem.Description = wfDef.WFD_Description;
            }
            wfInstanceItem.Created = wfeWorkflowInstance.WFI_Created;
            wfInstanceItem.CurrentActivity = wfeWorkflowInstance.WFI_CurrentActivity;
            wfInstanceItem.Finished = wfeWorkflowInstance.WFI_Finished;
            wfInstanceItem.InstanceID = wfeWorkflowInstance.WFI_ID;
            wfInstanceItem.InstanceXML = wfeWorkflowInstance.WFI_Xml;
            wfInstanceItem.NextActivity = wfeWorkflowInstance.WFI_NextActivity;
            wfInstanceItem.ParentWorkflowInstanceID = wfeWorkflowInstance.WFI_ParentWF;
            wfInstanceItem.Status = (EnumWorkflowInstanceStatus)Enum.Parse(typeof(EnumWorkflowInstanceStatus), wfeWorkflowInstance.WFI_Status, true);
            wfInstanceItem.Updated = wfeWorkflowInstance.WFI_Updated;
            wfInstanceItem.Version = "n/a";

            return wfInstanceItem;
        }
    }
}
