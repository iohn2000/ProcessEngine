using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.WsProcessEngine;
using Kapsch.IS.WsProcessEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service implementation: See method comments in IProcessService
/// </summary>
public class Service : IProcessService
{
    private ProcessDefinition processDefinition;

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
            ProcessDefinition.SaveWorkflowXmlDefinitionOnly(idWorkflow, workflowXml, userGuid);
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

        //todo 

        // wollerc: uncommented because of build problems
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
                    item.InstanceXML = "";//inst.WFI_Xml; // leave empty gets too big for gui
                    item.NextActivity = inst.WFI_NextActivity;
                    item.ParentWorkflowInstanceID = inst.WFI_ParentWF;
                    item.Status = (EnumWorkflowInstanceStatus) Enum.Parse(typeof(EnumWorkflowInstanceStatus), inst.WFI_Status, true);
                    item.Updated = inst.WFI_Updated;
                    item.Version = "n/a"; // todo version when instances was created need to be stored in workflow xml

                    allWcfInstances.Add(item);
                }

            }
            return allWcfInstances;
        }
        catch (WorkflowException ex)
        {
            throw Helper.Map(ex);
        }
        catch (Exception ex)
        {
            throw Helper.Map(ex);
        }
    }

    public void SaveWorkflowInstanceItem(string idWorkflowInstance, string instanceXml)
    {
        try
        {
            DatabaseAccess db = new DatabaseAccess();
            db.UpdateInstanceXml(idWorkflowInstance, instanceXml);
        }
        catch (Exception ex)
        {
            throw Helper.Map(ex);
        }
    }

    public WorkflowInstanceItem GetWorkflowInstanceItem(string workflowInstanceID)
    {
        try
        {
            WorkflowInstanceItem wfInstance = new WorkflowInstanceItem();
            DatabaseAccess db = new DatabaseAccess();

            WFEWorkflowInstance inst = db.GetWorkflowInstance(workflowInstanceID);

            var wfDef = db.GetWorkflowDefinition(inst.WFI_WFD_ID, false); // get latest version
            if (wfDef != null)
            {
                wfInstance.Name = wfDef.WFD_Name;
                wfInstance.DefinitionID = wfDef.Guid.ToString();
                wfInstance.Description = wfDef.WFD_Description;
            }
            wfInstance.Created = inst.WFI_Created;
            wfInstance.CurrentActivity = inst.WFI_CurrentActivity;
            wfInstance.Finished = inst.WFI_Finished;
            wfInstance.InstanceID = inst.WFI_ID;
            wfInstance.InstanceXML = inst.WFI_Xml; // leave empty gets too big for gui
            wfInstance.NextActivity = inst.WFI_NextActivity;
            wfInstance.ParentWorkflowInstanceID = inst.WFI_ParentWF;
            wfInstance.Status = (EnumWorkflowInstanceStatus) Enum.Parse(typeof(EnumWorkflowInstanceStatus), inst.WFI_Status, true);
            wfInstance.Updated = inst.WFI_Updated;
            wfInstance.Version = "n/a"; // todo version when instances was created need to be stored in workflow xml

            return wfInstance;
        }
        catch (WorkflowException ex)
        {
            throw Helper.Map(ex);
        }
        catch (Exception ex)
        {
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
    public string CreateWorkflowInstance(WorkflowMessageDataItem workflowMessageDataItem)
    {
        WorkflowHandler wfHandler = new WorkflowHandler();
        return wfHandler.CreateNewWorkflowInstance(WorkflowMessageDataItem.Map(workflowMessageDataItem));
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
}
