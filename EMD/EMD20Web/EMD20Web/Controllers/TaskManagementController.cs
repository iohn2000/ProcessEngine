using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using System.Xml.Linq;
using Kapsch.IS.Util.ReflectionHelper;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Core;
using Kapsch.IS.EMD.EMD20Web.Models.Enum;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("TaskManagement")]
    public class TaskManagementController : BaseController
    {
        private const string MANAGEROUTE = "Manage";

        [Route]
        public ActionResult Index()
        {
            return RedirectToAction("Manage");
        }

        [Route("Manage")]
        [Route("Manage/{guid}")]
        public ActionResult Manage(string guid)
        {
            //if (guid != null && guid.StartsWith("PERS"))
            //{
            //    return Redirect("http://gis-edp.kapsch.co.at/PersonProfile/Profile/" + guid);
            //}

            //return Redirect("http://gis-edp.kapsch.co.at/TaskManagement/Manage");

            TaskManagementModel model = new TaskManagementModel();
            model.InitializeSecurity(SecurityUser.NewSecurityUser(this.UserName));
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonManager persManager = new PersonManager();
            EMDPerson pers = persManager.GetPersonByUserId(this.UserName);
            EMDEmployment empl = emplHandler.GetMainEmploymentForPerson(pers.Guid);
            model.UserEmplGuid = empl.Guid;
            model.AvailableStatusList = this.GetAvailableStatusList();
            model.UserFullName = base.UserFullName;

            return View("Manage", model);
        }

        private List<TextValueModel> GetAvailableStatusList()
        {
            List<TextValueModel> returnValue = new List<TextValueModel>();
            var values = System.Enum.GetValues(typeof(EnumTaskStatusModel));
            returnValue.Add(new TextValueModel("Open (all types)", EnumTaskStatusModel.OpenAllTypes.ToString()));
            foreach (EnumTaskStatusModel enumtask in values)
            {
                if (enumtask != EnumTaskStatusModel.OpenAllTypes)
                {
                    returnValue.Add(new TextValueModel(enumtask.ToString(), enumtask.ToString()));
                }
            }

            return returnValue;
        }

        [Route("ReadTasks")]
        [Route("ReadTasks/{taskstatus}")]
        public ActionResult ReadTasks([DataSourceRequest]DataSourceRequest request, EnumTaskStatusModel taskstatus)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            List<TaskItemModel> taskItemModels = new List<TaskItemModel>();

            Dictionary<string, int> identifiers = new Dictionary<string, int>();

            PersonHandler persHandler = new PersonHandler();
            EmploymentHandler emplHandler = new EmploymentHandler();

            List<EMDTaskItem> taskItems = null;


            string searchString = "TSK_Status != \"closed\"";

            if (taskstatus != EnumTaskStatusModel.OpenAllTypes)
            {
                searchString = string.Format("TSK_Status = \"{0}\"", taskstatus.ToString().ToLower());
            }

            taskItems = new TaskItemHandler().GetObjects<EMDTaskItem, TaskItem>(searchString).Cast<EMDTaskItem>().OrderBy(a => a.Created).ToList();


            EmploymentManager emplManager = new EmploymentManager();


            List<TextValueModel> personModels = new List<TextValueModel>();

            foreach (EMDTaskItem taskItem in taskItems)
            {
                if (secUser.GetAllowedEmployment(taskItem.TSK_Approver_EmplGUID, true) != null)
                {
                    string key = taskItem.TSK_DecisionOptions.Trim().ToLower().Replace(" ", string.Empty);
                    if (!identifiers.ContainsKey(key))
                    {
                        identifiers.Add(key, identifiers.Count + 1);
                    }
                    TaskItemModel taskItemModel = TaskItemModel.copyFromObject(taskItem);

                    EMDPersonEmployment persEmplRequestor = emplManager.GetPersonEmployment(taskItemModel.TSK_Requestor_EmplGUID, true);
                    if (persEmplRequestor != null)
                    {
                        taskItemModel.Requestor_Name = persEmplRequestor.Pers.Display_FamilyName + " " + persEmplRequestor.Pers.Display_FirstName;
                        taskItemModel.Requestor_pers_guid = persEmplRequestor.Pers.Guid;
                    }

                    EMDPersonEmployment persEmplApprover = emplManager.GetPersonEmployment(taskItemModel.TSK_Approver_EmplGUID);
                    if (persEmplApprover != null)
                    {
                        if (persEmplApprover != null)
                        {
                            taskItemModel.ApproverName = persEmplApprover.Pers.Display_FamilyName + " " + persEmplApprover.Pers.Display_FirstName;
                            taskItemModel.ApproverPersonGuid = persEmplApprover.Pers.Guid;
                        }
                    }

                    EMDPersonEmployment persEmplEffected = emplManager.GetPersonEmployment(taskItemModel.TSK_EffectedPerson_EmplGUID);
                    if (persEmplEffected != null)
                    {
                        if (persEmplEffected != null)
                        {
                            taskItemModel.Effected_Person_Name = persEmplEffected.Pers.Display_FamilyName + " " + persEmplEffected.Pers.Display_FirstName;
                            taskItemModel.EffectedPersonPersonGuid = persEmplEffected.Pers.Guid;
                        }
                    }

                    // the key is used from assembled key WorfklowGuid-TaskActivityID
                    // taskItemModel.TempTypeId = Convert.ToString(identifiers[key]);
                    taskItemModel.TempTypeId = string.Format("{0} {1}", taskItem.TSK_ProcessGuid, taskItem.TSK_TaskActivityID);
                    taskItemModel.TempTypeDisplayName = string.Format("{0} {1}", taskItem.TSK_ProcessName, taskItem.TSK_TaskActivityID);
                    taskItemModels.Add(taskItemModel);
                }
            }

            return Json(taskItemModels.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [Route("Edit")]
        [Route("Edit/{tait_guid}/{hasMultipleItems}")]
        [Route("Edit/{tait_guid}/{hasMultipleItems}/{isPartialView}")]
        public ActionResult Edit(string tait_guid, bool hasMultipleItems, bool isPartialView = false)
        {
            TaskItemHandler tiHandler = new TaskItemHandler();
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonManager persManager = new PersonManager();

            EMDTaskItem taskItem = (EMDTaskItem)tiHandler.GetObject<EMDTaskItem>(tait_guid);
            EMDEmployment emplRequestor = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(taskItem.TSK_Requestor_EmplGUID);
            EMDPerson persRequestor = (EMDPerson)persManager.Get(emplRequestor.P_Guid);

            TaskItemModel taskItemModel = TaskItemModel.copyFromObject(taskItem);

            if (string.IsNullOrWhiteSpace(taskItem.TSK_Decision))
            {
                List<EMDTaskItem> taskItems = tiHandler.GetObjects<EMDTaskItem, TaskItem>(string.Format("TSK_LinkedTasks_ID = \"{0}\"", taskItem.TSK_LinkedTasks_ID)).Cast<EMDTaskItem>().ToList();

                EMDTaskItem foundItem = taskItems?.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.TSK_Decision));
                if (foundItem != null)
                {
                    taskItemModel.TSK_Decision = foundItem.TSK_Decision;
                }
            }

            taskItemModel.HasMultipleItems = hasMultipleItems;
            taskItemModel.BatchItems = new List<string>(); // { tait_guid };
            taskItemModel.Requestor_Name = persManager.getFullDisplayNameWithUserId(persRequestor);
            taskItemModel.ApproverPersonGuid = persRequestor.Guid;

            //TODO: Check ob dies genüber oder IsAllowedEmployment aufgerufen werden soll
            taskItemModel.InitializeSecurity(this.UserName, taskItemModel.TSK_Approver_EmplGUID); //Admin oder IsItSelf
            //if (!taskItemModel.CanManage || !taskItemModel.IsAllowedObject(this.UserName, taskItemModel.TSK_Approver_EmplGUID))
            if (!taskItemModel.CanManage && !taskItemModel.CanView)
                return GetNoPermissionView(isPartialView);

            EMDEmployment emplEffectedPerson = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(taskItem.TSK_EffectedPerson_EmplGUID);
            EMDPerson persEffectedPerson = (EMDPerson)persManager.Get(emplEffectedPerson.P_Guid);

            taskItemModel.EffectedPersonPersonGuid = persEffectedPerson.Guid;
            taskItemModel.Effected_Person_Name = persManager.getFullDisplayNameWithUserId(persEffectedPerson);

            if (taskItem.TSK_DecisionOptions != null && taskItem.TSK_DecisionOptions != String.Empty)
            {
                taskItemModel.DecisionList = taskItem.TSK_DecisionOptions.Split(',').ToList();
            }

            taskItemModel.SetApprovalResults();

            if (isPartialView)
            {
                return PartialView("Edit", taskItemModel);
            }
            else
            {
                return View("Edit", taskItemModel);
            }
        }


        [Route("DoEdit")]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult DoEdit(TaskItemModel model)
        {
            Exception handledException = null;
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;
            string message = "The task has been confirmed!";
            CoreTransaction transaction = new CoreTransaction();

            if (model != null && ModelState.IsValid)
            {
                try
                {
                    if (model.IsClosed)
                    {
                        throw new Exception("The task is already closed");
                    }


                    transaction.Begin();
                    //TODO: Check ob dies genügt oder IsAllowedEmployment aufgerufen werden soll
                    model.InitializeSecurity(this.UserName, model.TSK_Approver_EmplGUID); //Admin oder IsItSelf

                    if (!model.CanManage)
                    {
                        throw new Exception(SecurityHelper.NoPermissionText);
                    }

                    List<Tuple<string, string>> additionalParameters = new List<Tuple<string, string>>();
                    System.Reflection.PropertyInfo[] propsModel = ReflectionHelper.GetPropertyNames(model.GetType());
                    List<String> modelFields = new List<string>();
                    //List<String> fieldsForXML = new List<string>();
                    foreach (System.Reflection.PropertyInfo prop in propsModel)
                    {
                        modelFields.Add(prop.Name);
                    }

                    List<String> formFields = new List<string>();
                    formFields = Request.Form.AllKeys.ToList();
                    foreach (string formField in formFields)
                    {
                        bool fieldFound = false;
                        foreach (string modelFiled in modelFields)
                        {
                            if (modelFiled == formField)
                            {
                                fieldFound = true;
                            }
                        }
                        if (fieldFound == false && formField != "__RequestVerificationToken" && !formField.Contains("action"))
                        {
                            //fieldsForXML.Add(formField);
                            additionalParameters.Add(new Tuple<string, string>(formField, Request.Form[formField]));
                        }
                    }

                    TaskItemManager taskItemManager = new TaskItemManager(this.PersonGuid, "DoEdit task in EDPGUI");
                    EMDTaskItem taskItem = new EMDTaskItem();
                    ReflectionHelper.CopyProperties(ref model, ref taskItem);


                    enumTaskStatus taskStatus;
                    if (string.Equals(Request.Form["action:approve"], "Confirm", StringComparison.InvariantCultureIgnoreCase))
                    {
                        taskStatus = enumTaskStatus.closed;
                    }
                    else
                    {
                        taskStatus = enumTaskStatus.Saved;
                        message = "The task has been saved!";
                    }

                    if (model.BatchItems.Count > 1)
                    {
                        taskItemManager.FinishTasks(model.BatchItems, additionalParameters, model.TSK_Notes, taskStatus);
                        if (taskStatus == enumTaskStatus.closed)
                        {
                            message = string.Format("{0} tasks have been confirmed!", model.BatchItems.Count);
                        }
                        else
                        {
                            message = string.Format("{0} tasks have been saved!", model.BatchItems.Count);
                        }
                    }
                    else
                    {
                        taskItemManager.FinishTask(model.Guid, additionalParameters, model.TSK_Notes, taskStatus);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    transaction.Rollback();
                    if (model.BatchItems.Count > 1)
                    {
                        errmsg = string.Format("Could not confirm {0} tasks: {1}", model.BatchItems.Count, ex.Message.ToString());
                    }
                    else
                    {
                        errmsg = "Could not confirm task: " + ex.Message.ToString();
                    }


                    ModelState.AddModelError("error", errmsg);
                }
            }

            PartialViewResult result = GetPartialFormWithErrors("Edit", model, handledException, errmsg);

            if (result != null)
            {
                return result;
            }
            else
            {
                return Json(new { message = message });
            }

        }

        [Route("getTaskDecisions")]
        [Route("getTaskDecisions/{tait_guid}")]
        public JsonResult getTaskDecisions([DataSourceRequest]DataSourceRequest request, string tait_guid)
        {
            List<String> descicionList = new List<string>();
            try
            {
                TaskItemHandler tiHandler = new TaskItemHandler();
                EMDTaskItem taskItem = (EMDTaskItem)tiHandler.GetObject<EMDTaskItem>(tait_guid);

                if (taskItem.TSK_DecisionOptions != null && taskItem.TSK_DecisionOptions != String.Empty)
                {
                    descicionList = taskItem.TSK_DecisionOptions.Split(',').ToList();
                }

            }

            catch (Exception ex)
            {
                string errorMessage = "Error reading tasksk";
                var error = new ErrorModel(ex, errorMessage);
                logger.Error(errorMessage, ex, ControllerContext?.HttpContext);

                return this.Json(new DataSourceResult
                {
                    Errors = error
                });
            }
            return Json(descicionList, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        [Route("ReadAllAllowedPersons")]
        public List<TextValueModel> ReadAllAllowedPersons()
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            List<TextValueModel> personModels = new List<TextValueModel>();
            List<EMDPersonEmployment> employments = secUser.AllowedEmployments().ToList();

            employments.ForEach(item =>
            {

                TextValueModel persModel = new TextValueModel();
                persModel.Text = item.FullDisplayNameWithUserIdAndPersNr;
                persModel.Value = item.Empl.Guid;
                personModels.Add(persModel);
            });

            personModels = (from item in personModels orderby item.Text select item).ToList();

            return personModels;
        }

        [HttpGet]
        [Route("ReadAllAllowedPersonsForSelect")]
        public JsonResult ReadAllAllowedPersonsForSelect([DataSourceRequest]DataSourceRequest request)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            List<TextValueModel> personModels = new List<TextValueModel>();
            List<EMDPersonEmployment> employments = secUser.AllowedEmployments().ToList();

            employments.ForEach(item =>
            {

                TextValueModel persModel = new TextValueModel();
                persModel.Text = item.FullDisplayNameWithUserIdAndPersNr;
                persModel.Value = item.Empl.Guid;
                personModels.Add(persModel);
            });

            personModels = (from item in personModels orderby item.Text select item).ToList();

            return Json(personModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ValidateInput(false)]
        [Route("Forward")]
        public ActionResult AddActivity()
        {
            return PartialView("Forward");
        }

        [HttpGet]
        [Route("ReadTasksForSelect")]
        public JsonResult ReadTasksForSelect([DataSourceRequest]DataSourceRequest request)
        {
            List<TextValueModel> allowedPersons = this.ReadAllAllowedPersons();
            return Json(allowedPersons, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Route("ForwardTasks")]
        public ActionResult ForwardTasks(List<string> taskListItemGuids, string forwardToEmplGuid)
        {
            string message = "The tasks have been forwarded!";
            bool success = true;

            SecurityUser secUser = SecurityUser.NewSecurityUser(this.UserName);
            bool IsAllowedEmployment = secUser.IsAllowedEmployment(this.UserName, forwardToEmplGuid);



            var errors = ModelState.Values.SelectMany(v => v.Errors);
            string errmsg = String.Empty;

            try
            {
                if (!IsAllowedEmployment)
                {
                    throw new Exception(SecurityHelper.NoPermissionText);
                }

                //Copy Tasks to other user
                TaskItemManager taskItemManager = new TaskItemManager(this.PersonGuid);
                //  List<string> itemGuids = taskItemGuids.Split(',').ToList();
                taskItemManager.ForwardTasks(taskListItemGuids, forwardToEmplGuid, this.UserName);
                logger.Info(string.Format("{0} Tasks have been forwarded to:{1} ({2})", taskListItemGuids.Count, forwardToEmplGuid, String.Join(", ", taskListItemGuids)));
            }
            catch (Exception ex)
            {
                success = false;
                message = string.Format("The Tasks ({0}) could not be forwarded to:{1}!", taskListItemGuids, forwardToEmplGuid);
                logger.Error(message, ex);
            }

            return Json(new { success = success, message = message });
        }
    }
}