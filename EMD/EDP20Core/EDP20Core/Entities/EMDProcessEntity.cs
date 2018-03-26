using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Shows the relations between the EDP Database and the workflow-database, including the current workflow state.
    /// </summary>
    public class EMDProcessEntity : EMDObject<EMDProcessEntity>
    {
        /// <inheritdoc/> 
        public EMDProcessEntity(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        /// <inheritdoc />
        public EMDProcessEntity()
        { }

        /// <summary>
        /// Prefix of the Entity-GUID: "PREN".
        /// 
        /// </summary>
        public override String Prefix { get { return "PREN"; } }

        /// <summary>
        /// GUID of the affected Entity, can only be an Employment ("EMPL"), an Equipment (?) or an Enterprise-Location ("ENLO").
        /// </summary>
        public string EntityGuid { get; set; }

        /// <summary>
        /// The ID of the linked Workflow-Instance.
        /// </summary>
        public string WFI_ID { get; set; }

        /// <summary>
        /// The ID of the Workflow-Definition.
        /// </summary>
        public string WFD_ID { get; set; }

        /// <summary>
        /// The name of the Workflow-Definition
        /// </summary>
        public string WFD_Name { get; set; }

        /// <summary>
        /// The Action of the workflowprocess
        /// </summary>
        public string WorkflowAction { get; set; }

        /// <summary>
        /// Current State of the linked workflow. Gets updated by the linked workflow activities.
        /// </summary>
        public string WFResultMessages { get; set; }

        /// <summary>
        /// GUID of the Employment which requested the linked workflow-instance.
        /// </summary>
        public string RequestorEmplGuid { get; set; }

        /// <summary>
        /// GUID of the Person which is affected by the linked Workflow-Instance.
        /// </summary>
        public string EffectedPersGuid { get; set; }

        /// <summary>
        /// DateTime when the linked Workflow-Instance was started.
        /// </summary>
        public System.DateTime WFStartTime { get; set; }

        /// <summary>
        /// Targeted End-DateTime of the linked Workflow-Instance
        /// </summary>
        public System.DateTime WFTargetDate { get; set; }
    }
}
