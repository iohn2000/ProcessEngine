using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Entities;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Entities.EquipmentDef;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EquipmentInstanceModel : BaseModel
    {

        [Display(Name = "Equipment")]
        public string EquipmentName { get; set; }
        /// <summary>
        /// Parsed from Data Field in ObjectRelation
        /// </summary>
        public string IdWorkflowInstance { get; set; }
        public string ObjectRelationGuid { get; set; }

        public string EquipmentDefinitionGuid { get; set; }

        [Display(Name = "Package")]
        public string PackageName { get; set; }
        public int PackageStatus { get; set; }
        public int ProcessStatus { get; set; }
        /// <summary>
        /// Parsed from Data Field in ObjectRelation
        /// </summary>
        [Display(Name = "Technical Exception")]
        public string TechnicalException { get; set; }


        [Display(Name = "Equipment-Package Status")]
        public string EquipmentStatusStringShort { get; set; }
        public string EquipmentStatusStringLong { get; set; }
        public int EquipmentStatusStringInt { get; set; }
        [Display(Name = "Process Status")]
        public string ProcessStatusStringShort { get; set; }
        public string ProcessStatusStringLong { get; set; }
        public int ProcessStatusStringInt { get; set; }



        public string CssVisibilityValue
        {
            get
            {
                string canChange = "hidden";

                if (ProcessStatus == EDP.Core.Logic.ProcessStatus.STATUSITEM_ACTIVE || (IsAdmin && ProcessStatus != EDP.Core.Logic.ProcessStatus.STATUSITEM_REMOVED))
                {
                    canChange = "visible";
                }

                return canChange;
            }
        }

        public string CssChangeStatusVisibilityValue
        {
            get
            {
                string canChange = "hidden";

                if (ProcessStatus != EDP.Core.Logic.ProcessStatus.STATUSITEM_REMOVED && ProcessStatus != EDP.Core.Logic.ProcessStatus.STATUSITEM_INPROGRESS)
                {
                    canChange = "visible";
                }

                return canChange;
            }
        }

        /// <summary>
        /// Set the delete color to red, because only Admins are allowed to delete the equipment
        /// </summary>
        public string CssDeleteIconColor
        {
            get
            {
                string color = "#5f5f68";

                if (ProcessStatus != EDP.Core.Logic.ProcessStatus.STATUSITEM_ACTIVE)
                {
                    color = "#ff2d56";
                }

                return color;
            }
        }

        public bool IsDefault { get; set; }
        public int MaxNumberAllowedEquipments { get; set; }
        public bool CanKeep { get; set; }
        public bool IsAccountingJob { get; set; }
        public bool IsAccountingOnMainEmployment { get; set; }
        public bool IsPeriodic { get; set; }
        public string GuidApprover { get; set; }
        public string IdEmailTemplateAdd { get; set; }
        public string IdEmailTemplateChange { get; set; }
        public string IdEmailTemplateRemove { get; set; }
        public string NavisionSourceSystemNumber { get; set; }
        public string ActiveDirectoryGroupName { get; set; }

        #region Dynamic Values >> hardcode till we get a change

        public bool ShowComputerName { get; set; }
        public bool IsComputerNameMandatory { get; set; }

        public bool ShowEmailAddress { get; set; }
        public bool IsEmailAddressMandatory { get; set; }

        #endregion


        /// <summary>
        /// Helper Property for Checkbox
        /// </summary>
        public bool DoKeep { get; set; }


        public DateTime? TargetDate { get; set; }

        [Display(Name = "Technical Exception")]
        public string TechnicalExceptionHtmlTitle
        {
            get
            {
                if (string.IsNullOrEmpty(this.TechnicalException))
                {
                    return string.Empty;
                }
                return string.Format("- Exception: {0}", this.TechnicalException);
            }
        }

        public string WorkflowInstanceCssVisibility
        {
            get
            {
                if (!string.IsNullOrEmpty(IdWorkflowInstance))
                {
                    return "visible";
                }

                return "hidden";
            }
        }

        public string Info { get; internal set; }

        public EquipmentInstanceModel()
        {
            DoKeep = true;
        }



        public static List<EquipmentInstanceModel> GetEquipmentInstanceModels(string empl_guid, string userName, bool deliverInActive = false)
        {
            EmploymentManager empMngr = new EmploymentManager();
            List<EMDEquipmentInstance> emdEquipmentInstances = empMngr.GetConfiguredListOfEquipmentIntancesForEmployment(empl_guid, deliverInActive);

            List<EquipmentInstanceModel> equipmentInstanceModels = new List<EquipmentInstanceModel>();

            foreach (EMDEquipmentInstance emdEquipmentInstance in emdEquipmentInstances)
            {
                if (!string.IsNullOrEmpty(emdEquipmentInstance.ObjectRelationGuid))
                {
                    EquipmentInstanceModel equipmentInstanceModel = new EquipmentInstanceModel();
                    equipmentInstanceModel.InitializeSecurity(SecurityUser.NewSecurityUser(userName));
                    EMDEquipmentInstance currentEquipmentInstance = emdEquipmentInstance;
                    ReflectionHelper.CopyProperties(ref currentEquipmentInstance, ref equipmentInstanceModel);

                    EquipmentDefinitionConfig config = emdEquipmentInstance.GetEquipmentDefinition().GetEquipmentDefinitionConfig();
                    equipmentInstanceModel.IsDefault = config.IsDefault;
                    equipmentInstanceModel.MaxNumberAllowedEquipments = config.MaxNumberAllowedEquipments;
                    equipmentInstanceModel.CanKeep = config.CanKeep;
                    equipmentInstanceModel.DoKeep = true;
                    if (config.IsDefault && config.CanKeep)
                    {
                        equipmentInstanceModel.DoKeep = false;
                    }

                    equipmentInstanceModel.IsAccountingJob = config.IsAccountingJob;
                    equipmentInstanceModel.IsAccountingOnMainEmployment = config.IsAccountingOnMainEmployment;

                    equipmentInstanceModel.NavisionSourceSystemNumber = config.NavisionSourceSystemNumber.ToString();
                    equipmentInstanceModel.ActiveDirectoryGroupName = config.ActiveDirectoryGroupName;

                    if (config.DynamicFields != null)
                    {
                        foreach (DynamicField dynamicField in config.DynamicFields)
                        {
                            switch (dynamicField.Identifier)
                            {
                                case EnumDynamicFieldEquipment.ComputerName:
                                    equipmentInstanceModel.ShowComputerName = true;
                                    equipmentInstanceModel.IsComputerNameMandatory = dynamicField.IsMandatory;
                                    break;
                                case EnumDynamicFieldEquipment.EmailAddress:
                                    equipmentInstanceModel.ShowEmailAddress = true;
                                    equipmentInstanceModel.IsEmailAddressMandatory = dynamicField.IsMandatory;
                                    break;
                            }
                        }
                    }

                    equipmentInstanceModel.IsPeriodic = config.IsPeriodic;
                    equipmentInstanceModel.GuidApprover = config.GuidApprover;
                    equipmentInstanceModel.IdEmailTemplateAdd = config.IdEmailTemplateAdd;
                    equipmentInstanceModel.IdEmailTemplateChange = config.IdEmailTemplateChange;
                    equipmentInstanceModel.IdEmailTemplateRemove = config.IdEmailTemplateRemove;

                    PackageEquipmentStatus statusLogik = new PackageEquipmentStatus();
                    equipmentInstanceModel.EquipmentStatusStringShort = statusLogik.GetProcessStatusItem(emdEquipmentInstance.PackageStatus).StatusShort;
                    equipmentInstanceModel.EquipmentStatusStringLong = statusLogik.GetProcessStatusItem(emdEquipmentInstance.PackageStatus).StatusLong;
                    equipmentInstanceModel.EquipmentStatusStringInt = emdEquipmentInstance.PackageStatus;

                    EquipmentStatus statusLogikEqu = new EquipmentStatus();
                    equipmentInstanceModel.ProcessStatusStringShort = statusLogikEqu.GetProcessStatusItem(emdEquipmentInstance.ProcessStatus).StatusShort;
                    equipmentInstanceModel.ProcessStatusStringLong = statusLogikEqu.GetProcessStatusItem(emdEquipmentInstance.ProcessStatus).StatusLong;
                    equipmentInstanceModel.ProcessStatusStringInt = emdEquipmentInstance.ProcessStatus;

                    equipmentInstanceModels.Add(equipmentInstanceModel);
                }
            }


            return equipmentInstanceModels.OrderByDescending(a => a.CanKeep).ThenBy(a => a.DoKeep).ThenBy(a => a.EquipmentName).ToList();
        }

        public static List<EquipmentInstanceModel> GetEquipmentInstanceModelsForEquipmentOwner(string empl_guid, string userName, string pers_guid, bool deliverInActive = false)
        {
            EmploymentManager empMngr = new EmploymentManager();
            List<EMDEquipmentInstance> emdEquipmentInstances = empMngr.GetConfiguredListOfEquipmentIntancesForEmployment(empl_guid, deliverInActive);
            List<EquipmentInstanceModel> equipmentInstanceModels = new List<EquipmentInstanceModel>();

            EquipmentDefinitionOwnerManager equipmentDefinitionOwnerManager = new EquipmentDefinitionOwnerManager();
            List<EMDEquipmentDefinition> ownerEquipmentDefinitions = equipmentDefinitionOwnerManager.GetEquipmentDefinitionsForOwner(pers_guid);
            foreach (EMDEquipmentInstance emdEquipmentInstance in emdEquipmentInstances)
            {
                if (!string.IsNullOrEmpty(emdEquipmentInstance.ObjectRelationGuid))
                {
                    EquipmentInstanceModel equipmentInstanceModel = new EquipmentInstanceModel();
                    equipmentInstanceModel.InitializeSecurity(SecurityUser.NewSecurityUser(userName));
                    EMDEquipmentInstance currentEquipmentInstance = emdEquipmentInstance;
                    ReflectionHelper.CopyProperties(ref currentEquipmentInstance, ref equipmentInstanceModel);

                    EMDEquipmentDefinition equipmentDefinition = emdEquipmentInstance.GetEquipmentDefinition();
                    //Check if owner
                    if (ownerEquipmentDefinitions.Where(item => item.Guid == equipmentDefinition.Guid).Count() > 0)
                    {
                        EquipmentDefinitionConfig config = equipmentDefinition.GetEquipmentDefinitionConfig();

                        equipmentInstanceModel.IsDefault = config.IsDefault;
                        equipmentInstanceModel.MaxNumberAllowedEquipments = config.MaxNumberAllowedEquipments;
                        equipmentInstanceModel.CanKeep = config.CanKeep;
                        equipmentInstanceModel.DoKeep = true;
                        if (config.IsDefault && config.CanKeep)
                        {
                            equipmentInstanceModel.DoKeep = false;
                        }

                        equipmentInstanceModel.IsAccountingJob = config.IsAccountingJob;
                        equipmentInstanceModel.IsAccountingOnMainEmployment = config.IsAccountingOnMainEmployment;

                        equipmentInstanceModel.NavisionSourceSystemNumber = config.NavisionSourceSystemNumber.ToString();
                        equipmentInstanceModel.ActiveDirectoryGroupName = config.ActiveDirectoryGroupName;

                        if (config.DynamicFields != null)
                        {
                            foreach (DynamicField dynamicField in config.DynamicFields)
                            {
                                switch (dynamicField.Identifier)
                                {
                                    case EnumDynamicFieldEquipment.ComputerName:
                                        equipmentInstanceModel.ShowComputerName = true;
                                        equipmentInstanceModel.IsComputerNameMandatory = dynamicField.IsMandatory;
                                        break;
                                    case EnumDynamicFieldEquipment.EmailAddress:
                                        equipmentInstanceModel.ShowEmailAddress = true;
                                        equipmentInstanceModel.IsEmailAddressMandatory = dynamicField.IsMandatory;
                                        break;
                                }
                            }
                        }

                        equipmentInstanceModel.IsPeriodic = config.IsPeriodic;
                        equipmentInstanceModel.GuidApprover = config.GuidApprover;
                        equipmentInstanceModel.IdEmailTemplateAdd = config.IdEmailTemplateAdd;
                        equipmentInstanceModel.IdEmailTemplateChange = config.IdEmailTemplateChange;
                        equipmentInstanceModel.IdEmailTemplateRemove = config.IdEmailTemplateRemove;

                        PackageEquipmentStatus statusLogik = new PackageEquipmentStatus();
                        equipmentInstanceModel.EquipmentStatusStringShort = statusLogik.GetProcessStatusItem(emdEquipmentInstance.PackageStatus).StatusShort;
                        equipmentInstanceModel.EquipmentStatusStringLong = statusLogik.GetProcessStatusItem(emdEquipmentInstance.PackageStatus).StatusLong;
                        equipmentInstanceModel.EquipmentStatusStringInt = emdEquipmentInstance.PackageStatus;

                        EquipmentStatus statusLogikEqu = new EquipmentStatus();
                        equipmentInstanceModel.ProcessStatusStringShort = statusLogikEqu.GetProcessStatusItem(emdEquipmentInstance.ProcessStatus).StatusShort;
                        equipmentInstanceModel.ProcessStatusStringLong = statusLogikEqu.GetProcessStatusItem(emdEquipmentInstance.ProcessStatus).StatusLong;
                        equipmentInstanceModel.ProcessStatusStringInt = emdEquipmentInstance.ProcessStatus;

                        equipmentInstanceModels.Add(equipmentInstanceModel);
                    }
                }
            }


            return equipmentInstanceModels.OrderByDescending(a => a.CanKeep).ThenBy(a => a.DoKeep).ThenBy(a => a.EquipmentName).ToList();
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }
    }
}