using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.ProcessEngine.Webservice.Entities;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Kapsch.IS.ProcessEngine.Webservice
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service : IProcessService
    {
        private ProcessDefinition processDefinition;

        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Singleton for ProcessDefinition
        /// </summary>
        public ProcessDefinition ProcessDefinition
        {
            get
            {
                if (this.processDefinition == null)
                {
                    this.processDefinition = new ProcessDefinition();
                }
                return this.processDefinition;
            }
        }

        public int GetProcessInstanceCountFromWorkflowID(string idWorkflow)
        {
            try
            {
                return ProcessDefinition.GetProcessInstanceCountFromWorkflowID(idWorkflow);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public List<ActivityItem> GetActivityItems()
        {
            try
            {
                return Helper.Map(ProcessDefinition.GetAllActivities());
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public List<WorkflowItem> GetWorkflowItems()
        {
            try
            {
                List<WorkflowItem> workflowItems = new List<WorkflowItem>();


                workflowItems = ProcessDefinition.GetAllWorkflowDefinitions().Select(workflowDefinition =>
                Helper.Map(workflowDefinition, GetProcessInstanceCountFromWorkflowID(workflowDefinition.WFD_ID))
                ).ToList();

                foreach (var workflowItem in workflowItems)
                {
                    // remove xml to improve performance
                    workflowItem.Definition = "<workflow>no data</workflow>";
                }

                return workflowItems;
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public WorkflowItem GetWorkflowItem(string idWorkflow, string userGuid)
        {
            try
            {
                WFEWorkflowDefinition workflowDefinition = ProcessDefinition.GetWorkflowDefinitionByID(idWorkflow, userGuid);

                return Helper.Map(workflowDefinition, GetProcessInstanceCountFromWorkflowID(workflowDefinition.WFD_ID));
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public void SaveWorkflowMetaData(string idWorkflow, string name, string description, DateTime? validFrom, DateTime? validTo, string userGuid)
        {
            try
            {
                ProcessDefinition.SaveWorkflowMetaData(idWorkflow, name, description, validFrom, validTo, userGuid);
            }
            catch (DefaultException ex)
            {
                throw Helper.Map(ex);
            }
            catch (WorkflowException ex)
            {
                throw Helper.Map(ex);
            }
            catch (ActivityException ex)
            {
                throw Helper.Map(ex);
            }
            catch (VariableException ex)
            {
                throw Helper.Map(ex);
            }
            catch (PermissionException ex)
            {
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public WorkflowItem CreateWorkflow(string name, string description, DateTime? validFrom, DateTime? validTo)
        {
            try
            {
                WFEWorkflowDefinition workflowDefinition = ProcessDefinition.CreateWorkflowDefinition(name, description, validFrom, validTo);

                return Helper.Map(workflowDefinition, GetProcessInstanceCountFromWorkflowID(workflowDefinition.WFD_ID));
            }
            catch (DefaultException ex)
            {
                throw Helper.Map(ex);
            }
            catch (WorkflowException ex)
            {
                throw Helper.Map(ex);
            }
            catch (ActivityException ex)
            {
                throw Helper.Map(ex);
            }
            catch (VariableException ex)
            {
                throw Helper.Map(ex);
            }
            catch (PermissionException ex)
            {
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }


        }

        public WorkflowItem CheckoutWorkflow(string idWorkflow, string userGuid)
        {
            try
            {
                WFEWorkflowDefinition workflowDefinition = ProcessDefinition.CheckoutWorkflowDefinition(idWorkflow, userGuid);
                return Helper.Map(workflowDefinition, GetProcessInstanceCountFromWorkflowID(workflowDefinition.WFD_ID));
            }

            catch (PermissionException ex)
            {
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public WorkflowItem CheckinWorkflow(string idWorkflow, string userGuid)
        {
            try
            {
                ProcessDefinition.CheckinWorkflowDefinition(idWorkflow, userGuid);
                WFEWorkflowDefinition workflowDefinition = ProcessDefinition.GetWorkflowDefinitionByID(idWorkflow, userGuid);
                return Helper.Map(workflowDefinition, GetProcessInstanceCountFromWorkflowID(workflowDefinition.WFD_ID));
            }

            catch (PermissionException ex)
            {
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public WorkflowItem UndoCheckout(string idWorkflow, string userGuid)
        {
            try
            {
                ProcessDefinition.UndoCheckout(idWorkflow, userGuid);
                WFEWorkflowDefinition workflowDefinition = ProcessDefinition.GetWorkflowDefinitionByID(idWorkflow, userGuid);
                return Helper.Map(workflowDefinition, GetProcessInstanceCountFromWorkflowID(workflowDefinition.WFD_ID));
            }

            catch (PermissionException ex)
            {
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        /// <summary>
        /// How to delete a workflow?
        ///   *) only the last version 
        ///   
        ///   *) what if workflow is checked out? 
        ///      --> undo checkout or checkin first, then delete
        ///      
        ///   *) what if workflow is currently mapped (to an entity and method, e.g. ADD equipment) ?
        ///      --> delete not possible !
        /// </summary>
        /// <param name="workflowID"></param>
        public void DeleteWorkflow(string workflowID, bool deleteAllVersions = true)
        {

            DatabaseAccess databaseLayer = new DatabaseAccess();
            WFEWorkflowDefinition wfDefCheckedOut = databaseLayer.GetWorkflowDefinition(workflowID, getCheckedOutVersion: true);
            if (wfDefCheckedOut != null)
            {
                throw new WorkflowException("This workflow is already checked out. WorkflowID =  " + workflowID);
            }
            else
            {
                // we made it to delete !
                WFEWorkflowDefinition wfDefToDelete = databaseLayer.GetWorkflowDefinition(workflowID, getCheckedOutVersion: false);

                if (wfDefToDelete != null)
                {

                    if (deleteAllVersions)
                    {
                        for (int idx = 0; idx <= wfDefToDelete.WFD_Version; idx++)
                        {
                            wfDefToDelete = databaseLayer.GetWorkflowDefinition(workflowID, getCheckedOutVersion: false);
                            databaseLayer.DeletWorkflowByGUID(wfDefToDelete.Guid);
                        }
                    }
                    else
                    {
                        databaseLayer.DeletWorkflowByGUID(wfDefToDelete.Guid);
                    }
                }
            }

        }

        public void SaveWorkflowXmlDefinition(string idWorkflow, string workflowXml, string userGuid)
        {
            try
            {
                ProcessDefinition.SaveWorkflowXmlDefinitionOnly(idWorkflow, workflowXml, userGuid, true);
            }
            catch (WorkflowException ex)
            {
                throw Helper.Map(ex);
            }
            catch (ActivityException ex)
            {
                throw Helper.Map(ex);
            }
            catch (VariableException ex)
            {
                throw Helper.Map(ex);
            }
            catch (PermissionException ex)
            {
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        public string GetWorkflowXmlWithNewActivityId(string xml, string idActivity)
        {
            try
            {
                return ProcessDefinition.CreateWorkflowXmlWithActivity(xml, idActivity);
            }
            catch (Exception ex)
            {
                throw Helper.Map(ex);
            }
        }

        /// <summary>
        /// adds a subworkflow activity
        /// </summary>
        /// <param name="workflowXml">workflow xml</param>
        /// <param name="activityID">id of activity to add</param>
        /// <param name="subworkflowDefinitionID">workflow definition id of workflow that is started by subworkflow activity</param>
        /// <param name="subworkflowVariables">input variables that is needed for (sub)workflow</param>
        /// <returns></returns>
        public string GetWorkflowXmlWithNewSubworkflowActivity(string workflowXml, string activityID, string subworkflowDefinitionID, string subworkflowVariables)
        {
            string newWorkflowXml = "";
            newWorkflowXml = ProcessDefinition.CreateWorkflowXmlWithActivity(
                xml: workflowXml,
                idActivity: activityID,
                subWorkflowDefinitionID: subworkflowDefinitionID,
                subWorkflowVariables: subworkflowVariables);
            return newWorkflowXml;
        }

        /// <summary>
        /// checks if the obcoGuid is used in an workflow that is still active
        /// </summary>
        /// <param name="obcoGuid"></param>
        /// <returns></returns>
        public bool IsPackageUsedInActiveWorkflow(string obcoGuid)
        {
            WorkflowHandler wfH = new WorkflowHandler();
            return false; //not needed now and uses core in workflow api --> böse
                          //var wfInstances = wfH.GetActiveWorkflowsWithPackageInUse(obcoGuid);
                          //return wfInstances.Count > 0;
        }

        /// <summary>
        /// create engine alert with start activity that cause the pause mode
        /// </summary>
        /// <param name="workflowInstanceID">workflow to wake up</param>
        public void WakeupWorkflow(string workflowInstanceID)
        {
            DatabaseAccess db = new DatabaseAccess();
            // get start activity from workflow

            WFEWorkflowInstance wfi = db.GetWorkflowInstance(workflowInstanceID);
            db.CreateEngineAlert(wfi.WFI_CurrentActivity, workflowInstanceID, null, null, EnumAlertTypes.Normal);

        }

        public List<WorkflowInstanceItem> GetWorkflowInstances()
        {
            try
            {
                List<WorkflowInstanceItem> allWcfInstances = new List<WorkflowInstanceItem>();
                DatabaseAccess db = new DatabaseAccess();
                var allWFInstances = db.GetAllWorkflowInstances();
                if (allWFInstances.Count > 0)
                {
                    // get extra infos and fill WorkflowInstanceItems
                    foreach (var inst in allWFInstances)
                    {
                        // 
                        if (inst.WFI_Status == EnumWorkflowInstanceStatus.Finish.ToString() && inst.WFI_Updated.HasValue && inst.WFI_Updated.Value < DateTime.Now.AddDays(-2))
                        {
                            continue;
                        }
                        WorkflowInstanceItem item = new WorkflowInstanceItem();
                        var wfDef = db.GetWorkflowDefinition(inst.WFI_WFD_ID, false); // get latest version
                        if (wfDef != null)
                        {
                            item.Name = wfDef.WFD_Name;
                            item.DefinitionID = wfDef.Guid.ToString();
                            item.Description = wfDef.WFD_Description;
                        }
                        item.Created = inst.WFI_Created;
                        item.CurrentActivity = inst.WFI_CurrentActivity;
                        item.Finished = inst.WFI_Finished;
                        item.InstanceID = inst.WFI_ID;
                        item.InstanceXML = "<workflow>no data</workflow>";
                        item.NextActivity = inst.WFI_NextActivity;
                        item.ParentWorkflowInstanceID = inst.WFI_ParentWF;
                        item.Status = (EnumWorkflowInstanceStatus)Enum.Parse(typeof(EnumWorkflowInstanceStatus), inst.WFI_Status, true);
                        item.Updated = inst.WFI_Updated;
                        item.Version = "n/a"; //

                        allWcfInstances.Add(item);
                    }

                }
                return allWcfInstances;
            }
            catch (WorkflowException ex)
            {
                logger.Error("WorkflowException thrown: " + ex.Message, ex);
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                logger.Error("Exception thrown: " + ex.Message, ex);
                throw Helper.Map(ex);
            }
        }

        public void SaveWorkflowInstanceItem(string idWorkflowInstance, string instanceXml)
        {
            try
            {
                //ProcessDefinition.CheckForWellFormedXml(instanceXml, true);
                DatabaseAccess db = new DatabaseAccess();
                db.UpdateInstanceXml(idWorkflowInstance, instanceXml);
            }
            catch (WorkflowException ex)
            {
                logger.Error("WorkflowException thrown: " + ex.Message, ex);
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                logger.Error("Exception thrown: " + ex.Message, ex);
                throw Helper.Map(ex);
            }
        }

        public WorkflowInstanceItem GetWorkflowInstanceItem(string workflowInstanceID)
        {
            try
            {
                WFEWorkflowInstance wfeWorkflowInstance = new DatabaseAccess().GetWorkflowInstance(workflowInstanceID);
                if (wfeWorkflowInstance != null)
                {
                    return WorkflowInstanceItem.Map(wfeWorkflowInstance);
                }
                else
                {
                    return null;
                }
            }
            catch (WorkflowException ex)
            {
                logger.Error("WorkflowException thrown: " + ex.Message, ex);
                throw Helper.Map(ex);
            }
            catch (Exception ex)
            {
                logger.Error("Exception thrown: " + ex.Message, ex);
                throw Helper.Map(ex);
            }
        }

        /// <summary>
        /// get the name of the correct datahelper for workflow
        /// </summary>
        /// <param name="workflowDefinitionID"></param>
        /// <returns></returns>
        public string GetDataHelperName(string workflowDefinitionID)
        {
            WorkflowModel wfModel = new WorkflowModel();
            DatabaseAccess db = new DatabaseAccess();
            WFEWorkflowDefinition wfLatest = db.GetWorkflowDefinition(workflowDefinitionID, false);
            wfModel.LoadModelXml(wfLatest.WFD_Definition);
            return wfModel.DataHelperName;
        }

        /// <summary>
        /// Starts a workflow with parameters, packed in WorkflowMessage
        /// </summary>
        /// <param name="workflowMessageDataItem"></param>
        /// <returns>the workflow instance id</returns>
        public WorkflowInstanceItem CreateWorkflowInstance(WorkflowMessageDataItem workflowMessageDataItem)
        {
            WorkflowHandler wfHandler = new WorkflowHandler();
            ProcessInstance processInstance = wfHandler.CreateNewWorkflowInstance(WorkflowMessageDataItem.Map(workflowMessageDataItem));

            return GetWorkflowInstanceItem(processInstance.InstanceID);
        }

        public List<KeyValuePairItem> GetAllEmailDocumentTemplates()
        {
            return this.GetAllDocumentTemplates("email");
        }

        /// <summary>
        /// returns empty List if no found
        /// returns null of invalid template category
        /// </summary>
        /// <param name="templateCategory"></param>
        /// <returns></returns>
        private List<KeyValuePairItem> GetAllDocumentTemplates(string templateCategory)
        {
            DatabaseAccess db = new DatabaseAccess();
            List<DocumentTemplateType> allCategories = db.GetAllDocumentTemplateCategories();

            //check if valid category
            bool categoryExists = allCategories.Find(m => m.DTYP_Name == templateCategory) != null;
            if (categoryExists)
            {
                var dbList = db.GetAllDocumentTemplates(templateCategory);

                List<KeyValuePairItem> wsList = dbList.ConvertAll(new Converter<DocumentTemplate, KeyValuePairItem>
                    (
                        delegate (DocumentTemplate dT)
                        {
                            return new KeyValuePairItem(dT.TMPL_Name, dT.TMPL_Content);
                        }
                    ));
                return wsList;
            }
            else
            {
                throw new Exception("templateCategory: " + templateCategory + " is invalid.");
            }

        }

        /// <summary>
        /// Delivers the status of all requestes woins
        /// </summary>
        /// <param name="woins"></param>
        /// <returns></returns>
        public List<WorkflowInstanceStatusItem> GetStatusList(List<string> woins)
        {
            DatabaseAccess db = new DatabaseAccess();
            List<WorkflowInstanceStatusItem> workflowInstances = new List<WorkflowInstanceStatusItem>();

            foreach (string woin in woins)
            {
                WFEWorkflowInstance instance = db.GetWorkflowInstance(woin);
                WorkflowInstanceStatusItem item = null;
                if (instance != null)
                {
                    item = WorkflowInstanceStatusItem.Map(instance);
                }
                else
                {
                    item = new WorkflowInstanceStatusItem() { InstanceID = woin };
                }
                workflowInstances.Add(item);
            }

            return workflowInstances;
        }

        /// <summary>
        /// Only Aborted and Completed is allowed
        /// </summary>
        /// <param name="worfklowInstanceStatus"></param>
        /// <returns></returns>
        public WorkflowInstanceStatusItem SetWorkflowInstanceStatus(WorkflowInstanceStatusItem worfklowInstanceStatus)
        {
            try
            {
                if (worfklowInstanceStatus.Status == EnumWorkflowInstanceStatus.Aborted || worfklowInstanceStatus.Status == EnumWorkflowInstanceStatus.Finish)
                {



                    DatabaseAccess db = new DatabaseAccess();
                    WFEWorkflowInstance wfeInstance = db.GetWorkflowInstance(worfklowInstanceStatus.InstanceID);

                    wfeInstance.WFI_Status = EnumWorkflowInstanceStatus.Aborted.ToString();
                    db.UpdateInstanceStatus(worfklowInstanceStatus.InstanceID, worfklowInstanceStatus.Status);

                    db.AbortEngineAlerts(worfklowInstanceStatus.InstanceID, worfklowInstanceStatus.Status == EnumWorkflowInstanceStatus.Aborted ? EnumEngineAlertStatus.Aborted : EnumEngineAlertStatus.Completed);

                    return WorkflowInstanceStatusItem.Map(wfeInstance);
                }
                else
                {
                    throw new Exception(string.Format("You tried to set status: {0}, but only Aborted and Finish is allowed", worfklowInstanceStatus.Status));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception thrown: " + ex.Message, ex);
                throw Helper.Map(ex);
            }
        }

        public void DoCallback(string workflowInstanceID, string clientReferenceID, string callbackResult, string callBackStatus)
        {
            logger.Info("Called DoCallback with parameters: "+workflowInstanceID + "/" + clientReferenceID + "/" + callbackResult + "/" + callBackStatus);
            try
            {
                DatabaseAccess db = new DatabaseAccess();
                db.UpdateCallBack(workflowInstanceID, clientReferenceID, callbackResult, callBackStatus);
            }
            catch (Exception ex)
            {
                logger.Error("Exception thrown: "+ex.Message, ex);
                throw Helper.Map(ex);
            }
            
        }

        /// <summary>
        /// Gets a list of activity result messages for a given workflow intance.
        /// </summary>
        /// <param name="workflowInstanceID"></param>
        /// <returns>return an empty list of there is no messages for given workflow instance</returns>
        public List<ActivityResultMessageItem> GetActivityResultMessagesForWorkflowInstance(string workflowInstanceID)
        {
            List<ActivityResultMessageItem> result = new List<ActivityResultMessageItem>();
            DatabaseAccess db = new DatabaseAccess();

            List<WFEActivityResultMessage> dbResult = db.GetActivityResultMsgByWoin(workflowInstanceID);

            if (dbResult != null)
            {
                foreach (WFEActivityResultMessage msg in dbResult)
                {
                    ActivityResultMessageItem item = new ActivityResultMessageItem()
                    {
                        ARM_ActivityInstanceId = msg.ARM_ActivityInstanceId,
                        ARM_Created = msg.ARM_Created,
                        ARM_ID = msg.ARM_ID,
                        ARM_ResultMessage = msg.ARM_ResultMessage,
                        ARM_Woin = msg.ARM_Woin
                    };
                    result.Add(item);
                }
            }

            return result; 
        }

        //[WebMethod]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]

        public string KMPMonitoring()
        {
            try
            {
                
                string errorMessage = string.Empty;
                string status = "UP";
                int maxProcessesRunning = 1;
                int warningPollingWaitActivities = 400;
                int errorPollingWaitActivities = 600;
                DatabaseAccess dbAccess = new DatabaseAccess();
                //lockedWorkflows
                int processesRunning = dbAccess.GetAmountOfProcessEngineProcessesRunning();
                // >1 => Warning Nameing => ConcurrentProcesses
                // >5 => Critical

                //
                int pollingWaitActivities = dbAccess.GetAmountOfPollingWaitActivities();
                //

                if (processesRunning > maxProcessesRunning)
                {

                }
                if (pollingWaitActivities > errorPollingWaitActivities)
                {
                    status = "CRITICAL";
                    errorMessage += string.Format("Polling wait activity count has exeeded maximum {0}/{1} - this slows down the processing of the items", pollingWaitActivities, errorPollingWaitActivities);
                }
                else if (pollingWaitActivities > warningPollingWaitActivities)
                {
                    status = "WARNING";
                    errorMessage += string.Format("Polling wait activity count has exeeded maximum {0}/{1} - this slows down the processing of the items", pollingWaitActivities, warningPollingWaitActivities);
                }



            }
            catch (Exception)
            {

                throw;
            }
            
            return "";
        }
    }
}
