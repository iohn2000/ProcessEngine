using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EMD.EMD20Web.HelperExtensions;
using Kapsch.IS.EMD.EMD20Web.Models;
using Kapsch.IS.EMD.EMD20Web.Models.Workflow;
using Kapsch.IS.Util.Logging;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kapsch.IS.EMD.EMD20Web.Controllers
{
    [RoutePrefix("ProcessEntity")]
    public class ProcessEntityController : BaseController
    {
        public ProcessEntityController()
        {
            logger = ISLogger.GetLogger("ProcessEntityController");
        }

        [Route("Manage/{woinGuid}")]
        [Route("Manage/{woinGuid}/{isPartialView}")]
        public ActionResult Manage(string woinGuid, bool isPartialView = false)
        {
            EMDProcessEntity processEntity = new ProcessEntityManager().GetProcessEntityByWorfklowId(woinGuid);
            string title = string.Empty;
            // create the title for specific entity
            string prefix = EMDProcessEntity.GetPrefix(processEntity.EntityGuid);



            switch (prefix.ToLower())
            {
                case "obre":
                    EMDEquipmentDefinition equipmentDefinition = new EquipmentManager().GetEquipmentDefinitionFromObre(processEntity.EntityGuid);
                    title = string.Format("Equipment | {0} | Method: {1} | Workflow: {2} | WOIN: {3}",
                        equipmentDefinition.Name, processEntity.WorkflowAction, processEntity.WFD_Name, woinGuid);
                    break;
                case "empl":
                    Kapsch.IS.EDP.Core.Entities.Enhanced.EMDPersonEmployment personEmpl = new EmploymentManager().GetPersonEmployment(processEntity.EntityGuid, true);
                    title = string.Format("Employment | {0} | Method: {1} | Workflow: {2} | WOIN: {3}",
                     personEmpl.FullDisplayNameWithUserIdAndPersNr, processEntity.WorkflowAction, processEntity.WFD_Name, woinGuid);

                    break;
                default:
                    break;
            }

            AcitivityManageModel model = new AcitivityManageModel() { EntityGuid = woinGuid };



            model.Title = title;

            if (isPartialView)
            {
                return PartialView("Manage", model);
            }
            else
            {
                return View("Manage", model);
            }
        }

        [Route("ReadInstances/{woinGuid}")]
        [HandleError()]
        public ActionResult ReadInstances([DataSourceRequest]DataSourceRequest request, string woinGuid)
        {
            DataSourceResult myresult = null;


            try
            {
                List<ActivityResultMessageItem> workflowInstanceItems = Service.GetActivityResultMessagesForWorkflowInstance(woinGuid).ToList();
                // workflowInstanceItems = workflowInstanceItems.OrderByDescending(t => t.Created).ToList();

                List<ActivityResultMessageModel> worfklowInstanceModels = new List<ActivityResultMessageModel>();


                workflowInstanceItems.ForEach(cty =>
                {
                    ActivityResultMessageModel model = ActivityResultMessageModel.Map(cty);
                    worfklowInstanceModels.Add(model);
                });

                myresult = worfklowInstanceModels.OrderBy(a => a.Created).ToDataSourceResult(request, ModelState);

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


        [Route("StatusInformation/{guidEntity}/{isAdmin}")]
        [Route("StatusInformation/{guidEntity}/{isAdmin}/{isPartialView}")]
        [HandleError()]
        public ActionResult StatusInformation(string guidEntity, bool isAdmin, bool isPartialView = false)
        {
            ProcessEntityModel model = new ProcessEntityModel();
            model.EntityGuid = guidEntity;
            model.IsPartialView = isPartialView;
            ProcessEntityManager processEntityManager = new ProcessEntityManager();

            List<string> supervisors = new List<string>();
            try
            {
                EMDProcessEntity lastProcessEntity = processEntityManager.GetLastProcessEntity(guidEntity);


                if (lastProcessEntity != null)
                {
                    model = ProcessEntityModel.Map(lastProcessEntity);
                    model.IsPartialView = isPartialView;
                    try
                    {
                        WorkflowInstanceStatusItem[] workflowInstanceStatusItemArray = Service.GetStatusList(new string[] { lastProcessEntity.WFI_ID });

                        if (workflowInstanceStatusItemArray.Length > 0)
                        {
                            model.Status = workflowInstanceStatusItemArray[0].Status.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(string.Format("Getting Status for {0} failed!", lastProcessEntity.WFD_ID), ex);
                    }
                }



            }
            catch (Exception ex)
            {

                logger.Error(string.Format("GetEntityStatus failed for entity with Guid: {0}", guidEntity), ex);
            }

            string viewName = "StatusInformation";
            if (isAdmin)
            {
                viewName = "StatusInformationAdmin";
            }

            if (isPartialView)
            {
                return PartialView(viewName, model);
            }
            else
            {
                return View(viewName, model);
            }
        }
    }
}