using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EquipmentDefinitionPriceModel: BaseModel
    {
        public string EQDE_Guid { get; set; }
        /// <summary>
        /// price of the equipment definition
        /// </summary>

        //[DataType(DataType.Currency)]
        //[DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        //[DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        //[DisplayFormat(DataFormatString = "{0:#,###0.00}", ApplyFormatInEditMode = true)]
        //[DisplayFormat(DataFormatString = "{0:####.##}", ApplyFormatInEditMode = true)]
        //\d+[(,\d{2})]
        //\d+(,\d{2})?
        //\d+(,\d{0,2})?
        [Display(Name = "Price EUR"),RegularExpression(@"\d+(,\d{2})?", ErrorMessage ="Required format 0,00")]
        public decimal Price { get; set; }
        /// <summary>
        /// Billing period for the equipment definitions <see cref="EnumEquipmentDefinitionPriceBillingPeriod"/>
        /// </summary>
        [Display(Name = "Billing period")]
        public int BillingPeriod { get; set; }

        public string ActiveFromString
        {
            get
            {
                if (ActiveFrom > DateTime.Now)
                {
                    return ActiveFrom.ToString("dd.MM.yyyy");
                }
                return string.Empty;
            }
        }

        public string Guid { get; set; }

        //public string Name
        //{
        //    get
        //    {
        //        string name = string.Empty;

        //        EnumEquipmentDefinitionPriceBillingPeriod billingPeriod = (EnumEquipmentDefinitionPriceBillingPeriod)C_CT_ID;
        //        try
        //        {
        //            name = ObjectHelper.GetEnumDescription(billingPeriod);
        //        }
        //        catch (Exception)
        //        {
        //            name = C_CT_ID.ToString();
        //        }


        //        return name;
        //    }
        //}

        public static EquipmentDefinitionPriceModel New(string eqde_guid, bool isFuture = false)
        {
            EquipmentDefinitionPriceModel equipmentDefinitionPriceModel = new EquipmentDefinitionPriceModel()
            {
                EQDE_Guid = eqde_guid
            };

            if (isFuture)
            {
                equipmentDefinitionPriceModel.ActiveFrom = GetFutureDate();
            }

            return equipmentDefinitionPriceModel;
        }

        public static DateTime GetFutureDate()
        {
            DateTime now = DateTime.Now;
            DateTime today = new DateTime(now.Year, now.Month, now.Day);
            return today.AddDays(1);
        }

        public bool IsFuture
        {
            get
            {
                return ActiveFrom > DateTime.Now;
            }
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
        }

        public static EquipmentDefinitionPriceModel GetContactModel(string eqde_guid, bool isFuture = false)
        {
            EquipmentDefinitionPriceModel equipmentDefinitionPriceModel = EquipmentDefinitionPriceModel.New(eqde_guid, isFuture);
            EquipmentDefinitionPriceManager priceManager = new EquipmentDefinitionPriceManager();

            EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice = null;
            emdEquipmentDefinitionPrice = priceManager.GetPriceForEquipmentDefinition(eqde_guid, isFuture);

            if (emdEquipmentDefinitionPrice == null)
            {
                //??????
                //equipmentDefinitionPriceModel.EP_Guid = guidEmployment;
                //EMDContactType emdContactType = new ContactTypeHandler().GetObjects<EMDContactType, ContactType>("CT_ID == " + contactType + "").Cast<EMDContactType>().FirstOrDefault();


                //equipmentDefinitionPriceModel.CT_Guid = emdContactType.Guid;
                //equipmentDefinitionPriceModel.C_CT_ID = emdContactType.CT_ID;
            }
            else
            {
                equipmentDefinitionPriceModel = Map(emdEquipmentDefinitionPrice);
            }

            return equipmentDefinitionPriceModel;
        }

        public static EquipmentDefinitionPriceModel Map(EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice)
        {
            EquipmentDefinitionPriceModel equipmentDefinitionPriceModel = new EquipmentDefinitionPriceModel();
            ReflectionHelper.CopyProperties<EMDEquipmentDefinitionPrice, EquipmentDefinitionPriceModel>(ref emdEquipmentDefinitionPrice, ref equipmentDefinitionPriceModel);

            //TODO see if this makes sense
            //equipmentDefinitionPriceModel.Price = equipmentDefinitionPriceModel.IsFuture ? EquipmentDefinitionPriceModel.GetFutureTextInfo(emdEquipmentDefinitionPrice) : emdEquipmentDefinitionPrice.Price?.Trim();

            equipmentDefinitionPriceModel.Price = emdEquipmentDefinitionPrice.Price;
            equipmentDefinitionPriceModel.BillingPeriod = emdEquipmentDefinitionPrice.BillingPeriod;
            return equipmentDefinitionPriceModel;
        }

        public static string GetFutureTextInfo(EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice)
        {
            if (emdEquipmentDefinitionPrice != null)
            {
                return string.Format("{0} ({1})", emdEquipmentDefinitionPrice.Price, emdEquipmentDefinitionPrice.ActiveFrom.ToString("dd.MM.yyyy"));
            }
            return string.Empty;

        }

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
                return PriceInformationModel.FuturePriceInformation(new PriceInformationModel(this.Price, this.BillingPeriod, this.ActiveFrom));
            }

        }
    }
}