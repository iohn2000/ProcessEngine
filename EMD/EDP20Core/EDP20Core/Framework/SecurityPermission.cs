using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework
{
    public class SecurityPermission
    {
        public const string NotDefined = "NOTDEFINED";
        public const string EnterpriseManager_View = "EnterpriseManager.View";
        public const string EnterpriseManager_View_Manage = "EnterpriseManager.View.Manage";
        public const string CostCenterManager_View = "CostCenterManager.View";
        public const string CostCenterManager_View_Manage = "CostCenterManager.View.Manage";
        public const string OrgUnitManager_View = "OrgUnitManager.View";
        public const string OrgUnitManager_View_Manage = "OrgUnitManager.View.Manage";
        public const string OrgUnitSecurityManager_View = "OrgUnitSecurityManager.View";
        public const string OrgUnitSecurityManager_View_Manage = "OrgUnitSecurityManager.View.Manage";
        public const string OrgUnitRoleManager_View = "OrgUnitRoleManager.View";
        public const string OrgUnitRoleManager_View_Manage = "OrgUnitRoleManager.View.Manage";
        public const string EquipmentDefinitionManager_View = "EquipmentDefinitionManager.View";
        public const string EquipmentDefinitionManager_View_Manage = "EquipmentDefinitionManager.View.Manage";
        public const string EnterpriseLocationManager_View = "EnterpriseLocationManager.View";
        public const string EnterpriseLocationManager_View_Manage = "EnterpriseLocationManager.View.Manage";
        public const string LocationManager_View = "LocationManager.View";
        public const string LocationManager_View_Manage = "LocationManager.View.Manage";
        public const string CountryManager_View = "CountryManager.View";
        public const string CountryManager_View_Manage = "CountryManager.View.Manage";
        public const string RoleManager_View = "RoleManager.View";
        public const string RoleManager_View_Manage = "RoleManager.View.Manage";
        public const string ContactManager_View = "ContactManager.View";
        public const string ContactManager_View_Manage = "ContactManager.View.Manage";
        public const string AccountGroup_Manage = "AccountGroup.View";
        public const string AccountGroup_View_Manage = "AccountGroup.View.Manage";
        //public const string WorkflowManagement_View = "WorkflowManagement.View";
        public const string WorkflowManagement_View_Manage = "WorkflowManagement.View.Manage";
        //public const string WorkflowMappingManagement_View = "WorkflowMappingManagement.View";
        //public const string WorkflowMappingManagement_View_Manage = "WorkflowMappingManagement.View.Manage";
        public const string PersonManagement_View = "PersonManagement.View";
        public const string PersonManagement_View_Manage = "PersonManagement.View.Manage";
        public const string PersonManagement_Flags_Manage_PersonVisibleInPhonebook = "PersonManagement.Flags.Manage.PersonVisibleInPhonebook";
        public const string PersonManagement_Flags_Manage_PictureVisibleInPhonebook = "PersonManagement.Flags.Manage.PictureVisibleInPhonebook";
        public const string PersonManagement_Flags_Manage_PictureVisibleInAD = "PersonManagement.Flags.Manage.PictureVisibleInAD";
        public const string PersonManagement_Gender_View_Manage = "Person.Gender.View.Manage";

        public const string PackageManagement_View = "PackageManagement.View";
        public const string PackageManagement_View_Manage = "PackageManagement.View.Manage";
        public const string Personprofile_View_Package = "Personprofile.View.Package";
        public const string Personprofile_View_Package_Manage = "Personprofile.View.Package.Manage";
        public const string Personprofile_Settings_View = "Personprofile.Settings.View";
        public const string Personprofile_Settings_View_Manage = "Personprofile.Settings.View.Manage";
        public const string Personprofile_View_Equipment = "Personprofile.View.Equipment";
        public const string Personprofile_View_Equipment_Manage = "Personprofile.View.Equipment.Manage";
        public const string Personprofile_View_Manage_UploadImage = "Personprofile.View.Manage.UploadImage";
        public const string Personprofile_View_Employments_Users_View = "Personprofile.View.Employments.Users.View";
        public const string Personprofile_View_Guids_View = "Personprofile.View.Guids.View";
        public const string Personprofile_View_Settings_View_Employments_View_Extended = "Personprofile.View.Settings.View.Employments.View.Extended";
        //public const string Personprofile_View_Manage_PersonFlags = "Personprofile.View.Manage.PersonFlags";
        //public const string Personprofile_View_Manage_EmploymentFlags = "Personprofile.View.Manage.EmploymentFlags";
        public const string Personprofile_Flags_Manage_Employment_Main = "Personprofile.Flags.Manage.Employment.Main";
        public const string Personprofile_Flags_Manage_Employment_VisibleInPhonebook = "Personprofile.Flags.Manage.Employment.VisibleInPhonebook";
        public const string Personprofile_Flags_Manage_Employment_AdUpdate = "Personprofile.Flags.Manage.Employment.AdUpdate";
        //public const string Personprofile_Flags_Manage_Employment_DnaUpdate = "Personprofile.Flags.Manage.Employment.DnaUpdate";
        public const string Personprofile_Manage_Employment_DistributionGroup = "Personprofile.Manage.Employment.DistributionGroup";
        public const string Personprofile_Manage_Employment_Sponsor = "Personprofile.Manage.Employment.Sponsor";
        public const string Personprofile_Manage_Employment_PersNr = "Personprofile.Manage.Employment.PersNr";
        //public const string Personprofile_Manage_AddEmployment = "Personprofile.Manage.AddEmployment";
        //public const string Personprofile_Manage_ChangeEmployment = "Personprofile.Manage.ChangeEmployment";
        //public const string Personprofile_Manage_RemoveEmployment = "Personprofile.Manage.RemoveEmployment";
        public const string Onboarding = "Onboarding";
        public const string Change = "Change";
        public const string Offboarding = "Offboarding";
        public const string AdvancedSearch_View = "AdvancedSearch.View";
        public const string AdvancedSearch_View_Employment_ViewDetail = "AdvancedSearch.View.Employment.ViewDetail";
        public const string AdvancedSearch_View_Employment_ViewDetail_Historical = "AdvancedSearch.View.Employment.ViewDetail.Historical";
        public const string AdvancedSearch_View_Enterprise_ViewDetail = "AdvancedSearch.View.Enterprise.ViewDetail";
        public const string AdvancedSearch_View_Enterprise_ViewDetail_Historical = "AdvancedSearch.View.Enterprise.ViewDetail.Historical";
        public const string AdvancedSearch_View_Location_ViewDetail = "AdvancedSearch.View.Location.ViewDetail";
        public const string AdvancedSearch_View_Location_ViewDetail_Historical = "AdvancedSearch.View.Location.ViewDetail.Historical";
        public const string AdvancedSearch_View_CostCenter_ViewDetail = "AdvancedSearch.View.CostCenter.ViewDetail";
        public const string AdvancedSearch_View_CostCenter_ViewDetail_Historical = "AdvancedSearch.View.CostCenter.ViewDetail.Historical";
        public const string AdvancedSearch_View_User_ViewDetail = "AdvancedSearch.View.User.ViewDetail";
        public const string AdvancedSearch_View_User_ViewDetail_Historical = "AdvancedSearch.View.User.ViewDetail.Historical";
        //public const string Personprofile_View_Manage_PhonebookVisible = "Personprofile.View.Manage.PhonebookVisible";
        //public const string Personprofile_View_Manage_PictureVisibleInPhonebook = "Personprofile.View.Manage.PictureVisibleInPhonebook";
        //public const string Personprofile_View_Manage_PictureVisibleInAD = "Personprofile.View.Manage.PictureVisibleInAD";
        public const string Personprofile_User_View = "Personprofile.User.View";
        public const string Personprofile_User_View_Manage = "Personprofile.User.View.Manage";
        public const string PersonManagement_UserId_Manage = "PersonManagement.UserId.Manage";
        public const string TaskManagement_View_Manage_Approver = "TaskManagement.View.Manage.Approver";
        public const string Enterprise_View_Manage = "Enterprise.View.Manage";
        public const string Personprofile_View_Historical = "Personprofile.View.Historical";
        public const string Employment_ExitDate_View_Manage = "Employment.ExitDate.View.Manage";
        public const string CategoryManagement_View = "CategoryManagement.View";
        public const string CategoryManagement_View_Manage = "CategoryManagement.View.Manage";
        /// <summary>
        /// Used to determine if user is an equipment-owner
        /// </summary>
        public const string EquipmentDefinitionManager_Extended_View_Manage = "EquipmentDefinitionManager.Extended.View.Manage";
        public const string EquipmentDefinitionManager_Price_View_Manage = "EquipmentDefinitionManager.Price.View.Manage";


        public SecurityPermission(string permission, string ente_guids)
        {
            Permission = permission;
            ENTE_Guids = ente_guids;
        }

        public string Permission { get; internal set; }
        public string ENTE_Guids { get; internal set; }
    }

}
