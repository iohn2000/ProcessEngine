using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Entities;
using System.Web.Mvc.Html;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class PersonProfileEmploymentModels
    {
        private List<PersonProfileEmploymentModel> ListOfPersonProfileEmploymentModels;

        public PersonProfileEmploymentModels()
        {
            ListOfPersonProfileEmploymentModels = new List<PersonProfileEmploymentModel>();
        }
        public void Add(PersonProfileEmploymentModel personProfileEmploymentModel)
        {
            ListOfPersonProfileEmploymentModels.Add(personProfileEmploymentModel);
        }

        /// <summary>
        /// Returns a ordered list of employments
        /// </summary>
        /// <returns></returns>
        public List<PersonProfileEmploymentModel> getList()
        {
            return ListOfPersonProfileEmploymentModels.OrderByDescending(a => a.ExitDate).ThenByDescending(a => a.IsMainEmployment).ThenBy(a => a.EnterpriseNameShort).ToList();
        }
    }

    public class PersonProfileEmploymentModel
    {
        public bool IsHistorical { get; set; }

        public string Guid { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        public string JobTitleFuture { get; set; }

        public string DirectPhone { get; set; }

        public string DirectPhoneFuture { get; set; }

        public string Mobile { get; set; }

        public string MobileFuture { get; set; }

        public string Phone { get; set; }

        public string PhoneFuture { get; set; }

        [Display(Name = "Direct-eFax")]
        public string Fax { get; set; }

        public string FaxFuture { get; set; }

        public string eFax { get; set; }

        public string eFaxFuture { get; set; }

        public string Room { get; set; }

        public string RoomFuture { get; set; }

        public string RoomFutureNumber { get; set; }

        public string EmploymentType { get; set; }

        public string EmploymentTypeShort { get; set; }

        public string Enterprise { get; set; }

        public string EnterpriseNameShort { get; set; }

        public string Location { get; set; }

        [StringLength(255), Display(Name = "Cost center name")]
        public string CostCenterName { get; set; }

        [StringLength(255), Display(Name = "Cost center")]
        public string CostCenterKstID { get; set; }

        public string CostCenterAcId { get; set; }

        public string CostCenterResponsible { get; set; }

        public string CostCenterResponsibleGuid { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Personal Number")]
        public string PersonalNumber { get; set; }

        public int EP_ID { get; set; }

        public string OrgUnit { get; set; }

        public string LineManager { get; set; }
        public string LineManagerGuid { get; set; }

        public string Teamleader { get; set; }
        public string TeamleaderGuid { get; set; }

        public string Assistance { get; set; }
        public string AssistanceGuid { get; set; }

        public string Sponsor { get; set; }
        public string SponsorGuid { get; set; }

        public string ente_guid { get; set; }

        public byte Status { get; set; }

        public bool IsLineManager { get; set; }

        public string ElId { get; set; }

        public bool IsOffboardingDisabled
        {
            get;set;
        }

        public string DisabledTitle { get; set; }

        public bool IsWorkflowDisabled
        {
            get
            {

                bool isDisabled = true;

                switch ((int)Status)
                {
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ERROR:
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_NOTSET:
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ACTIVE:
                        isDisabled = false;
                        break;

                }

                return isDisabled;

            }
        }

        public bool CanOffboard { get; set; }

        public bool CanChange { get; set; }

        public bool CanManageUser { get; set; }
        public bool CanViewUser { get; set; }

        public bool CanManageContactData { get; set; }

        public bool ShowGuidEntities { get; set; }

        public bool IsSelf { get; set; }

        [Display(Name = "Pause Begin")]
        public DateTime LeaveFrom { get; set; }

        [Display(Name = "Pause Begin")]
        public string LeaveFromDateString
        {
            get
            {
                return HtmlHelperExtensions.GetDateString(LeaveFrom);
            }
        }

        [Display(Name = "Pause End")]
        public DateTime LeaveTo { get; set; }

        [Display(Name = "Pause End")]
        public string LeaveToDateString
        {
            get
            {
                return HtmlHelperExtensions.GetDateString(LeaveTo);
            }
        }

        public string HtmlCssClass { get; internal set; }
        public string HtmlHoverText { get; internal set; }
        public bool IsMainEmployment { get; set; }
        public DateTime? ExitDate { get; internal set; }
    }
}