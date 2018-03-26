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
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.Entities.PriceInformation;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("EquipmentDefinition")]
    public class EquipmentDefinitionController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        [Route]
        // GET: Package
        public ActionResult Index()
        {
            //return View("Manage");
            return RedirectToAction(MANAGEROUTE);
        }

        [Route("Manage")]
        public ActionResult Manage()
        {
            EquipmentDefinitionModel model = new EquipmentDefinitionModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView && !model.IsOwner)
                return GetNoPermissionView(false);

            //just return an empty view since all data is Ajax-driven
            return View("Manage", model);
        }

        private List<EquipmentDefinitionModel> getEquipmentDefinitionList(string empl_guid = null)
        {
            List<EquipmentDefinitionModel> myresult = new List<EquipmentDefinitionModel>();
            EquipmentDefinitionHandler eqdefHandler = new EquipmentDefinitionHandler();


            try
            {
                EquipmentDefinitionModel dummySecurityModel = EquipmentDefinitionModel.Initialize(new EMDEquipmentDefinition());
                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                dummySecurityModel.InitializeSecurity(secUser);
                List<EMDEquipmentDefinition> listEquipmentDefinition;

                if (dummySecurityModel.CanManage || dummySecurityModel.CanView || dummySecurityModel.IsOwner)
                {
                    if (dummySecurityModel.IsAdmin)
                    {
                        if (!string.IsNullOrWhiteSpace(empl_guid))
                        {
                            EquipmentDefinitionOwnerManager equipmentDefinitionOwnerManager = new EquipmentDefinitionOwnerManager();
                            listEquipmentDefinition = equipmentDefinitionOwnerManager.GetEquipmentDefinitionsForOwnerEmployment(empl_guid);
                        }
                        else
                        {
                            listEquipmentDefinition = eqdefHandler.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>().Cast<EMDEquipmentDefinition>().ToList();
                        }

                    }
                    else if (dummySecurityModel.IsOwner)
                    {
                        EquipmentDefinitionOwnerManager equipmentDefinitionOwnerManager = new EquipmentDefinitionOwnerManager();
                        listEquipmentDefinition = equipmentDefinitionOwnerManager.GetEquipmentDefinitionsForOwner(this.PersonGuid);
                    }
                    else
                    {
                        listEquipmentDefinition = new List<EMDEquipmentDefinition>();
                    }
                }
                else
                {
                    listEquipmentDefinition = new List<EMDEquipmentDefinition>();
                }


                listEquipmentDefinition.ForEach(item =>
                {
                    EquipmentDefinitionModel model = EquipmentDefinitionModel.Initialize((EMDEquipmentDefinition)item);
                    model.CanManage = dummySecurityModel.CanManage;
                    model.CanView = dummySecurityModel.CanView;
                    model.IsOwner = dummySecurityModel.IsOwner;
                    myresult.Add(model);
                });

                myresult = myresult.OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading equipmentDefinitions", e);
            }
            return myresult;
        }

        [Route("Read")]
        [Route("Read/{empl_guid}")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request, string empl_guid = null)
        {

            List<EquipmentDefinitionModel> listEquipmentDefinitions = getEquipmentDefinitionList(empl_guid);
            // return Json(listEquipmentDefinitions, JsonRequestBehavior.AllowGet);
            return Json(listEquipmentDefinitions.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Create")]
        [Route("Create/{isPartialView}")]
        [Route("Edit/{eqde_guid}")]
        [Route("Edit/{eqde_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string eqde_guid, bool isPartialView = false)
        {
            EquipmentDefinitionModel eqdefModel = null;
            try
            {
                if (!string.IsNullOrEmpty(eqde_guid))
                {
                    ViewBag.Titel = "Edit EquipmentDefinition";
                    EquipmentDefinitionHandler eqdeHandler = new EquipmentDefinitionHandler();
                    EMDEquipmentDefinition eqdef = (EMDEquipmentDefinition)eqdeHandler.GetObject<EMDEquipmentDefinition>(eqde_guid);
                    eqdefModel = EquipmentDefinitionModel.Initialize(eqdef);

                    eqdefModel.LastPriceInfo = new EquipmentDefinitionPriceManager().GetLastPriceForEquipmentDefinition(eqde_guid);

                    SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                    eqdefModel.InitializeSecurity(secUser);

                    if (!eqdefModel.CanManage && !eqdefModel.IsOwnerOfEquipment)
                        return GetNoPermissionView(isPartialView);

                }
                else
                {
                    eqdefModel = new EquipmentDefinitionModel();
                    eqdefModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                    if (!eqdefModel.CanManage)
                        return GetNoPermissionView(isPartialView);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (isPartialView)
            {
                return PartialView("Edit", eqdefModel);
            }
            else
            {
                return View("Edit", eqdefModel);
            }
        }



        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        [Route("DoCreate")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, EquipmentDefinitionModel eqdefModel)
        {
            Exception handledException = null;
            string message = string.Empty;
            eqdefModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            if (ModelState.IsValid)
            {
                if (eqdefModel.ClientReferenceSystemForPrice.HasValue && string.IsNullOrWhiteSpace(eqdefModel.ClientReferenceIDForPrice))
                {
                    ModelState.AddModelError("error", "You must set a Client Reference ID if you have chosen an external pricing system.");
                }
            }


            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (eqdefModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(eqdefModel.Guid))
                    {
                        ViewBag.Title = "Edit EquipmentDefinition";
                        if (!eqdefModel.CanManage && !eqdefModel.IsOwnerOfEquipment)
                        {
                            throw new Exception(SecurityHelper.NoPermissionText);
                        }
                        EquipmentDefinitionHandler eqdefHandler = new EquipmentDefinitionHandler(this.PersonGuid);
                        EMDEquipmentDefinition eqdef = new EMDEquipmentDefinition();
                        ReflectionHelper.CopyProperties(ref eqdefModel, ref eqdef);


                        eqdef.SetEquipmentDefinitionConfig(eqdefModel.GetEquipmentDefinitionConfig());

                        eqdefHandler.UpdateObject<EMDEquipmentDefinition>(eqdef);
                        ObjectHelper.CreateOrUpdateFilterRules(eqdefModel.RuleFilterModel, eqdefModel.Guid);
                        message = "The Equipment-Definition has been updated!";
                    }
                    else
                    {
                        ViewBag.Title = "Add new EquipmentDefinition";
                        if (!eqdefModel.CanManage)
                        {
                            throw new Exception(SecurityHelper.NoPermissionText);
                        }
                        EquipmentDefinitionHandler eqdeHandler = new EquipmentDefinitionHandler(this.PersonGuid);

                        EMDEquipmentDefinition eqDef = new EMDEquipmentDefinition();
                        ReflectionHelper.CopyProperties<EquipmentDefinitionModel, EMDEquipmentDefinition>(ref eqdefModel, ref eqDef);
                        eqDef.SetEquipmentDefinitionConfig(eqdefModel.GetEquipmentDefinitionConfig());

                        var savedObject = eqdeHandler.CreateObject<EMDEquipmentDefinition>(eqDef);
                        ObjectHelper.CreateOrUpdateFilterRules(eqdefModel.RuleFilterModel, savedObject.Guid);
                        message = "The Equipment-Definition has been created!";
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                    logger.Error(ex, ControllerContext?.HttpContext);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", eqdefModel, handledException, "The EquipmentDefinition couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = message });
            }
        }


        [HttpGet]
        [Route("View")]
        [Route("View/{eqde_guid}")]
        [Route("View/{eqde_guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string eqde_guid, bool isPartialView = false)
        {
            EquipmentDefinitionHandler eqdeHandler = new EquipmentDefinitionHandler();
            EMDEquipmentDefinition eqdef = (EMDEquipmentDefinition)eqdeHandler.GetObject<EMDEquipmentDefinition>(eqde_guid);

            EquipmentDefinitionModel eqdefModel = EquipmentDefinitionModel.Initialize(eqdef);
            eqdefModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!eqdefModel.CanView)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("View", eqdefModel);
            }
            else
            {
                return View("View", eqdefModel);
            }
        }


        [HttpPost]
        [Route("DeleteEquipmentDefinition")]
        public ActionResult DeleteEquipmentDefinition(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.EquipmentDefinitionManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                EquipmentManager manager = new EquipmentManager(this.PersonGuid);
                manager.Delete(guid);

                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success });
        }

        [Route("ManageOwners/{eqde_guid}")]
        [Route("ManageOwners/{eqde_guid}/{isPartialView}")]
        public ActionResult ManageOwners([DataSourceRequest]DataSourceRequest request, string eqde_guid, bool isPartialView = false)
        {
            EquipmentManager eqManager = new EquipmentManager();
            EMDEquipmentDefinition equipmentDefinition = eqManager.Get(eqde_guid);

            EquipmentDefinitionModel eqModel = EquipmentDefinitionModel.Initialize(equipmentDefinition);
            eqModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!eqModel.CanManage)
                return GetNoPermissionView(isPartialView);


            ManageEquipmentDefinitionOwnerModel ownerModel = new ManageEquipmentDefinitionOwnerModel();
            ownerModel.EquipmentDefinitionName = equipmentDefinition.Name;
            ownerModel.EquipmentDefinitionGuid = eqde_guid;

            EquipmentDefinitionOwnerManager ownerManager = new EquipmentDefinitionOwnerManager();
            List<EMDEmployment> owners = ownerManager.GetOwnersForEquipmentDefinitionAsEmployees(eqde_guid);

            PersonManager persManager = new PersonManager();

            owners.ForEach(item =>
            {
                string persName = persManager.getFullDisplayNameWithUserIdAndPersNr(item);
                TextValueModel tvm = new TextValueModel(persName, item.Guid);
                ownerModel.ConfiguredOwners.Add(tvm);
            });

            ownerModel.AvailableOwnersSelection = new Models.Shared.SelectionAddModel()
            {
                ObjectLabel = "Available Owners",
                TargetControllerMethodName = "GetAllPersonEmploymentsDs",
                TargetControllerName = "Employment",
                HideDeleteButton = true,
                JavaScriptSelectedEvent = "equipmentDefinitionOwner.Events.onOwnerSelected"
            };

            if (isPartialView)
            {
                return PartialView("ManageOwners", ownerModel);
            }
            else
            {
                return View("ManageOwners", ownerModel);
            }
        }

        [Route("DoManageOwners")]
        [Route("DoManageOwners/{eqde_guid}/{configuredOwners}")]
        //[Route("Edit")]
        public ActionResult DoManageOwners([DataSourceRequest]DataSourceRequest request, String eqde_guid, IList<TextValueModel> configuredOwners)
        {

            bool success = false;
            string errorMessage = string.Empty;

            if (configuredOwners != null && configuredOwners.Count > 0)
            {
                ManageEquipmentDefinitionOwnerModel ownerModel = new ManageEquipmentDefinitionOwnerModel();
                try
                {
                    EquipmentManager eqManager = new EquipmentManager();
                    EMDEquipmentDefinition equipmentDefinition = eqManager.Get(eqde_guid);

                    EquipmentDefinitionModel eqModel = EquipmentDefinitionModel.Initialize(equipmentDefinition);
                    eqModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                    if (!eqModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    ownerModel.EquipmentDefinitionGuid = eqde_guid;
                    ownerModel.EquipmentDefinitionName = equipmentDefinition.Name;
                    ownerModel.ConfiguredOwners = configuredOwners;

                    EmploymentManager emplManager = new EmploymentManager();
                    List<EMDPersonEmployment> allEmpls = emplManager.GetAllPersonEmployments();

                    EquipmentDefinitionOwnerManager ownerManager = new EquipmentDefinitionOwnerManager();
                    List<EMDEmployment> owners = ownerManager.GetOwnersForEquipmentDefinitionAsEmployees(eqde_guid);

                    PersonManager persManager = new PersonManager();

                    foreach (EMDPersonEmployment persEmpl in allEmpls.ToList())
                    {
                        foreach (EMDEmployment empl in owners)
                        {
                            if (empl.Guid == persEmpl.Empl.Guid)
                            {
                                allEmpls.Remove(persEmpl);
                            }
                        }
                    }

                    allEmpls.ForEach(item =>
                    {
                        TextValueModel tvm = new TextValueModel(item.FullDisplayNameWithUserIdAndPersNr, item.Empl.Guid);
                        ownerModel.AvailableOwners.Add(tvm);
                    });

                    List<String> configuredGuids = new List<string>();
                    foreach (TextValueModel item in configuredOwners)
                    {
                        configuredGuids.Add(item.Value);
                    }
                    ownerManager.UpdateOwnersForEquipment(eqde_guid, configuredGuids);
                    success = true;
                }
                catch (Exception ex)
                {
                    errorMessage = "The owners for the equipment definition could not been saved: " + ex.Message.ToString();
                }
            }
            else
            {
                errorMessage = "There must be at least one equipment owner selected!";
            }

            return Json(new { success = success, Url = MANAGEROUTE, message = "The owners for the equipment definition have been saved!", errorMessage = errorMessage });
        }

        [Route("ReadOwnersForFilter")]
        [HandleError()]
        public ActionResult ReadOwnersForFilter()
        {
            EquipmentDefinitionOwnerManager ownerManager = new EquipmentDefinitionOwnerManager();
            List<EMDEmployment> owners = ownerManager.GetAllOwners();

            EmploymentManager emplManager = new EmploymentManager();
            List<EMDPersonEmployment> ownerEmpls = emplManager.GetPersonEmploymentsForEmployees(owners);

            List<TextValueModel> ownersForDropdown = new List<TextValueModel>();
            ownerEmpls.ForEach(item =>
            {
                TextValueModel tvm = new TextValueModel(item.FullDisplayNameWithUserIdAndPersNr, item.Empl.Guid);
                ownersForDropdown.Add(tvm);
            });
            ownersForDropdown = ownersForDropdown.OrderBy(item => item.Text).ToList();
            return Json(ownersForDropdown, JsonRequestBehavior.AllowGet);
        }

        [Route("ManageCategories/{eqde_guid}")]
        [Route("ManageCategories/{eqde_guid}/{isPartialView}")]
        public ActionResult ManageCategories([DataSourceRequest]DataSourceRequest request, string eqde_guid, bool isPartialView = false)
        {
            EquipmentManager eqManager = new EquipmentManager();
            EMDEquipmentDefinition equipmentDefinition = eqManager.Get(eqde_guid);

            EquipmentDefinitionModel eqModel = EquipmentDefinitionModel.Initialize(equipmentDefinition);
            eqModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!eqModel.CanManage && !eqModel.IsOwnerOfEquipment)
                return GetNoPermissionView(isPartialView);

            ManageEquipmentDefinitionCategoryModel categoryModel = new ManageEquipmentDefinitionCategoryModel();
            categoryModel.EquipmentDefinitionName = equipmentDefinition.Name;
            categoryModel.EquipmentDefinitionGuid = eqde_guid;

            CategoryManager catManager = new CategoryManager();
            CategoryEntityManager catEntityManager = new CategoryEntityManager();
            List<EMDCategory> allCategories = catManager.GetCategories(EnumCategoryType.EquipmentDefinition);

            List<EMDCategory> configuredCategories = catEntityManager.GetCategoriesForEntity(eqde_guid, EnumCategoryType.EquipmentDefinition);

            foreach (EMDCategory cat in allCategories.ToList())
            {
                foreach (EMDCategory catConfigured in configuredCategories)
                {
                    if (catConfigured.Guid == cat.Guid)
                    {
                        allCategories.Remove(cat);
                    }
                }
            }

            allCategories.ForEach(item =>
            {
                TextValueModel tvm = new TextValueModel(item.Name, item.Guid);
                categoryModel.AvailableCategories.Add(tvm);
            });

            configuredCategories.ForEach(item =>
            {
                TextValueModel tvm = new TextValueModel(item.Name, item.Guid);
                categoryModel.ConfiguredCategories.Add(tvm);
            });

            if (isPartialView)
            {
                return PartialView("ManageCategories", categoryModel);
            }
            else
            {
                return View("ManageCategories", categoryModel);
            }
        }


        [Route("DoManageCategories")]
        [Route("DoManageCategories/{eqde_guid}/{configuredOwners}")]
        //[Route("Edit")]
        public ActionResult DoManageCategories([DataSourceRequest]DataSourceRequest request, String eqde_guid, IList<TextValueModel> configuredCategories)
        {
            bool success = false;
            string errorMessage = string.Empty;

            ManageEquipmentDefinitionCategoryModel categoryModel = new ManageEquipmentDefinitionCategoryModel();
            try
            {
                EquipmentManager eqManager = new EquipmentManager();
                EMDEquipmentDefinition equipmentDefinition = eqManager.Get(eqde_guid);

                EquipmentDefinitionModel eqModel = EquipmentDefinitionModel.Initialize(equipmentDefinition);
                eqModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                if (!eqModel.CanManage && !eqModel.IsOwnerOfEquipment)
                {
                    throw new Exception(SecurityHelper.NoPermissionText);
                }

                categoryModel.EquipmentDefinitionGuid = eqde_guid;
                categoryModel.EquipmentDefinitionName = equipmentDefinition.Name;
                categoryModel.ConfiguredCategories = configuredCategories;

                CategoryManager catManager = new CategoryManager();
                List<EMDCategory> allCategories = catManager.GetCategories(EnumCategoryType.EquipmentDefinition);
                CategoryEntityManager categoryEntityManager = new CategoryEntityManager();

                List<String> configuredGuids = new List<string>();
                if (configuredCategories != null)
                {
                    foreach (EMDCategory cat in allCategories.ToList())
                    {
                        foreach (TextValueModel catConfigured in configuredCategories)
                        {
                            if (catConfigured.Value == cat.Guid)
                            {
                                allCategories.Remove(cat);
                            }
                        }
                    }

                    foreach (TextValueModel item in configuredCategories)
                    {
                        configuredGuids.Add(item.Value);
                    }
                }

                allCategories.ForEach(item =>
                {
                    TextValueModel tvm = new TextValueModel(item.Name, item.Guid);
                    categoryModel.AvailableCategories.Add(tvm);
                });

                categoryEntityManager.UpdateCategoriesForEntity(eqde_guid, EnumCategoryType.EquipmentDefinition, configuredGuids);
                success = true;
            }
            catch (Exception ex)
            {
                errorMessage = "The categories for the equipment definition could not been saved: " + ex.Message.ToString();
            }

            return Json(new { success = success, Url = MANAGEROUTE, message = "The categories for the equipment definition have been saved!", errorMessage = errorMessage });
        }

        [HttpPost]
        [Route("UpdatePrices")]
        public ActionResult UpdatePrices()
        {
            ErrorModel errorModel = null;
            bool success = false;

            CoreTransaction transaction = new CoreTransaction();

            try
            {
                EquipmentDefinitionPriceManager equipmentDefinitionPriceManager = new EquipmentDefinitionPriceManager();
                equipmentDefinitionPriceManager.UpdateAllPricesFromExternalSystem(EnumClientReferenceSystemForPrice.KBCAccountingItem);


                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success });
        }

        [HttpPost]
        [Route("GetPrice")]
        public ActionResult GetPrice(EnumClientReferenceSystemForPrice enumClientReferenceSystemForPrice, string idClientReference)
        {
            ErrorModel errorModel = null;
            bool success = false;
            ExternalPriceInfoExtended priceInfo = null;

            CoreTransaction transaction = new CoreTransaction();

            try
            {
                EquipmentDefinitionPriceManager equipmentDefinitionPriceManager = new EquipmentDefinitionPriceManager();
                priceInfo = equipmentDefinitionPriceManager.GetPriceInfo(enumClientReferenceSystemForPrice, idClientReference);

                if (priceInfo != null)
                {
                    success = true;
                }
                else
                {
                    errorModel = new ErrorModel(new Exception(string.Format("No price info found for Client Reference: {0}", idClientReference)));
                }
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success, priceInfo = priceInfo });
        }

        [HttpPost]
        [Route("UpdatePrice")]
        public ActionResult UpdatePrice(string eqdeGuid, EnumClientReferenceSystemForPrice enumClientReferenceSystemForPrice, string idClientReference)
        {
            ErrorModel errorModel = null;
            bool success = false;
            ExternalPriceInfoExtended priceInfo = null;

            CoreTransaction transaction = new CoreTransaction();

            if (string.IsNullOrWhiteSpace(idClientReference))
            {
                errorModel = new ErrorModel(new Exception("The priceId must be set!"));
            }

            if (errorModel == null)
            {
                try
                {
                    EquipmentDefinitionPriceManager equipmentDefinitionPriceManager = new EquipmentDefinitionPriceManager();
                    priceInfo = equipmentDefinitionPriceManager.UpdatePriceInfo(eqdeGuid, enumClientReferenceSystemForPrice, idClientReference);

                    if (priceInfo != null)
                    {
                        success = true;
                    }
                    else
                    {
                        errorModel = new ErrorModel(new Exception(string.Format("No price info found for Client Reference: {0}", idClientReference)));
                    }
                }
                catch (Exception ex)
                {
                    errorModel = new ErrorModel(ex);
                    logger.Error(ex, ControllerContext?.HttpContext);
                }
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success, priceInfo = priceInfo });
        }
    }
}