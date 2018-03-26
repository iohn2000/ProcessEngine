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
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Role")]
    public class RoleController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        // GET: Role
        [Route]
        public ActionResult Index()
        {
            return RedirectToAction(MANAGEROUTE);
        }

        [Route("Manage")]
        public ActionResult Manage()
        {
            RoleModel model = new RoleModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            //PopulateOrgUnits();
            //just return an empty view since all data is Ajax-driven
            return View("Manage", model);
        }

        public List<RoleModel> getRoleList()
        {
            List<RoleModel> myresult = new List<RoleModel>();
            RoleHandler roleHandler = new RoleHandler();

            //SecurityActionManager securityManager = new SecurityActionManager();
            //List<EMDSecurityAction> emdSecurityActions = securityManager.GetList();

            try
            {
                RoleModel dummySecurityModel = RoleModel.Initialize(new EMDRole());
                dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                List<IEMDObject<EMDRole>> listRole = roleHandler.GetObjects<EMDRole, Role>();
                List<IEMDObject<EMDRole>> listRole2 = listRole.ToList();

                foreach (EMDRole item in listRole)
                {
                    // show only roles with no security action
                    //if (!emdSecurityActions.Exists(sec => sec.ROLE_Guid == item.Guid))
                    //{
                    RoleModel roleModel = RoleModel.Initialize(item);

                    EMDRole roleModelParent = (EMDRole)listRole2.Where(item2 => item2.Guid == item.Guid_Parent).FirstOrDefault();
                    EMDRole roleModelRoot = (EMDRole)listRole2.Where(item2 => item2.Guid == item.Guid_Root).FirstOrDefault();

                    if (roleModelParent != null)
                        roleModel.Name_Parent = roleModelParent.Name;

                    //if (roleModelRoot != null)
                    //    roleModel.Name_Root = roleModelRoot.Name;

                    roleModel.Guid_Parent = item.Guid_Parent;
                    //roleModel.Guid_Root = item.Guid_Root;

                    roleModel.CanManage = dummySecurityModel.CanManage;
                    roleModel.CanView = dummySecurityModel.CanView;

                    myresult.Add(roleModel);
                    //}
                }

                //listRole.ForEach(item =>
                //{
                //    myresult.Add(RoleModel.copyFromObject((EMDRole)item));
                //});

                myresult = myresult.OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading role", e);
            }
            return myresult;
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            List<RoleModel> roleList = getRoleList();
            DataSourceResult myresult;
            myresult = roleList.ToDataSourceResult(request, ModelState);

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadForSelect")]
        public ActionResult ReadForSelect([DataSourceRequest]DataSourceRequest request, string text = "%")
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                RoleHandler handler = new RoleHandler();
                List<IEMDObject<EMDRole>> emdEntities = handler.GetObjects<EMDRole, Role>("IsSecurity == false");


                //SecurityActionManager securityManager = new SecurityActionManager();
                //List<EMDSecurityAction> emdSecurityActions = securityManager.GetList();


                emdEntities.ForEach(entity =>
                {
                    //if (!emdSecurityActions.Exists(sec => sec.ROLE_Guid == entity.Guid))
                    //{
                    keyValuePairs.Add(new TextValueModel(((EMDRole)entity).Name, entity.Guid));
                    //}
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

        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{role_guid}")]
        [Route("Edit/{role_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string role_guid, bool isPartialView = false)
        {
            RoleHandler roleHandler = new RoleHandler();
            EMDRole role = (EMDRole)roleHandler.GetObject<EMDRole>(role_guid);
            RoleModel roleModel = RoleModel.Initialize(role);
            roleModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!roleModel.CanManage)
                return GetNoPermissionView(isPartialView);

            roleModel.AvailableRoles = this.getRoleList();

            EMDRole currentRole = (EMDRole)roleHandler.GetObject<EMDRole>(role_guid);
            RoleModel currentRoleModel = RoleModel.Initialize(currentRole);

            EMDRole parentRole = (EMDRole)roleHandler.GetObject<EMDRole>(currentRole.Guid_Parent);
            RoleModel parentRoleModel = RoleModel.Initialize(parentRole);

            List<EMDRole> listRoles = roleHandler.GetAllSubRolesFromParent(role_guid, 1);

            RoleModelList roleModelList = new RoleModelList();
            roleModelList.CurrentRole = currentRoleModel;
            roleModelList.ParentRole = parentRoleModel;
            if (parentRole.Guid == currentRole.Guid)
            {
                roleModelList.HasParent = false;
                roleModelList.ParentRoleLevel = 0;
                roleModelList.CurrentRoleLevel = 0;
            }
            else
            {
                roleModelList.HasParent = true;
                roleModelList.ParentRoleLevel = 0;
                roleModelList.CurrentRoleLevel = 1;
            }

            foreach (EMDRole rol in listRoles)
            {
                if (rol.Guid != currentRole.Guid && rol.Guid != parentRole.Guid)
                {
                    RoleModel enterpriseModel = RoleModel.Initialize(rol);
                    roleModelList.RoleModels.Add(enterpriseModel);
                }
            }
            roleModel.roleModelList = roleModelList;

            if (isPartialView)
            {
                return PartialView("Edit", roleModel);
            }
            else
            {
                return View("Edit", roleModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, RoleModel roleModel)
        {
            Exception handledException = null;
            roleModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (roleModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!roleModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }
                    RoleManager roleManager = new RoleManager(this.PersonGuid, MODIFY_COMMENT);
                    EMDRole role = new EMDRole();
                    ReflectionHelper.CopyProperties(ref roleModel, ref role);

                    if (roleManager.IsRoleIdAvailable(roleModel.R_ID, role))
                    {
                    
                        roleManager.Update(role);
                    }
                    else
                    {
                        ModelState.AddModelError("R_ID", "The role ID is not available. Please check available IDs with Button 'Get Next ID'.");
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Role: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", roleModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Role has been updated!" });
            }
        }

        [HttpGet]
        [Route("View")]
        [Route("View/{role_guid}")]
        [Route("View/{role_guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string role_guid, bool isPartialView = false)
        {
            RoleHandler roleHandler = new RoleHandler();
            EMDRole role = (EMDRole)roleHandler.GetObject<EMDRole>(role_guid);
            RoleModel roleModel = RoleModel.Initialize(role);
            roleModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!roleModel.CanView)
                return GetNoPermissionView(isPartialView);

            roleModel.AvailableRoles = this.getRoleList();

            EMDRole currentRole = (EMDRole)roleHandler.GetObject<EMDRole>(role_guid);
            RoleModel currentRoleModel = RoleModel.Initialize(currentRole);

            EMDRole parentRole = (EMDRole)roleHandler.GetObject<EMDRole>(currentRole.Guid_Parent);
            RoleModel parentRoleModel = RoleModel.Initialize(parentRole);

            List<EMDRole> listRoles = roleHandler.GetAllSubRolesFromParent(role_guid, 1);

            RoleModelList roleModelList = new RoleModelList();
            roleModelList.CurrentRole = currentRoleModel;
            roleModelList.ParentRole = parentRoleModel;
            if (parentRole.Guid == currentRole.Guid)
            {
                roleModelList.HasParent = false;
                roleModelList.ParentRoleLevel = 0;
                roleModelList.CurrentRoleLevel = 0;
            }
            else
            {
                roleModelList.HasParent = true;
                roleModelList.ParentRoleLevel = 0;
                roleModelList.CurrentRoleLevel = 1;
            }

            foreach (EMDRole rol in listRoles)
            {
                if (rol.Guid != currentRole.Guid && rol.Guid != parentRole.Guid)
                {
                    RoleModel enterpriseModel = RoleModel.Initialize(rol);
                    roleModelList.RoleModels.Add(enterpriseModel);
                }
            }
            roleModel.roleModelList = roleModelList;

            if (isPartialView)
            {
                return PartialView("View", roleModel);
            }
            else
            {
                return View("View", roleModel);
            }
        }

        [HttpGet]
        [Route("Create")]
        [Route("Create/{isPartialView}")]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, bool isPartialView = false)
        {
            RoleModel roleMod = new RoleModel();
            roleMod.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!roleMod.CanManage)
                return GetNoPermissionView(isPartialView);

            roleMod.AvailableRoles = this.getRoleList();

            if (isPartialView)
            {
                return PartialView("Create", roleMod);
            }
            else
            {
                return View("Create", roleMod);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoCreate")]
        public ActionResult DoCreate([DataSourceRequest]DataSourceRequest request, RoleModel roleModel)
        {
            Exception handledException = null;
            roleModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (roleModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!roleModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }



                    RoleManager roleManager = new RoleManager(this.PersonGuid, MODIFY_COMMENT);

                    if (roleManager.IsRoleIdAvailable(roleModel.R_ID))
                    {

                        EMDRole role = new EMDRole();
                        ReflectionHelper.CopyProperties<RoleModel, EMDRole>(ref roleModel, ref role);
                        roleManager.Create(role);
                    }
                    else
                    {
                        ModelState.AddModelError("R_ID", "The role ID is not available. Please check available IDs with Button 'Get Next ID'.");
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Role could not be created! " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", roleModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Role has been created!" });
            }
        }

        [HttpPost]
        [Route("DeleteRole")]
        public ActionResult DeleteRole(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.RoleManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                RoleManager manager = new RoleManager(this.PersonGuid);
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

        [HttpPost]
        [Route("GetNextRoleId")]
        public ActionResult GetNextRoleId(int requestRoleId)
        {
            bool success = true;
            string message = string.Empty;
            int nextRoleId = requestRoleId;
            if (nextRoleId == 0)
            {
                nextRoleId++;
            }
            try
            {
                RoleManager roleManager = new RoleManager();
                nextRoleId = roleManager.GetNextRoleIdNumber(requestRoleId);
            }
            catch (Exception ex)
            {
                success = false;
                logger.Error(string.Format("GetNextRoleId throwed an error for requestRoleId: {0}", requestRoleId), ex);
            }

            if (requestRoleId == nextRoleId)
            {
                message = string.Format("Your requested Role ID {0} is available.", requestRoleId);
            }
            else
            {
                message = string.Format("Your requested Role ID {0} is not available and was changed to {1}.", requestRoleId, nextRoleId);
            }

            return Json(new { success = success, nextRoleId = nextRoleId, message = message });
        }

        [HttpPost]
        [Route("IsRoleIdAvailable")]
        public ActionResult IsRoleIdAvailable(int requestRoleId)
        {
            bool success = true;
            bool isAvailable = false;
            try
            {
                RoleManager roleManager = new RoleManager();
                isAvailable = roleManager.IsRoleIdAvailable(requestRoleId);
            }
            catch (Exception ex)
            {
                success = false;
                logger.Error(string.Format("IsRoleIdAvailable throwed an error for requestRoleId: {0}", requestRoleId), ex);
            }

            return Json(new { success = success, isAvailable = isAvailable });
        }
    }
}