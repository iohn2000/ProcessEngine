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

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Category")]
    public class CategoryController : BaseController
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
            CategoryModel model = new CategoryModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage)
                return GetNoPermissionView(false);

            //just return an empty view since all data is Ajax-driven
            return View("Manage", model);
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Add")]
        [Route("Add/{isPartialView}")]
        [Route("Edit/{guid}/{isPartialView}")]
        [Route("Edit/{guid}")]
        public ActionResult Edit(string guid, bool isPartialView = false)
        {
            CategoryModel mappedModel = new CategoryModel();
            mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            if (!mappedModel.CanManage)
                return GetNoPermissionView(isPartialView);

            try
            {
                if (!string.IsNullOrEmpty(guid))
                {
                    ViewBag.Titel = "Edit Category";
                    CategoryManager categoryManager = new CategoryManager();
                    EMDCategory emdCategory = categoryManager.Get(guid);
                    mappedModel = CategoryModel.Initialize(emdCategory);
                }
                else
                {
                    ViewBag.Titel = "Add new Category";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            EquipmentManager equipmentManager = new EquipmentManager();
            List<EMDEquipmentDefinition> availableEquipmentDefinitions = equipmentManager.GetAllEquipmentDefinitions();

            foreach (EMDEquipmentDefinition equDef in availableEquipmentDefinitions)
            {
                TextValueModel tvm = new TextValueModel(equDef.Name, equDef.Guid);
                mappedModel.AvailableEquipments.Add(tvm);
            }

            CategoryEntityManager categoryEntityManager = new CategoryEntityManager();
            List<EMDCategoryEntity> configuredCategoryEntities = categoryEntityManager.GetEntitiesForCategory(guid, EnumCategoryType.EquipmentDefinition);

            configuredCategoryEntities.ForEach(item =>
            {
                EMDEquipmentDefinition emdEquipmentDefinition = equipmentManager.Get(item.EntityGuid);
                TextValueModel tvm = new TextValueModel(emdEquipmentDefinition.Name, emdEquipmentDefinition.Guid);
                mappedModel.ConfiguredEquipments.Add(tvm);
            });

            foreach (TextValueModel item in mappedModel.ConfiguredEquipments)
            {
                var foundItem = (from a in mappedModel.AvailableEquipments where a.Value == item.Value select a).FirstOrDefault();

                if (foundItem != null)
                {
                    mappedModel.AvailableEquipments.Remove(foundItem);
                }
            }
            

            if (isPartialView)
            {
                return PartialView("Edit", mappedModel);
            }
            else
            {
                return View("Edit", mappedModel);
            }
        }


        [ValidateInput(false)]
        [HttpPost]
        [Route("Edit")]
        public ActionResult Edit(CategoryModel categoryModel, IList<TextValueModel> configuredEquipments)
        {
            Exception handledException = null;
            string message = string.Empty;
            bool success = false;
            string errorMessage = string.Empty;
            string categoryGuid = string.Empty;
            CoreTransaction transaction = new CoreTransaction();
            
            if (categoryModel != null && ModelState.IsValid)
            {
                try
                {
                    categoryModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                    if (!categoryModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    CategoryManager categoryManager = new CategoryManager(transaction, this.PersonGuid);

                    transaction.Begin();
                    if (!string.IsNullOrEmpty(categoryModel.Guid))
                    {
                        ViewBag.Title = "Edit Category";
                        categoryGuid = categoryModel.Guid;
                        EMDCategory emdCategory = categoryManager.Get(categoryModel.Guid);
                        emdCategory = CategoryModel.Update(emdCategory, categoryModel);
                        categoryManager.Update(emdCategory);
                        
                        CategoryEntityManager categoryEntityManager = new CategoryEntityManager(transaction, this.PersonGuid);
                        List<EMDCategoryEntity> currentAssignedCategoryEntities = categoryEntityManager.GetEntitiesForCategory(categoryModel.Guid, EnumCategoryType.EquipmentDefinition);

                        // search for removed equipments
                        foreach (EMDCategoryEntity currentAssignedCategoryEntity in currentAssignedCategoryEntities)
                        {
                            var items = (from a in categoryModel.ConfiguredEquipments where a.Value == currentAssignedCategoryEntity.EntityGuid select a).ToList();
                            if (items.Count == 0)
                            {
                                categoryEntityManager.Delete(currentAssignedCategoryEntity.Guid);
                            }
                        }

                        // search for newly added equipmentDefinitions
                        if (configuredEquipments != null)
                        {
                            foreach (TextValueModel configuredUiEquipment in configuredEquipments)
                            {
                                if (!currentAssignedCategoryEntities.Exists(a => a.EntityGuid == configuredUiEquipment.Value))
                                {
                                    EMDCategoryEntity emdCategoryEntity = new EMDCategoryEntity();
                                    emdCategoryEntity.CATE_Guid = categoryModel.Guid;
                                    emdCategoryEntity.EntityGuid = configuredUiEquipment.Value;

                                    categoryEntityManager.Create(emdCategoryEntity);
                                }
                            }
                        }

                        message = "The Category has been updated!";
                        success = true;
                    }
                    else
                    {
                        ViewBag.Title = "Add new Category";
                        EMDCategory emdCategory = CategoryModel.Map(categoryModel);
                        emdCategory = categoryManager.Create(emdCategory);

                        // search for newly added equipments
                        foreach (TextValueModel configuredUiEquipment in categoryModel.ConfiguredEquipments)
                        {
                                EMDCategoryEntity emdCategoryEntity = new EMDCategoryEntity();
                                emdCategoryEntity.CATE_Guid = emdCategory.Guid;
                                emdCategoryEntity.EntityGuid = configuredUiEquipment.Value;

                                CategoryEntityManager categoryEntityManager = new CategoryEntityManager(transaction, this.PersonGuid);
                                categoryEntityManager.Create(emdCategoryEntity);
                        }

                        message = "The Category has been created!";
                        success = true;
                        categoryGuid = emdCategory.Guid;
                    }
                    transaction.Commit();
                }

                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    //TODO: write HelperMethod for generalizing this kind of handling
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                    logger.Error(ex, ControllerContext?.HttpContext);
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", categoryModel, handledException, "The Category couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { success = success, Url = MANAGEROUTE, message = message, errorMessage = errorMessage, categoryGuid = categoryGuid});
            }
        }

        /// <summary>
        /// Gets a json object for a list of categoryModels for a datagrid
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            List<CategoryModel> categoryList = GetCategoryList();
            DataSourceResult myresult;
            myresult = categoryList.ToDataSourceResult(request, ModelState);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets a list of categoryModels
        /// </summary>
        /// <param name="includePseudoCategories">Defines if the pseudo-categories "All" and "Others" are in the result</param>
        /// <returns><seealso cref="List{CategoryModel}"/></returns>
        public List<CategoryModel> GetCategoryList(bool includePseudoCategories = false)
        {
            List<CategoryModel> myresult = new List<CategoryModel>();
            CategoryManager categoryManager = new CategoryManager();
            try
            {
                CategoryModel dummySecurityModel = CategoryModel.Initialize(new EMDCategory());
                dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                List<EMDCategory> categories = categoryManager.GetCategories(EnumCategoryType.EquipmentDefinition, includePseudoCategories);

                categories.ForEach(cat =>
                {
                    CategoryModel model = CategoryModel.Initialize((EMDCategory)cat);
                    model.CanManage = dummySecurityModel.CanManage;
                    model.CanView = dummySecurityModel.CanView;
                    model.IsAdmin = dummySecurityModel.IsAdmin;
                    myresult.Add(model);
                }
                );

                myresult = myresult.OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading categories", e);
            }
            return myresult;
        }

        /// <summary>
        /// Returns a list of all the categoryTypes <see cref="Kapsch.IS.EDP.Core.Entities.EnumCategoryType"/>
        /// </summary>
        /// <returns></returns>
        [Route("GetCategoryTypes")]
        public ActionResult GetCategoryTypes()
        {
            List<EnumCategoryType> categoryTypeEnums = Enum.GetValues(typeof(EnumCategoryType)).Cast<EnumCategoryType>().ToList();
            List<TextValueModel> listCategoryTypeEnums = new List<TextValueModel>();
            foreach (EnumCategoryType categoryTypeEnum in categoryTypeEnums)
            {
                listCategoryTypeEnums.Add(new TextValueModel(categoryTypeEnum.ToString(), ((int)categoryTypeEnum).ToString()));
            }
            return Json(listCategoryTypeEnums, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("Delete")]
        public ActionResult Delete(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;
            
            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.CategoryManagement_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);
                
                CategoryManager categoryManager = new CategoryManager(this.PersonGuid);
                SecurityUser secUser = this.GetSecurityUserFromCache();
                categoryManager.Delete(guid, secUser.IsAdmin);

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
    }
}