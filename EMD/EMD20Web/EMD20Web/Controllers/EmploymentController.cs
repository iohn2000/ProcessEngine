using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Employment")]
    public class EmploymentController : BaseController
    {
        internal new IISLogger logger = ISLogger.GetLogger("EmploymentController");
        public const string INFINITY = "INFINITY";

        [Route()]
        public ActionResult Index()
        {
            //PopulateGenders();
            return View();
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{guid}/{isPartialView}")]
        [Route("Edit/{guid}")]
        public ActionResult Edit(string guid, bool isPartialView = false)
        {
            EmploymentModel mappedModel = new EmploymentModel();
            ViewBag.Titel = "Edit Employment";
            try
            {
                if (!string.IsNullOrEmpty(guid))
                {
                    ViewBag.Titel = "Edit Employment";

                    EMDEmployment emdEmployment = (EMDEmployment)new EmploymentHandler().GetObject<EMDEmployment>(guid);
                    mappedModel = new EmploymentModel(emdEmployment);

                    mappedModel.EnterpriseName = new EnterpriseManager().Get(mappedModel.ENTE_Guid).NameShort;

                    mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                }
                else
                {
                    ViewBag.Titel = "Add new User";
                }

                mappedModel.SponsorSelection = new Models.Shared.SelectionViewModel()
                {
                    ReferencePropertyName = "Sponsor_Guid",
                    ObjectLabel = "Sponsor",
                    ObjectValue = mappedModel.Sponsor_Guid,
                    ObjectText = new PersonManager().getFullDisplayNameWithUserIdAndPersNr(mappedModel.Sponsor_Guid),
                    TargetControllerMethodName = "GetEmploymentList",
                    TargetControllerName = "Employment"
                };
            }
            catch (Exception ex)
            {
                //TODO: write HelperMethod for generalizing this kind of handling

                logger.Error(ex, ControllerContext?.HttpContext);
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
        public ActionResult Edit(EmploymentModel employmentModel)
        {
            Exception handledException = null;
            string message = string.Empty;
            bool doSiteReload = false;

            if (employmentModel != null && ModelState.IsValid)
            {
                try
                {
                    EmploymentHandler emplHandler = new EmploymentHandler(this.PersonGuid, MODIFY_COMMENT);
                    EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(employmentModel.Guid);
                    EmploymentModel employmentModelPrevious = new EmploymentModel(empl);



                    employmentModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                    if (!string.IsNullOrEmpty(employmentModel.Guid))
                    {
                        bool hasChange = false;
                        ObjectFlagManager ofm = new ObjectFlagManager(this.PersonGuid);
                        if (employmentModel.CanManageMainEmployment)
                        {
                            if (employmentModelPrevious.Main != employmentModel.Main)
                            {


                                EmploymentManager employmentManager = new EmploymentManager(this.PersonGuid, MODIFY_COMMENT);
                                EMDEmployment currentEmployment = employmentManager.GetMainEploymentForPerson(employmentModel.P_Guid);

                                if (currentEmployment.Guid != employmentModel.Guid)
                                {
                                    employmentManager.ChangeMainEmployment(currentEmployment.Guid, employmentModel.Guid);
                                    hasChange = true;

                                    doSiteReload = true;
                                }
                            }
                            else
                            {
                                hasChange = true;
                                ofm.SetMainEmployment(employmentModel.Guid, employmentModel.Main);
                            }
                        }

                        if (employmentModel.CanManageVisibleInPhonebook)
                        {
                            if (employmentModelPrevious.Visible != employmentModel.Visible)
                            {
                                hasChange = true;
                                ofm.UpdateIsEmploymentVisibleInPhonebook(employmentModel.Guid, employmentModel.Visible);
                            }
                        }

                        if (employmentModel.CanManageAdUpdate)
                        {
                            if (employmentModelPrevious.AD_Update != employmentModel.AD_Update)
                            {
                                hasChange = true;
                                ofm.UpdateIsAD(employmentModel.Guid, employmentModel.AD_Update);
                            }
                        }

                        if (employmentModel.CanManagePersNr)
                        {
                            if (employmentModel.PersNr != null && empl.PersNr.Trim() != employmentModel.PersNr.Trim())
                            {
                                empl.PersNr = employmentModel.PersNr;
                                hasChange = true;
                            }
                        }

                        if (employmentModel.CanManageSponsor || employmentModel.CanManageDistributionGroup)
                        {
                            if (employmentModel.CanManageSponsor)
                            {
                                if (empl.Sponsor_Guid != employmentModel.Sponsor_Guid)
                                {
                                    empl.Sponsor_Guid = employmentModel.Sponsor_Guid;
                                    hasChange = true;
                                }
                            }

                            if (employmentModel.CanManageDistributionGroup)
                            {
                                DistributionGroupHandler distributionGroupHandler = new DistributionGroupHandler();
                                EMDDistributionGroup employmentType = (EMDDistributionGroup)distributionGroupHandler.GetObject<EMDDistributionGroup>(employmentModel.DGT_Guid);
                                if (employmentType != null)
                                {
                                    if (empl.DGT_Guid != employmentModel.DGT_Guid)
                                    {
                                        empl.DGT_ID = employmentType.DGT_ID;
                                        empl.DGT_Guid = employmentModel.DGT_Guid;
                                        hasChange = true;
                                    }
                                }
                                else
                                {
                                    throw new Exception(string.Format("The DistributionGroup for GUID: {0} could not be found. Saving not possible", employmentModel.DGT_Guid));
                                }

                            }
                        }

                        if (employmentModel.CanManageExitDate)
                        {
                            if (employmentModel.ExitAsString != employmentModelPrevious.ExitAsString && employmentModel.ExitAsString.ToUpper() == INFINITY)
                            {
                                empl.Exit = EMDEmployment.INFINITY;
                                hasChange = true;
                            }

                            if (employmentModel.LastDayAsString != employmentModelPrevious.LastDayAsString && employmentModel.LastDayAsString.ToUpper() == INFINITY)
                            {
                                empl.LastDay = EMDEmployment.INFINITY;
                                hasChange = true;
                            }
                        }

                        if (hasChange)
                        {
                            emplHandler.UpdateObject<EMDEmployment>(empl);
                            message = "The employment has been updated!";
                        }
                        else
                        {
                            message = "No changes were detected. Nothing updated.";
                        }
                    }
                    else
                    {
                        throw new Exception("Guid is null - Flag updating for employment is not possible");
                    }

                }

                catch (Exception ex)
                {
                    handledException = ex;
                    ModelState.AddModelError("error", string.Format("Could not {0}!<br>Technical Exception: {1}", ViewBag.Title, ex.Message));
                    logger.Error(ex, ControllerContext?.HttpContext);
                }

            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", employmentModel, handledException, "The Employment Assignements couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = message, employment = employmentModel, doSiteReload = doSiteReload });
            }
        }

        [Route("ReadForSettings")]
        [Route("ReadForSettings/{pers_guid}")]
        public ActionResult ReadForSettings([DataSourceRequest]DataSourceRequest request, string pers_guid, bool deliverInActive = false)
        {
            List<EmploymentModel> listModels = new List<EmploymentModel>();
            SecurityUser secUser = GetSecurityUserFromCache();
            try
            {
                EmploymentHandler eh = new EmploymentHandler();
                EmploymentManager employmentManager = new EmploymentManager();
                List<EMDEmployment> listEmployments = new List<EMDEmployment>();
                List<EMDEmployment> tempListEmployments = employmentManager.GetEmploymentsForPerson(pers_guid, deliverInActive);

                foreach (var employment in tempListEmployments)
                {
                    if (employment.ActiveTo > DateTime.Now || secUser.hasPermission(SecurityPermission.Personprofile_View_Settings_View_Employments_View_Extended, new SecurityUserParameterFlags(isLineManager: true, checkPlainPermisson: true), null, emplGuid: employment.Guid))
                    {
                        listEmployments.Add(employment);
                    }
                }


                List<EMDEmploymentType> employmentTypes = new EmploymentTypeHandler() { DeliverInActive = true, Historical = true }.GetObjects<EMDEmploymentType, EmploymentType>().Cast<EMDEmploymentType>().ToList();


                EnterpriseHandler enth = new EnterpriseHandler();


                ObjectFlagManager ofm = new ObjectFlagManager(this.PersonGuid);

                foreach (EMDEmployment emp in listEmployments)
                {

                    EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
                    DistributionGroupHandler distGrpHandler = new DistributionGroupHandler();

                    EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(emp.ENLO_Guid);

                    if (enlo != null)
                    {
                        try
                        {
                            EMDEnterprise enterprise = (EMDEnterprise)enth.GetObject<EMDEnterprise>(enlo.E_Guid);
                            EMDDistributionGroup distributionGroup = (EMDDistributionGroup)distGrpHandler.GetObject<EMDDistributionGroup>(emp.DGT_Guid);




                            EmploymentModel empModel = new EmploymentModel(emp);
                            empModel.EmploymentTypeName = employmentTypes?.FirstOrDefault(e => e.Guid == emp.ET_Guid).Name;
                            empModel.InitializeSecurity(secUser);
                            if (enterprise != null)
                            {
                                empModel.EnterpriseName = enterprise.NameShort;


                                if (distributionGroup != null)
                                    empModel.DistributionGroupName = distributionGroup.Name;

                                listModels.Add(empModel);
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMessage = string.Format("Gettting Enterprise for ENLO_GUID:{0} failed.", enlo.E_Guid);
                            var error = new ErrorModel(ex, errorMessage);
                            logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                            return this.Json(new DataSourceResult
                            {
                                Errors = error
                            });
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading employments";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }

            AddSecurityUserToCache(secUser);

            return Json(listModels.OrderByDescending(a => a.Exit).ThenByDescending(a => a.Main).ThenBy(a => a.EnterpriseName).ToList().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [Route("Read")]
        [Route("Read/{pers_guid}")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request, string pers_guid, bool deliverInActive = false)
        {
            List<EmploymentModel> listModels = new List<EmploymentModel>();
            SecurityUser secUser = GetSecurityUserFromCache();
            try
            {

                EmploymentHandler eh = new EmploymentHandler();
                EmploymentManager employmentManager = new EmploymentManager();
                List<EMDEmployment> listEmployments = employmentManager.GetEmploymentsForPerson(pers_guid, deliverInActive);

                List<EMDEmploymentType> employmentTypes = new EmploymentTypeHandler() { DeliverInActive = true, Historical = true }.GetObjects<EMDEmploymentType, EmploymentType>().Cast<EMDEmploymentType>().ToList();


                EnterpriseHandler enth = new EnterpriseHandler();


                ObjectFlagManager ofm = new ObjectFlagManager(this.PersonGuid);

                foreach (EMDEmployment emp in listEmployments)
                {
                    PersonProfileEmploymentModel ppem = new PersonProfileEmploymentModel();

                    ppem.CanOffboard = secUser.hasPermission(SecurityPermission.Offboarding, new SecurityUserParameterFlags(isLineManager: true), null, emp.Guid);
                    ppem.CanChange = secUser.hasPermission(SecurityPermission.Change, new SecurityUserParameterFlags(isLineManager: true), null, emp.Guid);

                    bool emplIsActive = false;
                    bool emplIsPast = false;
                    bool emplIsFuture = false;
                    if (emp.Entry > DateTime.Now && emp.FirstWorkDay > DateTime.Now)
                    {
                        emplIsFuture = true;
                    }
                    else if (emp.Exit < DateTime.Now && emp.LastDay < DateTime.Now)
                    {
                        emplIsPast = true;
                    }
                    else
                    {
                        emplIsActive = true;
                    }

                    bool mayManage = (ppem.CanOffboard || ppem.CanChange);
                    bool showEmployment = false;

                    showEmployment = (showEmployment || ((emplIsFuture || emplIsActive || emplIsPast) && mayManage));
                    showEmployment = (showEmployment || (emplIsActive && (ofm.IsEmploymentVisibleInPhonebook(emp.Guid) || secUser.hasPermission(String.Empty, new SecurityUserParameterFlags(isItself: true), null, emp.Guid))));

                    if (showEmployment)
                    {
                        EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
                        DistributionGroupHandler distGrpHandler = new DistributionGroupHandler();

                        EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(emp.ENLO_Guid);

                        if (enlo != null)
                        {
                            try
                            {
                                EMDEnterprise enterprise = (EMDEnterprise)enth.GetObject<EMDEnterprise>(enlo.E_Guid);
                                EMDDistributionGroup distributionGroup = (EMDDistributionGroup)distGrpHandler.GetObject<EMDDistributionGroup>(emp.DGT_Guid);
                                EMDEmployment emplSponsor = null;
                                if (emp.Sponsor_Guid != null)
                                    emplSponsor = (EMDEmployment)eh.GetObject<EMDEmployment>(emp.Sponsor_Guid);


                                EmploymentModel empModel = new EmploymentModel(emp);
                                empModel.EmploymentTypeName = employmentTypes?.FirstOrDefault(e => e.Guid == emp.ET_Guid).Name;
                                empModel.InitializeSecurity(secUser);
                                if (enterprise != null)
                                {
                                    empModel.EnterpriseName = enterprise.NameShort;
                                    if (emplSponsor != null)
                                    {
                                        PersonManager persMgr = new PersonManager();
                                        empModel.SponsorName = PersonManager.GetFullDisplayName(emplSponsor);
                                    }

                                    if (distributionGroup != null)
                                        empModel.DistributionGroupName = distributionGroup.Name;

                                    listModels.Add(empModel);
                                }
                            }
                            catch (Exception ex)
                            {
                                string errorMessage = string.Format("Gettting Enterprise for ENLO_GUID:{0} failed.", enlo.E_Guid);
                                var error = new ErrorModel(ex, errorMessage);
                                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                                return this.Json(new DataSourceResult
                                {
                                    Errors = error
                                });
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading employments";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }

            AddSecurityUserToCache(secUser);

            return Json(listModels.OrderByDescending(a => a.Exit).ThenByDescending(a => a.Main).ThenBy(a => a.EnterpriseName).ToList().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [Route("Update")]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, EmploymentModel employment)
        {
            if (employment != null && ModelState.IsValid)
            {
                EmploymentHandler eh = new EmploymentHandler(this.PersonGuid);
                EMDEmployment emp = new EMDEmployment();
                emp.Guid = employment.Guid;
                //emp.AD_Update = employment.AD_Update;
                //emp.Created = employment.cre

                eh.UpdateObject(emp);
            }

            return Json(ModelState.ToDataSourceResult());
        }


        [Route("ReadConfiguredListOfPackagesForEmployment/{ep_guid}")]
        public ActionResult ReadConfiguredListOfPackagesForEmployment([DataSourceRequest]DataSourceRequest request, string ep_guid)
        {
            EmploymentManager empMngr = new EmploymentManager();
            List<EMDPackageInstance> packages = empMngr.GetConfiguredListOfPackagesForEmployment(ep_guid);
            List<PackageModel> pModels = new List<PackageModel>();

            foreach (EMDPackageInstance package in packages)
            {
                EMDPackageInstance currentPackage = package;
                PackageModel pModel = new PackageModel();
                ReflectionHelper.CopyProperties(ref currentPackage, ref pModel);
                pModel.EMPL_Guid = ep_guid;
                pModel.Guid = package.Package.Guid;
                pModel.Package_Guid = package.Package.Guid;

                PackageStatus packageEquStatus = new PackageStatus();
                pModel.Name = package.PackageName;
                pModel.Description = package.Package.Description;
                pModel.PackageStatusShort = packageEquStatus.GetProcessStatusItem(currentPackage.PackageStatus).StatusShort;
                pModel.PackageStatusLong = packageEquStatus.GetProcessStatusItem(currentPackage.PackageStatus).StatusLong;
                pModel.PackageStatusInt = currentPackage.PackageStatus;

                pModels.Add(pModel);
            }

            return Json(pModels.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [Route("ReadConfiguredListOfEquipmentIntancesForEmployment/{empl_guid}/{deliverInActive}/{isEquipmentOwner}")]
        public ActionResult ReadConfiguredListOfEquipmentIntancesForEmployment([DataSourceRequest]DataSourceRequest request, string empl_guid, bool deliverInActive, bool isEquipmentOwner)
        {
            List<EquipmentInstanceModel> packageModels;
            SecurityUser secUser = this.GetSecurityUserFromCache();

            if (!secUser.IsAdmin && !secUser.IsItSelf(empl_guid, false) && isEquipmentOwner)
            {
                packageModels = EquipmentInstanceModel.GetEquipmentInstanceModelsForEquipmentOwner(empl_guid, this.UserName, this.PersonGuid, deliverInActive);
            }
            else
            {
                packageModels = EquipmentInstanceModel.GetEquipmentInstanceModels(empl_guid, this.UserName, deliverInActive);
            }

            return Json(packageModels.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }



        [Route("ReadAvailableListOfEquipmentDefinitionsForEmployment/{empl_guid}")]
        public ActionResult ReadAvailableListOfEquipmentDefinitionsForEmployment([DataSourceRequest]DataSourceRequest request, string empl_guid)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                EmploymentManager empMngr = new EmploymentManager();
                //List<EMDEquipmentDefinition> packages = empMngr.GetAvailableListOfEquipmentDefinitionsForEmployment(empl_guid);
                List<EMDEquipmentDefinition> packages = empMngr.GetAvailableEquipmentsForEmployment(empl_guid);

                

                packages.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDEquipmentDefinition)entity).Name, entity.Guid, entity.Description));
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

        [Route("ReadAvailableListOfEquipmentDefinitionsForEmploymentDs")]
        public ActionResult ReadAvailableListOfEquipmentDefinitionsForEmploymentDs([DataSourceRequest]DataSourceRequest request, string empl_guid, string ownerPersGuid)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                EmploymentManager empMngr = new EmploymentManager();
                EquipmentDefinitionPriceManager equipmentDefinitionPriceManager = new EquipmentDefinitionPriceManager();
                //List<EMDEquipmentDefinition> packages = empMngr.GetAvailableListOfEquipmentDefinitionsForEmployment(empl_guid);
                List<EMDEquipmentDefinition> packages = empMngr.GetAvailableEquipmentsForEmployment(empl_guid);
                List<EMDEquipmentDefinitionPrice> prices = equipmentDefinitionPriceManager.GetAllObjects();
                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName, empl_guid);
                //SecurityUser secUser = this.GetSecurityUserFromCache();

                EquipmentDefinitionOwnerManager equipmentDefinitionOwnerManager = new EquipmentDefinitionOwnerManager();
                List<EMDEquipmentDefinition> ownerEquipmentDefinitions = null;
                if (!string.IsNullOrWhiteSpace(ownerPersGuid))
                {
                    ownerEquipmentDefinitions = equipmentDefinitionOwnerManager.GetEquipmentDefinitionsForOwner(ownerPersGuid);
                }

                bool isAllowedEmployment = secUser.IsAllowedEmployment(secUser.UserId, empl_guid);

                bool hasPersonProfileEquipmentManagePermission = secUser.hasPermission(SecurityPermission.Personprofile_View_Equipment_Manage, new SecurityUserParameterFlags(checkPlainPermisson: true), null, null, this.PersonGuid);

                packages.ForEach(entity =>
                {
                    string description = HtmlStringHelper.StripHTML(entity.DescriptionLong);
                    //add price to description
                    EMDEquipmentDefinitionPrice price = prices.Where(item => item.EQDE_Guid == entity.Guid).FirstOrDefault();
                    if (price != null)
                    {
                        PriceInformationModel priceModel = new PriceInformationModel(price.Price, price.BillingPeriod);
                        description = string.Format("{0} ({1})", description, PriceInformationModel.PriceInformation(priceModel));
                    }


                    if (string.IsNullOrWhiteSpace(ownerPersGuid) || secUser.IsAdmin || secUser.IsItSelf(empl_guid, false) || isAllowedEmployment || hasPersonProfileEquipmentManagePermission)
                    {
                        keyValuePairs.Add(new TextValueModel(((EMDEquipmentDefinition)entity).Name, entity.Guid, description));
                    }
                    else
                    {
                        if (ownerEquipmentDefinitions.Where(item => item.Guid == entity.Guid).Count() > 0)
                        {
                            keyValuePairs.Add(new TextValueModel(((EMDEquipmentDefinition)entity).Name, entity.Guid, description));
                        }
                    }
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
            //return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
            return Json(keyValuePairs.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);

        }

        [Route("RemovePackageFromEmployment")]
        [Route("RemovePackageFromEmployment/{empl_guid}/{pack_guid}")]
        public ActionResult RemovePackageFromEmployment([DataSourceRequest]DataSourceRequest request, string empl_guid, string pack_guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            AddPackageToEmploymentModel model = new AddPackageToEmploymentModel();
            try
            {

                model.EP_Guid = empl_guid;
                model.oc_guid = pack_guid;
                model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                if (!model.CanManage)
                {
                    throw new Exception(SecurityHelper.NoPermissionText);
                }
                EmploymentManager empMngr = new EmploymentManager(this.PersonGuid);
                empMngr.RemovePackageFromEmployment(empl_guid, pack_guid);
                success = true;
                //return Json(new { success = true, responseText = "The package has been removed!" });
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
                //return Json(new { success = false, responseText = "The package could not be removed! - " + ex.Message.ToString() });
            }

            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success });
        }


        [Route("ReadUserList/{empl_guid}")]
        public ActionResult ReadUserList([DataSourceRequest]DataSourceRequest request, string empl_guid)
        {
            List<EMDUser> emdUsers = Manager.UserManager.GetEmploymentUsers(empl_guid);
            List<UserModel> userModels = UserModel.Map(emdUsers, true);

            return Json(userModels.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        private List<TextValueModel> GetEmploymentTvList(string text, int maxItemsResult = 0)
        {
            PersonHandler persHandler = new PersonHandler();
            EmploymentManager emplManager = new EmploymentManager();
            List<TextValueModel> listTextValueModels = new List<TextValueModel>();

            var items = (from empl in emplManager.GetActiveEmployments().ToList()
                         join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList() on empl.P_Guid equals pers.Guid
                         select new { pers, empl });

            if (!string.IsNullOrEmpty(text))
            {
                text = text.ToLower();
                items = items.Where(item => (PersonManager.getFullDisplayNameWithUserIdAndPersNr(item.pers, item.empl)).ToLower().Contains(text));
            }

            if (maxItemsResult > 0)
            {
                items = items.Take(maxItemsResult).ToList();
            }

            foreach (var item in items)
                listTextValueModels.Add(new TextValueModel(PersonManager.getFullDisplayNameWithUserIdAndPersNr(item.pers, item.empl), item.empl.Guid));

            return listTextValueModels.OrderBy(item => item.Text).ToList();
        }

        [Route("GetEmployments")]
        [HandleError()]
        public ActionResult GetEmployments([DataSourceRequest] DataSourceRequest request, string text)
        {

            List<TextValueModel> listTextValueModels = GetEmploymentTvList(text);

            return Json(listTextValueModels, JsonRequestBehavior.AllowGet);
        }


        [Route("GetEmploymentList")]
        [HandleError()]
        public ActionResult GetEmploymentList([DataSourceRequest] DataSourceRequest request, string text)
        {
            //List<TextValueModel> listTextValueModels = new List<TextValueModel>();
            //if (request.Filters.Count != 0)
            //{
            //    Kendo.Mvc.FilterDescriptor filterDescriptor = request.Filters[0] as Kendo.Mvc.FilterDescriptor;

            //    if (filterDescriptor != null && filterDescriptor.ConvertedValue.ToString().Length > 2)
            //    {
            //        listTextValueModels = GetEmploymentTvList(text);
            //    }
            //}


            List<TextValueModel> listTextValueModels = GetEmploymentTvList(text);
            return Json(listTextValueModels.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [Route("ReadEmploymentsForSelect")]
        [HandleError()]
        public ActionResult ReadEmploymentsForSelect(string ente_guid)
        {
            PersonHandler persHandler = new PersonHandler();
            EmploymentManager emplHandler = new EmploymentManager();
            List<TextValueModel> listTextValueModels = new List<TextValueModel>();

            var items = (from empl in emplHandler.GetEmploymentsByEnterprise(ente_guid)
                         join pers in persHandler.GetObjects<EMDPerson, Person>().Cast<EMDPerson>().ToList() on empl.P_Guid equals pers.Guid
                         select new { pers, empl });


            foreach (var item in items)
                listTextValueModels.Add(new TextValueModel(PersonManager.getFullDisplayNameWithUserIdAndPersNr(item.pers, item.empl), item.empl.Guid));

            listTextValueModels = listTextValueModels.OrderBy(item => item.Text).ToList();

            return Json(listTextValueModels, JsonRequestBehavior.AllowGet);
        }

        [Route("GetTeamLeader")]
        [HandleError()]
        public ActionResult GetTeamLeader(string guidOrgunit)
        {
            bool success = false;
            ErrorModel errorModel = null;
            OrgUnitRoleSearch searcher = new OrgUnitRoleSearch();
            string lineManager = string.Empty;
            PersonManager pm = new PersonManager(this.PersonGuid);

            List<string> supervisors = new List<string>();
            try
            {
                supervisors = searcher.SearchOrgUnitRoleForOrgUnit(RoleHandler.LINEMANAGER, guidOrgunit);
                if (supervisors.Count > 0)
                {
                    EMD.Data.EmploymentPerson supervisor = new EMD.Data.EmploymentPerson(supervisors[0]);
                    if (supervisor != null)
                    {
                        lineManager = pm.getFullDisplayNameWithUserId(supervisor.person);
                        success = true;
                    }

                    if (string.IsNullOrWhiteSpace(lineManager))
                    {
                        lineManager = "Data not available - please contact your business-prime!";
                    }
                }
                else
                {
                    lineManager = "Data not available - please contact your business-prime!";
                }
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                //handledException = ex;
                lineManager = "Data not available - please contact your business-prime!";
                logger.Error("GetTeamLeader failed for Employment", ex);
            }


            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success, lineManager = lineManager });
        }

        [Route("GetAllPersonEmploymentsDs")]
        [HandleError()]
        public ActionResult GetAllPersonEmploymentsDs([DataSourceRequest] DataSourceRequest request)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                EmploymentManager emplManager = new EmploymentManager();
                List<EMDPersonEmployment> allEmpls = emplManager.GetAllPersonEmployments();

                foreach (EMDPersonEmployment empl in allEmpls)
                    keyValuePairs.Add(new TextValueModel(PersonManager.getFullDisplayNameWithUserIdAndPersNr(empl.Pers, empl.Empl), empl.Empl.Guid));
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading EMDPersonEmployment";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
    }
}