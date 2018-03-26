using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.WF.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kapsch.IS.EDP.Core.WF
{
    public class WorkflowHelper
    {
        public static WorkflowMessageDataItem GetWorkflowMessageData(WorkflowBaseMessage baseDataHelper)
        {
            WorkflowMessageDataItem workflowMessageDataItem = new WorkflowMessageDataItem();
            workflowMessageDataItem.WorkflowDefinitionID = baseDataHelper.WFDefID;

            Type t = baseDataHelper.GetType();

            // workflowMessageDataItem.WorkflowVariables = new WorkflowMessageVariableItem[t.GetProperties().Length - 1];

            List<WorkflowMessageVariableItem> variableItems = new List<WorkflowMessageVariableItem>();

            for (int i = 0; i < t.GetProperties().Length; i++)
            {
                String propValue = "";
                object propObject = null;
                if (t.GetProperties()[i].CanRead)
                {
                    propObject = t.GetProperties()[i].GetValue(baseDataHelper);
                }

                if (t.GetProperties()[i].PropertyType == new XElement("a").GetType())
                {
                    try
                    {
                        propValue = baseDataHelper.SerializeXElement((XElement)propObject);
                    }
                    catch 
                    {
                        propValue = "";
                    }
                }
                else
                {

                    if (propObject != null)
                    {
                        propValue = propObject.ToString();
                    }
                    else
                    {
                        propValue = "";
                    }
                }

                if (propValue != null)
                {
                    WorkflowMessageVariableItem wfvar = new WorkflowMessageVariableItem()
                    {
                        VarName = t.GetProperties()[i].Name,
                        VarValue = propValue
                    };
                    variableItems.Add(wfvar);
                    // workflowMessageDataItem.WorkflowVariables[i] = wfvar;
                }
            }
            workflowMessageDataItem.WorkflowVariables = variableItems.ToArray();


            return workflowMessageDataItem;

        }

        public static XElement CreateXElementForKCCAdditionalData(string name, string value)
        {
            if (value == null)
                value = "";

            XElement colItem = new XElement("Item");
            colItem.SetAttributeValue("key", name);
            colItem.Value = value;
            return colItem;
        }

        /// <summary>
        /// Sets all relevant EmploymentInfos in the given object
        /// </summary>
        /// <param name="employmentInfos"></param>
        /// <param name="costcenterOldGuid"></param>
        /// <param name="costcenterNewGuid"></param>
        /// <param name="costCenterResponsibleOldEmplGuid"></param>
        /// <param name="costCenterResponsibleNewEmplGuid"></param>
        /// <param name="orgunitOldGuid"></param>
        /// <param name="orgunitNewGuid"></param>
        /// <param name="lineManagerOldEmplGuid"></param>
        /// <param name="lineManagerNewEmplGuid"></param>
        /// <param name="teamleaderOldEmplGuid"></param>
        /// <param name="teamleaderNewEmplGuid"></param>
        /// <param name="assistanceOldEmplGuid"></param>
        /// <param name="assistanceNewEmplGuid"></param>
        /// <param name="persNrOld"></param>
        /// <param name="persNrNew"></param>
        /// <param name="locationOldGuid"></param>
        /// <param name="locationNewGuid"></param>
        public static void SetEmploymentInfos(ref IEmploymentInfos employmentInfos,
            string costcenterOldGuid,
            string costcenterNewGuid,
            string costCenterResponsibleOldEmplGuid,
            string costCenterResponsibleNewEmplGuid,
            string orgunitOldGuid,
            string orgunitNewGuid,
            string lineManagerOldEmplGuid,
            string lineManagerNewEmplGuid,
            string teamleaderOldEmplGuid,
            string teamleaderNewEmplGuid,
            string assistanceOldEmplGuid,
            string assistanceNewEmplGuid,
            string persNrOld,
            string persNrNew,
            string locationOldGuid,
            string locationNewGuid
            )
        {
            employmentInfos.CostcenterOldGuid = costcenterOldGuid;
            employmentInfos.CostcenterNewGuid = costcenterNewGuid;
            employmentInfos.CostCenterResponsibleOldEmplGuid = costCenterResponsibleOldEmplGuid;
            employmentInfos.CostCenterResponsibleNewEmplGuid = costCenterResponsibleNewEmplGuid;
            employmentInfos.OrgunitOldGuid = orgunitOldGuid;
            employmentInfos.OrgunitNewGuid = orgunitNewGuid;
            employmentInfos.LineManagerOldEmplGuid = lineManagerOldEmplGuid;
            employmentInfos.LineManagerNewEmplGuid = lineManagerNewEmplGuid;
            employmentInfos.TeamleaderOldEmplGuid = teamleaderOldEmplGuid;
            employmentInfos.TeamleaderNewEmplGuid = teamleaderNewEmplGuid;
            employmentInfos.AssistanceOldEmplGuid = assistanceOldEmplGuid;
            employmentInfos.AssistanceNewEmplGuid = assistanceNewEmplGuid;
            employmentInfos.PersNrOld = persNrOld;
            employmentInfos.PersNrNew = persNrNew;
            employmentInfos.LocationOldGuid = locationOldGuid;
            employmentInfos.LocationNewGuid = locationNewGuid;
        }

        /// <summary>
        /// merges 2 lists of workflow variables and makes sure there are no duplicates
        /// </summary>
        /// <returns></returns>
        public static WorkflowMessageVariableItem[] MergeWorkflowVariableLists(WorkflowMessageVariableItem[] existingWorkflowVariablesList, Dictionary<string, string> additionalListOfWorkflowVariables)
        {
            List<WorkflowMessageVariableItem> tempList = existingWorkflowVariablesList.ToList();

            foreach (KeyValuePair<string, string> item in additionalListOfWorkflowVariables)
            {
                string wfVarName = item.Key;
                string wfVarValue = item.Value;

                //bool varExists = tempList.Exists( delegate(WorkflowMessageVariableItem m)  { return m.VarName == wfVarName;  } ); 
                //bool varExists = tempList.Exists( (WorkflowMessageVariableItem m) =>  { return m.VarName == wfVarName;  } );
                bool varExists = tempList.Exists(m => m.VarName == wfVarName);
                if (!varExists)
                {
                    // not in list so add it
                    tempList.Add(new WorkflowMessageVariableItem()
                    {
                        VarName = wfVarName,
                        VarValue = wfVarValue ?? "",
                    });
                }
            }

            return tempList.ToArray();
        }
    }


}
