using Kapsch.IS.ProcessEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace TESTING
{
    [TestClass]
    public class ProcessEngine_TEST
    {


        [TestMethod, TestCategory("ProcessEngine")]
        public void StartWorkflow_LowLevelWay()
        {
            //
            // low level without mapping
            //
            WorkflowHandler wfHandler = new WorkflowHandler();
            WorkflowMessageData msgData = new WorkflowMessageData();
            msgData.WorkflowDefinitionID = "WODE_c9e8251e38764e739797108a7cc06850"; 
            //msgData.WorkflowVariables.Add(new WorkflowMessageVariable("GoesInto", "Hello "));
            wfHandler.CreateNewWorkflowInstance(msgData);


        }




        private void RunWorkflow()
        {
            Runtime run = new Runtime();
            string returnMsg = run.RunEngine();
            Debug.WriteLine(returnMsg);
        }
    }
}
