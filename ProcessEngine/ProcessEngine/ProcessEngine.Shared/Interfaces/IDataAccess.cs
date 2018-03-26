using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Shared.Interfaces
{
    public interface IDataAccess
    {
        WFEEngineAlert GetTopEngineAlertOneTable(Guid uniquRuntimeId, EnumAlertTypes alertType = EnumAlertTypes.Default);

        void FinishWaitItem(int AWI_ID);

        int CreateEngineAlert(string startActivity, string workflowInstanceID, string callbackID, string inputParameters, EnumAlertTypes alertType, int pollingInterval, DateTime? lastPollingDate);

        int CreateEngineAlert(string startActivity, string workflowInstanceID, string callbackID, string inputParameters, EnumAlertTypes alertType);

        void FinishEngineAlert(string startActivity, string workflowInstanceID);

        void FinishEngineAlert(WFEEngineAlert eA);

        WFEWorkflowDefinition CreateNewWorkflowDefinition(WFEWorkflowDefinition newWFEDefinition);

        WFEWorkflowDefinition CreateNewWorkflowDefinition(WFEWorkflowDefinition newWFEDefinition, bool isCheckout = false);

        //WFEWorkflowDefinition CreateNewWorkflowMetaData(Guid workflowGUID, string name, string description, DateTime? validFrom, DateTime? validTo);

        void UpdateWorkflowMetaData(Guid workflowGUID, string name, string description, DateTime? validFrom, DateTime? validTo);

        void UpdateWorkflowDefinitionObject(WFEWorkflowDefinition wfeDef);

        void DeletWorkflowByGUID(Guid guid);

        void UpdateWorkflowDefinitionXml(Guid workflowGUID, string workflowXML);

        List<WFEWorkflowDefinition> GetAllWorkflowDefinitions(bool onlyactive = false);

        List<String> GetAllWorkflowDefinitionIDs();

        WFEWorkflowDefinition GetWorkflowDefinition(string workflowDefinitionID, bool getCheckedOutVersion = false);

        WFEWorkflowDefinition GetWorkflowDefinitionByGuid(Guid theGuid);

        [Obsolete]
        string CreateNewWorkflowInstance(string definitionID, string instanceID, string parentWorkflowInstanceID, string nextActivity);

        string GetInstanceXml(string instanceID);

        WFEWorkflowInstance GetWorkflowInstance(string instanceID);

        int GetActiveWorkflowProcesses(string wfDefinition);

        void UpdateInstanceXml(string instanceID, string instannceXml);

        void UpdateInstanceStatus(string instanceID, EnumWorkflowInstanceStatus newStatus);

        void UpdateInstanceCurrentActivity(string instanceID, string currentActivityInstanceName);

        bool PollingAlertExists(string activityID, string wfInstanceID);

        void UpdateLastPolling(string activityID, string wfInstanceID, DateTime? lastPoll);

        int CreateWaitItem(string instanceID, string activityInstanceID, string waitItemConfig, DateTime? duedate);

        WFEAsyncWaitItem GetWaitItem(string instanceID, string activityInstanceID);

        string WriteXmlDataForWorkflow(string instanceID, string xmlData);

        List<WFEActivityDefinition> GetAllActivityDefinitons();

        WFEActivityDefinition GetActivityDefinitionTemplate(string activityDefinitionID);

        void CreateCallback(string workflowInstanceID, string clientReferenceID, string currentActivity);

        List<WFEActivityResultMessage> GetActivityResultMsgByWoin(string workflowInstanceID);

        WFEActivityResultMessage SaveActivityResultMessageToWorkflowInstance(string workflowInstanceID, string activityInstanceID, string resultMessage);
    }
}
