using Kapsch.IS.ProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for WorkflowMessageVariableItem
/// </summary>
public class WorkflowMessageVariableItem
{

    public WorkflowMessageVariableItem() { }


    public WorkflowMessageVariableItem(string name, string val)
    {
        this.VarName = name;
        this.VarValue = val;
    }


    public string VarName { get; set; }

    public string VarValue { get; set; }

    public static WorkflowMessageVariable Map(WorkflowMessageVariableItem workflowMessageVariableItem)
    {
        return new WorkflowMessageVariable()
        {
            VarName = workflowMessageVariableItem.VarName,
            VarValue = workflowMessageVariableItem.VarValue
        };
    }

    public static List<WorkflowMessageVariable> Map(List<WorkflowMessageVariableItem> workflowMessageVariableItems)
    {
        List<WorkflowMessageVariable> workflowMessageVariables = new List<WorkflowMessageVariable>();


        foreach (var workflowMessageVariableItem in workflowMessageVariableItems)
        {
            workflowMessageVariables.Add(Map(workflowMessageVariableItem));
        }

        return workflowMessageVariables;
    }
}