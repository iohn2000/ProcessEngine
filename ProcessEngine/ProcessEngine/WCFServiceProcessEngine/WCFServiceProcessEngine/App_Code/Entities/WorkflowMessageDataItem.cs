using Kapsch.IS.ProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for WorkflowMessageDataItem
/// </summary>
[DataContract]
public class WorkflowMessageDataItem
{
    [DataMember]
    public string WorkflowDefinitionID { get; set; }

    /// <summary>
    /// collection of workflow variables; can also be serialized XElements
    /// </summary>
    [DataMember]
    public List<WorkflowMessageVariableItem> WorkflowVariables { get; set; }

    /// <summary>
    /// additional data in xml format. This is a simple storing and viewing, no business
    /// logic or similar will be apllied to it
    /// </summary>
    public string XmlData { get; set; }


    public static WorkflowMessageData Map(WorkflowMessageDataItem workflowMessageDataItem)
    {
        return new WorkflowMessageData()
        {
            WorkflowDefinitionID = workflowMessageDataItem.WorkflowDefinitionID,
            XmlData = workflowMessageDataItem.XmlData,
            WorkflowVariables = WorkflowMessageVariableItem.Map(workflowMessageDataItem.WorkflowVariables)
        };


    }
}