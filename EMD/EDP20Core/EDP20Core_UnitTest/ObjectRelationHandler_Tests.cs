using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.Util.ErrorHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EDP20Core_UnitTest
{
    /// <summary>
    /// Summary description for ObjectRelationHandler_Tests
    /// </summary>
    [TestClass]
    public class ObjectRelationHandler_Tests
    {
        public ObjectRelationHandler_Tests()
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

        [TestMethod, TestCategory("ObjectRelations")]
        public void MetaData_ADD_from_Emtpy_SimpleValue_TEST()
        {
            ObjectRelationManager obrMgr = new ObjectRelationManager();
            EMDObjectRelation obre = new EMDObjectRelation();
            obrMgr.AddMetaData_SimpleValue(obre, "testKey", "testValue");

            string expectedXml = @"<OBRE_Data>
  <SimpleValues>
    <Item key=""testKey"" value=""testValue"" />
  </SimpleValues>
</OBRE_Data>";

            Assert.AreEqual(expectedXml, obre.Data);
        }

        [TestMethod, TestCategory("ObjectRelations")]
        public void MetaData_ADD_from_existingValid_SimpleValue_TEST()
        {
            ObjectRelationManager obrMgr = new ObjectRelationManager();
            EMDObjectRelation obre = new EMDObjectRelation();
            obrMgr.AddMetaData_SimpleValue(obre, "testKey", "testValue");
            obrMgr.AddMetaData_SimpleValue(obre, "testKey2", "testValue2");

            string expectedXml = @"<OBRE_Data>
  <SimpleValues>
    <Item key=""testKey"" value=""testValue"" />
    <Item key=""testKey2"" value=""testValue2"" />
  </SimpleValues>
</OBRE_Data>";

            Assert.AreEqual(expectedXml, obre.Data);
        }

        [TestMethod, TestCategory("ObjectRelations")]
        [ExpectedException(typeof(BaseException), "Expected of BaseException")]
        public void MetaData_ADD_to_invalid_SimpleValue_TEST()
        {
            ObjectRelationManager obrMgr = new ObjectRelationManager();
            EMDObjectRelation obre = new EMDObjectRelation();
            obre.Data = "<ups></ups>";
            obrMgr.AddMetaData_SimpleValue(obre, "testKey", "testValue");
            obrMgr.AddMetaData_SimpleValue(obre, "testKey2", "testValue2");

            string expectedXml = @"<OBRE_Data>
  <SimpleValues>
    <Item key=""testKey"" value=""testValue"" />
    <Item key=""testKey2"" value=""testValue2"" />
  </SimpleValues>
</OBRE_Data>";

            Assert.AreEqual(expectedXml, obre.Data);
        }

        [TestMethod, TestCategory("ObjectRelations")]
        public void MetaDAta_GET_SimpleValue_TEST()
        {
            ObjectRelationManager obrMgr = new ObjectRelationManager();
            EMDObjectRelation obre = new EMDObjectRelation();
            obrMgr.AddMetaData_SimpleValue(obre, "testKey", "testValue");
            obrMgr.AddMetaData_SimpleValue(obre, "testKey2", "testValue2");
            obrMgr.AddMetaData_SimpleValue(obre, "testKey3", "testValue3");

            string result = obrMgr.GetMetaData_SimpleValue(obre, "testKey2");

            Assert.AreEqual("testValue2", result);
        }

        [TestMethod, TestCategory("ObjectRelations")]
        public void MetaDAta_GETALL_SimpleValue_TEST()
        {
            ObjectRelationManager obrMgr = new ObjectRelationManager();
            EMDObjectRelation obre = new EMDObjectRelation();
            obrMgr.AddMetaData_SimpleValue(obre, "testKey", "testValue");
            obrMgr.AddMetaData_SimpleValue(obre, "testKey2", "testValue2");
            obrMgr.AddMetaData_SimpleValue(obre, "testKey3", "testValue3");

            Dictionary<string, string> result = obrMgr.GetMetaData_AllSimpleValues(obre);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("testValue3", result["testKey3"]);
        }
    }
}
