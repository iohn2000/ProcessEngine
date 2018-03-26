using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class AdvancedSearchModel : BaseModel
    {
        [Display(Name = "Search"), Required(ErrorMessage = "Field required!"), StringLength(100, ErrorMessage = "The search string must have at least 2 characters", MinimumLength = 2)]
        public string SearchString { get; set; }

        [Display(Name = "Entity-Type"), Required(ErrorMessage = "Field required!")]
        public string SelectedEntity { get; set; }

        //[DataType(DataType.Date), Display(Name = "Start Date"),UIHint("Date"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true), Required(ErrorMessage = "Field required!")]
        //public DateTime StartDate { get; set; }

        [Display(Name = "Start Date"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true), Required(ErrorMessage = "Field required!")]
        public string StartDate { get; set; }

        //[Display(Name = "End Date"),Required(ErrorMessage = "Field required!")]
        //public DateTime EndDate { get; set; }

        [Display(Name = "End Date"), Required(ErrorMessage = "Field required!")]
        public string EndDate { get; set; }


        public List<TextValueModel> AvailableEntities { get; set; }

        public bool SearchStringIsGuid { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.NotDefined; } }
        public override String CanViewPermissionString { get { return SecurityPermission.AdvancedSearch_View; } }

        public bool CanManageLocation { get; set; }
        public bool CanManageEmployment { get; set; }
        public bool CanManageEnterprise { get; set; }

        public bool CanManageCostcenter { get; set; }

        public bool CanManageUser { get; set; }

        public bool CanViewLocation { get; set; }
        public bool CanViewEmployment { get; set; }
        public bool CanViewEnterprise { get; set; }

        public bool CanViewCostcenter { get; set; }

        public bool CanViewUser { get; set; }

        public bool CanViewAdvancedSearchLocation { get; set; }

        public bool CanViewAdvancedSearchEmployment { get; set; }

        public bool CanViewAdvancedSearchEnterprise { get; set; }

        public bool CanViewAdvancedSearchCostcenter { get; set; }

        public bool CanViewAdvancedSearchUser { get; set; }

        public bool CanViewAdvancedSearchLocationHistorical { get; set; }

        public bool CanViewAdvancedSearchEmploymentHistorical { get; set; }


        public bool CanViewAdvancedSearchEnterpriseHistorical { get; set; }


        public bool CanViewAdvancedSearchCostcenterHistorical { get; set; }

        public bool CanViewAdvancedSearchUserHistorical { get; set; }

        public bool CanViewProcessEntity { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanManage = false;


            SecurityUserParameterFlags flagsPlainPermission = new SecurityUserParameterFlags(checkPlainPermisson: true);

            this.CanViewAdvancedSearchCostcenter = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_CostCenter_ViewDetail, flagsPlainPermission);
            this.CanViewAdvancedSearchEmployment = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_Employment_ViewDetail, flagsPlainPermission);
            this.CanViewAdvancedSearchEnterprise = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_Enterprise_ViewDetail, flagsPlainPermission);
            this.CanViewAdvancedSearchLocation = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_Location_ViewDetail, flagsPlainPermission);
            this.CanViewAdvancedSearchUser = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_User_ViewDetail, flagsPlainPermission);

            this.CanViewAdvancedSearchCostcenterHistorical = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_CostCenter_ViewDetail_Historical, flagsPlainPermission);
            this.CanViewAdvancedSearchEmploymentHistorical = true; // securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_Employment_ViewDetail_Historical, flagsPlainPermission)
            this.CanViewAdvancedSearchEnterpriseHistorical = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_Enterprise_ViewDetail_Historical, flagsPlainPermission);
            this.CanViewAdvancedSearchLocationHistorical = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_Location_ViewDetail_Historical, flagsPlainPermission);
            this.CanViewAdvancedSearchUserHistorical = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_User_ViewDetail_Historical, flagsPlainPermission);

            this.CanManageCostcenter = securityUser.hasPermission(SecurityPermission.CostCenterManager_View_Manage, flagsPlainPermission);
            this.CanManageEmployment = securityUser.hasPermission(SecurityPermission.Onboarding, flagsPlainPermission);
            this.CanManageEnterprise = securityUser.hasPermission(SecurityPermission.EnterpriseManager_View_Manage, flagsPlainPermission);
            this.CanManageLocation = securityUser.hasPermission(SecurityPermission.LocationManager_View_Manage, flagsPlainPermission);
            this.CanManageUser = false;

            this.CanViewCostcenter = securityUser.hasPermission(SecurityPermission.CostCenterManager_View, flagsPlainPermission);
            this.CanViewEmployment = true;
            this.CanViewEnterprise = securityUser.hasPermission(SecurityPermission.EnterpriseManager_View, flagsPlainPermission);
            this.CanViewLocation = securityUser.hasPermission(SecurityPermission.LocationManager_View, flagsPlainPermission);
            this.CanViewUser = securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_User_ViewDetail, flagsPlainPermission);
            this.CanViewProcessEntity = securityUser.IsAdmin; // securityUser.hasPermission(SecurityPermission.AdvancedSearch_View_Employment_ViewDetail, flagsPlainPermission);

            //this.CanViewAdvancedSearchEnterpriseHistorical = false;
            if (this.CanViewAdvancedSearchCostcenter) this.AvailableEntities.Add(new TextValueModel("Costcenter", "ACCO", new { historical = this.CanViewAdvancedSearchCostcenterHistorical, showStartDateInfo = true, showEndDateInfo = true, startDateInfoText = "Search for active-from", endDateInfoText = "Search for active-to" }));
            if (this.CanViewAdvancedSearchEmployment) this.AvailableEntities.Add(new TextValueModel("Employment", "EMPL", new { historical = this.CanViewAdvancedSearchEmploymentHistorical, showStartDateInfo = true, showEndDateInfo = true, startDateInfoText = "Search for active-from", endDateInfoText = "Search for active-to" }));
            if (this.CanViewAdvancedSearchEnterprise) this.AvailableEntities.Add(new TextValueModel("Enterprise", "ENTE", new { historical = this.CanViewAdvancedSearchEnterpriseHistorical, showStartDateInfo = true, showEndDateInfo = true, startDateInfoText = "Search for active-from", endDateInfoText = "Search for active-to" }));
            if (this.CanViewAdvancedSearchLocation) this.AvailableEntities.Add(new TextValueModel("Location", "LOCA", new { historical = this.CanViewAdvancedSearchLocationHistorical, showStartDateInfo = true, showEndDateInfo = true, startDateInfoText = "Search for active-from", endDateInfoText = "Search for active-to" }));
            if (this.CanViewAdvancedSearchUser) this.AvailableEntities.Add(new TextValueModel("User", "USER", new { historical = this.CanViewAdvancedSearchUserHistorical, showStartDateInfo = true, showEndDateInfo = true, startDateInfoText = "Search for active-from", endDateInfoText = "Search for active-to" }));
            if (this.CanViewProcessEntity) this.AvailableEntities.Add(new TextValueModel("Processes", "PREN", new { historical = this.CanViewAdvancedSearchUserHistorical, showStartDateInfo = true, startDateInfoText = "Search for created-date", showEndDateInfo = true, endDateInfoText = "Search for created-date" }));
        }

        public AdvancedSearchModel()
        {
            AvailableEntities = new List<TextValueModel>();
        }
    }
}