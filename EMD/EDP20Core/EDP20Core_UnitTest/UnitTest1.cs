using System;
using System.Collections.Generic;

using System.Linq;
using System.Data.SqlClient;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;


//using Kendo.Mvc;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Reflection;
using Kapsch.IS.Util.ErrorHandling;
using System.Diagnostics;
using System.Collections;
using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using Kapsch.IS.EDP.Core.WF;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class CoreObjectTests
    {
       #region PersonTests

        [TestMethod]
        [TestCategory("PersonTests")]
        public void TestCreateUserIDProposalForPerson()
        {
            //String newUserIDs = uidh.CreateMainUserIDLogic("Mayer","Robert");
            var newUserIDs = Manager.UserManager.CreateUserIDProposalForPerson("Werner", "Gerhard", "A_");

            Assert.AreEqual(1, newUserIDs.Count);

            newUserIDs = Manager.UserManager.CreateUserIDProposalForPerson("Mayer", "Robert");

            Assert.AreEqual(3, newUserIDs.Count);
        }

        //readonly string TestPackage = "UT_Package";

        [TestMethod]
        [TestCategory("PersonTests")]
        public void TestLocationWithHistorize()
        {
            LocationHandler locationHandler = new LocationHandler() { Historical = true, DeliverInActive = true };
            int expectedCount = 0;
            using (SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EMD_Direct"].ConnectionString))
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM [dbo].[Location]", sqlConnection);
                SqlDataReader sqlReader = sqlCommand.ExecuteReader();

                List<Location> locations = new List<Location>();
                while (sqlReader.Read())
                {
                    expectedCount++;
                }
                sqlConnection.Close();
            }
            int actualCount = locationHandler.GetObjects<EMDLocation, Location>().Count;
            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestMethod]
        [TestCategory("PersonTests")]
        public void GetEMailForPerson()
        {
            PersonHandler ph = new PersonHandler();
            PersonManager persManager = new PersonManager();
            EMDPerson person = (EMDPerson)persManager.GetPersonByUserId("STAGL");
            ph.CreateMainMailProposalForPerson(person);
            System.Diagnostics.Debug.WriteLine("Mailadress given: " + person.MainMail);
            Assert.AreEqual("Wolfgang.Stagl2@kapsch.net", person.MainMail);
        }

        #endregion

        static List<EMDFilterRule> historizingTestData = new List<EMDFilterRule>();

        #region Historizing
        [TestMethod]
        [TestCategory("HistorizingTests")]
        public void TestCoreTransactionWithHistory()
        {
            CoreTransaction transaction = new CoreTransaction();

            transaction.Begin();

            EquipmentDefinitionHandler equipmentDefinitionHandler = new EquipmentDefinitionHandler()
            {
                Historical = true,
                DeliverInActive = true
            };

            EquipmentDefinitionHandler equipmentDefinitionHandlerTransactin = new EquipmentDefinitionHandler(transaction) { };


            var list1 = equipmentDefinitionHandlerTransactin.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>("1=1");
            var list2 = equipmentDefinitionHandler.GetObjects<EMDEquipmentDefinition, EquipmentDefinition>("1=1");

            Assert.IsTrue(list1.Count < list2.Count);
            transaction.Rollback();

            //   transaction.Commit();

        }

        #endregion

        [TestMethod]
        [TestCategory("Helper")]
        public void TestNewlineRemoveHelper()
        {
            string testString = "1.Zeile\n2.Zeile\n\r3.Zeile\r";

            string removedNewLines = StringHelper.ReplaceNewlines(testString);

            Assert.IsTrue(removedNewLines == "1.Zeile2.Zeile3.Zeile");
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


        [TestMethod]
        [TestCategory("FrameworkTests")]
        public void testGetInstanceFromGuid()
        {
            EMDObject<EMDPerson> pers = (EMDPerson)new PersonHandler().GetPersonByP_Id(5);

            string myguid = pers.Guid;

            EntityPrefix ep = EntityPrefix.Instance;
            Type entityType = ep.GetTypeFromGuid(myguid);

            Type genericType = typeof(IEMDObject<>);
            Type specificType = genericType.MakeGenericType(entityType);

            MethodInfo method = ep.GetType().GetMethod("GetInstanceFromGuid");
            MethodInfo generic = method.MakeGenericMethod(entityType);

            var pers1 = generic.Invoke(ep, new object[] { myguid });

            Type persType = pers.GetType();
            PropertyInfo pidProperty = persType.GetProperty("P_ID");

            int p_id = (int)pidProperty.GetValue(pers);

            Assert.AreEqual(5, p_id);

        }

        [TestMethod]
        [TestCategory("FrameworkTests")]
        public void testGetPropertyByNameandGuid()
        {
            EMDObject<EMDPerson> pers = (EMDPerson)new PersonHandler().GetPersonByP_Id(5);
            string myguid = pers.Guid;

            EntityPrefix ep = EntityPrefix.Instance;
            Type propType;
            var value = ep.GetPropertyByNameandGuid(myguid, "dpwKey", out propType);

            Assert.AreEqual(5, value);

        }

        [TestMethod]
        [TestCategory("FrameworkTests")]
        public void testGetGuidByFunctionCall()
        {
            EMDObject<EMDPerson> pers = (EMDPerson)new PersonHandler().GetPersonByP_Id(505);
            List<IEMDObject<EMDEmployment>> emplList = new EmploymentHandler().GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers.Guid + "\"");
            String myguid = null;
            foreach (EMDEmployment empl in emplList)
            {
                if (new ObjectFlagManager().IsMainEmployment(empl.Guid))
                {
                    myguid = empl.Guid;
                    break;
                }
            }

            Assert.IsNotNull(myguid, "did not find MainEmployment for effected person");

            EntityPrefix ep = EntityPrefix.Instance;
            Type propType;
            string queryString = "P_ID@@SearchResponsibleRoleForEmployment(10500)@@" + myguid;

            EntityQuery eq = new EntityQuery();

            try
            {
                var pid = new EntityQuery().Query(queryString, out propType);
                Assert.AreEqual(389, pid);
            }
            catch (Exception ex)
            {

                Assert.Fail(ex.ToString());
            }
        }



        [TestMethod]
        [TestCategory("FrameworkTests")]
        public void testGetGuidByFunctionCall_contacts()
        {
            string myguid1 = "EMPL_b842c7fc3ed240d7a0b48924783248d9";
            EMDObject<EMDPerson> pers = (EMDPerson)new PersonHandler().GetPersonByP_Id(505);
            List<IEMDObject<EMDEmployment>> emplList = new EmploymentHandler().GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers.Guid + "\"");
            String myguid = null;
            foreach (EMDEmployment empl in emplList)
            {
                if (new ObjectFlagManager().IsMainEmployment(empl.Guid))
                {
                    myguid = empl.Guid;
                    break;
                }
            }

            Assert.IsNotNull(myguid, "did not find MainEmployment for effected person");

            EntityPrefix ep = EntityPrefix.Instance;
            Type propType;
            // COTY_26a1bebc72004b82a87338a67e2e45f7 == ZimmerNummer
            string queryString = "Text@@SearchContactItemForEmployment(COTY_26a1bebc72004b82a87338a67e2e45f7)@@" + myguid;

            EntityQuery eq = new EntityQuery();
            var pid = new EntityQuery().Query(queryString, out propType);

            Assert.AreEqual("4.24", pid.ToString().Trim());

            queryString = "Text@@SearchContactItemForEmployment(COTY_82afdaa606e142088f915b1d66d38428)@@" + myguid1;
            eq = new EntityQuery();
            pid = new EntityQuery().Query(queryString, out propType);
            Assert.AreEqual("", pid.ToString().Trim());
        }

        [TestMethod]
        [TestCategory("FrameworkTests")]
        public void QueryWithMixedString_TEST()
        {
            EMDObject<EMDPerson> pers = (EMDPerson)new PersonHandler().GetPersonByP_Id(505);
            List<IEMDObject<EMDEmployment>> emplList = new EmploymentHandler().GetObjects<EMDEmployment, Employment>("P_Guid = \"" + pers.Guid + "\"");
            String myguid = null;
            foreach (EMDEmployment empl in emplList)
            {
                if (new ObjectFlagManager().IsMainEmployment(empl.Guid))
                {
                    myguid = empl.Guid;
                    break;
                }
            }

            Assert.IsNotNull(myguid, "did not find MainEmployment for effected person");

            EntityPrefix ep = EntityPrefix.Instance;
            Type propType;
            // COTY_26a1bebc72004b82a87338a67e2e45f7 == ZimmerNummer
            string queryString = @"""Die Person :"" + ""MainMail@@" + pers.Guid + @""" + ""hat Zimmernummer :"" + ""Text@@SearchContactItemForEmployment(COTY_26a1bebc72004b82a87338a67e2e45f7)@@" + myguid + @"""";

            EntityQuery eq = new EntityQuery();
            var pid = new EntityQuery().QueryMixedString(queryString, out propType);

            Assert.AreEqual("\"Die Person :\" + \"robert.mayer@kapsch.net\" + \"hat Zimmernummer :\" + \"4.24\"", pid.ToString().Trim());

        }

        [TestMethod]
        [TestCategory("FrameworkTests")]
        public void NoAtAtinString_TEST()
        {
            EntityQuery eq = new EntityQuery();
            Type propType;
            string queryString = "<!/!%§=)!adoa0348952\"öqfß2974afghpöqah>_:;";
            var value = new EntityQuery().Query(queryString, out propType);
            Assert.AreEqual(queryString, value.ToString());
        }

        [TestMethod]
        [TestCategory("FrameworkTests")]
        public void testGetStringParsedPropertyByNameandGuid()
        {
            EMDObject<EMDPerson> pers = (EMDPerson)new PersonHandler().GetPersonByP_Id(5);
            string myguid = pers.Guid;

            string queryString = "E_Guid@@" + myguid;

            EntityQuery eq = new EntityQuery();
            Type propType;
            var value = new EntityQuery().Query(queryString, out propType);

            Assert.AreEqual(5, value);

        }

        [TestMethod]
        [TestCategory("Workflow")]
        public void TestProcessMappingEnumIgnoreCase()
        {
            EMDProcessMapping emdProcessMapping = new EMDProcessMapping();
            emdProcessMapping.Method = "AdD";
            Assert.IsTrue(emdProcessMapping.MethodEnum == WorkflowAction.Add);
        }

        [TestMethod]
        [TestCategory("EmploymentChangeTypeHelper")]
        public void EmploymentChangeTypeHelper_TEST()
        {
            EmploymentChangeTypeHelper ecth = new EmploymentChangeTypeHelper(EnumEmploymentChangeType.EmploymentType);
            Assert.IsTrue(ecth.isNeeded(EnumChangeValueType.Enterprise) == false);
            Assert.IsTrue(ecth.isNeeded(EnumChangeValueType.EmploymentType) == true);
            Assert.IsTrue(ecth.isNeeded(EnumChangeValueType.NewEmpl) == true);
        }


        [TestMethod]
        [TestCategory("Workflow")]
        public void AdditionalWorkflowVariables_TEST()
        {
            //build some test data
            WorkflowMessageVariableItem[] existingWorkflowVariablesList = new[]
            {
                new WorkflowMessageVariableItem(){VarName = "EffPersEmplGuid", VarValue = "EMPL_xx01" },
                new WorkflowMessageVariableItem(){VarName = "RequestingPersEmplGuid", VarValue = "EMPL_xx02" },
                new WorkflowMessageVariableItem(){VarName = "ExitingVars01", VarValue = "should not be included 01" },
                new WorkflowMessageVariableItem(){VarName = "ExitingVars02", VarValue = "should not be included 02" },
            };

            Dictionary<string, string> additionalListOfWorkflowVariables = new Dictionary<string, string>() {
                {"ExitingVars01","should not be included 01" },
                {"NewVar 01","this goes in 01" },
                {"ExitingVars02","should not be included 02" },
                {"NewVar 02","this goes in 02" },
            };

            var result = WorkflowHelper.MergeWorkflowVariableLists(existingWorkflowVariablesList, additionalListOfWorkflowVariables);
            Assert.AreEqual(6, result.Count());

        }
    }
}
