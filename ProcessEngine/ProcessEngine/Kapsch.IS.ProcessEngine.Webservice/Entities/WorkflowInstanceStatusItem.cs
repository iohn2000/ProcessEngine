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
    /// Holds only the Status of an workflowInstance
    /// </summary>
    [DataContract]
    public class WorkflowInstanceStatusItem
    {
        [DataMember]
        public string InstanceID { get; set; }

        [DataMember]
        public EnumWorkflowInstanceStatus Status { get; set; }


        public WorkflowInstanceStatusItem()
        {

        }

        public static WorkflowInstanceStatusItem Map(WFEWorkflowInstance wfeWorkflowInstance)
        {
            WorkflowInstanceStatusItem wfInstanceItem = new WorkflowInstanceStatusItem();

            wfInstanceItem.InstanceID = wfeWorkflowInstance.WFI_ID;
            wfInstanceItem.Status = (EnumWorkflowInstanceStatus)Enum.Parse(typeof(EnumWorkflowInstanceStatus), wfeWorkflowInstance.WFI_Status, true);


            return wfInstanceItem;
        }
    }
}