using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.Util.ErrorHandling;



namespace TESTING
{
    [TestClass]
    public class DataAccess_TEST
    {

                [TestMethod()]
        public void GetNextActivityTest()
        {
            DatabaseAccess db = new DatabaseAccess();
            db.GetNextActivity("WOIN_dce5867896cb4545b8d936778b9a166b");
        }

        [TestMethod]
        public void GetActiveWorkflowProcesses_TEST()
        {
            DatabaseAccess dblayer = new DatabaseAccess();
            int amount = dblayer.GetActiveWorkflowProcesses("Add_EQ_with_1_Approval");
            Assert.AreEqual(0, amount);

            amount = dblayer.GetActiveWorkflowProcesses("asfds");
            Assert.AreEqual(0, amount);
        }

        [TestMethod]
        public void GetDocumentTemplate()
        {
            DatabaseAccess db = new DatabaseAccess();
            var result = db.GetDocumentTemplateByName("EQDERequested");
            Assert.AreEqual("EQDERequested", result.TMPL_Name);


            try
            {
                var result2 = db.GetDocumentTemplateByName("ddd"); //gibts net
                Assert.Fail();
            }
            catch (BaseException bEx)
            {
                Assert.AreEqual(1,1);
            }
            
        }
    }
}
