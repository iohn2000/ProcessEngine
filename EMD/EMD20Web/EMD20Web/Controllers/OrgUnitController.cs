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
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EDP.Core.Entities.Enhanced;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("OrgUnit")]
    public class OrgUnitController : BaseController
    {
        internal new IISLogger logger = ISLogger.GetLogger("OrgUnitController");

        private const string MANAGEROUTE = "Manage";

        [Route]
        // GET: Location
        public ActionResult Index()
        {
            return RedirectToAction(MANAGEROUTE);
        }

        [Route("ManageSecurity")]
        public ActionResult ManageSecurity()
        {
            OrgUnitModel model = new OrgUnitModel();
            //model.InitializeSecurity(this.UserName);
            //if (!model.CanManage && !model.CanView)
            //    return GetNoPermissionView(false);
            model.IsSecurity = true;

            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            if (secUser.IsAdmin)
            {
                model.CanManage = true;
                model.CanView = true;
            }
            else
            {
                return GetNoPermissionView(false);
            }




            PopulateOrgUnits();
            return View("Manage", model);
        }

        [Route("Manage")]
        public ActionResult Manage()
        {
            OrgUnitModel model = new OrgUnitModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage && !model.CanView)
                return GetNoPermissionView(false);

            PopulateOrgUnits();
            return View("Manage", model);
        }

        [Route("ReadAllowedForSelectDs")]
        [HandleError()]
        public ActionResult ReadAllowedForSelectDs([DataSourceRequest] DataSourceRequest request)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                //List<EMDEnterprise> enterprises = secUser.AllowedEnterprises(SecurityPermission.CostCenterManager_View_Manage).OrderBy(item => item.NameShort).ToList();
                List<EMDEnterprise> allEnterprises = Manager.EnterpriseManager.GetList();
                List<EMDOrgUnit> allowedOrgUnits = secUser.AllowedOrgUnits(SecurityPermission.OrgUnitManager_View_Manage).OrderBy(a => a.Name).ToList();

                foreach (EMDOrgUnit item in allowedOrgUnits)
                {
                    if (!item.IsSecurity)
                    {
                        EMDEnterprise foundEnterprise = allEnterprises.Find(e => e.Guid == item.E_Guid);
                        keyValuePairs.Add(new TextValueModel(OrgUnitModel.GetExtendedName(item, foundEnterprise), item.Guid));
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading allowed-orgunits";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }



        [Route("ReadAllowedWithSecurityForSelectDs")]
        [HandleError()]
        public ActionResult ReadAllowedWithSecurityForSelectDs([DataSourceRequest] DataSourceRequest request)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                //List<EMDEnterprise> enterprises = secUser.AllowedEnterprises(SecurityPermission.CostCenterManager_View_Manage).OrderBy(item => item.NameShort).ToList();
                List<EMDEnterprise> allEnterprises = Manager.EnterpriseManager.GetList();
                List<EMDOrgUnit> allowedOrgUnits = secUser.AllowedOrgUnits(SecurityPermission.OrgUnitManager_View_Manage).OrderBy(a => a.Name).ToList();

                foreach (EMDOrgUnit item in allowedOrgUnits)
                {
                    if (item.IsSecurity)
                    {
                        EMDEnterprise foundEnterprise = allEnterprises.Find(e => e.Guid == item.E_Guid);
                        keyValuePairs.Add(new TextValueModel(OrgUnitModel.GetExtendedName(item, foundEnterprise), item.Guid));
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading allowed-orgunits";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [Route("ReadForSelect")]
        public ActionResult ReadForSelect([DataSourceRequest]DataSourceRequest request, string text = "%")
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
                string errorMessage = "Error reading orgunits";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        [Route("ReadRootsForSelect")]
        public ActionResult ReadRootsForSelect()
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                List<EMDOrgUnit> orgUnits = new OrgUnitHandler().GetAllRootOrgunits(false);


                orgUnits.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(((EMDOrgUnit)entity).Name, entity.Guid));
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading orgunits";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }

        //http://localhost:8021/OrgUnit/ReadForSelectForEnterprise?ente_guid=
        [HttpGet]
        [Route("ReadForSelectForEnterprise")]
        //[Route("ReadForSelectForEnterprise/{ente_guid}")]
        public JsonResult ReadForSelectForEnterprise(string ente_guid, string text = "%")
        {
            if (text.Trim() == String.Empty) text = "%";
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            if (!string.IsNullOrEmpty(ente_guid))
            {

                try
                {
                    OrgUnitHandler handler = new OrgUnitHandler();
                    List<EMDOrgUnit> emdEntities = handler.GetOrgUnitsForCompany(ente_guid);


                    emdEntities.ForEach(entity =>
                    {
                        keyValuePairs.Add(new TextValueModel(entity.Name, entity.Guid));
                    });

                    if (text != "%")
                    {
                        keyValuePairs = (from item in keyValuePairs where item.Text.ToLower().Contains(text.ToLower()) select item).ToList();
                    }

                    keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
                }
                catch (Exception ex)
                {
                    string errorMessage = "Error reading orgunits";
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

        public List<OrgUnitModel> GetList()
        {
            List<OrgUnitModel> myresult = new List<OrgUnitModel>();
            OrgUnitHandler ouHandler = new OrgUnitHandler();

            try
            {
                OrgUnitModel dummySecurityModel = OrgUnitModel.Initialize(new EMDOrgUnit());
                dummySecurityModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                //Get OrgUnits
                List<EMDOrgUnit> listOrgUnit = ouHandler.GetObjects<EMDOrgUnit, OrgUnit>().Cast<EMDOrgUnit>().ToList();


                //Get Enterprises
                EnterpriseHandler enteHandler = new EnterpriseHandler();
                List<IEMDObject<EMDEnterprise>> listOfIEnterprise = enteHandler.GetObjects<EMDEnterprise, Enterprise>();
                List<EMDEnterprise> listEnterprise = new List<EMDEnterprise>();

                listOfIEnterprise.ForEach(item =>
                {
                    listEnterprise.Add((EMDEnterprise)item);
                });

                //Fill E_ShortName
                foreach (EMDOrgUnit ou in listOrgUnit)
                {
                    OrgUnitModel ouModel = OrgUnitModel.Initialize(ou);

                    EMDEnterprise ente = listEnterprise.Where(item => item.Guid == ou.E_Guid).FirstOrDefault();
                    if (ente != null)
                        ouModel.E_ShortName = ente.NameShort;

                    if (!string.IsNullOrEmpty(ou.Guid_Parent))
                    {
                        EMDOrgUnit parent = listOrgUnit.Where(item => item.Guid == ou.Guid_Parent).FirstOrDefault();
                        if (parent != null)
                        {
                            ouModel.ParentName = parent.Name;
                        }
                    }

                    if (!string.IsNullOrEmpty(ou.Guid_Root))
                    {
                        EMDOrgUnit root = listOrgUnit.Where(item => item.Guid == ou.Guid_Root).FirstOrDefault();
                        if (root != null)
                        {
                            ouModel.RootName = root.Name;
                        }
                    }

                    ouModel.CanManage = dummySecurityModel.CanManage;
                    ouModel.CanView = dummySecurityModel.CanView;
                    myresult.Add(ouModel);
                }

                myresult = myresult.OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading orgunits", e);
            }
            return myresult;
        }

        public List<OrgUnitModel> GetListForEnterprise(string ente_guid)
        {
            List<OrgUnitModel> myresult = new List<OrgUnitModel>();
            OrgUnitHandler ouHandler = new OrgUnitHandler();

            try
            {
                List<EMDOrgUnit> listOrgUnit = ouHandler.GetOrgUnitsForCompany(ente_guid);

                listOrgUnit.ForEach(item =>
                {
                    myresult.Add(OrgUnitModel.Initialize((EMDOrgUnit)item));
                });

                myresult = myresult.OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error reading orgunits for " + ente_guid, e);
            }
            return myresult;
        }

        [Route("Read")]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            DataSourceResult myresult = null;
            try
            {
                List<OrgUnitModel> list = this.GetList();
                myresult = list.ToDataSourceResult(request, ModelState);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading orgunits";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        private List<OrgUnitModel> GetSubOrgunitListFlat(bool isSecurity)
        {
            List<OrgUnitModel> orgUnitModels = new List<OrgUnitModel>();

            List<EMDOrgUnit> listOrgUnit = new OrgUnitHandler().GetObjects<EMDOrgUnit, OrgUnit>(string.Format("IsSecurity = {0}", isSecurity)).Cast<EMDOrgUnit>().ToList();
            List<EMDEnterprise> listEnterprise = new EnterpriseHandler().GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList();

            foreach (EMDOrgUnit ou in listOrgUnit)
            {
                OrgUnitModel currentModel = OrgUnitModel.Initialize(ou);

                EMDEnterprise ente = listEnterprise.SingleOrDefault(item => item.Guid == ou.E_Guid);
                if (ente != null)
                {
                    currentModel.E_ShortName = ente.NameShort;
                }

                if (!string.IsNullOrEmpty(ou.Guid_Parent))
                {
                    EMDOrgUnit parent = listOrgUnit.SingleOrDefault(item => item.Guid == ou.Guid_Parent);
                    if (parent != null)
                    {
                        currentModel.ParentName = parent.Name;
                    }
                }


                if (!string.IsNullOrEmpty(ou.Guid_Root))
                {
                    EMDOrgUnit root = listOrgUnit.SingleOrDefault(item => item.Guid == ou.Guid_Root);
                    if (root != null)
                    {
                        currentModel.RootName = root.Name;
                    }
                }



                // allow manage always because of performance issues
                currentModel.CanManage = true;
                // currentModel.InitializeSecurity(this.UserName);
                orgUnitModels.Add(currentModel);
            }


            return orgUnitModels;
        }


        private List<OrgUnitModel> GetSubOrgunitListTree(string parentGuid, bool isSecurity)
        {
            List<OrgUnitModel> orgUnitModels = new List<OrgUnitModel>();


            List<EMDOrgUnit> listOrgUnit = new OrgUnitHandler().GetAllSubOrgUnitsFromParent(parentGuid, isSecurity, 2);
            List<EMDEnterprise> listEnterprise = new EnterpriseHandler().GetObjects<EMDEnterprise, Enterprise>().Cast<EMDEnterprise>().ToList();

            //Fill E_ShortName
            OrgUnitModel lastModel = null;
            foreach (EMDOrgUnit ou in listOrgUnit)
            {
                if (ou.Level > 0)
                {
                    OrgUnitModel currentModel = OrgUnitModel.Initialize(ou);
                    EMDEnterprise ente = listEnterprise.Where(item => item.Guid == ou.E_Guid).FirstOrDefault();
                    if (ente != null)
                        currentModel.E_ShortName = ente.NameShort;

                    if (!string.IsNullOrEmpty(ou.Guid_Parent))
                    {
                        EMDOrgUnit parent = listOrgUnit.Where(item => item.Guid == ou.Guid_Parent).FirstOrDefault();
                        if (parent != null)
                        {
                            currentModel.ParentName = parent.Name;
                        }
                    }

                    if (!string.IsNullOrEmpty(ou.Guid_Root))
                    {
                        EMDOrgUnit root = listOrgUnit.Where(item => item.Guid == ou.Guid_Root).FirstOrDefault();
                        if (root != null)
                        {
                            currentModel.RootName = root.Name;
                        }
                    }


                    if (lastModel != null)
                    {
                        if (lastModel.Level == currentModel.Level)
                        {
                            if (lastModel.Parent == null)
                            {
                                if (currentModel.Level == 1)
                                {
                                    orgUnitModels.Add(currentModel);
                                }
                                else
                                {
                                    // error in logic
                                }

                            }
                            else
                            {
                                if (lastModel.Parent.Guid == currentModel.Guid_Parent)
                                {
                                    if (lastModel.Parent.Children == null)
                                    {
                                        lastModel.Parent.Children = new List<OrgUnitModel>();
                                    }
                                    currentModel.Parent = lastModel.Parent;
                                    if (currentModel.Parent.Guid != lastModel.Guid)
                                    {
                                        currentModel.Parent.Children.Add(currentModel);
                                    }
                                }
                            }
                        }


                        if (currentModel.Level > lastModel.Level)
                        {
                            if (currentModel.Guid_Parent == lastModel.Guid)
                            {
                                currentModel.Parent = lastModel;

                                if (currentModel.Parent.Children == null)
                                {
                                    currentModel.Parent.Children = new List<OrgUnitModel>();
                                }
                                if (currentModel.Parent.Guid != lastModel.Guid)
                                {
                                    currentModel.Parent.Children.Add(currentModel);
                                }

                                if (lastModel.Children == null)
                                {
                                    lastModel.Children = new List<OrgUnitModel>();
                                }

                                lastModel.Children.Add(currentModel);
                            }
                        }

                        if (currentModel.Level < lastModel.Level)
                        {
                            OrgUnitModel found = OrgUnitModel.Find(lastModel, currentModel.Guid_Parent);

                            if (found == null)
                            {
                                if (currentModel.Level == 1)
                                {
                                    orgUnitModels.Add(currentModel);
                                }
                                else
                                {
                                    // error in logic
                                }
                            }
                            else
                            {
                                currentModel.Parent = found;
                                if (currentModel.Parent.Children == null)
                                {
                                    if (currentModel.Parent.Guid != lastModel.Guid)
                                    {
                                        currentModel.Parent.Children = new List<OrgUnitModel>();
                                    }
                                }
                                if (currentModel.Parent.Guid != lastModel.Guid)
                                {
                                    currentModel.Parent.Children.Add(currentModel);
                                }
                            }

                        }
                    }
                    else
                    {
                        if (currentModel.Level == 1)
                        {
                            orgUnitModels.Add(currentModel);
                        }
                        else
                        {
                            // error in logic
                        }
                    }


                    lastModel = currentModel;
                }
            }
            // orgUnitModels = orgUnitModels.OrderBy(item => item.Name).ToList();




            /* SEARCH HELPER TO GET COUNT OF PERSONS - USE ORGUNITROLE ENHANCED LIST INSTEAD
            /*
            // Get OrgunitRoles
            List<EMDOrgUnitRole> listOrgUnitRolesTemp = new OrgUnitRoleHandler().GetObjects<EMDOrgUnitRole, OrgUnitRole>().Cast<EMDOrgUnitRole>().ToList();
            List<EMDEmployment> listEmployments = new EmploymentHandler().GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>().ToList();
            List<EMDRole> listRoles = new RoleHandler().GetObjects<EMDRole, Role>().Cast<EMDRole>().ToList();

            List<EMDOrgUnitRole> listOrgUnitRoles = new List<EMDOrgUnitRole>();
            // check if all dependencies are active
            foreach (EMDOrgUnitRole currentOrgunitRole in listOrgUnitRolesTemp)
            {
                if (listRoles.Where(o => o.Guid == currentOrgunitRole.R_Guid).Count() > 0 && listEmployments.Where(o => o.Guid == currentOrgunitRole.EP_Guid).Count() > 0)
                {
                    listOrgUnitRoles.Add(currentOrgunitRole);
                }
            }
            */

            //  List<EMDOrgUnitRoleEnhanced> eMDOrgUnitRoleEnhancedList = new OrgUnitRoleManager().GetOrgUnitRoleEnhancedList();

            foreach (OrgUnitModel model in orgUnitModels)
            {
                model.HasChildren = model.Children?.Count > 0;
                // model.CountAssignedPersons = listOrgUnitRoles.Where(o => o.O_Guid == model.Guid).Count();
                // model.CountAssignedPersons = eMDOrgUnitRoleEnhancedList.Where(o => o.O_Guid == model.Guid).Count();
                model.Parent = null;
                model.Children = null;

                // allow manage always because of performance issues
                model.CanManage = true;
                //   model.InitializeSecurity(this.UserName);
            }

            return orgUnitModels;

        }


        [Route("ReadSubOrgunits")]
        public ActionResult ReadSubOrgunits([DataSourceRequest]DataSourceRequest request, string parentGuid, bool isSecurity, bool showTree)
        {
            List<OrgUnitModel> orgUnitModels = new List<OrgUnitModel>();

            DataSourceResult myresult = null;

            try
            {
                if (showTree)
                {
                    orgUnitModels = GetSubOrgunitListTree(parentGuid, isSecurity);
                }
                else
                {
                    orgUnitModels = GetSubOrgunitListFlat(isSecurity);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading orgunits";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }

            // get a small list of orgunits first, then enhance data with infos
            myresult = orgUnitModels.ToDataSourceResult(request, ModelState);

            List<EMDOrgUnitRoleEnhanced> eMDOrgUnitRoleEnhancedList = new OrgUnitRoleManager().GetOrgUnitRoleEnhancedList();
            foreach (OrgUnitModel item in myresult.Data.Cast<OrgUnitModel>().ToList())
            {
                item.CountAssignedPersons = eMDOrgUnitRoleEnhancedList.Where(o => o.O_Guid == item.Guid).Count();
            }

            return Json(myresult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("ReadAllForSelectForEnterprise")]
        //[Route("ReadForSelectForEnterprise/{ente_guid}")]
        public JsonResult ReadAllForSelectForEnterprise([DataSourceRequest]DataSourceRequest request)
        {
            List<OrgUnitModel> myresult = this.GetList();
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Route("Edit")]
        [Route("Edit/{orgu_guid}")]
        [Route("Edit/{orgu_guid}/{isPartialView}")]
        public ActionResult Edit([DataSourceRequest]DataSourceRequest request, string orgu_guid, bool isPartialView = false)
        {
            OrgUnitHandler orguHandler = new OrgUnitHandler();
            EMDOrgUnit orgu = (EMDOrgUnit)orguHandler.GetObject<EMDOrgUnit>(orgu_guid);
            OrgUnitModel orgunitModel = OrgUnitModel.Initialize(orgu);
            List<EMDEnterprise> allEnterprises = Manager.EnterpriseManager.GetList();

            orgunitModel.EnterpriseSelection = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "E_Guid",
                ObjectLabel = "Enterprise",
                ObjectValue = orgunitModel.E_Guid,
                ObjectText = allEnterprises.Find(e => e.Guid == orgunitModel.E_Guid)?.NameShort,
                TargetControllerMethodName = "ReadForSelectDs",
                TargetControllerName = "Enterprise"
            };
            EMDOrgUnit currentOrgUnit = (EMDOrgUnit)orguHandler.GetObject<EMDOrgUnit>(orgu_guid);
            EMDOrgUnit parentOrgUnit = (EMDOrgUnit)orguHandler.GetObject<EMDOrgUnit>(currentOrgUnit.Guid_Parent);

            orgunitModel.ParentSelection = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "Guid_Parent",
                ObjectLabel = "Parent",
                ObjectValue = orgunitModel.Guid_Parent,
                ObjectText = OrgUnitModel.GetExtendedName(parentOrgUnit, allEnterprises.Find(e => e.Guid == parentOrgUnit.E_Guid)),
                TargetControllerMethodName = orgunitModel.IsSecurity ? "ReadAllowedWithSecurityForSelectDs" : "ReadAllowedForSelectDs",
                TargetControllerName = "OrgUnit"
            };

            orgunitModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!orgunitModel.CanManage || !orgunitModel.IsAllowedObject(this.UserName, orgu_guid, true))
                return GetNoPermissionView(isPartialView);





            OrgUnitModel currentOrgUnitModel = OrgUnitModel.Initialize(currentOrgUnit);

            if (orgunitModel.IsSecurity && !orgunitModel.IsAdmin)
            {
                return GetNoPermissionView(isPartialView);
            }



            List<EMDOrgUnit> listOrgUnits = orguHandler.GetAllSubOrgUnitsFromParent(orgu_guid, false, 1);

            EnterpriseController enteController = new EnterpriseController();

            OrgUnitModelList orgUnitModelList = new OrgUnitModelList();
            orgUnitModelList.CurrentOrgUnit = currentOrgUnitModel;

            if (!String.IsNullOrEmpty(currentOrgUnit.Guid_Parent))
            {
                OrgUnitModel parentOrgUnitModel = OrgUnitModel.Initialize(parentOrgUnit);
                orgUnitModelList.ParentOrgUnit = parentOrgUnitModel;
            }


            if (parentOrgUnit == null || parentOrgUnit.Guid == null || parentOrgUnit.Guid == currentOrgUnit.Guid)
            {
                orgUnitModelList.HasParent = false;
                orgUnitModelList.ParentOrgUnitLevel = 0;
                orgUnitModelList.CurrentOrgUnitLevel = 0;
            }
            else
            {
                orgUnitModelList.HasParent = true;
                orgUnitModelList.ParentOrgUnitLevel = 0;
                orgUnitModelList.CurrentOrgUnitLevel = 1;
            }

            foreach (EMDOrgUnit ou in listOrgUnits)
            {
                if (ou.Guid != currentOrgUnit.Guid && ou.Guid != parentOrgUnit.Guid)
                {
                    OrgUnitModel orgUnitModel = OrgUnitModel.Initialize(ou);
                    orgUnitModelList.OrgUnitModels.Add(orgUnitModel);
                }
            }
            orgunitModel.orgUnitModelList = orgUnitModelList;



            if (isPartialView)
            {
                return PartialView("Edit", orgunitModel);
            }
            else
            {
                return View("Edit", orgunitModel);
            }
        }

        [HttpPost, ValidateInput(false)]
        [Route("DoEdit")]
        //[Route("DoEdit/{pModel}")]
        public ActionResult DoEdit([DataSourceRequest] DataSourceRequest request, OrgUnitModel orguModel)
        {
            Exception handledException = null;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            if (orguModel != null && ModelState.IsValid)
            {
                try
                {
                    orguModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                    if (!orguModel.CanManage || !orguModel.IsAllowedObject(this.UserName, orguModel.Guid, true))
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    if (orguModel.IsSecurity && !orguModel.IsAdmin)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    OrgUnitHandler orguHandler = new OrgUnitHandler(this.PersonGuid);
                    EMDOrgUnit orgu = Manager.OrgUnitManager.Get(orguModel.Guid);



                    orgu = OrgUnitModel.Update(orgu, orguModel);
                    new OrgUnitManager().Update(this.PersonGuid, orgu);
                }
                catch (EntityNotAllowedException ex)
                {
                    handledException = ex;
                    errmsg = ex.Message;
                    ModelState.AddModelError("error", errmsg);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "Could not edit Orgunit: " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", orguModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Orgunit has been updated!" });
            }
        }

        [HttpGet]
        [Route("View")]
        [Route("View/{orgu_guid}")]
        [Route("View/{orgu_guid}/{isPartialView}")]
        public ActionResult View([DataSourceRequest]DataSourceRequest request, string orgu_guid, bool isPartialView = false)
        {
            OrgUnitHandler orguHandler = new OrgUnitHandler();
            EMDOrgUnit orgu = (EMDOrgUnit)orguHandler.GetObject<EMDOrgUnit>(orgu_guid);
            OrgUnitModel orguModel = OrgUnitModel.Initialize(orgu);
            orguModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!orguModel.CanView)
                return GetNoPermissionView(isPartialView);

            EMDOrgUnit currentOrgUnit = (EMDOrgUnit)orguHandler.GetObject<EMDOrgUnit>(orgu_guid);
            OrgUnitModel currentOrgUnitModel = OrgUnitModel.Initialize(currentOrgUnit);

            EMDOrgUnit parentOrgUnit = (EMDOrgUnit)orguHandler.GetObject<EMDOrgUnit>(currentOrgUnit.Guid_Parent);
            OrgUnitModel parentOrgUnitModel = OrgUnitModel.Initialize(parentOrgUnit);

            List<EMDOrgUnit> listOrgUnits = orguHandler.GetAllSubOrgUnitsFromParent(orgu_guid, false, 1);

            EnterpriseController enteController = new EnterpriseController();

            OrgUnitModelList orgUnitModelList = new OrgUnitModelList();
            orgUnitModelList.CurrentOrgUnit = currentOrgUnitModel;
            orgUnitModelList.ParentOrgUnit = parentOrgUnitModel;

            //if (parentOrgUnit.Guid == currentOrgUnit.Guid)
            if (parentOrgUnit == null || parentOrgUnit.Guid == null || parentOrgUnit.Guid == currentOrgUnit.Guid)
            {
                orgUnitModelList.HasParent = false;
                orgUnitModelList.ParentOrgUnitLevel = 0;
                orgUnitModelList.CurrentOrgUnitLevel = 0;
            }
            else
            {
                orgUnitModelList.HasParent = true;
                orgUnitModelList.ParentOrgUnitLevel = 0;
                orgUnitModelList.CurrentOrgUnitLevel = 1;
            }

            foreach (EMDOrgUnit ou in listOrgUnits)
            {
                if (ou.Guid != currentOrgUnit.Guid && ou.Guid != parentOrgUnit.Guid)
                {
                    OrgUnitModel orgUnitModel = OrgUnitModel.Initialize(ou);
                    orgUnitModelList.OrgUnitModels.Add(orgUnitModel);
                }
            }
            orguModel.orgUnitModelList = orgUnitModelList;

            if (isPartialView)
            {
                return PartialView("View", orguModel);
            }
            else
            {
                return View("View", orguModel);
            }
        }

        [HttpGet]
        [Route("Create")]
        [Route("Create/{isSecurity}")]
        [Route("Create/{isSecurity}/{isPartialView}")]
        public ActionResult Create(bool isSecurity, bool isPartialView = false)
        {
            OrgUnitModel orgunitModel = new OrgUnitModel();
            orgunitModel.IsSecurity = isSecurity;
            orgunitModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!orgunitModel.CanManage)
                return GetNoPermissionView(isPartialView);


            orgunitModel.EnterpriseSelection = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "E_Guid",
                ObjectLabel = "Enterprise",
                TargetControllerMethodName = "ReadForSelectDs",
                TargetControllerName = "Enterprise"
            };


            orgunitModel.ParentSelection = new Models.Shared.SelectionViewModel()
            {
                ReferencePropertyName = "Guid_Parent",
                ObjectLabel = "Parent",
                TargetControllerMethodName = "ReadAllowedForSelectDs",
                TargetControllerName = "OrgUnit"
            };

            if (isPartialView)
            {
                return PartialView("Create", orgunitModel);
            }
            else
            {
                return View("Create", orgunitModel);
            }
        }

        [HttpPost]
        [Route("DoCreate")]
        public ActionResult DoCreate([DataSourceRequest]DataSourceRequest request, OrgUnitModel orguModel)
        {
            Exception handledException = null;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            if (orguModel != null && ModelState.IsValid)
            {
                try
                {
                    orguModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                    if (!orguModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
                    if (!string.IsNullOrWhiteSpace(orguModel.E_Guid))
                    {
                        if (!secUser.IsAllowedEnterprise(orguModel.CanManagePermissionString, orguModel.E_Guid))
                        {
                            throw new Exception(SecurityHelper.NoPermissionText);
                        }
                    }
                    else
                    {
                        if (orguModel.IsSecurity)
                        {
                            throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "For a security orgunit an enterprise is required!");
                        }
                    }

                    if (orguModel.IsSecurity && !orguModel.IsAdmin)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    OrgUnitManager omgr = new OrgUnitManager(this.PersonGuid, this.GetType().FullName);
                    EMDOrgUnit orgu = new EMDOrgUnit();

                    orgu = OrgUnitModel.Update(orgu, orguModel);
                    //ReflectionHelper.CopyProperties<OrgUnitModel, EMDOrgUnit>(ref orguModel, ref orgu);

                    orgu = (EMDOrgUnit)omgr.Create(orgu);

                    if (string.IsNullOrEmpty(orgu.Guid_Parent))
                    {
                        orgu.Guid_Parent = orgu.Guid;
                        orgu.Guid_Root = orgu.Guid;

                        OrgUnitHandler orgunitHandler = new OrgUnitHandler();
                        orgunitHandler.UpdateObject<EMDOrgUnit>(orgu, false);
                    }

                }
                catch (EntityNotAllowedException ex)
                {
                    handledException = ex;
                    errmsg = ex.Message;
                    ModelState.AddModelError("error", errmsg);
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    errmsg = "The Orgunit could not be created! " + ex.Message.ToString();
                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Create", orguModel, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Orgunit has been created!" });
            }
        }

        [HttpPost]
        [Route("Delete")]
        public ActionResult Delete(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.OrgUnitManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                OrgUnitModel orgUnitModel = new OrgUnitModel();
                orgUnitModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                if (!orgUnitModel.CanManage || !orgUnitModel.IsAllowedObject(this.UserName, guid, true))
                    throw new Exception(SecurityHelper.NoPermissionText);

                OrgUnitManager manager = new OrgUnitManager(this.PersonGuid);
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
        [Route("CleanupOrgunits")]
        public ActionResult CleanupOrgunits()
        {
            ErrorModel errorModel = null;
            bool success = false;

            CoreTransaction transaction = new CoreTransaction();

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.OrgUnitManager_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                OrgUnitModel orgUnitModel = new OrgUnitModel();
                orgUnitModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                if (!orgUnitModel.CanManage)
                    throw new Exception(SecurityHelper.NoPermissionText);

                OrgUnitRoleManager manager = new OrgUnitRoleManager(transaction, this.UserName, MODIFY_COMMENT);

                transaction.dbContext.Configuration.AutoDetectChangesEnabled = false;
                transaction.dbContext.Configuration.ValidateOnSaveEnabled = false;
                transaction.Begin();

                manager.CleanupOrgunitRoleRelations();
                transaction.Commit();

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