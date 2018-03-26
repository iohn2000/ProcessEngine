using Kapsch.IS.WsProcessEngine.Entities;
using Kapsch.IS.WsProcessEngine.FaultContracts;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml.Linq;

namespace Kapsch.IS.WsProcessEngine
{
    [ServiceContract]
    public interface IProcessService
    {
        /// <summary>
        /// Counts running workflow instances
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <returns></returns>
        [OperationContract]
        int GetProcessInstanceCountFromWorkflowID(string idWorkflow);

        /// <summary>
        /// Gets a list of activity items
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <returns></returns>
        [OperationContract]
        List<ActivityItem> GetActivityItems();

        /// <summary>
        /// Returns a list of workflowitems
        /// doesn't show the checkedout user name
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<WorkflowItem> GetWorkflowItems();

        [OperationContract]
        WorkflowItem GetWorkflowItem(string idWorkflow, string userGuid);

        /// <summary>
        /// Saves a checked out workflow
        /// throws an exception if checkedout by another user or not checked out
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="validFrom"></param>
        /// <param name="validTo"></param>
        /// <param name="userGuid"></param>
        [OperationContract]
        [FaultContract(typeof(FcWorkflowException))]
        [FaultContract(typeof(FcActivityException))]
        [FaultContract(typeof(FcVariableException))]
        [FaultContract(typeof(FcPermissionException))]
        void SaveWorkflowMetaData(string idWorkflow, string name, string description, DateTime? validFrom, DateTime? validTo, string userGuid);

        /// <summary>
        /// Saves a checked out workflow
        /// throws an exception if checkedout by another user or not checked out
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <param name="workflowXml"></param>
        /// <param name="userGuid"></param>
        [OperationContract]
        [FaultContract(typeof(FcWorkflowException))]
        [FaultContract(typeof(FcActivityException))]
        [FaultContract(typeof(FcVariableException))]
        [FaultContract(typeof(FcPermissionException))]
        void SaveWorkflowXmlDefinition(string idWorkflow, string workflowXml, string userGuid);


        /// <summary>
        /// Creates a new workflow.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="validFrom"></param>
        /// <param name="validTo"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(FcWorkflowException))]
        [FaultContract(typeof(FcActivityException))]
        [FaultContract(typeof(FcVariableException))]
        WorkflowItem CreateWorkflow(string name, string description, DateTime? validFrom, DateTime? validTo);

        /// <summary>
        /// Checks out a workflow for a specific user
        /// throws an exception if already checkout or checkedout by another user
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(FcPermissionException))]
        WorkflowItem CheckoutWorkflow(string idWorkflow, string userGuid);

        /// <summary>
        /// Checks in a workflow for a specific user
        /// throws an exception if checkedout by another user
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(FcPermissionException))]
        WorkflowItem CheckinWorkflow(string idWorkflow, string userGuid);

        /// <summary>
        /// Undo Checkout a workflow for a specific user
        /// throws an exception if checkedout by another user
        /// </summary>
        /// <param name="idWorkflow"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(FcPermissionException))]
        WorkflowItem UndoCheckout(string idWorkflow, string userGuid);

        [OperationContract]
        void DeleteWorkflow(string idWorkflow, bool deleteAllVersions);

        /// <summary>
        /// adds all necessary activity TAG to the given xml
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="idActivity"></param>
        /// <returns></returns>
        [OperationContract]
        string GetWorkflowXmlWithNewActivityId(string xml, string idActivity);

        [OperationContract]
        string GetWorkflowXmlWithNewSubworkflowActivity(string workflowXml, string activityID, string subworkflowDefinitionID, string subworkflowVariables);


        [OperationContract]
        bool IsPackageUsedInActiveWorkflow(string obcoGuid);

        [OperationContract]
        void WakeupWorkflow(string workflowInstanceID);

        [OperationContract]
        List<WorkflowInstanceItem> GetWorkflowInstances();

        [OperationContract]
        void SaveWorkflowInstanceItem(string idWorkflowInstance, string instanceXml);

        [OperationContract]
        WorkflowInstanceItem GetWorkflowInstanceItem(string workflowInstanceID);

        [OperationContract]
        string GetDataHelperName(string workflowDefinitionID);

        [OperationContract]
        string CreateWorkflowInstance(WorkflowMessageDataItem workflowMessageDataItem);

        [OperationContract]
        List<KeyValuePairItem> GetAllEmailDocumentTemplates();
    }
}
