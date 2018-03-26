using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.WF;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.ReflectionHelper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("EnterpriseLocation")]
    public class EnterpriseLocationController : BaseController
    {
        private const string MANAGEROUTE = "Manage";



        [Route]
        public ActionResult Index()
        {
            return RedirectToAction(MANAGEROUTE);
        }

        [Route("Manage")]
        [Route("Manage/{isPartialView}")]
        public ActionResult Manage(bool isPartialView = false)
        {
            EnterpriseLocationModel model = new EnterpriseLocationModel();

            string enloGuid = Request.Params["Guid"];
            if (!string.IsNullOrWhiteSpace(enloGuid))
            {
                EMDEnterpriseLocation enlo = new EnterpriseLocationManager().Get(enloGuid);
                if (enlo != null)
                {
                    EMDEnterprise enterprise = new EnterpriseManager().Get(enlo.E_Guid);
                    EMDLocation location = new LocationManager().Get(enlo.L_Guid);
                    if (enterprise != null && location != null)
                    {
                        model.EnterpriseNameEnhanced = enterprise.NameShort;
                        model.LocationNameEnhanced = location.Name;
                    }
                }
            }

            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);


            if (isPartialView)
            {
                return PartialView("Manage", model);
            }
            else
            {
                return View("Manage", model);
            }
        }

        private List<EnterpriseLocationModel> getEnterpriseLocationList()
        {
            List<EnterpriseLocationModel> myresult = new List<EnterpriseLocationModel>();
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();

            // get all enterprises
            List<EMDEnterprise> enterprises = new EnterpriseManager().GetList();
            List<EMDLocation> locations = new LocationManager().GetList();



            try
            {
                EnterpriseLocationModel dummySecurityModel = EnterpriseLocationModel.Initialize(new EMDEnterpriseLocation());
                dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                List<EMDEnterpriseLocation> listEnterprise = enloHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>().Cast<EMDEnterpriseLocation>().ToList();

                listEnterprise.ForEach(item =>
                {
                    EnterpriseLocationModel model = EnterpriseLocationModel.Initialize((EMDEnterpriseLocation)item);
                    EMDEnterprise foundEnterprise = enterprises.FirstOrDefault(a => a.Guid == item.E_Guid);
                    model.EnterpriseNameEnhanced = string.Format("{0} ({1})", foundEnterprise?.NameShort, foundEnterprise?.E_ID_new);
                    model.EnterpriseName = foundEnterprise?.NameShort;
                    model.EnterpriseNumber = foundEnterprise?.E_ID_new?.ToString();
                    model.EnteGuid = foundEnterprise?.Guid;
                    EMDLocation foundLocation = locations.FirstOrDefault(a => a.Guid == item.L_Guid);
                    model.LocationNameEnhanced = string.Format("{0} Object {1}", foundLocation?.Name, foundLocation?.EL_ID);
                    model.LocationName = foundLocation?.Name;
                    model.LocationObjectNumber = foundLocation?.EL_ID?.ToString();
                    model.LocaGuid = foundLocation?.Guid;
                    model.CanManage = dummySecurityModel.CanManage;
                    model.CanView = dummySecurityModel.CanView;
                    myresult.Add(model);
                });

                myresult = myresult.OrderBy(item => item.E_Guid).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading Enterprise-Locations", e);
            }
            return myresult;
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            List<EnterpriseLocationModel> enterpriseLocationList = getEnterpriseLocationList();



            enterpriseLocationList = enterpriseLocationList.OrderByDescending(b => b.Created).ToList();



            return Json(enterpriseLocationList.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{enlo_guid}")]
        [Route("Edit/{enlo_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string enlo_guid, bool isPartialView = false)
        {
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(enlo_guid);
            EnterpriseLocationModel enloModel = EnterpriseLocationModel.Initialize(enlo);
            enloModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!enloModel.CanManage)
                return GetNoPermissionView(isPartialView);

            EnterpriseController enteController = new EnterpriseController();
            LocationController locaController = new LocationController();


            if (isPartialView)
            {
                return PartialView("Edit", enloModel);
            }
            else
            {
                return View("Edit", enloModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, EnterpriseLocationModel enloModel)
        {
            Exception handledException = null;
            enloModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));


            int idEnterprise = 0;

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (enloModel != null && ModelState.IsValid)
            {
                try
                {
                    // set also the old enterprise
                    EMDEnterprise enterprise = new EnterpriseManager().Get(enloModel.E_Guid);
                    idEnterprise = enterprise.E_ID;


                    if (!enloModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    EnterpriseLocationHandler enteHandler = new EnterpriseLocationHandler(this.PersonGuid);
                    EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enteHandler.GetObject<EMDEnterpriseLocation>(enloModel.Guid);
                    enlo.DistList_ext = enloModel.DistList_ext;
                    enlo.DistList_int = enloModel.DistList_int;
                    enlo.E_Guid = enloModel.E_Guid;
                    enlo.E_ID = idEnterprise;
                    enlo.L_Guid = enloModel.L_Guid;

                    enteHandler.UpdateObject<EMDEnterpriseLocation>(enlo);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Enterprise-Location: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", enloModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Enterprise-Location has been updated!" });
            }
        }

        [HttpGet]
        [Route("View")]
        [Route("View/{enlo_guid}")]
        [Route("View/{enlo_guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string enlo_guid, bool isPartialView = false)
        {
            EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler();
            EMDEnterpriseLocation enlo = (EMDEnterpriseLocation)enloHandler.GetObject<EMDEnterpriseLocation>(enlo_guid);
            EnterpriseLocationModel enloModel = EnterpriseLocationModel.Initialize(enlo);
            enloModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!enloModel.CanView)
                return GetNoPermissionView(isPartialView);

            EnterpriseController enteController = new EnterpriseController();
            LocationController locaController = new LocationController();

            if (isPartialView)
            {
                return PartialView("View", enloModel);
            }
            else
            {
                return View("View", enloModel);
            }
        }

        [HttpGet]
        [Route("Create")]
        [Route("Create/{isPartialView}")]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, bool isPartialView = false)
        {
            EnterpriseLocationModel enloModel = new EnterpriseLocationModel();
            enloModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!enloModel.CanManage)
                return GetNoPermissionView(isPartialView);

            EnterpriseController enteController = new EnterpriseController();
            LocationController locaController = new LocationController();

            if (isPartialView)
            {
                return PartialView("Create", enloModel);
            }
            else
            {
                return View("Create", enloModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoCreate")]
        public ActionResult DoCreate([DataSourceRequest]DataSourceRequest request, EnterpriseLocationModel enloModel)
        {
            Exception handledException = null;
            enloModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (enloModel != null && ModelState.IsValid)
            {

                CoreTransaction transaction = new CoreTransaction();
                transaction.Begin();
                try
                {
                    // set also the old enterprise
                    EMDEnterprise enterprise = new EnterpriseManager().Get(enloModel.E_Guid);
                    int idEnterprise = enterprise.E_ID;

                    if (!enloModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    EnterpriseLocationHandler enloHandler = new EnterpriseLocationHandler(transaction, this.PersonGuid);

                    EMDEnterpriseLocation enlo = new EMDEnterpriseLocation();
                    ReflectionHelper.CopyProperties<EnterpriseLocationModel, EMDEnterpriseLocation>(ref enloModel, ref enlo);
                    enlo.E_ID = idEnterprise;

                    enloHandler.CreateObject<EMDEnterpriseLocation>(enlo);
                    // TODO publish proc eng webservice to dev Robert

                    EnterpriseLocationManager enterpriseLocationManager = new EnterpriseLocationManager(transaction, this.PersonGuid);
                    EnloAddWorkflowMessage message = enterpriseLocationManager.GetWorkflowVariablesForNewEnterpriseLocation(transaction, this.UserMainEmplGuid, enlo.E_Guid, enlo.L_Guid);



                    transaction.Commit();

                    message.CreateWorkflowInstance(this.PersonGuid, MODIFY_COMMENT);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    errmsg = "The Enterprise-Location could not be created! " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", enloModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Enterprise-Location has been created and the workflow progress is started!" });
            }
        }


        [HttpPost]
        [Route("DeleteEnterpriseLocation")]
        public ActionResult DeleteEnterpriseLocation(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.EnterpriseLocationManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                EnterpriseLocationManager manager = new EnterpriseLocationManager(this.PersonGuid);
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