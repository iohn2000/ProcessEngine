using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Kapsch.IS.EMD.EMD20Web.Models.Shared;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.Entities.EquipmentDef;
using System.Threading;
using System.Globalization;

namespace Kapsch.IS.EMD.EMD20Web.Models.Onboarding
{
    public class EquipmentDefinitionModel : BaseModel
    {
        static IISLogger logger = ISLogger.GetLogger("EquipmentDefinitionModel");

        private static List<TextValueModel> availableNavisionSourcSystemNumbers;

        public static List<TextValueModel> AvailableNavisionSourcSystemNumbers
        {
            get
            {
                if (availableNavisionSourcSystemNumbers == null)
                {

                    availableNavisionSourcSystemNumbers = new List<TextValueModel>();


                    foreach (string item in System.Enum.GetNames(typeof(EnumNavisionSourceSystemNumber)))
                    {
                        EnumNavisionSourceSystemNumber currentItem = (EnumNavisionSourceSystemNumber)System.Enum.Parse(typeof(EnumNavisionSourceSystemNumber), item, true);

                        switch (currentItem)
                        {
                            case EnumNavisionSourceSystemNumber.Undefined:
                                availableNavisionSourcSystemNumbers.Add(new TextValueModel("No number", item));
                                break;
                            case EnumNavisionSourceSystemNumber.DPWUSER:
                                availableNavisionSourcSystemNumbers.Add(new TextValueModel("DPW user", item));
                                break;

                            case EnumNavisionSourceSystemNumber.ITUSER:
                                availableNavisionSourcSystemNumbers.Add(new TextValueModel("IT user", item));
                                break;
                            case EnumNavisionSourceSystemNumber.ZUKOUSER:
                                availableNavisionSourcSystemNumbers.Add(new TextValueModel("ZUKO user", item));
                                break;
                            default:
                                // add also a new found enum type (marked with * to recognize it immediatly)
                                availableNavisionSourcSystemNumbers.Add(new TextValueModel(string.Format("*{0}(new)", item), item));
                                break;
                        }
                    }
                }

                return availableNavisionSourcSystemNumbers;
            }
        }



        [ScaffoldColumn(true)]
        public string Guid { get; set; }
        public string HistoryGuid { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public System.DateTime Created { get; set; }
        public int Q_ID { get; set; }
        [Required]
        public string Name { get; set; }

        [Display(Name = "Description (url)"), Url]
        public string Description { get; set; }

        [DataType(DataType.MultilineText)]
        public string Config { get; set; }

        public string PackageName { get; set; }

        public string PackageGuid { get; set; }

        public bool IsSelected { get; set; }

        public override String CanManagePermissionString { get { return SecurityPermission.EquipmentDefinitionManager_View_Manage; } }
        public override String CanViewPermissionString { get { return SecurityPermission.EquipmentDefinitionManager_View; } }

        [Display(Name = "Is Default Keep (for Change)")]
        public bool IsDefault { get; set; }
        [Display(Name = "Maximum of allowed equipments")]
        public int MaxNumberAllowedEquipments { get; set; }
        [Display(Name = "Can Keep Equipment")]
        public bool CanKeep { get; set; }
        [Display(Name = "Is accounting job")]
        public bool IsAccountingJob { get; set; }
        [Display(Name = "Is accounting on main employment")]
        public bool IsAccountingOnMainEmployment { get; set; }
        [Display(Name = "Is periodic")]
        public bool IsPeriodic { get; set; }
        [Display(Name = "Approver Role")]
        public string GuidApprover { get; set; }
        [Display(Name = "E-Mail Template Add")]
        public string IdEmailTemplateAdd { get; set; }
        [Display(Name = "E-Mail Template Change")]
        public string IdEmailTemplateChange { get; set; }
        [Display(Name = "E-Mail Template Remove")]
        public string IdEmailTemplateRemove { get; set; }

        [Display(Name = "Navision number")]
        public string NavisionSourceSystemNumber { get; set; }

        [Display(Name = "AD Group Name")]
        public string ActiveDirectoryGroupName { get; set; }

        [Display(Name = "Pricing System")]
        public Nullable<int> ClientReferenceSystemForPrice { get; set; }

        [Display(Name = "External Price")]
        public bool HasExternalPriceSystem
        {
            get
            {
                return ClientReferenceSystemForPrice != null && ClientReferenceSystemForPrice > -1;
            }
        }

        /// <summary>
        /// Indicates if the price information of the equipment is managed in edp or in an external system. If it has a value, the price information comes from a external system 
        /// and this is the name of key field that is used for the import.
        /// </summary>
        [Display(Name = "Client Reference ID")]
        public string ClientReferenceIDForPrice { get; set; }


        public Decimal Price { get; set; }
        public bool EditPriceInformationAllowed { get; set; }

        public bool HasFuturePriceInformation { get; set; }

        public int BillingPeriod { get; set; }


        [Display(Name = "Future Price")]
        public Decimal FuturePrice { get; private set; }
        public int FutureBillingPeriod { get; private set; }
        public DateTime FuturePriceActiveFrom { get; set; }

        public bool IsOwner { get; set; }
        public bool IsOwnerOfEquipment { get; set; }
        public bool CanManagePrice { get; set; }

        [Display(Name = "Working Instructions (url)"), Url]
        public string WorkingInstructions { get; set; }
        [Display(Name = "Description Long")]
        public string DescriptionLong { get; set; }

        /// <summary>
        /// <seealso cref="PriceInformationModel.PriceInformation(PriceInformationModel)"/>
        /// </summary>
        public string PriceInformation
        {
            get
            {
                return PriceInformationModel.PriceInformation(new PriceInformationModel(this.Price, this.BillingPeriod));
            }
        }

        /// <summary>
        /// <seealso cref="PriceInformationModel.FuturePriceInformation(PriceInformationModel)"/>
        /// </summary>
        public string FuturePriceInformation
        {
            get
            {
                return PriceInformationModel.FuturePriceInformation(new PriceInformationModel(this.FuturePrice, this.FutureBillingPeriod, this.FuturePriceActiveFrom));
            }

        }

        public string CssGridButtonPriceVisible
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.ClientReferenceIDForPrice) ? "visible" : "hidden";
            }
        }

        #region Dynamic Values >> hardcode till we get a change

        [Display(Name = "Show")]
        public bool ShowComputerName { get; set; }

        [Display(Name = "Mandatory")]
        public bool IsComputerNameMandatory { get; set; }

        [Display(Name = "Show")]
        public bool ShowEmailAddress { get; set; }

        [Display(Name = "Mandatory")]
        public bool IsEmailAddressMandatory { get; set; }

        #endregion

        #region FilterRule
        public RuleFilterModel RuleFilterModel { get; set; }
        public EMDEquipmentDefinitionPrice LastPriceInfo { get; internal set; }

        public string LastPriceHtml
        {
            get
            {
                if (LastPriceInfo == null)
                {
                    return string.Empty;
                }

                return string.Format("Last Price ({0}): {1} Euro / {2}", LastPriceInfo.Created.ToString("dd.MM.yyyy"), LastPriceInfo.Price.ToString("F2"), ((EnumEquipmentDefinitionPriceBillingPeriod)LastPriceInfo.BillingPeriod).ToString());
            }
        }

        #endregion

        public EquipmentDefinitionModel()
        {
            this.Guid = String.Empty;
            this.Q_ID = -1;
            this.Name = String.Empty;
            this.Description = String.Empty;
            this.Config = String.Empty;
            this.Created = DateTime.Now;
            this.IsSelected = true;
            this.ClientReferenceIDForPrice = string.Empty;
            this.Price = 0;
            this.BillingPeriod = 0;
            this.FuturePrice = 0;
            this.FutureBillingPeriod = 0;
            this.EditPriceInformationAllowed = false;
            this.HasFuturePriceInformation = false;
            RuleFilterModel = new RuleFilterModel();
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            this.IsOwner = securityUser.hasPermission(SecurityPermission.EquipmentDefinitionManager_Extended_View_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true));
            if (this.IsOwner)
                this.CanManagePrice = true;

            EquipmentDefinitionOwnerManager equipmentDefinitionOwnerManager = new EquipmentDefinitionOwnerManager();
            PersonManager persManager = new PersonManager();
            EMDPerson person = persManager.GetPersonByUserId(securityUser.UserId);

            this.IsOwnerOfEquipment = equipmentDefinitionOwnerManager.IsPersonOwnerOfEquipment(this.Guid, person.Guid);

        }

        public static EquipmentDefinitionModel Initialize(EMDEquipmentDefinition equipmentDefinition)
        {
            EquipmentDefinitionModel equipmentDefinitionModel = new EquipmentDefinitionModel();
            equipmentDefinitionModel.ActiveFrom = equipmentDefinition.ActiveFrom;
            equipmentDefinitionModel.ActiveTo = equipmentDefinition.ActiveTo;
            equipmentDefinitionModel.Config = equipmentDefinition.Config;
            equipmentDefinitionModel.Created = equipmentDefinition.Created;
            equipmentDefinitionModel.Description = equipmentDefinition.Description;
            equipmentDefinitionModel.Guid = equipmentDefinition.Guid;
            equipmentDefinitionModel.HistoryGuid = equipmentDefinition.HistoryGuid;
            equipmentDefinitionModel.Modified = equipmentDefinition.Modified;
            equipmentDefinitionModel.Name = equipmentDefinition.Name;
            equipmentDefinitionModel.Q_ID = equipmentDefinition.Q_ID;
            equipmentDefinitionModel.ValidFrom = equipmentDefinition.ValidFrom;
            equipmentDefinitionModel.ValidTo = equipmentDefinition.ValidTo;
            equipmentDefinitionModel.WorkingInstructions = equipmentDefinition.WorkingInstructions;
            equipmentDefinitionModel.DescriptionLong = equipmentDefinition.DescriptionLong == null ? string.Empty : equipmentDefinition.DescriptionLong;
            equipmentDefinitionModel.ClientReferenceSystemForPrice = equipmentDefinition.ClientReferenceSystemForPrice;
            equipmentDefinitionModel.ClientReferenceIDForPrice = equipmentDefinition.ClientReferenceIDForPrice;
            if (!string.IsNullOrWhiteSpace(equipmentDefinition.ClientReferenceIDForPrice))
                equipmentDefinitionModel.EditPriceInformationAllowed = false;
            else
                equipmentDefinitionModel.EditPriceInformationAllowed = true;


            if (!string.IsNullOrEmpty(equipmentDefinitionModel.Guid))
            {
                equipmentDefinitionModel.RuleFilterModel = ObjectHelper.GetRuleFilterModel(equipmentDefinitionModel.Guid);
            }

            EquipmentDefinitionPriceManager priceManager = new EquipmentDefinitionPriceManager();
            EMDEquipmentDefinitionPrice price = priceManager.GetEquipmentDefinitionPriceForEquipment(equipmentDefinition.Guid);
            EMDEquipmentDefinitionPrice futurePrice = priceManager.GetEquipmentDefinitionPriceForEquipment(equipmentDefinition.Guid, true);
            if (price != null)
            {
                equipmentDefinitionModel.Price = price.Price;
                equipmentDefinitionModel.BillingPeriod = price.BillingPeriod;
            }
            else
            {
                equipmentDefinitionModel.Price = 0;
                equipmentDefinitionModel.BillingPeriod = 0;
            }
            if (futurePrice != null)
            {
                equipmentDefinitionModel.FuturePrice = futurePrice.Price;
                equipmentDefinitionModel.FutureBillingPeriod = futurePrice.BillingPeriod;
                equipmentDefinitionModel.FuturePriceActiveFrom = futurePrice.ActiveFrom;
                equipmentDefinitionModel.HasFuturePriceInformation = true;
            }
            else
            {
                equipmentDefinitionModel.FuturePrice = 0;
                equipmentDefinitionModel.FutureBillingPeriod = 0;
            }

            EquipmentDefinitionConfig config = equipmentDefinition.GetEquipmentDefinitionConfig();
            equipmentDefinitionModel.IsDefault = config.IsDefault;
            equipmentDefinitionModel.MaxNumberAllowedEquipments = config.MaxNumberAllowedEquipments;
            equipmentDefinitionModel.CanKeep = config.CanKeep;
            equipmentDefinitionModel.IsAccountingJob = config.IsAccountingJob;
            equipmentDefinitionModel.IsAccountingOnMainEmployment = config.IsAccountingOnMainEmployment;
            equipmentDefinitionModel.IsPeriodic = config.IsPeriodic;
            equipmentDefinitionModel.GuidApprover = config.GuidApprover;
            equipmentDefinitionModel.IdEmailTemplateAdd = config.IdEmailTemplateAdd;
            equipmentDefinitionModel.IdEmailTemplateChange = config.IdEmailTemplateChange;
            equipmentDefinitionModel.IdEmailTemplateRemove = config.IdEmailTemplateRemove;
            equipmentDefinitionModel.NavisionSourceSystemNumber = config.NavisionSourceSystemNumber.ToString();
            equipmentDefinitionModel.ActiveDirectoryGroupName = config.ActiveDirectoryGroupName;

            if (config.DynamicFields != null)
            {
                foreach (DynamicField dynamicField in config.DynamicFields)
                {
                    switch (dynamicField.Identifier)
                    {
                        case EnumDynamicFieldEquipment.ComputerName:
                            equipmentDefinitionModel.ShowComputerName = true;
                            equipmentDefinitionModel.IsComputerNameMandatory = dynamicField.IsMandatory;
                            break;
                        case EnumDynamicFieldEquipment.EmailAddress:
                            equipmentDefinitionModel.ShowEmailAddress = true;
                            equipmentDefinitionModel.IsEmailAddressMandatory = dynamicField.IsMandatory;
                            break;
                    }
                }
            }

            return equipmentDefinitionModel;
        }

        public EquipmentDefinitionConfig GetEquipmentDefinitionConfig()
        {
            EquipmentDefinitionConfig equipmentDefinitionConfig = new EquipmentDefinitionConfig()
            {
                IsDefault = this.IsDefault,
                GuidApprover = this.GuidApprover,
                IdEmailTemplateAdd = this.IdEmailTemplateAdd,
                IdEmailTemplateChange = this.IdEmailTemplateChange,
                IdEmailTemplateRemove = this.IdEmailTemplateRemove,
                IsAccountingJob = this.IsAccountingJob,
                IsAccountingOnMainEmployment = this.IsAccountingOnMainEmployment,
                CanKeep = this.CanKeep,
                IsPeriodic = this.IsPeriodic,
                MaxNumberAllowedEquipments = this.MaxNumberAllowedEquipments,
                ActiveDirectoryGroupName = this.ActiveDirectoryGroupName,
                DynamicFields = new List<DynamicField>()

            };

            // add dynamic fields only if show is choosen
            if (this.ShowComputerName)
            {
                equipmentDefinitionConfig.DynamicFields.Add(new DynamicField() { Identifier = EnumDynamicFieldEquipment.ComputerName, IsMandatory = this.IsComputerNameMandatory, Name = "Computer Name", Type = EnumDynamicFieldType.String });
            }
            if (this.ShowEmailAddress)
            {
                equipmentDefinitionConfig.DynamicFields.Add(new DynamicField() { Identifier = EnumDynamicFieldEquipment.EmailAddress, IsMandatory = this.IsEmailAddressMandatory, Name = "E-Mail", Type = EnumDynamicFieldType.String });
            }


            try
            {
                equipmentDefinitionConfig.NavisionSourceSystemNumber = (EnumNavisionSourceSystemNumber)System.Enum.Parse(typeof(EnumNavisionSourceSystemNumber), this.NavisionSourceSystemNumber, true);
            }
            catch (Exception)
            {
                equipmentDefinitionConfig.NavisionSourceSystemNumber = EnumNavisionSourceSystemNumber.Undefined;
            }

            return equipmentDefinitionConfig;
        }

        public static EquipmentDefinitionModel Map(EMDEquipmentDefinition emdEquipmentDefinitionModel, string packageName)
        {
            EquipmentDefinitionModel equipmentDefinitionModel = new EquipmentDefinitionModel();

            ReflectionHelper.CopyProperties(ref emdEquipmentDefinitionModel, ref equipmentDefinitionModel);

            equipmentDefinitionModel.PackageName = packageName;

            EquipmentDefinitionConfig config = emdEquipmentDefinitionModel.GetEquipmentDefinitionConfig();
            equipmentDefinitionModel.IsDefault = config.IsDefault;
            equipmentDefinitionModel.MaxNumberAllowedEquipments = config.MaxNumberAllowedEquipments;
            equipmentDefinitionModel.CanKeep = config.CanKeep;
            equipmentDefinitionModel.IsAccountingJob = config.IsAccountingJob;
            equipmentDefinitionModel.IsAccountingOnMainEmployment = config.IsAccountingOnMainEmployment;
            equipmentDefinitionModel.IsPeriodic = config.IsPeriodic;
            equipmentDefinitionModel.GuidApprover = config.GuidApprover;
            equipmentDefinitionModel.IdEmailTemplateAdd = config.IdEmailTemplateAdd;
            equipmentDefinitionModel.IdEmailTemplateChange = config.IdEmailTemplateChange;
            equipmentDefinitionModel.IdEmailTemplateRemove = config.IdEmailTemplateRemove;
            equipmentDefinitionModel.NavisionSourceSystemNumber = config.NavisionSourceSystemNumber.ToString();
            equipmentDefinitionModel.ActiveDirectoryGroupName = config.ActiveDirectoryGroupName;

            foreach (DynamicField dynamicField in config.DynamicFields)
            {
                switch (dynamicField.Identifier)
                {
                    case EnumDynamicFieldEquipment.ComputerName:
                        equipmentDefinitionModel.ShowComputerName = true;
                        equipmentDefinitionModel.IsComputerNameMandatory = dynamicField.IsMandatory;
                        break;
                    case EnumDynamicFieldEquipment.EmailAddress:
                        equipmentDefinitionModel.ShowEmailAddress = true;
                        equipmentDefinitionModel.IsEmailAddressMandatory = dynamicField.IsMandatory;
                        break;
                }
            }

            equipmentDefinitionModel.RuleFilterModel = ObjectHelper.GetRuleFilterModel(equipmentDefinitionModel.Guid);

            return equipmentDefinitionModel;
        }

        public static List<EquipmentDefinitionModel> GetEquipmentDefinitionsFromPackages(List<string> packageGuids)
        {
            List<EquipmentDefinitionModel> equipmentDefinitionsModels = new List<EquipmentDefinitionModel>();

            ObjectContainerHandler obcoH = new ObjectContainerHandler();


            PackageManager packageManager = new PackageManager();

            foreach (string packageGuid in packageGuids)
            {
                if (!string.IsNullOrEmpty(packageGuid))
                {

                    string packageName = "not found";

                    try
                    {
                        EMDObjectContainer package = (EMDObjectContainer)obcoH.GetObject<EMDObjectContainer>(packageGuid);

                        packageName = package.Name;
                    }
                    catch (Exception e)
                    {
                        logger.Info("EquipmentDefinitionModel.GetEquipmentDefinitionsFromPackages(List<string> packageGuids) => TODO: write HelperMethod for generalizing this kind of handling", e);
                    }


                    List<EMDEquipmentDefinition> emdEquipmentDefinitions = packageManager.GetConfiguredEquipmentDefinitionsForPackage(packageGuid);

                    foreach (EMDEquipmentDefinition emdEquipmentDefinition in emdEquipmentDefinitions)
                    {
                        EquipmentDefinitionModel model = Map(emdEquipmentDefinition, packageName);
                        model.PackageGuid = packageGuid;
                        model.PackageName = packageName;
                        equipmentDefinitionsModels.Add(model);
                    }
                }
            }

            return equipmentDefinitionsModels;
        }
    }
}