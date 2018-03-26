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
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.EDP.Core.Framework;
using System.Reflection;
using Kapsch.IS.EMD.EMD20Web.Core;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Country")]
    public class CountryController : BaseController
    {
        internal new IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string MANAGEROUTE = "Manage";

        [Route()]
        // GET: Country
        public ActionResult Index()
        {
            return RedirectToAction(MANAGEROUTE);
        }

        [Route("Manage")]
        public ActionResult Manage()
        {
            CountryModel model = new CountryModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            //just return an empty view since all data is Ajax-driven
            return View("Manage", model);
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{cty_guid}")]
        [Route("Edit/{cty_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string cty_guid, bool isPartialView = false)
        {
            CountryHandler ctyHandler = new CountryHandler();
            EMDCountry cty = (EMDCountry)ctyHandler.GetObject<EMDCountry>(cty_guid);
            CountryModel ctyModel = CountryModel.Initialize(cty);
            ctyModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!ctyModel.CanManage)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("Edit", ctyModel);
            }
            else
            {
                return View("Edit", ctyModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, CountryModel ctyModel)
        {
            Exception handledException = null;
            ctyModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (ctyModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!ctyModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    CountryHandler ctyHandler = new CountryHandler(this.PersonGuid);
                    EMDCountry cty = new EMDCountry();
                    ReflectionHelper.CopyProperties(ref ctyModel, ref cty);
                    ctyHandler.UpdateObject<EMDCountry>(cty);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Country: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", ctyModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Country has been updated!" });
            }
        }

        [HttpGet]
        [Route("View")]
        [Route("View/{cty_guid}")]
        [Route("View/{cty_guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string cty_guid, bool isPartialView = false)
        {
            CountryHandler ctyHandler = new CountryHandler();
            EMDCountry cty = (EMDCountry)ctyHandler.GetObject<EMDCountry>(cty_guid);
            CountryModel ctyModel = CountryModel.Initialize(cty);
            ctyModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!ctyModel.CanView)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("View", ctyModel);
            }
            else
            {
                return View("View", ctyModel);
            }
        }

        [HttpGet]
        [Route("Create")]
        [Route("Create/{isPartialView}")]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, bool isPartialView = false)
        {
            CountryModel ctyMod = new CountryModel();
            ctyMod.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!ctyMod.CanManage)
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("Create", ctyMod);
            }
            else
            {
                return View("Create", ctyMod);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoCreate")]
        public ActionResult DoCreate([DataSourceRequest]DataSourceRequest request, CountryModel ctyModel)
        {
            Exception handledException = null;
            ctyModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (ctyModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!ctyModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    CountryHandler ctyHandler = new CountryHandler(this.PersonGuid);

                    EMDCountry cty = new EMDCountry();
                    ReflectionHelper.CopyProperties<CountryModel, EMDCountry>(ref ctyModel, ref cty);
                    ctyHandler.CreateObject<EMDCountry>(cty);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Country could not be created!" + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", ctyModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Country has been created!" });
            }
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            List<CountryModel> countryList = getCountryList();
            DataSourceResult myresult;
            myresult = countryList.ToDataSourceResult(request, ModelState);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        public List<CountryModel> getCountryList()
        {
            List<CountryModel> myresult = new List<CountryModel>();
            CountryHandler ctyHandler = new CountryHandler();

            try
            {
                CountryModel dummySecurityModel = CountryModel.Initialize(new EMDCountry());
                dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                List<IEMDObject<EMDCountry>> listCountry = ctyHandler.GetObjects<EMDCountry, Country>();

                listCountry.ForEach(cty =>
                    {
                        CountryModel model = CountryModel.Initialize((EMDCountry)cty);
                        model.CanManage = dummySecurityModel.CanManage;
                        model.CanView = dummySecurityModel.CanView;
                        myresult.Add(model);
                    }
                );

                myresult = myresult.OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading countries", e);
            }
            return myresult;
        }

        [Route("ReadForSelect")]
        public ActionResult ReadForSelect([DataSourceRequest]DataSourceRequest request)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                CountryHandler handler = new CountryHandler();
                List<IEMDObject<EMDCountry>> emdEntities = handler.GetObjects<EMDCountry, Country>(null);


                emdEntities.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDCountry)entity).Name, entity.Guid));
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading countries";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Route("DeleteCountry")]
        public ActionResult DeleteCountry(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.CountryManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                CountryManager manager = new CountryManager(this.PersonGuid);
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