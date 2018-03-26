using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.ProcessEngine.DLLConfiguration;
using System.Configuration;
using System.Reflection;

namespace TESTING
{
    /// <summary>
    /// Summary description for DLLConfig_TEST
    /// </summary>
    [TestClass]
    public class DLLConfig_TEST
    {
        public DLLConfig_TEST()
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

        [TestMethod]
        public void ActivityConfig_Reads()
        {
            // read app config section
            ActivityDLLConfigurationSection dllConfig;
            dllConfig = ConfigurationManager.GetSection("ActivityDLLsConfig") as ActivityDLLConfigurationSection;
            foreach (ActivityDLLElement item in dllConfig.ActivityDLLs)
            {
                Console.WriteLine("{0} : {1}",item.DLLName,item.DLLPath);
            }

            Assert.AreEqual(2,dllConfig.ActivityDLLs.Count);
        }

         [TestMethod]
        public void GetAllActivitiesFromDLLs_TEST()
        {
            ActivityDLLConfigurationSection dllConfig;
            dllConfig = ConfigurationManager.GetSection("ActivityDLLsConfig") as ActivityDLLConfigurationSection;
            ActivityDLLCacheHandler acH = new ActivityDLLCacheHandler();

           
            Tuple<Dictionary<string, object>, Dictionary<string, MethodInfo>> returnValue =  acH.ReloadActivityCache(dllConfig);
            
            Assert.AreEqual(13,returnValue.Item1.Count);
            Assert.AreEqual(13,returnValue.Item2.Count);
        }
    }
}
