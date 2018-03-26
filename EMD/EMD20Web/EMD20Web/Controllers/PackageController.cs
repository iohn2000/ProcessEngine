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
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Package")]
    //[Route("{action = index}")] 
    public class PackageController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        [Route]
        // GET: Package
        public ActionResult Index()
        {
            //return View("Manage");
            return RedirectToAction("Manage");
        }

        [Route("Manage")]
        public ActionResult Manage()
        {
            PackageModel model = new PackageModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            //just return an empty view since all data is Ajax-driven
            return View("Manage", model);
        }


        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            PackageManager pm = new PackageManager();
            List<FilterRuleSubSetForCriteria> filterRules = new List<FilterRuleSubSetForCriteria>();

            //filterRules.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Company,BaseFilterAction.DENYALL,))

            List<EMDObjectContainer> packages = pm.GetFilteredListofPackages(null);

            packages = (from item in packages orderby item.Name select item).ToList();

            PackageModel dummySecurityModel = new PackageModel();
            dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            List<PackageModel> packageModels = new List<PackageModel>();

            foreach (EMDObjectContainer package in packages)
            {
                PackageModel packageModel = new PackageModel(package);
                packageModel.CanManage = dummySecurityModel.CanManage;
                packageModel.CanView = dummySecurityModel.CanView;
                packageModels.Add(packageModel);
            }

            return Json(packageModels.ToDataSourceResult(request));
            //return Json(packages.ToDataSourceResult(request));
        }

        [Route("Create")]
        [Route("Create/{isPartialView}")]
        [HttpGet]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, bool isPartialView = false)
        {
            PackageModel pm = new PackageModel();
            pm.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!pm.CanManage)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("Create", pm);
            }
            else
            {
                return View("Create", pm);
            }
        }

        /// <summary>
        /// deletes package and related basepackage and all related filter rules
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("DeletePackage")]
        [HttpPost]
        public ActionResult DeletePackage(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            CoreTransaction transi = new CoreTransaction();
            transi.Begin();

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.PackageManagement_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                PackageManager pm = new PackageManager(transi, this.PersonGuid);

                // delete base package if exits plus filterules
                pm.DeleteBasePackage(guid, transi);
                // then package
                pm.DeletePackage(guid, transi);

                transi.Commit();
                success = true;
            }
            catch (BaseException bEx)
            {
                transi.Rollback();
                throw bEx;
            }
            catch (Exception ex)
            {
                transi.Rollback();
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success });
        }

        [Route("DoCreate")]
        [HttpPost]
        public ActionResult DoCreate([DataSourceRequest] DataSourceRequest request, PackageModel pModel)
        {
            Exception handledException = null;
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (pModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!pModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    PackageManager pm = new PackageManager(this.PersonGuid);
                    EMDObjectContainer oc = new EMDObjectContainer();

                    oc.Description = pModel.Description;
                    oc.Name = pModel.Name;
                    oc.ObjectKey = "EQDE"; // whats in the container? --> EQ Defs --> its a package !
                    oc.Type = "??"; //TODO: DISCUSS Was kommt in den Type des ObjectContainers?

                    List<FilterRuleSubSetForCriteria> subSets = new List<FilterRuleSubSetForCriteria>();
                    if (pModel.RuleFilterModel.Enterprises != null && pModel.RuleFilterModel.Enterprises.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Company, getBaseFilterActionFromFlag(pModel.RuleFilterModel.EnterpriseInvertFlag), pModel.RuleFilterModel.Enterprises, !pModel.RuleFilterModel.EnteIsNotInherited));
                    }

                    if (pModel.RuleFilterModel.Locations != null && pModel.RuleFilterModel.Locations.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Location, getBaseFilterActionFromFlag(pModel.RuleFilterModel.LocationInvertFlag), pModel.RuleFilterModel.Locations));
                    }

                    if (pModel.RuleFilterModel.Accounts != null && pModel.RuleFilterModel.Accounts.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.CostCenter, getBaseFilterActionFromFlag(pModel.RuleFilterModel.AccountInvertFlag), pModel.RuleFilterModel.Accounts));
                    }

                    if (pModel.RuleFilterModel.EmploymentTypes != null && pModel.RuleFilterModel.EmploymentTypes.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.EmploymentType, getBaseFilterActionFromFlag(pModel.RuleFilterModel.EmploymentTypeInvertFlag), pModel.RuleFilterModel.EmploymentTypes));
                    }

                    if (pModel.RuleFilterModel.UserTypes != null && pModel.RuleFilterModel.UserTypes.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.UserType, getBaseFilterActionFromFlag(pModel.RuleFilterModel.UserTypeInvertFlag), pModel.RuleFilterModel.UserTypes));
                    }

                    oc = pm.CreatePackage(oc, subSets);



                    // create base and location packages
                    if (pModel.Base == true)
                    {
                        if (pModel.RuleFilterModel.EnterpriseInvertFlag == true)
                        {
                            errmsg = "The Package could not be added: no all but supported for Base Package";
                            ModelState.AddModelError("error", errmsg);
                        }
                        else pm.CreateBasePackage(oc.Guid, "ENTE", pModel.RuleFilterModel.Enterprises, null, null, null, pModel.RuleFilterModel.EnterpriseInvertFlag, !pModel.RuleFilterModel.EnteIsNotInherited);
                    }
                    if (pModel.Access == true)
                    {
                        if (pModel.RuleFilterModel.LocationInvertFlag == true)
                        {
                            errmsg = "The Package could not be added: no all but supported for Base Package";
                            ModelState.AddModelError("error", errmsg);
                        }
                        else pm.CreateBasePackage(oc.Guid, "LOCA", null, pModel.RuleFilterModel.Locations, null, null, pModel.RuleFilterModel.LocationInvertFlag);
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Package could not be added: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", pModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Package has been created!" });
            }

        }

        [HttpGet]
        [Route("View")]
        [Route("View/{pack_Guid}")]
        [Route("View/{pack_Guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string pack_Guid, bool isPartialView = false)
        {
            ObjectContainerHandler och = new ObjectContainerHandler();
            EMDObjectContainer oc = (EMDObjectContainer)och.GetObject<EMDObjectContainer>(pack_Guid);

            PackageModel pModel = new PackageModel(oc);
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!pModel.CanView)
                return GetNoPermissionView(isPartialView);

            pModel.Name = oc.Name;
            pModel.Description = oc.Description;

            if (!string.IsNullOrEmpty(oc.Guid))
            {
                pModel.RuleFilterModel = ObjectHelper.GetRuleFilterModel(oc.Guid);
            }

            if (isPartialView)
            {
                return PartialView("View", pModel);
            }
            else
            {
                return View("View", pModel);
            }
        }

        [Route("Edit/{pack_Guid}")]
        [Route("Edit/{pack_Guid}/{isPartialView}")]
        //[Route("Edit")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string pack_Guid, bool isPartialView = false)
        {
            ObjectContainerHandler och = new ObjectContainerHandler();
            EMDObjectContainer oc = (EMDObjectContainer)och.GetObject<EMDObjectContainer>(pack_Guid);

            PackageModel pModel = new PackageModel(oc);
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!pModel.CanManage)
                return GetNoPermissionView(isPartialView);

            pModel.Name = oc.Name;
            pModel.Description = oc.Description;


            if (!string.IsNullOrEmpty(oc.Guid))
            {
                pModel.RuleFilterModel = ObjectHelper.GetRuleFilterModel(oc.Guid);
            }


            List<EMDBaseContainer> packageslocations = new BaseContainerHandler().GetObjects<EMDBaseContainer, BaseContainer>("BACOPrefix == \"LOCA\"").Cast<EMDBaseContainer>().ToList(); // new PackageManager().GetBasePackages("LOCA", new FilterCriteria());
            List<EMDBaseContainer> packagesEnterprises = new BaseContainerHandler().GetObjects<EMDBaseContainer, BaseContainer>("BACOPrefix == \"ENTE\"").Cast<EMDBaseContainer>().ToList(); // new PackageManager().GetBasePackages("LOCA", new FilterCriteria());


            pModel.Base = packagesEnterprises.Exists(a => a.OBCOGuid == pModel.Guid);
            pModel.Access = packageslocations.Exists(a => a.OBCOGuid == pModel.Guid);



            if (isPartialView)
            {
                return PartialView("Edit", pModel);
            }
            else
            {
                return View("Edit", pModel);
            }
        }

        private bool getBaseFilterActionFlagFromString(string baseFilterAction)
        {
            if (baseFilterAction == BaseFilterAction.ALLOWALL)
                return true;
            else if (baseFilterAction == BaseFilterAction.DENYALL)
                return false;
            else
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "The BaseFilterAction was expected to be true or false but was: " + baseFilterAction);
        }

        [Route("DoEdit")]
        //[Route("DoEdit/{pModel}")]
        [HttpPost]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, PackageModel pModel)
        {
            Exception handledException = null;
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (pModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!pModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    PackageManager pm = new PackageManager(this.PersonGuid);
                    ObjectContainerHandler och = new ObjectContainerHandler(this.PersonGuid);
                    EMDObjectContainer oc = (EMDObjectContainer)och.GetObject<EMDObjectContainer>(pModel.Guid);

                    oc.Description = pModel.Description;
                    oc.Name = pModel.Name;
                    oc.ObjectKey = "EQDE"; // whats in the container? --> EQ Defs --> its a package !
                    oc.Type = "??"; //TODO: DISCUSS Was kommt in den Type des ObjectContainers?

                    List<FilterRuleSubSetForCriteria> subSets = new List<FilterRuleSubSetForCriteria>();
                    if (pModel.RuleFilterModel.Enterprises != null && pModel.RuleFilterModel.Enterprises.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Company, getBaseFilterActionFromFlag(pModel.RuleFilterModel.EnterpriseInvertFlag), pModel.RuleFilterModel.Enterprises, !pModel.RuleFilterModel.EnteIsNotInherited));
                    }

                    if (pModel.RuleFilterModel.Locations != null && pModel.RuleFilterModel.Locations.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.Location, getBaseFilterActionFromFlag(pModel.RuleFilterModel.LocationInvertFlag), pModel.RuleFilterModel.Locations));
                    }

                    if (pModel.RuleFilterModel.Accounts != null && pModel.RuleFilterModel.Accounts.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.CostCenter, getBaseFilterActionFromFlag(pModel.RuleFilterModel.AccountInvertFlag), pModel.RuleFilterModel.Accounts));
                    }

                    if (pModel.RuleFilterModel.EmploymentTypes != null && pModel.RuleFilterModel.EmploymentTypes.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.EmploymentType, getBaseFilterActionFromFlag(pModel.RuleFilterModel.EmploymentTypeInvertFlag), pModel.RuleFilterModel.EmploymentTypes));
                    }

                    if (pModel.RuleFilterModel.UserTypes != null && pModel.RuleFilterModel.UserTypes.Count > 0)
                    {
                        subSets.Add(new FilterRuleSubSetForCriteria(EnumFilterCriteria.UserType, getBaseFilterActionFromFlag(pModel.RuleFilterModel.UserTypeInvertFlag), pModel.RuleFilterModel.UserTypes));
                    }

                    pm.UpdateBasePackage(oc.Guid, pModel.Base, pModel.Access, subSets);
                    oc = pm.UpdatePackage(oc, subSets);

                    // TODO: ADD/REMOVE packages
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Package: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", pModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The package has been updated!" });
            }
        }

        [Route("ManageEquipment/{pack_guid}")]
        [Route("ManageEquipment/{pack_guid}/{isPartialView}")]
        public ActionResult ManageEquipment([DataSourceRequest]DataSourceRequest request, string pack_guid, bool isPartialView = false)
        {

            EmploymentManager emplMngr = new EmploymentManager();
            PackageManager packMngr = new PackageManager();
            ObjectContainerHandler ocHandler = new ObjectContainerHandler();
            EMDObjectContainer package = (EMDObjectContainer)ocHandler.GetObject<EMDObjectContainer>(pack_guid);

            PackageModel pModel = new PackageModel(package);
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!pModel.CanManage)
                return GetNoPermissionView(isPartialView);

            ManagePackageEquipmentModel peModel = new ManagePackageEquipmentModel();
            peModel.PackageGuid = package.Guid;
            peModel.PackageName = package.Name;

            List<FilterRuleSubSetForCriteria> filterRules = new List<FilterRuleSubSetForCriteria>();
            List<EMDEquipmentDefinition> equipmentsAvailable = packMngr.GetAvailableEquipmentDefinitionsForPackage(pack_guid, null);

            equipmentsAvailable = (from item in equipmentsAvailable orderby item.Name ascending select item).ToList();

            List<EMDEquipmentDefinition> equipmentsConfigured = packMngr.GetConfiguredEquipmentDefinitionsForPackage(pack_guid);

            equipmentsConfigured = (from item in equipmentsConfigured orderby item.Name ascending select item).ToList();

            foreach (EMDEquipmentDefinition equDef in equipmentsAvailable)
            {
                TextValueModel tvm = new TextValueModel(equDef.Name, equDef.Guid);
                peModel.AvailableEquipments.Add(tvm);
            }

            foreach (EMDEquipmentDefinition equDef in equipmentsConfigured)
            {
                TextValueModel tvm = new TextValueModel(equDef.Name, equDef.Guid);
                peModel.ConfiguredEquipments.Add(tvm);
            }

            if (isPartialView)
            {
                return PartialView("ManageEquipment", peModel);
            }
            else
            {
                return View("ManageEquipment", peModel);
            }
        }

        [Route("DoManageEquipment")]
        [Route("DoManageEquipment/{pack_guid}/{configuredEquipments}")]
        //[Route("Edit")]
        public ActionResult DoManageEquipment([DataSourceRequest]DataSourceRequest request, String pack_guid, IList<TextValueModel> configuredEquipments)
        {

            bool success = false;
            string errorMessage = string.Empty;

            ManagePackageEquipmentModel peModel = new ManagePackageEquipmentModel();
            try
            {
                PackageManager packMngr = new PackageManager(this.PersonGuid);

                ObjectContainerHandler ocHandler = new ObjectContainerHandler();
                EMDObjectContainer package = (EMDObjectContainer)ocHandler.GetObject<EMDObjectContainer>(pack_guid);
                PackageModel packageModel = new PackageModel(package);
                packageModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                if (!packageModel.CanManage)
                {
                    throw new Exception(SecurityHelper.NoPermissionText);
                }

                peModel.PackageGuid = package.Guid;
                peModel.PackageName = package.Name;
                peModel.ConfiguredEquipments = configuredEquipments;

                List<EMDEquipmentDefinition> equipmentsAvailable = packMngr.GetAvailableEquipmentDefinitionsForPackage(pack_guid, null);

                equipmentsAvailable = (from item in equipmentsAvailable orderby item.Name ascending select item).ToList();
                foreach (EMDEquipmentDefinition equDef in equipmentsAvailable)
                {
                    TextValueModel tvm = new TextValueModel(equDef.Name, equDef.Guid);
                    peModel.AvailableEquipments.Add(tvm);
                }

                List<String> configuredGuids = new List<string>();
                foreach (TextValueModel item in configuredEquipments)
                {
                    configuredGuids.Add(item.Value);
                }
                packMngr.UpdateEquipmentDefinitionsForPackage(pack_guid, configuredGuids);
                success = true;
            }
            catch (Exception ex)
            {
                errorMessage = "The equipments for the package could not been saved: " + ex.Message.ToString();

            }


            return Json(new { success = success, Url = MANAGEROUTE, message = "The equipments for the package have been saved!", errorMessage = errorMessage });

        }

        [Route("ViewManageEquipment/{pack_guid}")]
        [Route("ViewManageEquipment/{pack_guid}/{isPartialView}")]
        public ActionResult ViewManageEquipment([DataSourceRequest]DataSourceRequest request, string pack_guid, bool isPartialView = false)
        {

            EmploymentManager emplMngr = new EmploymentManager();
            PackageManager packMngr = new PackageManager();
            ObjectContainerHandler ocHandler = new ObjectContainerHandler();
            EMDObjectContainer package = (EMDObjectContainer)ocHandler.GetObject<EMDObjectContainer>(pack_guid);

            PackageModel pModel = new PackageModel(package);
            pModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!pModel.CanView)
                return GetNoPermissionView(isPartialView);

            ManagePackageEquipmentModel peModel = new ManagePackageEquipmentModel();
            peModel.PackageGuid = package.Guid;
            peModel.PackageName = package.Name;

            List<FilterRuleSubSetForCriteria> filterRules = new List<FilterRuleSubSetForCriteria>();
            List<EMDEquipmentDefinition> equipmentsAvailable = packMngr.GetAvailableEquipmentDefinitionsForPackage(pack_guid, null);

            equipmentsAvailable = (from item in equipmentsAvailable orderby item.Name ascending select item).ToList();

            List<EMDEquipmentDefinition> equipmentsConfigured = packMngr.GetConfiguredEquipmentDefinitionsForPackage(pack_guid);

            equipmentsConfigured = (from item in equipmentsConfigured orderby item.Name ascending select item).ToList();

            foreach (EMDEquipmentDefinition equDef in equipmentsAvailable)
            {
                TextValueModel tvm = new TextValueModel(equDef.Name, equDef.Guid);
                peModel.AvailableEquipments.Add(tvm);
            }

            foreach (EMDEquipmentDefinition equDef in equipmentsConfigured)
            {
                TextValueModel tvm = new TextValueModel(equDef.Name, equDef.Guid);
                peModel.ConfiguredEquipments.Add(tvm);
            }

            if (isPartialView)
            {
                return PartialView("ViewManageEquipment", peModel);
            }
            else
            {
                return View("ViewManageEquipment", peModel);
            }
        }

        private String getBaseFilterActionFromFlag(bool InvertFlag)
        {
            if (InvertFlag)
                return BaseFilterAction.ALLOWALL;
            else
                return BaseFilterAction.DENYALL;
        }



        [Route("ReadPackagesForEmployment")]
        public ActionResult ReadPackagesForEmployment([DataSourceRequest]DataSourceRequest request, string EP_Guid)
        {
            //TODO Demo Implementierung
            List<EMDObjectContainer> packageModels = new List<EMDObjectContainer>();
            return Json(packageModels.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [Route("ReadAvailableListOfPackagesForEmployment/{empl_guid}")]
        public ActionResult ReadAvailableListOfPackagesForEmployment([DataSourceRequest]DataSourceRequest request, string empl_guid)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                EmploymentManager empMgr = new EmploymentManager();
                List<EMDObjectContainer> availablePackages = empMgr.GetAvailableListOfPackagesForEmployment(empl_guid);


                availablePackages.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDObjectContainer)entity).Name, entity.Guid, entity.Description));
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading roles";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadPackagesForEmploymentAvailableToAdd")]
        public ActionResult ReadPackagesForEmploymentAvailableToAdd([DataSourceRequest]DataSourceRequest request, string EP_Guid)
        {
            //TODO Hier alles überarbeiten
            List<EMDObjectContainer> packageModels = new List<EMDObjectContainer>();
            packageModels = packageModels.OrderBy(p => p.Name).ToList();
            return Json(packageModels.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [Route("AddPackageToEmployment")]
        public ActionResult AddPackageToEmployment([DataSourceRequest]DataSourceRequest request, string EP_Guid, string BR_Guid)
        {
            return Json(new[] { " " });
        }

        [Route("ReadEquipmentsForEmployment")]
        public ActionResult ReadEquipmentsForEmployment([DataSourceRequest]DataSourceRequest request, string EP_Guid)
        {
            return Json(new[] { " " });
        }

        [Route("CreateRuleFilter")]
        [Route("CreateRuleFilter/{isPartialView}")]
        public ActionResult CreateRuleFilter(bool isPartialView = false)
        {
            //just return an empty view since all data is Ajax-driven
            PackageModel pm = new PackageModel();

            if (isPartialView)
            {
                return PartialView("CreateRuleFilter", pm);
            }
            else
            {
                return View("CreateRuleFilter", pm);
            }
        }


        [Route("ReadForSelectForEnterprisePackages")]
        [Route("ReadForSelectForEnterprisePackages/{ente_guid}")]
        public ActionResult ReadForSelectForEnterprisePackages(string ente_guid, bool isOnboarding = false)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            if (!string.IsNullOrEmpty(ente_guid))
            {
                try
                {
                    PackageManager packageManager = new PackageManager();
                    List<EMDObjectContainer> enterprisePackages = packageManager.GetBasePackages("ENTE", ente_guid, null, null, isOnboarding ? GetDefaultUsertypesForOnboarding() : null);

                    enterprisePackages.ForEach(entity =>
                    {
                        keyValuePairs.Add(new TextValueModel(((EMDObjectContainer)entity).Name, entity.Guid));
                    });

                    keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();

                }
                catch (Exception ex)
                {
                    string errorMessage = "Error reading packages";
                    var error = new ErrorModel(ex, errorMessage);
                    logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                    return this.Json(new DataSourceResult
                    {
                        Errors = error
                    });
                }
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadForSelectForLocationPackages")]
        [Route("ReadForSelectForLocationPackages/{loca_guid}")]
        public ActionResult ReadForSelectForLocationPackages(string loca_guid, bool isOnboarding = false)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            if (!string.IsNullOrEmpty(loca_guid))
            {
                try
                {
                    PackageManager packageManager = new PackageManager();
                    List<EMDObjectContainer> locationPackages = packageManager.GetBasePackages("LOCA", null, loca_guid, null, isOnboarding ? GetDefaultUsertypesForOnboarding() : null);

                    locationPackages.ForEach(entity =>
                    {
                        keyValuePairs.Add(new TextValueModel(((EMDObjectContainer)entity).Name, entity.Guid));
                    });

                    keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();

                }
                catch (Exception ex)
                {
                    string errorMessage = "Error reading packages";
                    var error = new ErrorModel(ex, errorMessage);
                    logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                    return this.Json(new DataSourceResult
                    {
                        Errors = error
                    });
                }
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }


        public static List<string> GetDefaultUsertypesForOnboarding()
        {
            List<string> result = new List<string>();

            result.Add(EnumUserType.ADUserFullAccount.ToString());
            result.Add(EnumUserType.ADUserLimitedAccount.ToString());

            return result;
        }

    }
}