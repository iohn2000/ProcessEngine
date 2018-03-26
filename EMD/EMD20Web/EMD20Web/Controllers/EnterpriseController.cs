using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EMD.EMD20Web.Core;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Enterprise")]
    public class EnterpriseController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        public EnterpriseController()
        {
            logger = ISLogger.GetLogger(this.GetType().FullName);
        }

        [Route]
        public ActionResult Index()
        {
            return RedirectToAction(MANAGEROUTE);
        }

        [Route("Manage")]
        [Route("Manage/{isPartialView}")]
        public ActionResult Manage(bool isPartialView = false)
        {
            EnterpriseModel model = new EnterpriseModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            //just return an empty view since all data is Ajax-driven
            PopulateEnterprises();
            PopulateOrgUnits();
            if (isPartialView)
            {
                return PartialView("Manage", model);
            }
            else
            {
                return View("Manage", model);
            }
        }

        public List<EnterpriseModel> getList(bool deliverInActive)
        {
            List<EnterpriseModel> myresult = new List<EnterpriseModel>();
            EnterpriseHandler enteHandler = new EnterpriseHandler();
            enteHandler.DeliverInActive = deliverInActive;
            try
            {
                logger.Debug("Getting Enterprise Objects", ControllerContext?.HttpContext);
                EnterpriseModel dummySecurityModel = EnterpriseModel.Initialize(new EMDEnterprise());
                dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                List<EMDEnterprise> listEnterprise = enteHandler.GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList();

                logger.Debug("Getting Enterprise Objects Finished", ControllerContext?.HttpContext);
                listEnterprise.ForEach(item =>
                {
                    EnterpriseModel model = EnterpriseModel.Initialize((EMDEnterprise)item);
                    EMDEnterprise entefound = (from a in listEnterprise where a.Guid == model.Guid_Root select a).FirstOrDefault();
                    if (entefound != null)
                    {
                        model.E_ID_new_Root = entefound.E_ID_new.HasValue ? entefound.E_ID_new.Value : 0;
                    }
                    entefound = (from a in listEnterprise where a.Guid == model.Guid_Parent select a).FirstOrDefault();
                    if (entefound != null)
                    {
                        model.E_ID_new_Parent = entefound.E_ID_new.HasValue ? entefound.E_ID_new.Value : 0;
                    }

                    model.CanManage = dummySecurityModel.CanManage;
                    model.CanView = dummySecurityModel.CanView;
                    myresult.Add(model);
                });

                logger.Debug("Copied Object Data to Models", ControllerContext?.HttpContext);
                myresult = myresult.OrderBy(item => item.NameShort).ToList();
                logger.Debug("Ordered Models", ControllerContext?.HttpContext);
            }
            catch (Exception ex)
            {
                logger.Error("Unhandled Exception getting List of EnterpriseModel", ex, ControllerContext?.HttpContext);
                //  throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading enterprises", e);
            }
            return myresult;
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            List<EnterpriseModel> enterpriseList = getList(false);
            DataSourceResult myresult;
            myresult = enterpriseList.ToDataSourceResult(request, ModelState);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{ente_guid}")]
        [Route("Edit/{ente_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string ente_guid, bool isPartialView = false)
        {
            EnterpriseHandler enteHandler = new EnterpriseHandler();
            EMDEnterprise enterprise = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(ente_guid);
            EnterpriseViewModel enteModel = EnterpriseViewModel.copyFromObject(enterprise);
            enteModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!enteModel.CanManage)
                return GetNoPermissionView(isPartialView);


            EMDEnterprise currentEnterprise = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(ente_guid);
            EnterpriseModel currentEnterpriseModel = EnterpriseModel.Initialize(currentEnterprise);

            EMDEnterprise parentEnterprise = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(currentEnterprise.Guid_Parent);
            EnterpriseModel parentEnterpriseModel = EnterpriseModel.Initialize(parentEnterprise);

            List<EMDEnterprise> listEnterprises = enteHandler.GetAllSubEnterprisesFromParent(ente_guid, 1);

            EnterpriseModelList enterpriseModelList = new EnterpriseModelList();
            enterpriseModelList.CurrentEnterprise = currentEnterpriseModel;
            enterpriseModelList.ParentEnterprise = parentEnterpriseModel;
            if (parentEnterprise != null && parentEnterprise.Guid == currentEnterprise.Guid)
            {
                enterpriseModelList.HasParent = false;
                enterpriseModelList.ParentEnterpriseLevel = 0;
                enterpriseModelList.CurrentEnterpriseLevel = 0;
            }
            else
            {
                enterpriseModelList.HasParent = true;
                enterpriseModelList.ParentEnterpriseLevel = 0;
                enterpriseModelList.CurrentEnterpriseLevel = 1;
            }

            foreach (EMDEnterprise ent in listEnterprises)
            {
                if (ent.Guid != currentEnterprise.Guid && ent.Guid != parentEnterprise.Guid)
                {
                    EnterpriseModel enterpriseModel = EnterpriseModel.Initialize(ent);
                    enterpriseModelList.EnterpriseModels.Add(enterpriseModel);
                }
            }
            enteModel.enterpriseModelList = enterpriseModelList;

            if (isPartialView)
            {
                return PartialView("Edit", enteModel);
            }
            else
            {
                return View("Edit", enteModel);
            }
        }


        [Route("GetOrganizationUnits")]
        [HandleError()]
        public ActionResult GetOrganizationUnits([DataSourceRequest] DataSourceRequest request)
        {

            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                OrgUnitHandler handler = new OrgUnitHandler();
                List<IEMDObject<EMDOrgUnit>> emdEntities = handler.GetObjects<EMDOrgUnit, OrgUnit>(null);

                emdEntities.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDOrgUnit)entity).Name, entity.Guid));
                });


                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }

            catch (Exception ex)
            {
                string errorMessage = "Error reading organization units";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);

        }

        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        public ActionResult DoEdit(EnterpriseModel enteModel)
        {
            Exception handledException = null;
            enteModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (enteModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!enteModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    EnterpriseManager enterpriseManager = new EnterpriseManager(this.PersonGuid);
                    EMDEnterprise enterprise = new EMDEnterprise();
                    ReflectionHelper.CopyProperties(ref enteModel, ref enterprise);
                    enterpriseManager.Update(enterprise);

                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Enterprise: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", enteModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = "The Enterprise has been updated!" });
            }
        }

        [HttpGet]
        [Route("Create")]
        [Route("Create/{isPartialView}")]
        public ActionResult Create(bool isPartialView = false)
        {
            EnterpriseModel enteModel = new EnterpriseModel();
            enteModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!enteModel.CanManage)
                return GetNoPermissionView(isPartialView);

            OrgUnitController ouc = new OrgUnitController();


            if (isPartialView)
            {
                return PartialView("Create", enteModel);
            }
            else
            {
                return View("Create", enteModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoCreate")]
        public ActionResult DoCreate(EnterpriseModel enteModel)
        {
            Exception handledException = null;
            enteModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (enteModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!enteModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    EnterpriseManager enterpriseManager = new EnterpriseManager(this.PersonGuid);
                    EMDEnterprise enterprise = new EMDEnterprise();
                    ReflectionHelper.CopyProperties<EnterpriseModel, EMDEnterprise>(ref enteModel, ref enterprise);
                    enterpriseManager.Create(enterprise);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Enterprise could not be created!" + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", enteModel, handledException, errmsg);


            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Enterprise has been created!" });
            }
        }

        [Route("ReadForSelect")]
        public ActionResult ReadForSelect(string ente_guid = null)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                EnterpriseHandler handler = new EnterpriseHandler();
                List<IEMDObject<EMDEnterprise>> emdEntities = handler.GetObjects<EMDEnterprise, Enterprise>(null);


                emdEntities.ForEach(entity =>
                {
                    if (string.IsNullOrEmpty(ente_guid) || ente_guid != entity.Guid)
                    {
                        keyValuePairs.Add(new TextValueModel(((EMDEnterprise)entity).NameShort, entity.Guid, new { E_ID = ((EMDEnterprise)entity).E_ID }));
                    }
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }

            catch (Exception ex)
            {
                string errorMessage = "Error reading enterprises";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadForChangeEmployment")]
        public ActionResult ReadForChangeEmployment(string ente_guid = null)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();

            try
            {
                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                List<EMDEnterprise> allowedEnterprises = secUser.AllowedEnterprises(SecurityPermission.Onboarding);

                EnterpriseManager enteManager = new EnterpriseManager();
                List<EMDEnterprise> kccEnterprises = enteManager.GetEnterpriseLeafes(new List<int> { 2000, 1310 });

                foreach (EMDEnterprise ente in allowedEnterprises)
                {

                    if (kccEnterprises.FindLast(e => e.Guid == ente.Guid) != null)
                    {
                        if (keyValuePairs.Find(a => a.Value == ente.Guid) == null)
                        {
                            if (ente.HasEmployees)
                            {
                                keyValuePairs.Add(new Models.TextValueModel(ente.NameShort, ente.Guid, new { E_ID = ente.E_ID, isKcc = true }));
                            }
                        }
                    }
                    else
                    {
                        if (keyValuePairs.Find(a => a.Value == ente.Guid) == null)
                        {
                            if (ente.HasEmployees)
                            {
                                keyValuePairs.Add(new Models.TextValueModel(ente.NameShort, ente.Guid, new { E_ID = ente.E_ID, isKcc = false }));
                            }
                        }
                    }

                }

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }

            catch (Exception ex)
            {
                string errorMessage = "Error reading enterprises";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadForSelectDs")]
        [HandleError()]
        public ActionResult ReadForSelectDs([DataSourceRequest] DataSourceRequest request )
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                //List<EMDEnterprise> enterprises = secUser.AllowedEnterprises(SecurityPermission.CostCenterManager_View_Manage).OrderBy(item => item.NameShort).ToList();
                List<EMDEnterprise> enterprises = secUser.AllowedEnterprises(SecurityPermission.Enterprise_View_Manage).OrderBy(item => item.NameShort).ToList();
                foreach (EMDEnterprise ente in enterprises)
                    keyValuePairs.Add(new TextValueModel(ente.NameShort, ente.Guid, new { E_ID = ente.E_ID }));
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading accounts";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("View")]
        [Route("View/{ente_guid}")]
        [Route("View/{ente_guid}/{isPartialView}")]
        public ActionResult Viewer(string ente_guid, bool isPartialView = false)
        {
            EnterpriseHandler enteHandler = new EnterpriseHandler();
            EMDEnterprise enterprise = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(ente_guid);
            EnterpriseViewModel enteModel = EnterpriseViewModel.copyFromObject(enterprise);
            enteModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!enteModel.CanView)
                return GetNoPermissionView(isPartialView);

            EMDEnterprise currentEnterprise = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(ente_guid);
            EnterpriseModel currentEnterpriseModel = EnterpriseModel.Initialize(currentEnterprise);

            EMDEnterprise parentEnterprise = (EMDEnterprise)enteHandler.GetObject<EMDEnterprise>(currentEnterprise.Guid_Parent);
            EnterpriseModel parentEnterpriseModel = EnterpriseModel.Initialize(parentEnterprise);

            List<EMDEnterprise> listEnterprises = enteHandler.GetAllSubEnterprisesFromParent(ente_guid, 1);

            EnterpriseModelList enterpriseModelList = new EnterpriseModelList();
            enterpriseModelList.CurrentEnterprise = currentEnterpriseModel;
            enterpriseModelList.ParentEnterprise = parentEnterpriseModel;
            if (parentEnterprise.Guid == currentEnterprise.Guid)
            {
                enterpriseModelList.HasParent = false;
                enterpriseModelList.ParentEnterpriseLevel = 0;
                enterpriseModelList.CurrentEnterpriseLevel = 0;
            }
            else
            {
                enterpriseModelList.HasParent = true;
                enterpriseModelList.ParentEnterpriseLevel = 0;
                enterpriseModelList.CurrentEnterpriseLevel = 1;
            }

            foreach (EMDEnterprise ent in listEnterprises)
            {
                if (ent.Guid != currentEnterprise.Guid && ent.Guid != parentEnterprise.Guid)
                {
                    EnterpriseModel enterpriseModel = EnterpriseModel.Initialize(ent);
                    enterpriseModelList.EnterpriseModels.Add(enterpriseModel);
                }
            }
            enteModel.enterpriseModelList = enterpriseModelList;

            if (isPartialView)
            {
                return PartialView("View", enteModel);
            }
            else
            {
                return View("View", enteModel);
            }
        }


        [HttpPost]
        [Route("DeleteEnterprise")]
        public ActionResult DeleteEnterprise(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.EnterpriseManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                EnterpriseManager manager = new EnterpriseManager(this.PersonGuid);
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

    }
}
