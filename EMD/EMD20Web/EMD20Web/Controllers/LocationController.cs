using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Core;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Location")]
    public class LocationController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        [Route]
        // GET: Location
        public ActionResult Index()
        {
            return RedirectToAction(MANAGEROUTE);
        }

        [Route("Manage")]
        [Route("Manage/{isPartialView}")]
        public ActionResult Manage(bool isPartialView = false)
        {
            PopulateCountries();
            Models.Location.ManageLocationModel model = new Models.Location.ManageLocationModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(isPartialView);
            //return View("~/Views/Shared/ErrorHandling/_NoPermission.cshtml", "_Layout");
            //return View("NoPermission", "_Layout");



            if (isPartialView)
            {
                return PartialView("Manage", model);
            }
            else
            {
                return View("Manage", model);
            }
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{loca_guid}")]
        [Route("Edit/{loca_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string loca_guid, bool isPartialView = false)
        {
            LocationHandler locaHandler = new LocationHandler();
            EMDLocation loca = (EMDLocation)locaHandler.GetObject<EMDLocation>(loca_guid);
            LocationModel locaModel = LocationModel.Initialize(loca);
            locaModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!locaModel.CanManage)
                return GetNoPermissionView(isPartialView);


            CountryController cc = new CountryController();

            if (isPartialView)
            {
                return PartialView("Edit", locaModel);
            }
            else
            {
                return View("Edit", locaModel);
            }
        }

        [HttpGet]
        [Route("View")]
        [Route("View/{loca_guid}")]
        [Route("View/{loca_guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string loca_guid, bool isPartialView = false)
        {
            LocationHandler locaHandler = new LocationHandler();
            EMDLocation loca = (EMDLocation)locaHandler.GetObject<EMDLocation>(loca_guid);
            LocationModel locaModel = LocationModel.Initialize(loca);
            locaModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!locaModel.CanView)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("View", locaModel);
            }
            else
            {
                return View("View", locaModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, LocationModel locaModel)
        {
            Exception handledException = null;
            locaModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            if (locaModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!locaModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    LocationHandler locaHandler = new LocationHandler(this.PersonGuid);
                    EMDLocation loca = new EMDLocation();
                    ReflectionHelper.CopyProperties(ref locaModel, ref loca);
                    locaHandler.UpdateObject<EMDLocation>(loca);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Location: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", locaModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Location has been updated!" });
            }
        }

        [HttpGet]
        [Route("Create")]
        [Route("Create/{isPartialView}")]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, bool isPartialView = false)
        {
            LocationModel locaMod = new LocationModel();
            locaMod.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!locaMod.CanManage)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("Create", locaMod);
            }
            else
            {
                return View("Create", locaMod);
            }
        }

        [HttpPost]
        [Route("DoCreate")]
        public ActionResult DoCreate([DataSourceRequest]DataSourceRequest request, LocationModel locaModel)
        {
            Exception handledException = null;
            locaModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (locaModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!locaModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    LocationHandler locaHandler = new LocationHandler(this.PersonGuid);
                    EMDLocation loca = new EMDLocation();
                    ReflectionHelper.CopyProperties<LocationModel, EMDLocation>(ref locaModel, ref loca);
                    locaHandler.CreateObject<EMDLocation>(loca);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Location could not be created! " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", locaModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Location has been created!" });
            }
        }

        public List<LocationModel> getList()
        {
            List<LocationModel> myresult = new List<LocationModel>();
            LocationHandler locaHandler = new LocationHandler();

            try
            {
                LocationModel dummySecurityModel = LocationModel.Initialize(new EMDLocation());
                SecurityUser securityUser = SecurityUser.NewSecurityUser(this.UserName);
                dummySecurityModel.InitializeSecurity(securityUser);

                List<IEMDObject<EMDLocation>> listLocation = locaHandler.GetObjects<EMDLocation, Location>();

                listLocation.ForEach(item =>
                    {
                        LocationModel locaModel = LocationModel.Initialize((EMDLocation)item);
                        locaModel.InitializeSecurity(securityUser);
                        locaModel.CanManage = dummySecurityModel.CanManage;
                        locaModel.CanView = dummySecurityModel.CanView;
                        myresult.Add(locaModel);
                        //myresult.Add(LocationModel.Initialize((EMDLocation)item));
                    }
                );

                myresult = myresult.OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading locations", e);
            }
            return myresult;
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            DataSourceResult myresult = null;
            try
            {
                List<LocationModel> locationList = getList();


                myresult = locationList.ToDataSourceResult(request, ModelState);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading locations";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadForSelect")]
        public ActionResult ReadForSelect([DataSourceRequest]DataSourceRequest request, string text = "%")
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                LocationHandler handler = new LocationHandler();
                List<IEMDObject<EMDLocation>> emdEntities = handler.GetObjects<EMDLocation, Location>(null);


                emdEntities.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDLocation)entity).Name, entity.Guid));
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }

            catch (Exception ex)
            {
                string errorMessage = "Error reading locations";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }


        //http://localhost:8021/Location/ReadForSelectForEnterprise?ente_guid=
        [Route("ReadForSelectForEnterprise")]
        public ActionResult ReadForSelectForEnterprise(string ente_guid)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                EnterpriseLocationHandler enterpriseLocationHandler = new EnterpriseLocationHandler();
                List<IEMDObject<EMDEnterpriseLocation>> emdEnterpriseLocations = enterpriseLocationHandler.GetObjects<EMDEnterpriseLocation, EnterpriseLocation>("E_Guid=\"" + ente_guid + "\"");

                LocationHandler locationHandler = new LocationHandler();
                List<IEMDObject<EMDLocation>> emdLocations = locationHandler.GetObjects<EMDLocation, Location>(null);

                foreach (EMDEnterpriseLocation emdEnterpriseLocation in emdEnterpriseLocations)
                {
                    foreach (EMDLocation emdLocation in emdLocations)
                    {
                        if (emdEnterpriseLocation.L_Guid == emdLocation.Guid)
                        {
                            keyValuePairs.Add(new TextValueModel(((EMDLocation)emdLocation).Name, emdLocation.Guid));
                            break;
                        }
                    }
                }

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading locations";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        private void PopulateCountries()
        {
            CountryHandler cHandler = new CountryHandler();
            List<IEMDObject<EMDCountry>> listCountries = cHandler.GetObjects<EMDCountry, Country>();

            List<TextValueModel> listTextVal = new List<TextValueModel>();
            foreach (EMDCountry item in listCountries)
            {
                TextValueModel textValItem = new TextValueModel(item.Name, item.Guid);
                listTextVal.Add(textValItem);
            }

            ViewData["CountryNames"] = listTextVal;
            ViewData["CountryNamesDefault"] = listTextVal.FirstOrDefault();
        }


        [HttpPost]
        [Route("DeleteLocation")]
        public ActionResult DeleteLocation(string guid)
        {
            if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.LocationManager_View_Manage))
                return GetNoPermissionView(false);

            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.LocationManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                LocationManager manager = new LocationManager(this.PersonGuid);
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