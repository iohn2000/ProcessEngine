using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.Util.Serialiser;

namespace TESTING
{
    [TestClass]
    public class WorkflowMesssageData_TEST
    {
        [TestMethod, TestCategory("WorkflowMessageData")]
        public void CreateWorkflowMessage_TEST()
        {
            WorkflowMessageData wfData = new WorkflowMessageData();
            wfData.WorkflowDefinitionID = "WorkflowReadOnlyTester";
            wfData.WorkflowVariables.Add(new WorkflowMessageVariable("name1","value1"));
            wfData.WorkflowVariables.Add(new WorkflowMessageVariable("name2","value2"));

            Assert.AreEqual("value1",wfData.WorkflowVariables[0].VarValue);
        }
    }
}
