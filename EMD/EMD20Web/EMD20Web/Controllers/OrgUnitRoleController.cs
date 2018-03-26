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
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EMD.EMD20Web.Filters;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.Util.Logging;
using System.Diagnostics;
using System.Net;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EMD.EMD20Web.Core;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("OrgUnitRole")]
    public class OrgUnitRoleController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        public OrgUnitRoleController()
        {
            logger = ISLogger.GetLogger("OrgUnitRoleController");
        }

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
            OrgUnitRoleModel model = new OrgUnitRoleModel();

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

        public List<OrgUnitRoleModel> GetOrgunitRoleList(string guidOrgunit = null)
        {
            OrgUnitRoleModel dummySecurityModel = OrgUnitRoleModel.Initialize(new EMDOrgUnitRole());
            dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            List<OrgUnitRoleModel> myResult = new List<OrgUnitRoleModel>();
            OrgUnitRoleManager manager = new OrgUnitRoleManager();

            try
            {
                List<EMDOrgUnitRoleEnhanced> eMDOrgUnitRoleEnhancedList = manager.GetOrgUnitRoleEnhancedList(guidOrgunit);

                foreach (EMDOrgUnitRoleEnhanced item in eMDOrgUnitRoleEnhancedList)
                {
                    OrgUnitRoleModel model = OrgUnitRoleModel.Initialize((EMDOrgUnitRoleEnhanced)item);
                    model.CanManage = dummySecurityModel.CanManage;
                    model.CanView = dummySecurityModel.CanView;
                    myResult.Add(model);
                }

                myResult = myResult.OrderBy(item => item.PERS_Name).ToList();
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading OrgUnit-Roles", ex);
            }

            return myResult;
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request, string guid_orgunit)
        {
            DataSourceResult myresult = null;

            try
            {
                List<OrgUnitRoleModel> orgUnitRoleList = GetOrgunitRoleList(guid_orgunit);
                orgUnitRoleList = orgUnitRoleList.ToList();
                myresult = orgUnitRoleList.ToDataSourceResult(request, ModelState);
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

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{ouro_guid}")]
        [Route("Edit/{ouro_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string ouro_guid, bool isPartialView = false)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);

            OrgUnitRoleHandler ouroHandler = new OrgUnitRoleHandler();
            OrgUnitHandler ouHandler = new OrgUnitHandler();
            RoleHandler rHandler = new RoleHandler();

            EMDOrgUnitRole orgUnitRole = (EMDOrgUnitRole)ouroHandler.GetObject<EMDOrgUnitRole>(ouro_guid);

            OrgUnitRoleModel ouroModel = OrgUnitRoleModel.Initialize(orgUnitRole);
            ouroModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!ouroModel.CanManage || !ouroModel.IsAllowedObject(this.UserName, orgUnitRole.O_Guid, true))
                return GetNoPermissionView(isPartialView);

            EMDRole personRole = new RoleManager().GetRoleById(RoleHandler.PERSON);

            if (!secUser.IsAdmin && personRole.Guid == orgUnitRole.R_Guid)
            {
                return GetNoPermissionView(isPartialView);
            }

            EmploymentHandler emplHandler = new EmploymentHandler();
            if (orgUnitRole.EP_Guid != null)
            {
                EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(orgUnitRole.EP_Guid);
                if (empl != null)
                {
                    PersonManager persManager = new PersonManager();
                    //EMDPerson pers = (EMDPerson)persHandler.GetObject<EMDPerson>(empl.P_Guid);
                    EMDPerson pers = persManager.Get(empl.P_Guid);
                    ouroModel.PERS_Name = persManager.getFullDisplayNameWithUserIdAndPersNr(empl);
                }
            }

            //OrgUnitController orgUnitController = new OrgUnitController();
            //ouroModel.availableOrgUnits = orgUnitController.GetList();

            //RoleController roleController = new RoleController();
            //ouroModel.availableRoles = roleController.getRoleList();

            List<EMDOrgUnit> avOrgUnits = secUser.AllowedOrgUnits(SecurityPermission.OrgUnitRoleManager_View_Manage);

            //OrgUnitHandler orgUnitHandler = new OrgUnitHandler();
            //List<EMDOrgUnit> avOrgUnits = orgUnitHandler.GetObjects<EMDOrgUnit, OrgUnit>().Cast<EMDOrgUnit>().ToList();
            List<TextValueModel> avOrgUnitsTextValue = new List<TextValueModel>();
            List<EMDEnterprise> enterprises = new EnterpriseHandler().GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList();
            avOrgUnits.ForEach(item =>
            {
                EMDEnterprise enterprise = (from a in enterprises where a.Guid == item.E_Guid select a).FirstOrDefault();
                if (enterprise != null)
                {
                    avOrgUnitsTextValue.Add(new TextValueModel(string.Format("{0} >> {1}", item.Name, enterprise.NameShort), item.Guid));
                }
                else
                {
                avOrgUnitsTextValue.Add(new TextValueModel(item.Name, item.Guid));
                }
            });

            ouroModel.availableOrgUnits = avOrgUnitsTextValue;

            RoleHandler roleHandler = new RoleHandler();
            List<EMDRole> avRoles = roleHandler.GetObjects<EMDRole, Role>().Cast<EMDRole>().ToList();
            List<TextValueModel> avRolesTextValue = new List<TextValueModel>();
            avRoles.ForEach(item =>
            {
                if (secUser.IsAdmin || (item.R_ID != RoleHandler.PERSON))
                    avRolesTextValue.Add(new TextValueModel(item.Name, item.Guid));
            });


            ouroModel.availableRoles = avRolesTextValue;

            List<EMDPersonEmployment> avEmployment = secUser.AllowedEmploymentsForEnterprises();
            List<TextValueModel> avEmploymentsTextValue = new List<TextValueModel>();
            avEmployment.ForEach(item =>
            {
                avEmploymentsTextValue.Add(new TextValueModel(item.FullDisplayNameWithUserIdAndPersNr, item.Empl.Guid));
            });

            ouroModel.availableEmployments = avEmploymentsTextValue;

            if (isPartialView)
            {
                return PartialView("Edit", ouroModel);
            }
            else
            {
                return View("Edit", ouroModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, OrgUnitRoleModel orgUnitRoleModel)
        {
            Exception handledException = null;
            SecurityUser securityUser = SecurityUser.NewSecurityUser(this.UserName);

            orgUnitRoleModel.InitializeSecurity(securityUser);

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (orgUnitRoleModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!orgUnitRoleModel.CanManage || !orgUnitRoleModel.IsAllowedObject(this.UserName, orgUnitRoleModel.O_Guid, true))
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    OrgUnitRoleHandler ouroHandler = new OrgUnitRoleHandler(this.PersonGuid);
                    EMDOrgUnitRole orgUnitRole = new EMDOrgUnitRole();


                    ReflectionHelper.CopyProperties(ref orgUnitRoleModel, ref orgUnitRole);

                    EMDRole role = new RoleManager().Get(orgUnitRole.R_Guid);
                    if (role != null)
                    {
                        orgUnitRole.R_ID = role.R_ID;
                    }


                    new OrgUnitRoleManager().Update(this.PersonGuid, securityUser.IsAdmin, orgUnitRole);
                }
                catch (EntityNotAllowedException ex)
                {
                    handledException = ex;
                    errmsg = ex.Message;
                    if (ex.EntityClassName.Equals("orgunitrole", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (ex.EntityNotAllowedExceptionType != EnumEntityNotAllowedError.EntityAllowedOnlyOnceForSelectedParameters)
                        {
                            errmsg = "The role Person is already configured for the requested Employment in the Orgunit!";
                        }
                    }
                    ModelState.AddModelError("error", errmsg);
                }
                catch (EdpSecurityException ex)
                {
                    handledException = ex;
                    errmsg = ex.Message;
                    if (ex.EntityClassName.Equals("orgunitrole", StringComparison.InvariantCultureIgnoreCase))
                    {
                        errmsg = "Only Admins are allowed to add the role Person to a Orgunit!";
                    }
                    ModelState.AddModelError("error", errmsg);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Orgunit-Role: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", orgUnitRoleModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Orgunit-Role has been updated!" });
            }
        }

        [HttpGet]
        [Route("View")]
        [Route("View/{ouro_guid}")]
        [Route("View/{ouro_guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string ouro_guid, bool isPartialView = false)
        {
            OrgUnitRoleHandler ouroHandler = new OrgUnitRoleHandler();
            OrgUnitHandler ouHandler = new OrgUnitHandler();
            RoleHandler rHandler = new RoleHandler();

            EMDOrgUnitRole orgUnitRole = (EMDOrgUnitRole)ouroHandler.GetObject<EMDOrgUnitRole>(ouro_guid);

            OrgUnitRoleModel ouroModel = OrgUnitRoleModel.Initialize(orgUnitRole);
            ouroModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!ouroModel.CanView)
                return GetNoPermissionView(isPartialView);

            EmploymentHandler emplHandler = new EmploymentHandler();
            if (orgUnitRole.EP_Guid != null)
            {
                EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(orgUnitRole.EP_Guid);
                if (empl != null)
                {
                    PersonManager persManager = new PersonManager(this.PersonGuid);
                    //EMDPerson pers = (EMDPerson)persHandler.GetObject<EMDPerson>(empl.P_Guid);
                    EMDPerson pers = persManager.Get(empl.P_Guid);
                    ouroModel.PERS_Name = persManager.getFullDisplayNameWithUserIdAndPersNr(empl);

                    EMDPersonEmployment emplPers = new EMDPersonEmployment(pers, empl);
                    List<TextValueModel> avEmploymentsTextValue = new List<TextValueModel>();
                    avEmploymentsTextValue.Add(new TextValueModel(emplPers.FullDisplayNameWithUserIdAndPersNr, emplPers.Empl.Guid));

                    ouroModel.availableEmployments = avEmploymentsTextValue;
                }
            }


            //SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            //List<EMDOrgUnit> avOrgUnits = secUser.AllowedOrgUnits(SecurityPermission.OrgUnitRoleManager_View_Manage);

            OrgUnitHandler orgUnitHandler = new OrgUnitHandler();
            EMDOrgUnit avOrgUnit = (EMDOrgUnit)orgUnitHandler.GetObject<EMDOrgUnit>(orgUnitRole.O_Guid);

            List<TextValueModel> avOrgUnitsTextValue = new List<TextValueModel>();
            avOrgUnitsTextValue.Add(new TextValueModel(avOrgUnit.Name, avOrgUnit.Guid));
            ouroModel.availableOrgUnits = avOrgUnitsTextValue;

            RoleHandler roleHandler = new RoleHandler();
            List<EMDRole> avRoles = roleHandler.GetObjects<EMDRole, Role>().Cast<EMDRole>().ToList();
            List<TextValueModel> avRolesTextValue = new List<TextValueModel>();
            avRoles.ForEach(item =>
            {
                avRolesTextValue.Add(new TextValueModel(item.Name, item.Guid));
            });

            ouroModel.availableRoles = avRolesTextValue;



            if (isPartialView)
            {
                return PartialView("View", ouroModel);
            }
            else
            {
                return View("View", ouroModel);
            }
        }

        [HttpGet]
        [Route("CreateByGuid/{orgunitGuid}")]
        [Route("CreateByGuid/{orgunitGuid}/{isPartialView}")]
        [Route("Create")]
        [Route("Create/{isPartialView}")]
        public ActionResult Create(string orgunitGuid, bool isPartialView = false)
        {
            OrgUnitRoleModel ouroModel = new OrgUnitRoleModel();
            ouroModel.O_Guid = orgunitGuid;
            ouroModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!ouroModel.CanManage)
                return GetNoPermissionView(isPartialView);

            #region no security

            OrgUnitController orgUnitController = new OrgUnitController();
            List<OrgUnitModel> allOrgunits = orgUnitController.GetList();

            ouroModel.availableOrgUnits = new List<TextValueModel>();
            List<TextValueModel> avOrgUnitsTextValue = new List<TextValueModel>();

            List<EMDEnterprise> enterprises = new EnterpriseHandler().GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList();
            allOrgunits.ForEach(item =>
            {
                EMDEnterprise enterprise = (from a in enterprises where a.Guid == item.E_Guid select a).FirstOrDefault();
                if (enterprise != null)
                {
                    avOrgUnitsTextValue.Add(new TextValueModel(string.Format("{0} >> {1}", item.Name, enterprise.NameShort), item.Guid));
                }
                else
                {
                avOrgUnitsTextValue.Add(new TextValueModel(item.Name, item.Guid));
                }
            });

            ouroModel.availableOrgUnits = avOrgUnitsTextValue;
            RoleController roleController = new RoleController();
            RoleHandler roleHandler = new RoleHandler();
            List<EMDRole> avRoles = roleHandler.GetObjects<EMDRole, Role>().Cast<EMDRole>().ToList();
            List<TextValueModel> avRolesTextValue = new List<TextValueModel>();

            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);

            avRoles.ForEach(item =>
            {
                if (secUser.IsAdmin || (item.R_ID != RoleHandler.PERSON))
                    avRolesTextValue.Add(new TextValueModel(item.Name, item.Guid));
            });

            ouroModel.availableRoles = avRolesTextValue;


            List<EMDPersonEmployment> avEmployment = secUser.AllowedEmploymentsForEnterprises();
            List<TextValueModel> avEmploymentsTextValue = new List<TextValueModel>();
            avEmployment.ForEach(item =>
            {
                avEmploymentsTextValue.Add(new TextValueModel(item.FullDisplayNameWithUserIdAndPersNr, item.Empl.Guid));
            });

            ouroModel.availableEmployments = avEmploymentsTextValue;



            #endregion

            #region use Security

            // TODO: mayerr performance issue security

            //SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            //List<EMDOrgUnit> avOrgUnits = secUser.AllowedOrgUnits(SecurityPermission.OrgUnitRoleManager_View_Manage);
            //List<TextValueModel> avOrgUnitsTextValue = new List<TextValueModel>();
            //avOrgUnits.ForEach(item =>
            //{
            //    avOrgUnitsTextValue.Add(new TextValueModel(item.Name, item.Guid));
            //});

            //ouroModel.availableOrgUnits = avOrgUnitsTextValue;

            //RoleHandler roleHandler = new RoleHandler();
            //List<EMDRole> avRoles = roleHandler.GetObjects<EMDRole, Role>().Cast<EMDRole>().ToList();
            //List<TextValueModel> avRolesTextValue = new List<TextValueModel>();
            //avRoles.ForEach(item =>
            //{
            //    avRolesTextValue.Add(new TextValueModel(item.Name, item.Guid));
            //});

            //ouroModel.availableRoles = avRolesTextValue;

            //List<EMDPersonEmployment> avEmployment = secUser.AllowedEmploymentsForEnterprises();
            //List<TextValueModel> avEmploymentsTextValue = new List<TextValueModel>();
            //avEmployment.ForEach(item =>
            //{
            //    avEmploymentsTextValue.Add(new TextValueModel(item.FullDisplayNameWithUserIdAndPersNr, item.Empl.Guid));
            //});

            //ouroModel.availableEmployments = avEmploymentsTextValue;

            #endregion

            if (isPartialView)
            {
                return PartialView("Create", ouroModel);
            }
            else
            {
                return View("Create", ouroModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoCreate")]
        public ActionResult DoCreate(OrgUnitRoleModel orgUnitRoleModel)
        {
            Exception handledException = null;
            orgUnitRoleModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (orgUnitRoleModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!orgUnitRoleModel.CanManage || !orgUnitRoleModel.IsAllowedObject(this.UserName, orgUnitRoleModel.O_Guid, true))
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    OrgUnitRoleHandler ouroHandler = new OrgUnitRoleHandler(this.PersonGuid);
                    EMDOrgUnitRole orgUnitRole = new EMDOrgUnitRole();
                    ReflectionHelper.CopyProperties<OrgUnitRoleModel, EMDOrgUnitRole>(ref orgUnitRoleModel, ref orgUnitRole);


                    EMDRole role = new RoleManager().Get(orgUnitRole.R_Guid);
                    if (role != null)
                    {
                        orgUnitRole.R_ID = role.R_ID;
                    }

                    new OrgUnitRoleManager().Create(this.PersonGuid, secUser.IsAdmin, orgUnitRole);
                }
                catch (EntityNotAllowedException ex)
                {
                    handledException = ex;
                    errmsg = ex.Message;
                    if (ex.EntityClassName.Equals("orgunitrole", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (ex.EntityNotAllowedExceptionType != EnumEntityNotAllowedError.EntityAllowedOnlyOnceForSelectedParameters)
                        {
                            errmsg = "The role Person is already configured for the requested Employment in the Orgunit!";
                        }
                    }
                    ModelState.AddModelError("error", errmsg);
                }
                catch (EdpSecurityException ex)
                {
                    handledException = ex;
                    errmsg = ex.Message;
                    if (ex.EntityClassName.Equals("orgunitrole", StringComparison.InvariantCultureIgnoreCase))
                    {
                        errmsg = "Only Admins are allowed to add the role Person to a Orgunit!";
                    }
                    ModelState.AddModelError("error", errmsg);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Orgunit-Role could not be created!" + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                    logger.Error(errmsg, ex, ControllerContext?.HttpContext);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", orgUnitRoleModel, handledException, errmsg);


            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Orgunit-Role has been created!" });
            }
        }

        [Route("GetOrganizationUnits")]
        [HandleError()]
        public ActionResult GetOrganizationUnits([DataSourceRequest] DataSourceRequest request, string text)
        {

            OrgUnitManager manager = new OrgUnitManager();
            List<EMDOrgUnit> emdUnits = manager.GetOrganizationUnits(text, 300);

            List<OrgUnitModel> myresult = new List<OrgUnitModel>();
            emdUnits.ForEach(item =>
            {
                myresult.Add(OrgUnitModel.Initialize((EMDOrgUnit)item));
            });

            myresult = myresult.OrderBy(item => item.Name).ToList();

            return Json(myresult, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [Route("Delete")]
        public ActionResult Delete(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.OrgUnitRoleManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                OrgUnitRoleManager manager = new OrgUnitRoleManager(this.PersonGuid);
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