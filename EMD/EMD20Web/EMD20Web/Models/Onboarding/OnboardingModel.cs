using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kapsch.IS.EMD.EMD20Web.Models.Onboarding
{
    public class OnboardingModel : BaseModel
    {
        public string pers_guid { get; set; }

        [Required, Display(Name = "Employment Type")]
        public string EmploymentType { get; set; }

        [Display(Name = "Sponsor")]
        public string SponsorGuid { get; set; }

        [Required, Display(Name = "User Domain")]
        public string GuidDomain { get; set; }

        [Required, Display(Name = "Enterprise")]
        public string Enterprise { get; set; }

        [Required, Display(Name = "Costcenter"), UIHint("CostcenterDropdown")]
        public string CostCenter { get; set; }

        [Required, UIHint("LocationDropdown")]
        public string Location { get; set; }


        [Required, Display(Name = "Org Unit"), UIHint("OrgUnitDropdown")]
        public string OrgUnit { get; set; }

        [Required, Display(Name = "Base Package Enterprise"), UIHint("EnterprisePackagesDropdown")]
        public string EnterprisePackages { get; set; }

        [Display(Name = "Base Package Location"), UIHint("LocationPackagesDropdown")]
        public string LocationPackages { get; set; }

        [Required, Display(Name = "Distribution Group"), UIHint("DistributionGroupDropdown")]
        public string DistributionGroup { get; set; }

        [Required, Display(Name = "Entry Date"), UIHint("Date"), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EntryDate { get; set; }

        //[Required,Display(Name="First day of work"), UIHint("Date"), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Required, Display(Name = "First Day Of Work"), UIHint("Date")]
        public DateTime? FirstDayOfWork { get; set; }

        [Display(Name = "Until Date"), UIHint("Date")]
        public DateTime? UntilDate { get; set; }

        [Display(Name = "Last Day Of Work"), UIHint("Date")]
        public DateTime? LastDay { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Room number")]
        public string Room { get; set; }

        [Display(Name = "Direct 1")]
        public string FixedLine { get; set; }

        [Display(Name = "Phone")]
        public string Extension { get; set; }

        [Display(Name = "Mobile Phone")]
        public string MobilePhone { get; set; }


        // Additional-Data
        [Required, Display(Name = "Personnel Requisition Number")]
        public string PersonnelRequisitionNumber { get; set; }

        [Display(Name = "No Approval Needed")]
        public bool NoApprovalNeeded { get; set; }

        [Display(Name = "Reason For no Approval *")]
        public string NoApprovalNeededReason { get; set; }

        [Required, Display(Name = "Simcard"), UIHint("YesNoDropdownlist")]
        public bool Simcard { get; set; }

        [Required, Display(Name = "Datacard"), UIHint("YesNoDropdownlist")]
        public bool Datacard { get; set; }

        [Display(Name = "Hardware Equipment")]
        public bool HardwareEquipment { get; set; }

        [Display(Name = "Kapsch Client Hardware Available (Laptop/PC) => Serial No. ")]
        public bool KapschClientHardwareAvailable { get; set; }

        [Display(Name = "")]
        public string KapschClientHardwareAvailableSerialNo { get; set; }

        [Display(Name = "No Hardware Required")]
        public bool NoHardwareRequired { get; set; }

        [Display(Name = "Software Equipment")]
        public string SoftwareEquipment { get; set; }


        // Request

        [Display(Name = "Navision Request")]
        public string NavisionRequest { get; set; }

        [Display(Name = "Navision Country Company shortcuts")]
        public string NavisionCountryCompanyShortcuts { get; set; }

        [Display(Name = "Citrix Navision access *"), UIHint("YesNoDropdownlist")]
        public bool CitrixNavisionAccess { get; set; }

        [Display(Name = "KIS Request")]
        public string KISRequest { get; set; }

        [Display(Name = "KSMP Request")]
        public string KSMPRequest { get; set; }

        [Display(Name = "DocuWare @ Request (Austria only)")]
        public string DocuWareRequest { get; set; }

        [Display(Name = "General IT Access Rights")]
        public string GeneralITAccessRight { get; set; }

        [Display(Name = "Cognos TM1 Planning")]
        public string CognosTM1Planning { get; set; }

        [Display(Name = "Development Support")]
        public string DevelopmentSupport { get; set; }
        [Display(Name = "EMail-Type")]
        public string EMailType { get; set; }

        [Display(Name = "Personnel Number")]
        public string PersNr { get; set; }

        public List<EquipmentDefinitionModel> EquipmentDefinitionModelList = new List<EquipmentDefinitionModel>();

        public List<TextValueModel> EnterpriseList = new List<TextValueModel>();

        public SelectionViewModel SponsorSelection { get; internal set; }

        public override String CanManagePermissionString { get { return SecurityPermission.Onboarding; } }
        public override String CanViewPermissionString { get { return SecurityPermission.NotDefined; } }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.CanView = false;
        }

        public OnboardingModel()
        {
            SponsorSelection = new SelectionViewModel();
            this.pers_guid = String.Empty;
            this.EmploymentType = String.Empty;
            this.Enterprise = String.Empty;
            this.CostCenter = String.Empty;
            this.Location = String.Empty;
            this.OrgUnit = String.Empty;
            this.EnterprisePackages = String.Empty;
            this.LocationPackages = String.Empty;
            this.DistributionGroup = String.Empty;
            this.JobTitle = String.Empty;
            this.Room = String.Empty;
            this.FixedLine = String.Empty;
            this.Extension = String.Empty;
            this.MobilePhone = String.Empty;
            this.PersonnelRequisitionNumber = String.Empty;
            this.NoApprovalNeeded = false;
            this.NoApprovalNeededReason = String.Empty;
            this.KapschClientHardwareAvailableSerialNo = String.Empty;
            this.SoftwareEquipment = String.Empty;
            this.NavisionRequest = String.Empty;
            this.NavisionCountryCompanyShortcuts = String.Empty;
            this.KISRequest = String.Empty;
            this.KSMPRequest = String.Empty;
            this.DocuWareRequest = String.Empty;
            this.GeneralITAccessRight = String.Empty;
            this.CognosTM1Planning = String.Empty;
            this.DevelopmentSupport = String.Empty;
            this.PersNr = string.Empty;
            this.EMailType = string.Empty;
        }

        /// <summary>
        /// collect all equipments from list of object containers (packages)
        /// use case : get EQs from BasePackages
        /// </summary>
        /// <param name="obcoGuids"></param>
        /// <returns>a list of all equipments caseting ase webservice safe equipments</returns>
        public List<NewEquipmentInfo> GetEquipmentsFromPackages(List<string> obcoGuids)
        {
            List<NewEquipmentInfo> eqList = new List<NewEquipmentInfo>();

            string ortyGuid = new ObjectRelationTypeHandler().GetGuidForRelationName(ObjectRelationTypeList.EquipmentByPackage);
            ObjectContainerContentHandler obccH = new ObjectContainerContentHandler();

            ObjectContainerContentHandler contentHandler = new ObjectContainerContentHandler();

            foreach (string obcoGuid in obcoGuids)
            {
                if (obcoGuid != null)
                {
                    List<string> eqdeList = obccH.GetOBCCGuidsByOBCOGuid(obcoGuid);


                    foreach (string item in eqdeList)
                    {
                        NewEquipmentInfo e = new NewEquipmentInfo();
                        e.FromTemplateGuid = obcoGuid;
                        e.OrtyGuid = ortyGuid;
                        e.EqdeGuid = ((EMDObjectContainerContent)contentHandler.GetObject<EMDObjectContainerContent>(item)).ObjectGuid;
                        eqList.Add(e);
                    }
                }
                else
                {
                    // do nothing
                }

            }
            return eqList;
        }
    }
}