using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EMD.EMD20Web.Models.Workflow;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.Util.ReflectionHelper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web.Mvc;
using VizConf;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("Workflow")]
    public class WorkflowController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        [Route("Manage")]
        [HandleError()]
        public ActionResult Manage()
        {
            WorkflowModel model = new WorkflowModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!model.CanManage)
                return GetNoPermissionView(false);

            //throw new Exception("Simulating an exception on a general level");
            return View("Manage", model);
        }


        [Route("WorkflowInstances")]
        [Route("WorkflowInstances/{isPartialView}")]
        [HandleError()]
        public ActionResult WorkflowInstances(bool isPartialView = false)
        {
            if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.WorkflowManagement_View_Manage))
                return GetNoPermissionView(isPartialView);

            if (isPartialView)
            {
                return PartialView("WorkflowInstances");
            }
            else
            {
                return View("WorkflowInstances");
            }
        }

        [Route("WorkflowImage")]
        [HandleError()]
        public ActionResult WorkflowImage()
        {
            //throw new Exception("Simulating an exception on a general level");
            return PartialView();
        }

        [Route("Mapping")]
        [HandleError()]
        public ActionResult Mapping()
        {
            if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.WorkflowManagement_View_Manage))
                return GetNoPermissionView(false);

            //throw new Exception("Simulating an exception on a general level");
            return View();
        }

        [Route("Read")]
        [HandleError()]
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            DataSourceResult myresult = null;
            try
            {
                List<WorkflowItem> workflowItems = Service.GetWorkflowItems().ToList();
                workflowItems = workflowItems.OrderByDescending(t => t.Created).ToList();
                workflowItems = workflowItems.Where(a => a.ValidTo == null || a.ValidTo > DateTime.Now).ToList();

                myresult = workflowItems.ToDataSourceResult(request, ModelState,
                    wfDef =>
                    {

                        return WorkflowModel.Map(wfDef, PersonGuid);

                    }
                );
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading workflows";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }


        [Route("ReadMappings")]
        [HandleError()]
        public ActionResult ReadMappings([DataSourceRequest]DataSourceRequest request)
        {
            DataSourceResult myresult = null;
            try
            {
                List<WorkflowActionModel> models = WorkflowActionModel.GetList();
                myresult = models.ToDataSourceResult(request);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading mappings";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }


        [Route("ReadInstances")]
        [HandleError()]
        public ActionResult ReadInstances([DataSourceRequest]DataSourceRequest request)
        {
            DataSourceResult myresult = null;
            try
            {
                List<WorkflowInstanceItem> workflowInstanceItems = Service.GetWorkflowInstances().ToList();
                // workflowInstanceItems = workflowInstanceItems.OrderByDescending(t => t.Created).ToList();

                List<WorkflowInstanceModel> worfklowInstanceModels = new List<WorkflowInstanceModel>();
                worfklowInstanceModels = worfklowInstanceModels.OrderByDescending(t => t.Created).ToList();

                workflowInstanceItems.ForEach(cty =>
                {
                    WorkflowInstanceModel model = WorkflowInstanceModel.Map(cty);
                    worfklowInstanceModels.Add(model);
                });

                myresult = worfklowInstanceModels.ToDataSourceResult(request, ModelState);

            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading workflow instances";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(myresult, JsonRequestBehavior.AllowGet);
        }


        [Route("ReadEmailTemplatesForSelect")]
        public ActionResult ReadEmailTemplatesForSelect([DataSourceRequest]DataSourceRequest request)
        {
            List<TextValueModel> keyValuePairs = new List<TextValueModel>();
            try
            {
                List<KeyValuePairItem> emailTemplates = Service.GetAllEmailDocumentTemplates().ToList();

                emailTemplates.ForEach(entity =>
                {
                    keyValuePairs.Add(new TextValueModel(entity.Key, entity.Key));
                });

                keyValuePairs = keyValuePairs.OrderBy(pair => pair.Text).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading email templates";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);


            }
            return Json(keyValuePairs, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Route("Edit")]
        [Route("Add/{isPartialView}")]
        [Route("Edit/{idWorkflow}/{isPartialView}")]
        [Route("Edit/{idWorkflow}")]
        public ActionResult Edit(string idWorkflow, bool isPartialView = false)
        {
            SecurityUser securityUser = SecurityUser.NewSecurityUser(this.UserName);

            WorkflowModel mappedModel = new WorkflowModel();
            mappedModel.InitializeSecurity(securityUser);
            if (!mappedModel.CanManage)
                return GetNoPermissionView(isPartialView);

            ViewBag.Titel = "Edit Workflow";
            try
            {
                if (!string.IsNullOrEmpty(idWorkflow))
                {
                    ViewBag.Titel = "Edit Workflow";
                    WorkflowItem wfDef = Service.GetWorkflowItem(idWorkflow, PersonGuid);

                    mappedModel = WorkflowModel.Map(wfDef, PersonGuid, true);
                    mappedModel.OwnUsername = this.UserName;
                    mappedModel.InitializeSecurity(securityUser);
                }
                else
                {
                    ViewBag.Titel = "Add new Workflow";
                }

            }
            catch (Exception e)
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                logger.Info("Mehgtod: WorkflowController.Edit(stirng idWorkflow, bool is PertialView=false) => TODO: write HelperMethod for generalizing this kind of handling", e);

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
        public ActionResult Edit(WorkflowModel workflowModel)
        {
            Exception handledException = null;
            workflowModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            bool isNew = string.IsNullOrEmpty(workflowModel.IdWorkflow);
            if (workflowModel != null && ModelState.IsValid)
            {
                try
                {
                    if (!workflowModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    if (!isNew)
                    {
                        Service.SaveWorkflowMetaData(workflowModel.IdWorkflow, workflowModel.Name, workflowModel.Description, workflowModel.ValidFrom, workflowModel.ValidTo, PersonGuid);
                    }
                    else
                    {
                        Service.CreateWorkflow(workflowModel.Name, workflowModel.Description, workflowModel.ValidFrom, workflowModel.ValidTo);
                    }
                }
                catch (FaultException<FcDefaultException> ex)
                {
                    handledException = ex;
                    DefaultErrorType defaultErrorType = (DefaultErrorType)ex.Detail.ErrorType;
                    if (defaultErrorType == DefaultErrorType.NameAlreadyExists)
                    {
                        ModelState.AddModelError("error", "Please choose a different Name, because the 'Workflow name' already exists!");
                    }
                    else
                    {
                        ModelState.AddModelError("error", string.Format("A DefaultException was thrown with message: {0}", ex.Message));
                    }
                }
                catch (FaultException<FcWorkflowException> ex)
                {
                    handledException = ex;
                    WorkflowErrorType worklowErrorType = (WorkflowErrorType)ex.Detail.ErrorType;

                    ModelState.AddModelError("error", string.Format("A Workflowexception was thrown with errortype {0}", worklowErrorType));
                }
                catch (FaultException ex)
                {
                    handledException = ex;
                    if (!isNew)
                    {
                        ModelState.AddModelError("error", string.Format("Could not edit Workflow: {0}", ex.Message));
                    }
                    else
                    {
                        ModelState.AddModelError("error", string.Format("Could not create Workflow: {0}.", ex.Message));
                    }
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    if (!isNew)
                    {
                        ModelState.AddModelError("error", string.Format("Could not edit Workflow: {0}", ex.Message));
                    }
                    else
                    {
                        ModelState.AddModelError("error", string.Format("Could not create Workflow: {0}.", ex.Message));
                    }

                }
            }


            PartialViewResult result = GetPartialFormWithErrors("Edit", workflowModel, handledException, "The Workflow couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Workflow has been created!" });
            }
        }

        [HttpGet]
        [ValidateInput(false)]
        [Route("Configure")]
        [Route("Configure/{idWorkflow}")]
        [Route("Configure/{idWorkflow}/{isPartialView}")]
        public ActionResult Configure(string idWorkflow, bool isPartialView = false)
        {
            WorkflowModel mappedModel = null;
            try
            {
                WorkflowItem wfDef = Service.GetWorkflowItem(idWorkflow, PersonGuid);


                mappedModel = WorkflowModel.Map(wfDef, PersonGuid, true);
                mappedModel.OwnUsername = this.UserName;
                mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));

                if (!mappedModel.CanManage)
                    return GetNoPermissionView(isPartialView);
            }
            catch (Exception e) //TODO: in case it's an BaseException we can do some more output... 
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                logger.Info("Method: WorkflowController.Configure(stirng idWorkflow, bool isPartialView=false) => TODO: write HelperMethod for generalizing this kind of handling", e);

            }


            if (isPartialView)
            {
                return PartialView("Configure", mappedModel);
            }
            else
            {
                return View("Configure", mappedModel);
            }
        }

        /// <summary>
        /// Checks out a workflow for a specific user
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Checkout")]
        public ActionResult Checkout(string idWorkflow)
        {
            WorkflowModel mappedModel = null;
            bool success = false;
            ErrorModel errorModel = null;

            try
            {
                Service.CheckoutWorkflow(idWorkflow, PersonGuid);
                WorkflowItem workflowItem = Service.GetWorkflowItem(idWorkflow, PersonGuid);
                mappedModel = WorkflowModel.Map(workflowItem, PersonGuid, true);

                mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                if (!mappedModel.CanManage)
                {
                    throw new Exception(SecurityHelper.NoPermissionText);
                }
                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, idWorkflow = idWorkflow, errorModel = errorModel });
            }

            return Json(new { success = success, username = mappedModel.CheckedOutBy, version = mappedModel.Version, idWorkflow = idWorkflow });
        }


        /// <summary>
        /// Checks in a workflow of a specific user
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Checkin")]
        public ActionResult Checkin(string idWorkflow)
        {
            WorkflowModel mappedModel = null;
            bool success = false;
            ErrorModel errorModel = null;
            string newXml = string.Empty;
            try
            {
                Service.CheckinWorkflow(idWorkflow, PersonGuid);
                WorkflowItem workflowItem = Service.GetWorkflowItem(idWorkflow, PersonGuid);
                mappedModel = WorkflowModel.Map(workflowItem, PersonGuid, true);
                newXml = workflowItem.Definition;

                mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                if (!mappedModel.CanManage)
                {
                    throw new Exception(SecurityHelper.NoPermissionText);
                }

                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, idWorkflow = idWorkflow, errorModel = errorModel });
            }



            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = success, username = mappedModel.CheckedOutBy, version = mappedModel.Version, idWorkflow = idWorkflow, xml = newXml }), "application/json");

        }

        /// <summary>
        /// Undo Checkout a workflow of a specific user
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UndoCheckout")]
        public ActionResult UndoCheckout(string idWorkflow)
        {
            WorkflowModel mappedModel = null;
            bool success = false;
            ErrorModel errorModel = null;
            try
            {
                Service.UndoCheckout(idWorkflow, PersonGuid);
                WorkflowItem workflowItem = Service.GetWorkflowItem(idWorkflow, PersonGuid);
                mappedModel = WorkflowModel.Map(workflowItem, PersonGuid, true);

                mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                if (!mappedModel.CanManage)
                {
                    throw new Exception(SecurityHelper.NoPermissionText);
                }

                success = true;
            }
            catch (Exception ex)
            {
                errorModel = new ErrorModel(ex);
                logger.Error(ex, ControllerContext?.HttpContext);
            }

            if (!success)
            {
                return Json(new { success = success, idWorkflow = idWorkflow, errorModel = errorModel });
            }

            return Json(new { success = success, username = mappedModel.CheckedOutBy, xml = mappedModel.Definition, version = mappedModel.Version, idWorkflow = idWorkflow, model = mappedModel });
        }

        [HttpPost]
        [Route("SaveWorkflow")]
        public ActionResult SaveWorkflow(string idWorkflow, string xml)
        {

            bool success = false;
            string errorMessage = string.Empty;
            string newXml = xml;
            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.WorkflowManagement_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                Service.SaveWorkflowXmlDefinition(idWorkflow, xml, PersonGuid);
                newXml = Service.GetWorkflowItem(idWorkflow, PersonGuid).Definition;
                success = true;
            }
            catch (FaultException<FcWorkflowException> ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }
            catch (FaultException<FcPermissionException> ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }
            catch (FaultException ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }
            catch (Exception ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }


            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = success, idWorkflow = idWorkflow, xml = newXml, errorMessage = errorMessage }), "application/json");
        }


        [HttpGet]
        [ValidateInput(false)]
        [Route("AddActivity")]
        public ActionResult AddActivity()
        {
            return PartialView("AddActivity");
        }


        [Route("ReadActivitiesForSelect")]
        public ActionResult ReadActivitiesForSelect()
        {
            List<ActivityModel> activityModels = new List<ActivityModel>();
            try
            {
                activityModels = ActivityModel.Map(Service.GetActivityItems().ToList());
                activityModels = activityModels.OrderBy(pair => pair.Name).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = "Error reading activities";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(activityModels, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Route("GetWorkflowXmlWithNewActivityId")]
        public ActionResult GetWorkflowXmlWithNewActivityId(string xml, string idActivity, string idWorkflow)
        {
            string newXml = xml;
            bool success = false;
            string errorMessage = string.Empty;
            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.WorkflowManagement_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                if (!string.IsNullOrEmpty(idWorkflow))
                {
                    // Get Variable XML from Core and send it to Webservice
                    string dataHelperName = Service.GetDataHelperName(idWorkflow);
                    if (string.IsNullOrEmpty(dataHelperName))
                    {
                        throw new Exception(string.Format("No DataHelper found for workflow: {0}", idWorkflow));
                    }
                    string xmlVariables = WorkflowBaseMessage.GetWorkflowVariablesAsString(dataHelperName);
                    if (string.IsNullOrEmpty(dataHelperName))
                    {
                        throw new Exception(string.Format("Variables couldn't be created for workflow: {0}", idWorkflow));
                    }
                    newXml = Service.GetWorkflowXmlWithNewSubworkflowActivity(xml, idActivity, idWorkflow, xmlVariables);
                }
                else
                {
                    newXml = Service.GetWorkflowXmlWithNewActivityId(xml, idActivity);
                }

                success = true;
            }
            catch (FaultException<FcPermissionException> ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }
            catch (Exception ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }

            // Important: Use JsonConvert, because otherwise CDATA values are lost
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = success, xml = newXml, errorMessage = errorMessage }), "application/json");

        }

        [HttpPost]
        [Route("GetWorkflowXml")]
        public ActionResult GetWorkflowXml(string idWorkflow)
        {
            string newXml = string.Empty;
            bool success = false;
            string errorMessage = string.Empty;
            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.WorkflowManagement_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                newXml = Service.GetWorkflowItem(idWorkflow, PersonGuid).Definition;
                success = true;
            }
            catch (FaultException<FcPermissionException> ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }
            catch (Exception ex)
            {
                errorMessage = MessageHelper.GetMessage(ex);
            }


            // Important: Use JsonConvert, because otherwise CDATA values are lost
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = success, xml = newXml, errorMessage = errorMessage }), "application/json");
        }


        #region Mappings

        [HttpGet]
        [Route("EditMapping")]
        [Route("AddMapping")]
        [Route("AddMapping/{isPartialView}")]
        [Route("EditMapping/{guidProcessmapping}/{isPartialView}")]
        [Route("EditMapping/{guidProcessmapping}")]
        public ActionResult EditMapping(string guidProcessmapping, bool isPartialView = false)
        {
            ProcessMappingHandler processMappingHandler = new ProcessMappingHandler();
            EMDProcessMapping emdProcessMapping = (EMDProcessMapping)processMappingHandler.GetObject<EMDProcessMapping>(guidProcessmapping);


            WorkflowActionModel mappedModel = new WorkflowActionModel();

            ViewBag.Titel = "Edit Mapping";
            try
            {
                mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                if (!mappedModel.CanManage)
                    return GetNoPermissionView(isPartialView);

                if (!string.IsNullOrEmpty(guidProcessmapping))
                {
                    ViewBag.Titel = "Edit Mapping";


                    mappedModel = WorkflowActionModel.Map(emdProcessMapping);
                    mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                }
                else
                {
                    ViewBag.Titel = "Add new Mapping";
                }

            }
            catch (Exception e)
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                logger.Info("WorkflowController.EditMapping(string guidProcessmapping, bool isPartialView=false) => TODO: write HelperMethod for generalizing this kind of handling", e);

            }




            if (isPartialView)
            {
                return PartialView("EditMapping", mappedModel);
            }
            else
            {
                return View("EditMapping", mappedModel);
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        [Route("EditMapping")]
        public ActionResult EditMapping(WorkflowActionModel processModelMapping)
        {
            Exception handledException = null;
            bool hasEntities = false;

            try
            {

                if (!string.IsNullOrEmpty(processModelMapping.TypePrefix))
                {
                    Type type = EntityPrefix.Instance.GetTypeFromPrefix(processModelMapping.TypePrefix);
                    if (type.GetInterfaces().Contains(typeof(IProcessMapping)))
                    {
                        IProcessMapping instanceObject = (IProcessMapping)Activator.CreateInstance(type);
                        hasEntities = instanceObject.HasEntities();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Info("WorkflowController.EditMapping(WorkflowActionModel processModelMapping) => TODO: write HelperMethod for generalizing this kind of handling", e);
            }

            if (!hasEntities)
            {
                if (ModelState["EntityMappingGuid"] != null)
                {
                    ModelState["EntityMappingGuid"].Errors.Clear();
                }
                // delete the entitymapping guid if set from UI
                processModelMapping.EntityMappingGuid = null;
            }


            var errors = ModelState.Values.SelectMany(v => v.Errors);



            if (processModelMapping != null && ModelState.IsValid)
            {
                try
                {
                    processModelMapping.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                    if (!processModelMapping.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    ProcessMappingHandler processMappingHandler = new ProcessMappingHandler();
                    WorkflowActionModel tempModel = processModelMapping;

                    EMDProcessMapping emdProcessMapping = new EMDProcessMapping();
                    ReflectionHelper.CopyProperties<WorkflowActionModel, EMDProcessMapping>(ref tempModel, ref emdProcessMapping);
                    emdProcessMapping.TypePrefix = processModelMapping.TypePrefix;
                    emdProcessMapping.Method = processModelMapping.MethodMappingModel.MethodName;

                    ProcessMappingManager procMapMgr = new ProcessMappingManager();

                    emdProcessMapping = procMapMgr.CreateMapping(emdProcessMapping);

                    if (emdProcessMapping != null)
                    {
                        if (string.IsNullOrEmpty(processModelMapping.Guid))
                        {
                            processModelMapping.Guid = emdProcessMapping.Guid;
                        }
                        // update filter rules
                        ObjectHelper.CreateOrUpdateFilterRules(processModelMapping.RuleFilterModel, processModelMapping.Guid);
                    }
                    else
                    {
                        // TODO Chris : throw gui error already exists
                        ModelState.AddModelError("error", string.Format("Workflow Mapping already exists.", WorkflowErrorType.RuleViolation));
                    }
                }
                //catch(FaultException<ServiceException> service)
                catch (FaultException<FcWorkflowException> ex)
                {
                    handledException = ex;
                    WorkflowErrorType worklowErrorType = (WorkflowErrorType)ex.Detail.ErrorType;

                    ModelState.AddModelError("error", string.Format("A Workflowexception was thrown with errortype {0}", worklowErrorType));
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    ModelState.AddModelError("error", string.Format("Could not edit Workflow: {0}", ex.Message));
                }
            }


            PartialViewResult result = GetPartialFormWithErrors("Edit", processModelMapping, handledException, "The Workflow couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Workflow has been created!" });
            }
        }


        [Route("ReadMappingEntities")]
        [HandleError()]
        public ActionResult ReadMappingEntities(string prefix)
        {
            List<TextValueModel> mappingEntitiesModel = new List<TextValueModel>();

            if (!string.IsNullOrEmpty(prefix))
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        Type type = EntityPrefix.Instance.GetTypeFromPrefix(prefix);




                        if (type.GetInterfaces().Contains(typeof(IProcessMapping)))
                        {
                            IProcessMapping instanceObject = (IProcessMapping)Activator.CreateInstance(type);

                            if (instanceObject.HasEntities())
                            {
                                List<KeyValuePair<string, string>> keyValues = instanceObject.GetEntityList();
                                foreach (var item in keyValues)
                                {
                                    mappingEntitiesModel.Add(new TextValueModel(item.Value, item.Key));
                                }
                            }
                        }


                    }
                }
                catch (Exception e) //TODO: in case it's an BaseException we can do some more output... 
                {
                    //TODO: write HelperMethod for generalizing this kind of handling
                    var emptyList = new List<TextValueModel>();
                    ModelState.AddModelError(e.GetType().ToString(), e.Message);

                }
            }
            return Json(mappingEntitiesModel.OrderBy(model => model.Text).ToList(), JsonRequestBehavior.AllowGet);
        }


        [Route("ReadMethods")]
        [HandleError()]
        public ActionResult ReadMethods(string prefix)
        {
            List<WorkflowActionMethodModel> processingMethodModels = new List<WorkflowActionMethodModel>();

            if (!string.IsNullOrEmpty(prefix))
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        Type type = EntityPrefix.Instance.GetTypeFromPrefix(prefix);


                        if (type.GetInterfaces().Contains(typeof(IProcessMapping)))
                        {
                            IProcessMapping instanceObject = (IProcessMapping)Activator.CreateInstance(type);

                            List<WorkflowAction> processingMethods = instanceObject.GetMappingMethods();
                            foreach (var item in processingMethods)
                            {
                                processingMethodModels.Add(new WorkflowActionMethodModel(item));
                            }
                        }
                    }
                }
                catch (Exception e) //TODO: in case it's an BaseException we can do some more output... 
                {
                    //TODO: write HelperMethod for generalizing this kind of handling
                    var emptyList = new List<TextValueModel>();
                    ModelState.AddModelError(e.GetType().ToString(), e.Message);

                }
            }
            return Json(processingMethodModels.OrderBy(model => model.MethodName).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("HasMappingEntities")]
        public ActionResult HasMappingEntities(string prefix)
        {
            bool success = true;
            bool hasEntities = false;
            ErrorModel errorModel = null;


            if (!string.IsNullOrEmpty(prefix))
            {
                try
                {
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        Type type = EntityPrefix.Instance.GetTypeFromPrefix(prefix);


                        if (type.GetInterfaces().Contains(typeof(IProcessMapping)))
                        {
                            IProcessMapping instanceObject = (IProcessMapping)Activator.CreateInstance(type);


                            hasEntities = instanceObject.HasEntities();


                        }


                    }
                }
                catch (Exception ex)
                {
                    errorModel = new ErrorModel(ex);
                    logger.Error(ex, ControllerContext?.HttpContext);
                }
            }




            if (!success)
            {
                return Json(new { success = success, errorModel = errorModel });
            }

            return Json(new { success = success, hasEntities = hasEntities });
        }


        [Route("GetWorkflows")]
        [HandleError()]
        public ActionResult GetWorkflows()
        {
            List<TextValueModel> workflowModelList = new List<TextValueModel>();
            try
            {
                List<WorkflowItem> workflowItems = Service.GetWorkflowItems().ToList();
                workflowItems = workflowItems.OrderByDescending(t => t.Created).ToList();

                foreach (var item in workflowItems)
                {
                    workflowModelList.Add(new TextValueModel(item.Name, item.Id));

                }

            }
            catch (Exception e) //TODO: in case it's an BaseException we can do some more output... 
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                var emptyList = new List<WorkflowModel>();
                ModelState.AddModelError(e.GetType().ToString(), e.Message);

            }
            return Json(workflowModelList.OrderBy(model => model.Text).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("DeleteWorkflow")]
        public ActionResult DeleteWorkflow(string idWorkflow)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                Service.DeleteWorkflow(idWorkflow, true);

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
        [Route("DeleteWorkflowMapping")]
        public ActionResult DeleteWorkflowMapping(string guid)
        {
            ErrorModel errorModel = null;
            bool success = false;

            try
            {
                if (!SecurityHelper.HasPermission(this.UserName, SecurityPermission.WorkflowManagement_View_Manage))
                    throw new Exception(SecurityHelper.NoPermissionText);

                ProcessMappingManager processMappingManager = new ProcessMappingManager();
                processMappingManager.Delete(guid);

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
        [Route("GetWorkflowImage")]
        [ValidateInput(false)]
        public ActionResult GetWorkflowImage(string workflowxml)
        {
            ErrorModel errorModel = null;
            bool success = false;
            string imgSrc = string.Empty;

            try
            {
                Dictionary<string, string> vizConf = new Dictionary<string, string>();

                DLLStepConfigurationSection dllSteps = ConfigurationManager.GetSection("DLLStepsConfig") as DLLStepConfigurationSection;

                foreach (DLLStepElement ee in dllSteps.DLLSteps)
                {
                    vizConf.Add(ee.Namespace, ee.Style);
                }

                GraphVizHelper graphVizHelper = new GraphVizHelper();
                byte[] output = graphVizHelper.ReturnGraph(workflowxml, vizConf, GraphVizWrapper.Enums.GraphReturnType.Png);

                // base64String
                var base64 = Convert.ToBase64String(output);
                imgSrc = String.Format("data:image/gif;base64,{0}", base64);

                // SVG
                //imgSrc = System.Text.Encoding.UTF8.GetString(output);
                //int startIndex = imgSrc.IndexOf("<svg");
                //imgSrc = imgSrc.Substring(startIndex);


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

            //  return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = success, base64image = imgSrc, errorMessage = message }), "application/json");

            return Json(new { success = success, base64image = imgSrc });
        }


        #endregion


        /// <summary>
        /// Checks out a workflow for a specific user
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("StartWorkflowInstance")]
        public ActionResult StartWorkflowInstance(string idWorkflowInstance)
        {
            bool success = false;
            ErrorModel errorModel = null;

            try
            {
                Service.WakeupWorkflow(idWorkflowInstance);

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


        [HttpGet]
        [Route("EditWorkflowInstance")]
        [Route("EditWorkflowInstance/{idWorkflowInstance}/{isPartialView}")]
        [Route("EditWorkflowInstance/{idWorkflowInstance}")]
        public ActionResult EditWorkflowInstance(string idWorkflowInstance, bool isPartialView = false)
        {
            WorkflowInstanceModel mappedModel = new WorkflowInstanceModel();
            mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            if (!mappedModel.CanManage)
                return GetNoPermissionView(isPartialView);

            ViewBag.Titel = "Edit Workflow Instance";
            try
            {


                if (!string.IsNullOrEmpty(idWorkflowInstance))
                {
                    ViewBag.Titel = "Edit Workflow Instance";
                    WorkflowInstanceItem workflowInstance = Service.GetWorkflowInstanceItem(idWorkflowInstance);

                    mappedModel = WorkflowInstanceModel.Map(workflowInstance);
                    mappedModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                }


            }
            catch (Exception e)
            {
                //TODO: write HelperMethod for generalizing this kind of handling
                logger.Info("WorkflowController.EditWorkflowInstance(string idWorkflowInstance, bool isPartialView=false) => TODO: write HelperMethod for generalizing this kind of handling", e);
            }



            if (isPartialView)
            {
                return PartialView("EditWorkflowInstance", mappedModel);
            }
            else
            {
                return View("EditWorkflowInstance", mappedModel);
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        [Route("EditWorkflowInstance")]
        public ActionResult EditWorkflowInstance(WorkflowInstanceModel workflowInstanceModel)
        {
            Exception handledException = null;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (workflowInstanceModel != null && ModelState.IsValid)
            {
                try
                {
                    workflowInstanceModel.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
                    if (!workflowInstanceModel.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    if (!string.IsNullOrEmpty(workflowInstanceModel.InstanceID))
                    {
                        Service.SaveWorkflowInstanceItem(workflowInstanceModel.InstanceID, workflowInstanceModel.InstanceXML);
                    }

                }
                catch (Exception ex)
                {
                    handledException = ex;
                    ModelState.AddModelError("error", string.Format("Could not edit Workflow Instance: {0}", ex.Message));
                }
            }


            PartialViewResult result = GetPartialFormWithErrors("EditWorkflowInstance", workflowInstanceModel, handledException, "The Workflow Instance couldn't be saved. Please check all comments on the depending fields.");

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { Url = MANAGEROUTE, message = "The Workflow instance was saved!" });
            }
        }

    }
}