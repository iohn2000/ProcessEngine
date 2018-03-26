using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using System.Configuration;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EmploymentTabModels
    {
        private List<EmploymentTabModel> ListOfEmploymentTabs;

        public EmploymentTabModels()
        {
            ListOfEmploymentTabs = new List<EmploymentTabModel>();
        }
        public void Add(EmploymentTabModel employmentTabModel)
        {
            ListOfEmploymentTabs.Add(employmentTabModel);
        }

        public List<EmploymentTabModel> getList()
        {
            return ListOfEmploymentTabs;
        }

        protected static EmploymentTabModel CreateObject(EMDEmployment employment)
        {
            EnterpriseHandler enth = new EnterpriseHandler();
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(employment.ENLO_Guid);
            EMDEnterprise enterprise = (EMDEnterprise)enth.GetObject<EMDEnterprise>(enlo.E_Guid);
            EmploymentPackageTabModel etm = new EmploymentPackageTabModel();
            ObjectFlagManager ofm = new ObjectFlagManager();
            etm.IsMainEmployment = ofm.IsMainEmployment(employment.Guid);


            bool emplIsPast = false;
            if (employment.Exit < DateTime.Now && employment.LastDay < DateTime.Now)
            {
                emplIsPast = true;
            }

            etm.EP_Guid = employment.Guid;

            etm.E_Guid = enterprise.Guid;
            etm.EnterpriseNameShort = enterprise.NameShort;


            if (emplIsPast)
            {
                //  etm.EnterpriseNameShort = string.Format("* {0}", etm.EnterpriseNameShort);
                etm.HtmlCssClass = "tabHistorical";
                etm.HtmlHoverText = "This is an historical entry";
            }
            else if (etm.IsMainEmployment)
            {
                etm.HtmlCssClass = "tabIsMain";
                etm.HtmlHoverText = "This is the main employment";
            }
            else
            {
                etm.HtmlCssClass = "tabIsAdditional";
            }

            return etm;
        }

    }

    public class EmploymentPackageTabModels
    {
        internal IISLogger logger = ISLogger.GetLogger("EmploymentPackageTabModels");

        private List<EmploymentPackageTabModel> ListOfEmploymentTabs;

        public EmploymentPackageTabModels()
        {
            ListOfEmploymentTabs = new List<EmploymentPackageTabModel>();
        }

        public EmploymentPackageTabModels(string pers_guid, SecurityUser securityUser)
        {
            ListOfEmploymentTabs = new List<EmploymentPackageTabModel>();
            PersonManager pm = new PersonManager();
            EmploymentHandler eh = new EmploymentHandler();
            eh.DeliverInActive = true;

            EmploymentManager emplmgr = new EmploymentManager();


            EMDPerson person = pm.Get(pers_guid);

            List<EMDEmployment> tempEmployments = eh.GetEmploymentsForPerson(pers_guid).Cast<EMDEmployment>().OrderByDescending(a => a.LastDay).ToList();
            // show permissions for line managers and admins
            List<EMDEmployment> employments = new List<EMDEmployment>();
            foreach (var employment in tempEmployments)
            {
                if (employment.ActiveTo > DateTime.Now || securityUser.hasPermission(string.Empty, new SecurityUserParameterFlags(isLineManager: true), null, emplGuid: employment.Guid))
                {
                    employments.Add(employment);
                }
            }


            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            foreach (EMDEmployment emp in employments)
            {
                //EnterpriseHandler enth = new EnterpriseHandler();
                EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(emp.ENLO_Guid);

                if (enlo != null)
                {
                    EmploymentPackageTabModel eptm = CreateObject(emp, securityUser);
                    eptm.InitializeSecurity(securityUser);
                    ListOfEmploymentTabs.Add(eptm);
                }
            }
        }

        private EmploymentPackageTabModel CreateObject(EMDEmployment employment, SecurityUser securityUser)
        {

            EnterpriseHandler enth = new EnterpriseHandler();
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(employment.ENLO_Guid);
            EMDEnterprise enterprise = (EMDEnterprise)enth.GetObject<EMDEnterprise>(enlo.E_Guid);
            EmploymentPackageTabModel etm = new EmploymentPackageTabModel();
            EmploymentTypeHandler emtyH = new EmploymentTypeHandler();
            EMDEmploymentType emty = (EMDEmploymentType)emtyH.GetObject<EMDEmploymentType>(employment.ET_Guid);
            ObjectFlagManager ofm = new ObjectFlagManager();
            etm.IsMainEmployment = ofm.IsMainEmployment(employment.Guid);
            etm.EP_Guid = employment.Guid;

            etm.E_Guid = enterprise.Guid;
            etm.EnterpriseNameShort = enterprise.NameShort;

            etm.EmploymentType = emty.Name;
            etm.ExitDate = employment.Exit;


            bool emplIsActive = false;
            bool emplIsPast = false;
            bool emplIsFuture = false;
            if (employment.Entry > DateTime.Now && employment.FirstWorkDay > DateTime.Now)
            {
                emplIsFuture = true;
            }
            else if (employment.Exit < DateTime.Now && employment.LastDay < DateTime.Now)
            {
                emplIsPast = true;
            }
            else
            {
                emplIsActive = true;
            }


            if (emplIsPast)
            {
                // etm.EnterpriseNameShort = string.Format("* {0}", etm.EnterpriseNameShort);
                etm.HtmlCssClass = "tabHistorical";
                etm.HtmlHoverText = "This is an historical entry";
            }
            else if (etm.IsMainEmployment)
            {
                etm.HtmlCssClass = "tabIsMain";
                etm.HtmlHoverText = "This is the main employment";
            }



            EmploymentTabModel tabModel = etm;


            EmploymentPackageTabModel packageTabModel = (EmploymentPackageTabModel)tabModel;
            packageTabModel.IsAdmin = securityUser.IsAdmin;
            packageTabModel.CanManagePackages = securityUser.hasPermission(SecurityPermission.Personprofile_View_Package_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true));
            packageTabModel.EmploymentIsActive = emplIsActive;
            packageTabModel.EmploymentIsFuture = emplIsFuture;
            packageTabModel.EmploymentIsPast = emplIsPast;
            return packageTabModel;
        }

        public void Add(EmploymentPackageTabModel employmentTabModel)
        {
            ListOfEmploymentTabs.Add(employmentTabModel);
        }

        public List<EmploymentPackageTabModel> getList()
        {
            return ListOfEmploymentTabs.OrderByDescending(a => a.ExitDate).ThenByDescending(a => a.IsMainEmployment).ThenBy(a => a.EnterpriseNameShort).ToList();
            //  return ListOfEmploymentTabs.OrderByDescending(a => a.IsMainEmployment).ThenBy(a => a.ExitDate).ThenBy(a => a.EnterpriseNameShort).ToList();
        }

        internal void SetSelectedPackageTabEquipment(string empl_guid, string obre_guid)
        {
            EmploymentPackageTabModel tabModel = (from a in ListOfEmploymentTabs where a.EP_Guid == empl_guid select a).FirstOrDefault();
            if (tabModel != null)
            {
                tabModel.IsSelectedTab = true;
                if (!string.IsNullOrWhiteSpace(obre_guid))
                {
                    tabModel.FilteredObreGuid = obre_guid;
                }
            }

        }
    }

    public class EmploymentPackageTabModel : EmploymentTabModel
    {
        //public override String CanManagePermissionString { get { return SecurityPermission.NotDefined; } }
        //public override String CanViewPermissionString { get { return SecurityPermission.NotDefined; } }

        public EmploymentPackageTabModel()
        {
            FilteredObreGuid = string.Empty;
        }


        public bool CanManagePackages { get; set; }
        public bool CanViewPackages { get; set; }
        public bool CanManageEquipments { get; set; }
        public string HtmlHoverText { get; internal set; }
        public bool EmploymentIsActive { get; set; }
        public bool EmploymentIsPast { get; set; }
        public bool EmploymentIsFuture { get; set; }
        public bool IsSelectedTab { get; internal set; }
        public string FilteredObreGuid { get; internal set; }

        //public bool CanAddEquipment { get; set; }
        //public bool CanAddPackage { get; set; }
        public bool ShowGuidEntities { get; set; }

        public bool IsEquipmentOwner { get; internal set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            InitializeBaseSecurity(securityUser);

            //if (this.CanManageEquipments)
            //    this.CanAddEquipment = true;

            //SecurityUser secUser = SecurityUser.NewSecurityUser(userName);

            //if (secUser.IsItSelf(this.EP_Guid))
            //{
            //    this.CanAddEquipment = true;
            //    this.CanAddPackage= true;
            //}



        }

        public override void InitializeBaseSecurity(SecurityUser securityUser)
        {
            //base.InitializeBaseSecurity(userName);

            this.CanManageEquipments = securityUser.hasPermission(SecurityPermission.Personprofile_View_Equipment_Manage, new SecurityUserParameterFlags(isItself: true, isLineManager: true, isAssistence: true), null, EP_Guid);
            this.CanManagePackages = securityUser.hasPermission(SecurityPermission.Personprofile_View_Package_Manage, new SecurityUserParameterFlags(isItself: true, isLineManager: true, isAssistence: true), null, EP_Guid);
            this.CanViewPackages = securityUser.hasPermission(SecurityPermission.Personprofile_View_Package, new SecurityUserParameterFlags(isItself: true, isLineManager: true, isAssistence: true), null, EP_Guid);
            this.ShowGuidEntities = securityUser.hasPermission(SecurityPermission.Personprofile_View_Guids_View, new SecurityUserParameterFlags(checkPlainPermisson: true), null, EP_Guid);
            this.IsEquipmentOwner = securityUser.hasPermission(SecurityPermission.EquipmentDefinitionManager_Extended_View_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true), null, EP_Guid);
            this.CanManageEquipments = this.CanManageEquipments || this.IsEquipmentOwner;
        }
    }

    public class EmploymentTabModel : BaseModel
    {
        public string EnterpriseNameShort { get; set; }

        public string EmploymentType { get; set; }

        public string E_Guid { get; set; }

        public string EP_Guid { get; set; }

        public string obre_guid { get; set; }

        public bool IsMainEmployment { get; set; }

        public DateTime? ExitDate { get; set; }

        public string HtmlCssClass { get; internal set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public override void InitializeBaseSecurity(SecurityUser securityUser)
        {

        }

    }

    public class AddPackageToEmploymentModel : BaseModel
    {
        [Required(ErrorMessage = "Field required")]
        public string EP_Guid { get; set; }

        [Display(Name = "Package"), Required(ErrorMessage = "Field required")]
        public string oc_guid { get; set; }

        [Required, Display(Name = "Target Date")]
        public DateTime? TargetDate { get; set; }

        //public bool CanAddPackage { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            this.CanManage = securityUser.hasPermission(SecurityPermission.Personprofile_View_Package_Manage, new SecurityUserParameterFlags(isItself: true, isLineManager: true, isAssistence: true), null, EP_Guid);
        }
    }

    public class AddEquipmentToEmploymentModel : BaseModel
    {
        public AddEquipmentToEmploymentModel()
        {
            EquipmentSelection = new SelectionViewModel();
        }

        [Required(ErrorMessage = "Field required")]
        public string Empl_Guid { get; set; }

        [Display(Name = "Equipment"), Required(ErrorMessage = "Field required")]
        public string obre_guid { get; set; }

        [Required, Display(Name = "Target Date")]
        public DateTime? TargetDate { get; set; }

        public SelectionViewModel EquipmentSelection { get; set; }

        public string ActingUserPersGuid { get; set; }

        public bool IsEquipmentOwner { get; set; }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            this.CanManage = securityUser.hasPermission(SecurityPermission.Personprofile_View_Equipment_Manage, new SecurityUserParameterFlags(isItself: true, isLineManager: true, isAssistence: true), null, Empl_Guid);
            this.IsEquipmentOwner = securityUser.hasPermission(SecurityPermission.EquipmentDefinitionManager_Extended_View_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true), null, null);
        }
    }


}