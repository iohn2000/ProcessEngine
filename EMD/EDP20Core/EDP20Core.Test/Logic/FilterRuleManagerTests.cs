using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;

using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using NSubstitute;
using System.Diagnostics;

namespace EDP20Core.Test.Logic
{
    [TestFixture, Category("Logic"), Category("Filter")]
    public class FilterRuleManagerTests
    {
        public static List<EMDFilterRule> testRules;

        [TestCase("ENTE_d063bd6639584080a163ee72a0f713d6", true)]
        public void Filter_withMockupAndInheritence_Test(string enteGuid, bool expectedResult)
        {
            string objGuid = "mockery";
                        
            string enterpriseRoot = "ENTE_1c0b8655bf834740b65a4931885bec2a";

            //EMDEnterprise enteA = this.createEnterprise(7777,7777,enterpriseRoot,enterpriseRoot,"FirmaA", "FirmaA");
            //EMDEnterprise enteASub1 = this.createEnterprise(7778, 7778, enteA.Guid , enterpriseRoot, "FirmaASub1", "FirmaASub1");
            //EMDEnterprise enteASub1A = this.createEnterprise(7779, 7779, enteASub1.Guid, enterpriseRoot, "FirmaASub1A", "FirmaASub1A");
            //EMDEnterprise enteASub1B = this.createEnterprise(7780, 7780, enteASub1.Guid, enterpriseRoot, "FirmaASub1B", "FirmaASub1B");
            //EMDEnterprise enteASub2 = this.createEnterprise(7781, 7781, enteA.Guid, enterpriseRoot, "FirmaASub2", "FirmaASub2");

            testRules = new List<EMDFilterRule>();

            //testRules.Add(this.createRule(objGuid, 0, null, null, null, null, null, "denyall"));
            //testRules.Add(this.createRule(objGuid, 1, "allow", enteA.Guid, null, null, null, null));

            testRules.Add(this.createRule(objGuid, 0, null, null, null, null, null, "denyall"));
            testRules.Add(this.createRule(objGuid, 1, "allow", "ENTE_d063bd6639584080a163ee72a0f713d6", null, null, null, null));


            EnterpriseHandler eh = new EnterpriseHandler();
            //eh.DeleteObject(enteA, false, true, null);
            //eh.DeleteObject(enteASub1, false, true, null);
            //eh.DeleteObject(enteASub1A, false, true, null);
            //eh.DeleteObject(enteASub1B, false, true, null);

            //var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            //MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            //FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);
            ////Use testManager for tests
            //MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());


            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);
            //Use testManager for tests
            MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());


            //bool result = firuMgr.CheckRule( ,new FilterCriteria(enteGuid, null, null, null));
            //bool result = firuMgr.CheckRule(objGuid,enteGuid,null,null,null,null );
            //bool result = firuMgr.CheckRule(objGuid,new FilterCriteria(enteGuid, null, null, null, null));
            bool result = firuMgr.CheckRule(objGuid, new FilterCriteria(enteGuid, "","",""));
            Assert.AreEqual(expectedResult, result);


        }

        private EMDEnterprise createEnterprise(int e_ID, int e_ID_new, string parentGuid, string rootGuid, string nameLong, string nameShort)
        {
            EnterpriseManager em = new EnterpriseManager();
            EMDEnterprise ente = new EMDEnterprise();
            ente.E_ID = e_ID;
            ente.E_ID_new = e_ID_new;
            ente.Guid_Parent = parentGuid;
            ente.Guid_Root = rootGuid;
            ente.NameLong = nameLong;
            ente.NameShort = nameShort;
            EnterpriseHandler eh = new EnterpriseHandler();
            em.Create(ente);
            return em.Get(ente.Guid);
        }

        [TestCase("user3", true)]
        [TestCase("user1", false)]
        public void Filter_withMockup_Test(string userGuid, bool expectedResult)
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

            
            bool result = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", new List<string>() { userGuid }));
            Assert.AreEqual(expectedResult, result);
        }

        static private object[][] testCases = new[] { new object[]  { new[] { "user6", "user7", "user8", "user1", "user11", "user12", "user23" }, true},
                                                      new object[]  { new[] { "user1", "user2", "user4", "user5", "user10", "user11", "user12" }, false},
                                                      new object[]  { new[] { "user6", "user7", "user8", "user31", "user32", "user33", "user23" }, true}
                                                      };

        [Test, TestCaseSource("testCases"), Category("Logic"), Category("Filter")]
        public void Filter_multipleUSTY_manual_TEST(string[] usertypeList, bool expectedResult)
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

            //List<string> ustyInput = new List<string>() { "user6", "user7", "user8", "user1", "user11", "user12", "user23" };

            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);
            //Use testManager for tests
            MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());

            bool result = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (string usty in usertypeList.ToList())
            {
                bool r = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", new List<string>() { usty }));
                if (r) result = true;
            }
            sw.Stop();
            Console.WriteLine("Zeit verbraten = {0}", sw.ElapsedMilliseconds);

            Assert.AreEqual(expectedResult, result);

        }

        static private object[][] testCases_Filter_multipleUSTY_ALLBUT_TEST = new[] { new object[]  { new[] { "user6", "user7", "user8", "user1", "user11", "user12", "user23" }, false},
                                                                                      new object[]  { new[] { "user6", "user8", "user23" }, true} };
        

    [Test, TestCaseSource("testCases_Filter_multipleUSTY_ALLBUT_TEST"), Category("Logic"), Category("Filter")]
        public void Filter_multipleUSTY_ALLBUT_TEST(string[] usertypeList, bool expectedResult)
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
            
            result = firuMgr.CheckRule(objGuid, "ente", "loca", "acco", "emty", usertypeList.ToList());
            Assert.AreEqual(expectedResult, result);

        }

        static private object[][] testCases_Filter_multipleUSTY_TEST = new[] { new object[]  { new[] { "user6", "user7", "user1"}, true},
                                                                               new object[]  { new[] { "user6", "user7", "userxx" }, false} };

        [Test, TestCaseSource("testCases_Filter_multipleUSTY_TEST"), Category("Logic"), Category("Filter")]
        public void Filter_multipleUSTY_TEST(string[] usertypeList, bool expectedResult)
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

            result = firuMgr.CheckRule(objGuid, "ente", "loca", "acco", "emty", usertypeList.ToList());
            Assert.AreEqual(expectedResult, result);
        }

        static private object[][] testCases_Filter_AllowAll_Test = new[] { new object[]  { new[] { "user3"}, true},
                                                                           new object[]  { new[] { "user1"}, true},
                                                                           new object[]  { new[] { "asdf"}, true}};

        [Test, TestCaseSource("testCases_Filter_AllowAll_Test"), Category("Logic"), Category("Filter")]
        public void Filter_AllowAll_Test(string[] usertypeList, bool expectedResult)
        {
            string objGuid = "mockery";
            testRules = new List<EMDFilterRule>();

            testRules.Add(this.createRule(objGuid, 0, "allowall", null, null, null, null, null));


            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();
            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager("TEST_guid", MockFilterRuleHandler);
            //Use testManager for tests
            MockFilterRuleHandler.Received().ReadRulesFromDatase(Arg.Any<string>());

            bool result = firuMgr.CheckRule(objGuid, new FilterCriteria("ente", "loca", "acco", "emty", usertypeList.ToList()));
            Assert.AreEqual(expectedResult, result);
        }

        static private object[][] testCases_NoRuleforUserTypeFilter_butCriteriaExist = new[] { new object[]  { new[] { "user1", "user2", "user3"}, "ente01", true},
                                                                                               new object[]  { new[] { "user6", "user7", "userxx" }, "ente04", false} };

        [Test, TestCaseSource("testCases_NoRuleforUserTypeFilter_butCriteriaExist"), Category("Logic"), Category("Filter")]
        public void NoRuleforUserTypeFilter_butCriteriaExist(string[] usertypeList, string enteGuid, bool expectedResult)
        {
            bool result;
            string objGuid = "mockery";
            List<string> ustyInput;
            var MockFilterRuleHandler = Substitute.For<IFilterRuleHandler>();

            testRules = new List<EMDFilterRule>();
            testRules.Add(this.createRule(objGuid, 0, FilterManager.CONST_DENYALL, null, null, null, null, null));
            testRules.Add(this.createRule(objGuid, 1, FilterManager.CONST_ALLOW, "ente01", null, null, null, null));
            testRules.Add(this.createRule(objGuid, 2, FilterManager.CONST_ALLOW, "ente02", null, null, null, null));
            testRules.Add(this.createRule(objGuid, 3, FilterManager.CONST_ALLOW, "ente03", null, null, null, null));

            MockFilterRuleHandler.ReadRulesFromDatase(Arg.Any<string>()).Returns(testRules);
            FilterManager firuMgr = new FilterManager(objGuid, MockFilterRuleHandler);

            ustyInput = new List<string>() { "user1", "user2", "user3" };
            //ustyInput = new List<string>() { "user1" };
            result = firuMgr.CheckRule(objGuid, enteGuid, "", "", "", ustyInput);
            Assert.AreEqual(expectedResult, result);
        }

        private EMDFilterRule createRule(string objGuid, int order, string filterAction, string eGuid, string lGuid, string etGuid, string accoGuid, string userGuid, bool enteIsInherited = true)
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
            r.EnteIsInherited = enteIsInherited;
            return r;
        }
    }
}
