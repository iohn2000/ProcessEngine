using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EmploymentModel : BaseModel
    {
        [Display(Name = "Guid")]
        public string Guid { get; set; }
        public string P_Guid { get; set; }
        public string E_Guid { get; set; }
        public string ENTE_Guid { get; set; }
        public string ENLO_Guid { get; set; }
        public string LOCA_Guid { get; set; }
        public string ET_Guid { get; set; }

        [Display(Name = "Employment Type")]
        public string EmploymentTypeName { get; set; }

        [Display(Name = "Distribution Group"), UIHint("DistributionGroupDropdown")]
        public string DGT_Guid { get; set; }
        [Display(Name = "Dist.Group")]
        public string DistributionGroupName { get; set; }
        public int EP_ID { get; set; }
        public Nullable<int> E_ID { get; set; }
        public int P_ID { get; set; }
        public Nullable<int> L_ID { get; set; }
        public int ET_ID { get; set; }
        public Nullable<System.DateTime> Entry { get; set; }
        public Nullable<System.DateTime> Exit { get; set; }
        public string ExitAsString { get; set; }
        public Nullable<System.DateTime> LastDay { get; set; }
        public string LastDayAsString { get; set; }
        [Display(Name = "Phone")]
        public bool Visible { get; set; }
        public string PersNr { get; set; }
        public string dpwKey { get; set; }
        [Display(Name = "Main")]
        public bool Main { get; set; }
        [Display(Name = "AD")]
        public bool AD_Update { get; set; }
        public Nullable<System.DateTime> Exit_Report { get; set; }

        public Nullable<int> DGT_ID { get; set; }
        public Nullable<int> Sponsor { get; set; }
        [Display(Name = "Sponsor")]
        public string Sponsor_Guid { get; set; }

        [Display(Name = "First work day")]
        public Nullable<System.DateTime> FirstWorkDay { get; set; }
        public string SponsorName { get; set; }

        [Display(Name = "Enterprise")]
        public string EnterpriseName { get; set; }

        [Display(Name = "Inactive From")]
        public DateTime? LeaveFrom { get; set; }

        [Display(Name = "Inactive To")]
        public DateTime? LeaveTo { get; set; }
        public bool IsLegalActive { get; set; }
        public bool IsSystemActive { get; set; }
        public string ValidityStatus { get; set; }

        public bool CanManageMainEmployment { get; set; }
        public bool CanManageVisibleInPhonebook { get; set; }
        public bool CanManageAdUpdate { get; set; }
        public bool CanManageDistributionGroup { get; set; }
        public bool CanManageSponsor { get; set; }

        public bool CanManagePersNr { get; set; }
        public bool CanSave { get; set; }
        public bool CanManageExitDate { get; set; }

        [Display(Name = "Status")]
        public string StatusDisplayName
        {
            get
            {
                string status = "unknown";

                switch ((int)Status)
                {
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ERROR:
                        status = "Error";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_NOTSET:
                        status = "Not set";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ACTIVE:
                        status = "Active";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_REMOVED:
                        status = "Removed";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_ORDERED:
                        status = "Ordered";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_QUEUED:
                        status = "Queued";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_INPROGRESS:
                        status = "In progress";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_TIMEOUT:
                        status = "Timeout";
                        break;
                    case EDP.Core.Logic.ProcessStatus.STATUSITEM_DECLINED:
                        status = "Declinded";
                        break;


                    default:
                        break;
                }

                return status;
            }
        }

        [Display(Name = "Method")]
        public string WorkflowAction { get; set; }

        public byte Status { get; private set; }
        public bool ShowSponsor { get; private set; }

        public SelectionViewModel SponsorSelection { get; internal set; }

        //private EMDPerson person;

        //public EMDPerson Person
        //{
        //    get
        //    {
        //        if (this.person == null)
        //        {
        //            PersonHandler handler = new PersonHandler();
        //            this.person = (EMDPerson)handler.GetObject<EMDPerson>(this.P_Guid);
        //        }

        //        return this.person;
        //    }
        //}

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            SecurityUserParameterFlags flags = new SecurityUserParameterFlags(checkPlainPermisson: true);
            this.IsAdmin = securityUser.IsAdmin;

            EmploymentTypeHandler emplTypeHandler = new EmploymentTypeHandler();
            EMDEmploymentType emplType = (EMDEmploymentType)emplTypeHandler.GetObject<EMDEmploymentType>(this.ET_Guid);

            this.ShowSponsor = emplType.MustHaveSponsor;

            if (!securityUser.IsAdmin)
            {


                this.CanManageAdUpdate = securityUser.hasPermission(SecurityPermission.Personprofile_Flags_Manage_Employment_AdUpdate, new SecurityUserParameterFlags(isItself: false), null, Guid);
                this.CanManageMainEmployment = securityUser.hasPermission(SecurityPermission.Personprofile_Flags_Manage_Employment_Main, new SecurityUserParameterFlags(checkPlainPermisson: true));
                this.CanManageVisibleInPhonebook = securityUser.hasPermission(SecurityPermission.Personprofile_Flags_Manage_Employment_VisibleInPhonebook, new SecurityUserParameterFlags(), null, Guid);
                this.CanManageDistributionGroup = securityUser.hasPermission(SecurityPermission.Personprofile_Manage_Employment_DistributionGroup, new SecurityUserParameterFlags(checkPlainPermisson: true));


                this.CanManageSponsor = securityUser.hasPermission(SecurityPermission.Personprofile_Manage_Employment_Sponsor, new SecurityUserParameterFlags(), this.E_Guid);
                this.CanManagePersNr = securityUser.hasPermission(SecurityPermission.Personprofile_Manage_Employment_PersNr, new SecurityUserParameterFlags(checkPlainPermisson: true));

                //if (this.IsLegalActive)
                this.CanManageExitDate = securityUser.hasPermission(SecurityPermission.Employment_ExitDate_View_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true));


                //Save Button
                if (this.CanManageAdUpdate || this.CanManageMainEmployment || this.CanManageVisibleInPhonebook || this.CanManageDistributionGroup || this.CanManageSponsor)
                {
                    this.CanSave = true;
                    this.CanManage = true;
                }
                else
                {
                    this.CanSave = false;
                    this.CanManage = false;
                }
            }
            else
            {
                this.CanManageAdUpdate = true;
                this.CanManageMainEmployment = true;
                this.CanManageVisibleInPhonebook = true;
                this.CanManageDistributionGroup = true;
                this.CanManageSponsor = true;
                this.CanManagePersNr = true;
                this.CanManageExitDate = true;

                this.CanSave = true;
                this.CanManage = true;
            }
        }

        public EmploymentModel()
        {
            SponsorSelection = new SelectionViewModel();
        }

        public EmploymentModel(EMDEmployment employment)
        {
            string method = "None";
            EMDProcessEntity processEntity = null;
            if (employment.Status != EmploymentProcessStatus.STATUSITEM_ACTIVE)
            {
                processEntity = new ProcessEntityManager().GetLastProcessEntity(employment.Guid);
                if (processEntity != null)
                {
                    method = processEntity.WorkflowAction;
                }
            }


            SponsorSelection = new SelectionViewModel();
            this.Guid = employment.Guid;
            this.ENLO_Guid = employment.ENLO_Guid;
            this.EnterpriseName = "";
            this.ValidFrom = employment.ValidFrom;
            this.ValidTo = employment.ValidTo;
            ObjectFlagManager ofm = new ObjectFlagManager();
            this.Main = ofm.IsMainEmployment(this.Guid);
            this.Visible = ofm.IsEmploymentVisibleInPhonebook(this.Guid);
            this.AD_Update = ofm.UpdateAD(this.Guid);
            this.DGT_ID = employment.DGT_ID;
            this.DGT_Guid = employment.DGT_Guid;
            this.Sponsor = employment.Sponsor;
            this.Sponsor_Guid = employment.Sponsor_Guid;
            this.Status = employment.Status;
            this.WorkflowAction = method;
            this.ValidityStatus = employment.ValidityStatus;
            this.ET_Guid = employment.ET_Guid;
            this.ET_ID = employment.ET_ID;
            this.PersNr = employment.PersNr.Trim();

            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(employment.ENLO_Guid);

            this.ENTE_Guid = enlo.E_Guid;
            this.E_ID = enlo.E_ID;
            this.LOCA_Guid = enlo.L_Guid;
            this.L_ID = enlo.L_ID;
            this.Entry = employment.Entry;
            this.Exit = employment.Exit;
            this.ExitAsString = GetDateAsString(employment.Exit);
            this.LastDay = employment.LastDay;
            this.LastDayAsString = GetDateAsString(employment.LastDay);
            this.FirstWorkDay = employment.FirstWorkDay;
            this.dpwKey = employment.dpwKey;
            this.EP_ID = employment.EP_ID;
            this.Exit_Report = employment.Exit_Report;
            this.E_Guid = enlo.E_Guid;
            this.P_Guid = employment.P_Guid;
            this.P_ID = employment.P_ID;
            DistributionGroupHandler dgHandler = new DistributionGroupHandler();
            EMDDistributionGroup distributionGroup = (EMDDistributionGroup)dgHandler.GetObject<EMDDistributionGroup>(this.DGT_Guid);
            this.DistributionGroupName = distributionGroup.Name;

            this.LeaveFrom = employment.LeaveFrom;
            this.LeaveTo = employment.LeaveTo;

            this.IsLegalActive = employment.IsLegalActive;
            this.IsSystemActive = employment.IsSystemActive;
        }

        private string GetDateAsString(DateTime? date)
        {
            DateTime actualADate;
            if (date == null)
                return "Null";
            else
                actualADate = Convert.ToDateTime(date);

            if (actualADate >= EMDEmployment.INFINITY)
            {
                return "Infinity";
            }
            else
            {
                return actualADate.ToString("dd.MM.yyyy");
            }
        }
    }
}