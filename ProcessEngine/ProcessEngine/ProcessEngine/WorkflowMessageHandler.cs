using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kapsch.IS.ProcessEngine
{
    /// <summary>
    /// Handler for Workflow Messages
    /// </summary>
    public class WorkflowMessageHandler
    {
        /// <summary>
        /// Adds or Updates a WorkflowMessageVariable in the collection.
        /// </summary>
        /// <param name="wfMsgData"></param>
        /// <param name="varKey"></param>
        /// <param name="varValue"></param>
        public static void UpdateOrAddVariable(WorkflowMessageData wfMsgData, string varKey, string varValue)
        {
            WorkflowMessageVariable item = wfMsgData.WorkflowVariables.Find(match => match.VarName == varKey);
            if (item != null)
                item.VarValue = varValue;
            else
                wfMsgData.WorkflowVariables.Add(new WorkflowMessageVariable(varKey, varValue));
        }
        /// <summary>
        /// Adds or Updates a WorkflowMessageVariable in the collection.
        /// </summary>
        /// <param name="wfMsgData"></param>
        /// <param name="wfVar"></param>
        public static void UpdateOrAddVariable(WorkflowMessageData wfMsgData, WorkflowMessageVariable wfVar)
        {
            UpdateOrAddVariable(wfMsgData, wfVar.VarName, wfVar.VarValue);
        }
        /// <summary>
        /// Update the additional xml data variable
        /// </summary>
        /// <param name="wfMsgData"></param>
        /// <param name="xDoc"></param>
        public static void UpdateXmlData(WorkflowMessageData wfMsgData, XDocument xDoc)
        {
            wfMsgData.XmlData = xDoc.ToString(SaveOptions.None);
        }


    }
}
