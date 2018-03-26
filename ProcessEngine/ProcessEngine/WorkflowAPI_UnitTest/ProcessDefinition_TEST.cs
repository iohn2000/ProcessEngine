using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;

namespace TESTING
{
    /// <summary>
    /// Summary description for ProcessDefinition_TEST
    /// </summary>
    [TestClass]
    public class ProcessDefinition_TEST
    {
        public ProcessDefinition_TEST()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        [TestMethod, TestCategory("ProcessDefinition")]
        public void CheckoutSaveMetaData_TEST()
        {
            ProcessDefinition pDef = new ProcessDefinition();
            WFEWorkflowDefinition newWfDef = pDef.CreateWorkflowDefinition("testWF_saveMeta_" + DateTime.Now.Millisecond.ToString(), "testcheckout", null, null);
            pDef.CheckoutWorkflowDefinition(newWfDef.WFD_ID, "fleckj");

            pDef.SaveWorkflowMetaData(newWfDef.WFD_ID,newWfDef.WFD_Name + "__metaChange",newWfDef.WFD_Description,newWfDef.WFD_ValidFrom,newWfDef.WFD_ValidTo,"fleckj");

            pDef.CheckinWorkflowDefinition(newWfDef.WFD_ID, "fleckj");

            
        }

        [TestMethod, TestCategory("ProcessDefinition")]
        public void GetListOfWorkflowDefinitions_TEST()
        {
            ProcessDefinition pDef = new ProcessDefinition();
            var lstWfDef = pDef.GetAllWorkflowDefinitions();
            var checkOutTestItem = lstWfDef.FindAll(m => m.WFD_ID == "checkoutTest");

            foreach (var i in lstWfDef)
            {
                Console.WriteLine("{0}\t\t{1}\t\tVer.:{2}\t\t{3}", i.Guid, i.WFD_ID, i.WFD_Version, i.WFD_CheckedOutBy);
            }

            Assert.AreEqual(1, checkOutTestItem.Count, "should only 1 one wf-def");
        }

        [TestMethod, TestCategory("ProcessDefinition")]
        public void GetWfByID_TEST()
        {
            ProcessDefinition pDef = new ProcessDefinition();
            var wf = pDef.GetWorkflowDefinitionByID("checkoutTest", "fleckj");
            Assert.AreEqual(2, wf.WFD_Version);

            var wf2 = pDef.GetWorkflowDefinitionByID("checkoutTest", "xxxxxxxxx");
            Assert.AreEqual(2, wf2.WFD_Version);
        }

        [TestMethod, TestCategory("ProcessDefinition")]
        public void checkout_wf_TEST()
        {
            ProcessDefinition pDef = new ProcessDefinition();
            WFEWorkflowDefinition newWfDef = pDef.CreateWorkflowDefinition("testWF_save_" + DateTime.Now.Millisecond.ToString(), "testcheckout", null, null);

            pDef.CheckoutWorkflowDefinition(newWfDef.WFD_ID, "fleckj");

            WFEWorkflowDefinition x = pDef.GetWorkflowDefinitionByID(newWfDef.WFD_ID, "fleckj");

            Assert.AreEqual(newWfDef.WFD_ID, x.WFD_ID);
            Assert.AreEqual(0, x.WFD_Version);
            Assert.AreNotEqual(newWfDef.Guid, x.Guid);

            // try to checkout second time, should cause errors
            try
            {
                pDef.CheckoutWorkflowDefinition(newWfDef.WFD_ID, "fleckj");
                Assert.Fail("should trow an exception");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(1, 1, "in exception so ok");
            }

        }

        [TestMethod, TestCategory("ProcessDefinition")]
        public void SaveWorkflow_TEST()
        {
            ProcessDefinition pDef = new ProcessDefinition();
            var newWfDef = pDef.CreateWorkflowDefinition("testWF_save_" + DateTime.Now.Millisecond.ToString(), "test save", null, null);
            const string xmlDefString = "<xx>something new</xx>";

            try
            {
                pDef.SaveWorkflowXmlDefinitionOnly(newWfDef.WFD_ID, xmlDefString, "fleckjTest");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(1, 1, "ok  - worklfow not checked out --> cannot save");
            }

            // do checkout first
            var wfCheckedout = pDef.CheckoutWorkflowDefinition(newWfDef.WFD_ID, "sepp");

            try
            {
                pDef.SaveWorkflowXmlDefinitionOnly(newWfDef.WFD_ID, "das ist kein xml <x>", "fleckjTest");
            }
            catch (WorkflowException ex)
            {
                Assert.AreEqual(1, 1, "ok - cannot save invalid xml");
            }

            pDef.SaveWorkflowXmlDefinitionOnly(newWfDef.WFD_ID, xmlDefString, "sepp");


            var updatedWF = pDef.GetWorkflowDefinitionByID(newWfDef.WFD_ID, "sepp");
            Assert.AreEqual(xmlDefString, updatedWF.WFD_Definition);

        }

        [TestMethod, TestCategory("ProcessDefinition")]
        public void CheckinWorkflow_TEST()
        {
            ProcessDefinition pDef = new ProcessDefinition();
            var newWfDef = pDef.CreateWorkflowDefinition("testWF_checkin_" + DateTime.Now.Millisecond.ToString(), "test save", null, null);
            const string xmlDefString = "<xx>something new</xx>";
            // do checkout first
            var wfCheckedout = pDef.CheckoutWorkflowDefinition(newWfDef.WFD_ID, "sepp");
            pDef.SaveWorkflowXmlDefinitionOnly(newWfDef.WFD_ID, xmlDefString, "sepp");


            pDef.CheckinWorkflowDefinition(newWfDef.WFD_ID, "sepp");
            var newVersionWF = pDef.GetWorkflowDefinitionByID(newWfDef.WFD_ID, "sepp");
            Assert.AreEqual(1, newVersionWF.WFD_Version);
            Assert.AreEqual(xmlDefString, newVersionWF.WFD_Definition);
        }

        [TestMethod, TestCategory("ProcessDefinition")]
        public void UndoCheckout_TEST()
        {
            ProcessDefinition pDef = new ProcessDefinition();
            var newWfDef = pDef.CreateWorkflowDefinition("testWF_save_" + DateTime.Now.Millisecond.ToString(), "testcheckout", null, null);

            pDef.CheckoutWorkflowDefinition(newWfDef.WFD_ID, "sepp");
            pDef.SaveWorkflowXmlDefinitionOnly(newWfDef.WFD_ID, "<new></new>", "sepp");


            pDef.UndoCheckout(newWfDef.WFD_ID, "sepp");

            var removedWF = pDef.GetWorkflowDefinitionByID(newWfDef.WFD_ID, "sepp");
            Assert.AreNotEqual(-1, removedWF.WFD_Version);


        }
    }
}
