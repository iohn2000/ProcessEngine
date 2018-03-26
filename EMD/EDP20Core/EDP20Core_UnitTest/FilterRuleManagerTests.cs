using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace EDP20Core_UnitTest
{
    /// <summary>
    /// Summary description for FilterRuleManagerTests
    /// </summary>
    [TestClass]
    public class FilterRuleManagerTests
    {
        public FilterRuleManagerTests()
        {
        }

        public static List<EMDFilterRule> testRules;

        [TestInitialize]
        public void TestInit()
        {
            //testRules.Add(new EMDFilterRule());
            //Add rules that are the "Result" of the faked call
        }

        [TestMethod, TestCategory("Filter")]
        public void Filter_withMockup_Test()
        {
            string objGuid = "mockery";
            testRules = new List<EMDFilterRule>();

            testRules.Add(this.createRule(objGuid, 0, null, null, null, null, null, "allowall"));
            testRules.Add(this.createRule(objGuid, 1, "deny", null, null, null, null, "user1"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user2"));

            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager("TEST_guid", MockFilterRuleHandler);
            //Use testManager for tests
            MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());

            bool result = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", new List<string>() { "user3" }));
            Assert.AreEqual(true, result);

            result = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", new List<string>() { "user1" }));
            Assert.AreEqual(false, result);

        }


        [TestMethod, TestCategory("Filter")]
        public void Filter_multipleUSTY_manual_TEST()
        {
            string objGuid = "mockery";
            testRules = new List<EMDFilterRule>();

            testRules.Add(this.createRule(objGuid, 0, null, null, null, null, null, "allowall"));
            testRules.Add(this.createRule(objGuid, 1, "deny", null, null, null, null, "user1"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user2"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user4"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user5"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user10"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user11"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user12"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user13"));

            List<string> ustyInput = new List<string>() { "user6", "user7", "user8", "user1", "user11", "user12", "user23" };

            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);
            //Use testManager for tests
            MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());

            bool result = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (string usty in ustyInput)
            {
                bool r = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", new List<string>() { usty }));
                if (r) result = true;
            }
            sw.Stop();
            Console.WriteLine("Zeit verbraten = {0}", sw.ElapsedMilliseconds);

            Assert.AreEqual(true, result);

        }

        [TestMethod, TestCategory("Filter")]
        public void Filter_multipleUSTY_ALLBUT_TEST()
        {
            List<string> ustyInput;
            string objGuid = "mockery";
            testRules = new List<EMDFilterRule>();

            testRules.Add(this.createRule(objGuid, 0, null, null, null, null, null, "allowall"));
            testRules.Add(this.createRule(objGuid, 1, "deny", null, null, null, null, "user1"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user2"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user4"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user5"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user10"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user11"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user12"));
            testRules.Add(this.createRule(objGuid, 2, "deny", null, null, null, null, "user13"));


            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);
            //Use testManager for tests
            MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());

            bool result = false;

            ustyInput = new List<string>() { "user6", "user7", "user8", "user1", "user11", "user12", "user23" };
            result = firuMgr.CheckRule(objGuid, "ente", "loca", "acco", "emty", ustyInput);
            Assert.AreEqual(false, result);

            ustyInput = new List<string>() { "user6", "user8", "user23" };
            result = firuMgr.CheckRule(objGuid, "ente", "loca", "acco", "emty", ustyInput);
            Assert.AreEqual(true, result);


        }

        [TestMethod, TestCategory("Filter")]
        public void Filter_multipleUSTY_TEST()
        {
            bool result;
            string objGuid = "mockery";
            List<string> ustyInput;
            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();



            testRules = new List<EMDFilterRule>();
            testRules.Add(this.createRule(objGuid, 0, null, null, null, null, null, FilterManager.CONST_DENYALL));
            testRules.Add(this.createRule(objGuid, 1, FilterManager.CONST_ALLOW, null, null, null, null, "user1"));
            testRules.Add(this.createRule(objGuid, 2, FilterManager.CONST_ALLOW, null, null, null, null, "user2"));
            testRules.Add(this.createRule(objGuid, 3, FilterManager.CONST_ALLOW, null, null, null, null, "user3"));

            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);

            ustyInput = new List<string>() { "user6", "user7", "user1" };
            result = firuMgr.CheckRule(objGuid, "ente", "loca", "acco", "emty", ustyInput);
            Assert.AreEqual(true, result);

            ustyInput = new List<string>() { "user6", "user7", "userXX" };
            result = firuMgr.CheckRule(objGuid, "ente", "loca", "acco", "emty", ustyInput);
            Assert.AreEqual(false, result);

        }

        [TestMethod, TestCategory("Filter")]
        public void Filter_AllowAll_Test()
        {
            string objGuid = "mockery";
            testRules = new List<EMDFilterRule>();

            testRules.Add(this.createRule(objGuid, 0, "allowall", null, null, null, null, null));


            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager("TEST_guid", MockFilterRuleHandler);
            //Use testManager for tests
            MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());

            bool result = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", new List<string>() { "user3" }));
            Assert.AreEqual(true, result);

            result = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", new List<string>() { "user1" }));
            Assert.AreEqual(true, result);

            result = firuMgr.CheckRule(objGuid, new FilterCriteria("cxc", "ewg", "asfd", "emasgty", new List<string>() { "asdf" }));
            Assert.AreEqual(true, result);

        }

        [TestMethod, TestCategory("Filter")]
        public void NoRuleforUserTypeFilter_butCriteriaExist()
        {
            bool result;
            string objGuid = "mockery";
            List<string> ustyInput;
            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();



            testRules = new List<EMDFilterRule>();
            testRules.Add(this.createRule(objGuid, 0, FilterManager.CONST_DENYALL, null, null, null, null, null));
            testRules.Add(this.createRule(objGuid, 1, FilterManager.CONST_ALLOW, "ente01", null, null, null,null));
            testRules.Add(this.createRule(objGuid, 2, FilterManager.CONST_ALLOW, "ente02", null, null, null, null));
            testRules.Add(this.createRule(objGuid, 3, FilterManager.CONST_ALLOW, "ente03", null, null, null, null));

            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);

            ustyInput = new List<string>() { "user1", "user2", "user3" };
            //ustyInput = new List<string>() { "user1" };
            result = firuMgr.CheckRule(objGuid, "ente01", "", "", "", ustyInput);
            Assert.AreEqual(true, result);

            result = firuMgr.CheckRule(objGuid, "ente04", "", "", "", ustyInput);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestWithoutMock()
        {
            FilterManager fm = new FilterManager("EQDE_ff2821f95d2841d387b158564fb80e84"); // obre instance OBRE_e4bfea7b6c6d4100ba847d8bef556db7


            // test filter 
            // ente = kcc-at                      : ENTE_9d1d6d4a3b4a40fdb49c3bae354f8af0
            // loca = building H                  : LOCA_2166ff9598c44ccd8e422d00e7afe995
            // emty = apprentice                  : EMTY_201c70ed10374bfbbc1ffd23f2297d88
            // acco = 204250 - KCC Lehrlinge kfm  : ACCO_95367b8c049d495e9919e05d01b34520
            // usertype = full                    : 
            //bool isAllowed = fm.CheckRule(
            //    "ENTE_08d82c0b282845af9220f3ff41d78109",
            //    "LOCA_c0a770a607e44787856c8eaa786b8852",
            //    "EMTY_82d3847b57ea4be1a0212872fcaf8ef8",
            //    "ACCO_56647e0cd6ee43da9fa44e90689f6599",
            //    "ADUserFullAccount");


            bool isAllowed = fm.CheckRule(
    "xx",
    "cc",
    "vv",
    "vv",
    "ADUserFullAccount");

            Assert.IsTrue(isAllowed);
        }

        private EMDFilterRule createRule(string objGuid, int order, string filterAction, string eGuid, string lGuid, string etGuid, string accoGuid, string userGuid)
        {
            EMDFilterRule r = new EMDFilterRule();
            r.Obj_Guid = objGuid;
            r.FilterAction = filterAction;
            r.E_Guid = eGuid;
            r.L_Guid = lGuid;
            r.ET_Guid = etGuid;
            r.ACC_Guid = accoGuid;
            r.USTY_Enum = userGuid;
            r.FilterOrder = order;
            return r;
        }
    }
}
