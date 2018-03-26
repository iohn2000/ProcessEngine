using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using System.Linq;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Utils;

namespace EDP20Core_UnitTest
{
    /// <summary>
    /// Summary description for ContactTest
    /// </summary>
    [TestClass]
    public class ContactTests
    {
        public ContactTests()
        {
            //
            // TODO: Add constructor logic here
            //
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

        private const string TEST_EMPL_GUID = "TEST_Contact Tests 123456789";

        [TestMethod]
        public void TestCreateInFuture()
        {
            using (CoreTransaction transaction = new CoreTransaction())
            using (EmploymentHandler emplH = new EmploymentHandler(transaction, null, "TestCreateInFuture"))
            using (ContactHandler contH = new ContactHandler(transaction, null, "TestCreateInFuture"))
            {
                ContactManager contM = new ContactManager(transaction, null, "TestCreateInFuture");
                try
                {
                    transaction.Begin();
                    EMDEmployment testEmpl = CreateAndWriteTestEmpl(emplH);

                    #region Test add future with no existing contact

                    EMDContact fCont0 = CreateTestRoom(contM, testEmpl);
                    fCont0.ActiveFrom = DateTime.Now.AddMonths(1);
                    fCont0 = contM.WriteOrModifyContact(fCont0);

                    List<IEMDObject<EMDContact>> conts = contH.GetObjectsForEmployment(testEmpl.Guid, true);
                    Assert.AreEqual(1, conts.Count);
                    Assert.AreEqual(EMDContact.INFINITY, conts.First().ActiveTo);

                    #endregion

                    #region Test add future with one existing

                    EMDContact fCont1 = CreateTestRoom(contM, testEmpl);
                    fCont1.ActiveFrom = DateTime.Now.AddMonths(2);
                    fCont1 = contM.WriteOrModifyContact(fCont1);

                    conts = contH.GetObjectsForEmployment(testEmpl.Guid, true);
                    Assert.AreEqual(2, conts.Count);

                    #endregion

                    #region Test replace on WriteOrModify

                    EMDContact fCont2 = CreateTestRoom(contM, testEmpl);
                    fCont2.ActiveFrom = DateTime.Now.AddMonths(2).AddDays(1);
                    fCont2 = contM.WriteOrModifyContact(fCont2);

                    conts = contH.GetObjectsForEmployment(testEmpl.Guid, true);
                    Assert.AreEqual(2, conts.Count);

                    EMDContact current = conts.Cast<EMDContact>().Where(c => c.Guid == fCont0.Guid).Single();

                    Assert.IsTrue(DateTimeHelper.IsDateTimeEqual(fCont2.ActiveFrom, current.ActiveTo, 2500)); //Validity-Shift (2000 ms) + 500 ms tolerance
                    #endregion

                    #region Test GetObjectsForEmployment futurize = false

                    conts = contH.GetObjectsForEmployment(testEmpl.Guid, false);
                    Assert.AreEqual(0, conts.Count);

                    #endregion

                    #region Test add contact with other type

                    EMDContact contMobile = CreateTestMobile(contM, testEmpl);
                    contMobile = contM.WriteOrModifyContact(contMobile);

                    conts = contH.GetObjectsForEmployment(testEmpl.Guid, true);
                    Assert.AreEqual(3, conts.Count);
                    Assert.AreEqual(2, conts.Cast<EMDContact>().Where(c => c.C_CT_ID == ContactTypeHandler.ROOM).Count());

                    #endregion
                }
                finally
                {
                    transaction.Rollback();
                }
            }
        }

        private EMDContact CreateTestRoom(ContactManager cm, EMDEmployment empl)
        {
            EMDContact cont = new EMDContact();
            cont.EP_Guid = empl.Guid;
            cont.Text = "Room for TestCreateInFuture";
            cont.E_Guid = "ENTE_08d82c0b282845af9220f3ff41d78109";

            cont.C_ID = 666;
            cont.C_EP_ID = 2000670;
            cont.C_E_ID = 666;

            return cm.CreateContactRoom(cont);
        }

        private EMDContact CreateTestMobile(ContactManager cm, EMDEmployment empl)
        {
            EMDContact cont = new EMDContact();
            cont.EP_Guid = empl.Guid;
            cont.Text = "Mobile for TestCreateInFuture";
            cont.E_Guid = "ENTE_08d82c0b282845af9220f3ff41d78109";

            cont.C_ID = 666;
            cont.C_EP_ID = 2000670;
            cont.C_E_ID = 666;

            return cm.CreateContactMobile(cont);
        }

        private EMDEmployment CreateAndWriteTestEmpl(EmploymentHandler h)
        {
            EMDEmployment empl = new EMDEmployment();

            //Use random values of existing data for dependencies
            empl.P_Guid = "PERS_4bdfbaa14e6747ab92360232235a7925";
            empl.P_ID = 7997;
            empl.ET_Guid = "EMTY_82d3847b57ea4be1a0212872fcaf8ef8";
            empl.ET_ID = 1;
            empl.DGT_Guid = "DIST_76dbad87706241caa8158520450516b3";
            empl.EP_ID = 666;
            empl.PersNr = "666";

            return h.CreateObject(empl, TEST_EMPL_GUID) as EMDEmployment;
        }
    }
}
