using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Kapsch.IS.EMD.EMD20Web.Models.Workflow
{
    public class WorkflowInstanceModel : BaseModel
    {

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData { get; set; }


        public System.Nullable<System.DateTime> Created { get; set; }

        /// <summary>
        /// for Grid Filtering to find a date without time
        /// </summary>
        [Display(Name = "Created")]
        public System.Nullable<System.DateTime> CreatedDateOnly
        {
            get { return this.Created?.Date; }
        }


        public string CurrentActivity { get; set; }


        public string DefinitionID { get; set; }


        public bool IsEditable
        {
            get
            {
                bool isEditable = false;

                switch (Status)
                {
                    case EnumWorkflowInstanceStatus.Error:
                    case EnumWorkflowInstanceStatus.Paused:
                        isEditable = true;
                        break;
                }

                return isEditable;
            }
        }

        public string EditWorkflowIconCssName
        {
            get
            {
                string name = "pageview";

                if (IsEditable)
                {
                    name = "edit";
                }

                return name;
            }
        }


        public string EditWorkflowCssVisibilityValue
        {
            get
            {
                string canChange = "visible";

                switch (Status)
                {
                    case EnumWorkflowInstanceStatus.Undefined:
                        canChange = "hidden";
                        break;
                }

                return canChange;
            }
        }

        public string RerunCssVisibilityValue
        {
            get
            {
                string canChange = "hidden";


                if (IsEditable)
                {
                    canChange = "visible";
                }

                return canChange;
            }
        }

        [Display(Name = "Description")]
        public string Description { get; set; }


        public DateTime? Finished { get; set; }


        public string InstanceID { get; set; }


        public string InstanceXML { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }


        public string NextActivity { get; set; }


        public string ParentWorkflowInstanceID { get; set; }


        public EnumWorkflowInstanceStatus Status { get; set; }

        [Display(Name = "Status")]
        public string StatusDisplayValue
        {
            get
            {
                switch (Status)
                {
                    case EnumWorkflowInstanceStatus.NotStarted:
                        return "Not started";

                    case EnumWorkflowInstanceStatus.Executing:
                        return "Executing";

                    case EnumWorkflowInstanceStatus.Sleeping: //polling - wait
                        return "Wait";

                    case EnumWorkflowInstanceStatus.Error:
                        return "Error to Handle";

                    case EnumWorkflowInstanceStatus.Aborted:
                        return "Aborted";

                    case EnumWorkflowInstanceStatus.Paused: //timeout case
                        return "Sleeping";

                    case EnumWorkflowInstanceStatus.Resumed:
                        return "Resumed";

                    case EnumWorkflowInstanceStatus.Finish:
                        return "Finish";

                    case EnumWorkflowInstanceStatus.Reset:
                        return "Reset";

                    case EnumWorkflowInstanceStatus.StopError:
                        return "Stop Error";

                    case EnumWorkflowInstanceStatus.Undefined:
                    default:
                        return "Unknown";

                }
            }
        }


        public DateTime? Updated { get; set; }


        [Display(Name = "Version")]
        public string Version { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.WorkflowManagement_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.NotDefined; } }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static WorkflowInstanceModel Map(WorkflowInstanceItem workflowIntanceItem)
        {
            return new WorkflowInstanceModel()
            {
                ExtensionData = workflowIntanceItem.ExtensionData,
                Created = workflowIntanceItem.Created,
                CurrentActivity = workflowIntanceItem.CurrentActivity,
                DefinitionID = workflowIntanceItem.DefinitionID,
                Description = workflowIntanceItem.Description,
                Finished = workflowIntanceItem.Finished,
                InstanceID = workflowIntanceItem.InstanceID,
                InstanceXML = workflowIntanceItem.InstanceXML,
                Name = workflowIntanceItem.Name,
                NextActivity = workflowIntanceItem.NextActivity,
                ParentWorkflowInstanceID = workflowIntanceItem.ParentWorkflowInstanceID,
                Status = workflowIntanceItem.Status,
                Updated = workflowIntanceItem.Updated,
                Version = workflowIntanceItem.Version
            };
        }
    }
}