using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.Util.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class WorkflowModel : BaseModel
    {
        public string OwnUserGuid { get; set; }

        [ScaffoldColumn(true)]
        public string IdWorkflow { get; set; }
        [Required, Display(Name = "Workflow name")]
        public string Name { get; set; }
        [Required, Display(Name = "Description")]
        public string Description { get; set; }
        public string Definition { get; set; }
        [Display(Name = "Processes")]
        public string ActiveProcesses { get; set; }
        [Display(Name = "Version")]
        public string Version { get; set; }
        [Display(Name = "Checkedout by")]
        public string CheckedOutBy { get; set; }

        public string CheckedOutByUserId { get; set; }

        public System.DateTime Created { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedDateOnly
        {
            get { return this.Created.Date; }
        }

        [Display(Name = "Active From")]
        public new DateTime? ValidFrom
        {
            get { return base.ValidFrom; }
            set { base.ValidFrom = value??new DateTime(); }
        }
       
        [Display(Name = "Active From")]
        public new DateTime? ValidFromDateOnly
        {
            get { return this.ValidFrom?.Date; }
        }
        [Display(Name = "Active To")]
        public new System.DateTime? ValidTo { get; set; }

        [Display(Name = "Active To")]
        public new DateTime? ValidToDateOnly
        {
            get { return this.ValidTo?.Date; }
        }

        public override String CanManagePermissionString
        {
            get { return SecurityPermission.WorkflowManagement_View_Manage; }
        }

        public override String CanViewPermissionString
        {
            get { return SecurityPermission.NotDefined; }
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        [Display(Name = "Checked out")]
        public bool IsCheckedOut
        {
            get
            {
                if (string.IsNullOrEmpty(CheckedOutByUserId))
                {
                    return false;
                }
                return true;
            }
        }

        public string OwnUsername { get; internal set; }

        public WorkflowModel()
        {

        }

        public static WorkflowModel Map(WorkflowItem workflowItem, string currentUserGuid, bool getCheckoutName = false)
        {
            WorkflowModel model = new WorkflowModel()
            {
                CheckedOutBy = "_",
                OwnUserGuid = currentUserGuid,
                IdWorkflow = workflowItem.Id,
                Definition = workflowItem.Definition,
                CheckedOutByUserId = workflowItem.CheckedOutBy,
                Description = workflowItem.Description,
                ActiveProcesses = workflowItem.ActiveProcesses.ToString(),
                Name = workflowItem.Name,
                Version = workflowItem.Version,
                Created = workflowItem.Created,
                ValidFrom = workflowItem.ValidFrom,
                ValidTo = workflowItem.ValidTo
            };

            if (getCheckoutName && model.IsCheckedOut)
            {
                try
                {
                    PersonHandler personHandler = new PersonHandler();
                    EMDPerson person = (EMDPerson)personHandler.GetObject<EMDPerson>(model.CheckedOutByUserId);

                    model.CheckedOutBy = person.UserID;
                }
                catch (Exception ex)
                {
                    model.CheckedOutBy = "No matching user";

                    IISLogger logger = ISLogger.GetLogger("WorkflowModel");
                    logger.Error("CheckedOutByUserID throwed an exception", ex);
                }
            }

            return model;
        }
    }
}