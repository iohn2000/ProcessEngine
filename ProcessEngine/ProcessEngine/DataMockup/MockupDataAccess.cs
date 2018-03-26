
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.Interfaces;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using System;
using System.Collections.Generic;

namespace DataMockup
{
    public class MockupDataAccess : IDataAccess
    {
        private List<WFEWorkflowDefinition> workFlowDefinitions = new List<WFEWorkflowDefinition>();



        public WFEWorkflowDefinition CreateNewWorkflowDefinition(WFEWorkflowDefinition newWFEDefinition)
        {
            this.workFlowDefinitions.Add(newWFEDefinition);
            return newWFEDefinition;
        }

        public WFEWorkflowDefinition CreateNewWorkflowDefinition(WFEWorkflowDefinition newWFEDefinition, bool isCheckout = false)
        {
            newWFEDefinition.WFD_Version = -1;
            this.workFlowDefinitions.Add(newWFEDefinition);
            return newWFEDefinition;
        }

        public string CreateNewWorkflowInstance(string definitionID, string instanceID)
        {
            throw new NotImplementedException();
        }

        public string CreateNewWorkflowInstance(string definitionID, string instanceID, string parentWorkflowInstanceID, string nextActivity)
        {
            throw new NotImplementedException();
        }

        public int CreateWaitItem(string instanceID, string activityInstanceID, string waitItemConfig, DateTime? duedate)
        {
            throw new NotImplementedException();
        }

        public void DeletWorkflowByGUID(Guid guid)
        {
            //throw new NotImplementedException();
        }

        public void FinishEngineAlert(WFEEngineAlert eA)
        {
            //throw new NotImplementedException();
        }

        public void FinishEngineAlert(string startActivity, string workflowInstanceID)
        {
            //throw new NotImplementedException();
        }

        public void FinishWaitItem(int AWI_ID)
        {
            //throw new NotImplementedException();
        }

        public int GetActiveWorkflowProcesses(string wfDefinition)
        {
            throw new NotImplementedException();
        }

        public WFEActivityDefinition GetActivityDefinitionTemplate(string activityDefinitionID)
        {
            throw new NotImplementedException();
        }

        public List<WFEActivityDefinition> GetAllActivityDefinitons()
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllWorkflowDefinitionIDs()
        {
            throw new NotImplementedException();
        }

        public List<WFEWorkflowDefinition> GetAllWorkflowDefinitions(bool onlyactive = false)
        {
            throw new NotImplementedException();
        }

        public string GetInstanceXml(string instanceID)
        {
            throw new NotImplementedException();
        }

        public WFEEngineAlert GetTopEngineAlert(Guid uniquRuntimeId, EnumAlertTypes alertType = EnumAlertTypes.Default)
        {
            throw new NotImplementedException();
        }

        public WFEEngineAlert GetTopEngineAlertOneTable(Guid uniquRuntimeId, EnumAlertTypes alertType = EnumAlertTypes.Default)
        {
            throw new NotImplementedException();
        }

        public WFEAsyncWaitItem GetWaitItem(string instanceID, string activityInstanceID)
        {
            throw new NotImplementedException();
        }

        public WFEWorkflowDefinition GetWorkflowDefinition(string workflowDefinitionID, bool getCheckedOutVersion = false)
        {
            throw new NotImplementedException();
        }

        public WFEWorkflowDefinition GetWorkflowDefinitionByGuid(Guid theGuid)
        {
            throw new NotImplementedException();
        }

        public WFEWorkflowInstance GetWorkflowInstance(string instanceID)
        {
            throw new NotImplementedException();
        }

        public bool PollingAlertExists(string activityID, string wfInstanceID)
        {
            throw new NotImplementedException();
        }

        public void UpdateInstanceCurrentActivity(string instanceID, string currentActivityInstanceName)
        {
            //throw new NotImplementedException();
        }

        public void UpdateInstanceXml(string instanceID, string instannceXml)
        {
            //throw new NotImplementedException();
        }

        public void UpdateInstanceStatus(string instanceID, EnumWorkflowInstanceStatus newStatus)
        {
            //throw new NotImplementedException();
        }

        public void UpdateLastPolling(string activityID, string wfInstanceID, DateTime? lastPoll)
        {
            //throw new NotImplementedException();
        }

        public void UpdateWorkflowDefinitionObject(WFEWorkflowDefinition wfeDef)
        {
            //throw new NotImplementedException();
        }

        public void UpdateWorkflowDefinitionXml(Guid workflowGUID, string workflowXML)
        {
            //throw new NotImplementedException();
        }

        public void UpdateWorkflowMetaData(Guid workflowGUID, string name, string description, DateTime? validFrom, DateTime? validTo)
        {
            //throw new NotImplementedException();
        }

        public string WriteXmlDataForWorkflow(string instanceID, string xmlData)
        {
            throw new NotImplementedException();
        }

        public int CreateEngineAlert(string startActivity, string workflowInstanceID, string callbackID, string inputParameters, EnumAlertTypes alertType, int pollingInterval, DateTime? lastPollingDate)
        {
            //throw new NotImplementedException();
            return -1;
        }

        public int CreateEngineAlert(string startActivity, string workflowInstanceID, string callbackID, string inputParameters, EnumAlertTypes alertType)
        {
            //throw new NotImplementedException();
            return -1;
        }

        public void CreateCallback(string workflowInstanceID, string clientReferenceID, string currentActivity)
        {
            // new NotImplementedException();
        }

        public List<WFEActivityResultMessage> GetActivityResultMsgByWoin(string workflowInstanceID)
        {
            throw new NotImplementedException();
        }

        public WFEActivityResultMessage SaveActivityResultMessageToWorkflowInstance(string workflowInstanceID, string activityInstanceID, string resultMessage)
        {
            throw new NotImplementedException();
        }
    }
}
