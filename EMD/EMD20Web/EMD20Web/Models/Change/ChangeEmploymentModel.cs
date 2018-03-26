using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Change
{
    public class ChangeEmploymentModel
    {
        [Required, Display(Name = "Change")]
        public EnumChangeType SelectedChangeType { get; set; }

        public SelectionViewModel SponsorSelection { get; internal set; }

        public List<ChangeTypeModel> AvailableChangeTypeList
        {
            get
            {
                return ChangeTypeModel.GetChangeTypeModelList();
            }
        }

        public ChangeEmploymentModel()
        {
            // initialize lists to get them for the form-post event
            SponsorSelection = new SelectionViewModel();
            this.EquipmentInstanceModels = new List<EquipmentInstanceModel>();

        }

        public string SourceEmploymentGuid { get; set; }

        public string SourceEnterpriseGuid { get; set; }

        [Required, Display(Name = "Target Date")]
        public DateTime? TargetDate { get; set; }

        [Required, Display(Name = "Employment Type")]
        public string GuidEmploymentType { get; set; }

        [Required, Display(Name = "E-Mail Type")]
        public string EMailType { get; set; }

        [Required, Display(Name = "Enterprise")]
        public string GuidTargetEnte { get; set; }


        [Required, Display(Name = "Costcenter")]
        public string GuidCostcenter { get; set; }

        [Required, Display(Name = "Org Unit")]
        public string GuidOrgUnit { get; set; }

        [Display(Name = "Move all roles (e.g. Line Manager, Team Leader, ...)")]
        public bool MoveAllRoles { get; set; }

        [Display(Name = "Personal Number")]
        public string PersonalNumber { get; set; }


        [Required, Display(Name = "Sponsor")]
        public string GuidSponsorEmployment { get; set; }

        [Required, Display(Name = "Location")]
        public string GuidLocation { get; set; }

        public bool ShowEquipmentApprovement { get; set; }

        [Display(Name = "Approve equipment move")]
        public bool ApproveEquipmentMove { get; set; }


        public List<EquipmentInstanceModel> EquipmentInstanceModels { get; internal set; }


        #region Additional Data

        // Additional-Data for Enterprise ID == 20
        [Display(Name = "Personnel Requisition Number")]
        public string PersonnelRequisitionNumber { get; set; }

        [Display(Name = "No Approval Needed")]
        public bool NoApprovalNeeded { get; set; }

        [Display(Name = "Reason For no Approval")]
        public string NoApprovalNeededReason { get; set; }

        [Display(Name = "Simcard"), UIHint("YesNoDropdownlist")]
        public bool Simcard { get; set; }

        [Display(Name = "Datacard"), UIHint("YesNoDropdownlist")]
        public bool Datacard { get; set; }

        #endregion

        [Display(Name = "Inactive From")]
        public DateTime? LeaveFrom { get; set; }

        [Display(Name = "Inactive To")]
        public DateTime? LeaveTo { get; set; }
        public string CostCenterResponsibleName { get; internal set; }
    }
}