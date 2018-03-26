using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EMD.EMD20Web.Models.Onboarding;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Core;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("EquipmentDefinitionPrice")]
    public class EquipmentDefinitionPriceController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        [Route]
        public ActionResult Index()
        {
            return RedirectToAction(MANAGEROUTE);
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{eqde_guid}")]
        [Route("Edit/{eqde_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string eqde_guid, bool isPartialView = false)
        {
            EquipmentDefinitionPriceHandler equipmentDefinitionPriceHandler = new EquipmentDefinitionPriceHandler();
            EquipmentDefinitionPriceFutureModel model = new EquipmentDefinitionPriceFutureModel();

            List<EMDEquipmentDefinitionPrice> prices = equipmentDefinitionPriceHandler.GetActiveObjectsInInterval<EMDEquipmentDefinitionPrice, EquipmentDefinitionPrice>(DateTime.Now, EMDContact.INFINITY, string.Format("EQDE_Guid == \"{0}\"", eqde_guid)).ToList();

            model.CurrentEquipmentDefinitionPrice = EquipmentDefinitionPriceModel.New(eqde_guid, false);
            
            model.FutureEquipmentDefinitionPrice = EquipmentDefinitionPriceModel.New(eqde_guid, true);

            foreach (EMDEquipmentDefinitionPrice emdPrice in prices)
            {
                EquipmentDefinitionPriceModel equipmentDefinitionPriceModel = EquipmentDefinitionPriceModel.Map(emdPrice);

                if (equipmentDefinitionPriceModel.IsFuture)
                {
                    model.FutureEquipmentDefinitionPrice = equipmentDefinitionPriceModel;
                }
                else
                {
                    model.CurrentEquipmentDefinitionPrice= equipmentDefinitionPriceModel;
                }
            }

            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.IsOwnerOfEquipment)
                return GetNoPermissionView(isPartialView);


            if (isPartialView)
            {
                return PartialView("Edit", model);
            }
            else
            {
                return View("Edit", model);
            }
            
        }

        [HttpPost, ValidateInput(false)]
        [Route("Edit")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult Edit(EquipmentDefinitionPriceFutureModel equipmentDefinitionPriceFutureModel)
        {
            Exception handledException = null;
            equipmentDefinitionPriceFutureModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            EquipmentDefinitionPriceManager equipmentDefinitionPriceManager = new EquipmentDefinitionPriceManager(this.PersonGuid, MODIFY_COMMENT);

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            if (!equipmentDefinitionPriceFutureModel.IsFutureChecked)
            {
                ModelState.Remove("FutureEquipmentDefinitionPrice.Price");
                ModelState.Remove("FutureEquipmentDefinitionPrice.BillingPeriod");
            }


            if (equipmentDefinitionPriceFutureModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!equipmentDefinitionPriceFutureModel.CanManage && !equipmentDefinitionPriceFutureModel.IsOwnerOfEquipment)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    EquipmentManager equipmentManager = new EquipmentManager();
                    EMDEquipmentDefinition emdEquipmentDefinition = equipmentManager.Get(equipmentDefinitionPriceFutureModel.CurrentEquipmentDefinitionPrice.EQDE_Guid);
                    if (emdEquipmentDefinition != null && !string.IsNullOrWhiteSpace(emdEquipmentDefinition.ClientReferenceIDForPrice))
                    {
                        //Edit not allowed because of ClientReferenceID
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Could not save changes to price information, because a clientReferenceId for price exists");
                    }

                    bool hasChanges = true;
                    if (!string.IsNullOrEmpty(equipmentDefinitionPriceFutureModel.CurrentEquipmentDefinitionPrice.Guid))
                    {
                        EMDEquipmentDefinitionPrice existing = equipmentDefinitionPriceManager.Get(equipmentDefinitionPriceFutureModel.CurrentEquipmentDefinitionPrice.Guid);
                        if (existing.Price == equipmentDefinitionPriceFutureModel.CurrentEquipmentDefinitionPrice.Price && existing.BillingPeriod == equipmentDefinitionPriceFutureModel.CurrentEquipmentDefinitionPrice.BillingPeriod)
                        {
                            hasChanges = false;
                        }

                    }
                    if (hasChanges)
                    {
                        equipmentDefinitionPriceManager.CreateOrUpdate(
                            PriceModelToEMDEquipmentDefinitionPrice(equipmentDefinitionPriceFutureModel.CurrentEquipmentDefinitionPrice));
                    }


                    hasChanges = true;
                    if (!string.IsNullOrEmpty(equipmentDefinitionPriceFutureModel.FutureEquipmentDefinitionPrice.Guid))
                    {
                        if (!equipmentDefinitionPriceFutureModel.IsFutureChecked)
                        {
                            hasChanges = false;
                            equipmentDefinitionPriceManager.Delete(equipmentDefinitionPriceFutureModel.FutureEquipmentDefinitionPrice.Guid);
                        }
                        else
                        {
                            EMDEquipmentDefinitionPrice existing = equipmentDefinitionPriceManager.Get(equipmentDefinitionPriceFutureModel.FutureEquipmentDefinitionPrice.Guid);
                            if (existing.Price == equipmentDefinitionPriceFutureModel.FutureEquipmentDefinitionPrice.Price 
                                && existing.ActiveFrom == equipmentDefinitionPriceFutureModel.FutureEquipmentDefinitionPrice.ActiveFrom
                                && existing.BillingPeriod == equipmentDefinitionPriceFutureModel.FutureEquipmentDefinitionPrice.BillingPeriod)
                            {
                                hasChanges = false;
                            }
                        }

                    }

                    if (hasChanges)
                    {
                        equipmentDefinitionPriceManager.CreateOrUpdate(
                            PriceModelToEMDEquipmentDefinitionPrice(equipmentDefinitionPriceFutureModel.FutureEquipmentDefinitionPrice));
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit equipment definition price : " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", equipmentDefinitionPriceFutureModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new {equipmentDefinitionPriceModel = equipmentDefinitionPriceFutureModel,  message = "The Equipment Definition Price has been updated!" });
            }


        }

        private EMDEquipmentDefinitionPrice PriceModelToEMDEquipmentDefinitionPrice(EquipmentDefinitionPriceModel equipmentDefinitionPriceModel)
        {
            EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice = new EMDEquipmentDefinitionPrice();
            ReflectionHelper.CopyProperties<EquipmentDefinitionPriceModel, EMDEquipmentDefinitionPrice>(ref equipmentDefinitionPriceModel, ref emdEquipmentDefinitionPrice);
            return emdEquipmentDefinitionPrice;
        }

        //private string SaveEquipmentDefinitionPrice(EquipmentDefinitionPriceModel equipmentDefinitionPriceModel)
        //{
        //    EquipmentDefinitionPriceManager equipmentDefinitionPriceManager = new EquipmentDefinitionPriceManager(this.PersonGuid, MODIFY_COMMENT);
        //    EMDEquipmentDefinitionPrice emdEquipmentDefinitionPrice = new EMDEquipmentDefinitionPrice();
        //    ReflectionHelper.CopyProperties<EquipmentDefinitionPriceModel, EMDEquipmentDefinitionPrice>(ref equipmentDefinitionPriceModel, ref emdEquipmentDefinitionPrice);

        //    if (!EMDEquipmentDefinitionPrice.IsEMDGuid(emdEquipmentDefinitionPrice.Guid))
        //    {
        //        if (emdEquipmentDefinitionPrice.Price != 0 && emdEquipmentDefinitionPrice.BillingPeriod != 0)
        //        {
        //            equipmentDefinitionPriceManager.WriteOrModifyEquipmentDefinitionPrice(emdEquipmentDefinitionPrice);
        //        }
        //    }
        //    else
        //    {

        //        if (EMDEquipmentDefinitionPrice.IsEMDGuid(emdEquipmentDefinitionPrice.Guid))
        //        {
        //            equipmentDefinitionPriceManager.Update(emdEquipmentDefinitionPrice);
        //        }
        //        else
        //        {
        //            equipmentDefinitionPriceManager.WriteOrModifyEquipmentDefinitionPrice(emdEquipmentDefinitionPrice);
        //        }
        //    }

        //    return emdEquipmentDefinitionPrice.Price.ToString();
        //}

        /// <summary>
        /// Returns a list of all the billing periods of an equipmentPriceDefinition <see cref="Kapsch.IS.EDP.Core.Entities.EnumEquipmentDefinitionPriceBillingPeriod"/>
        /// </summary>
        /// <returns></returns>
        [Route("GetBillingPeriods")]
        public ActionResult GetBillingPeriods()
        {
            List<EnumEquipmentDefinitionPriceBillingPeriod> billingEnums = Enum.GetValues(typeof(EnumEquipmentDefinitionPriceBillingPeriod)).Cast<EnumEquipmentDefinitionPriceBillingPeriod>().ToList();
            List<TextValueModel> listBillingEnums = new List<TextValueModel>();
            foreach (EnumEquipmentDefinitionPriceBillingPeriod billingEnum in billingEnums)
            {
                listBillingEnums.Add(new TextValueModel(billingEnum.ToString(), ((int)billingEnum).ToString()));
            }
            return Json(listBillingEnums, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Returns a list of all the ClientReferenceSystemsForPric of an equipmentPriceDefinition <see cref="Kapsch.IS.EDP.Core.Entities.EnumClientReferenceSystemForPrice"/>
        /// </summary>
        /// <returns></returns>
        [Route("GetClientReferenceSystemsForPrice")]
        public ActionResult GetClientReferenceSystemsForPrice()
        {
            List<EnumClientReferenceSystemForPrice> itemEnums = Enum.GetValues(typeof(EnumClientReferenceSystemForPrice)).Cast<EnumClientReferenceSystemForPrice>().ToList();
            List<TextValueModel> listBillingEnums = new List<TextValueModel>();
            foreach (EnumClientReferenceSystemForPrice billingEnum in itemEnums)
            {
                listBillingEnums.Add(new TextValueModel(billingEnum.ToString(), ((int)billingEnum).ToString()));
            }
            return Json(listBillingEnums, JsonRequestBehavior.AllowGet);
        }


    }
}