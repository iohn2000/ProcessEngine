using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDPWfActivities_UnitTest.NavTicketSrvTEST;
using System.Collections.Generic;
using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System.Xml.Linq;
using Kapsch.IS.EDP.WFActivity;
using Kapsch.IS.EDP.Core.Entities;
using System.Linq;

namespace EDPWfActivities_UnitTest
{
    [TestClass]
    public class WfExpressions
    {

        Tuple<EMDEmployment, EMDPerson> fleck;
        Tuple<EMDEmployment, EMDPerson> stagl;



        [TestInitialize]
        public void initStuff()
        {
            fleck = this.getMainEmplforPers(11162);
            stagl = this.getMainEmplforPers(5);

        }



        [TestMethod, TestCategory("WorkflowExpression")]
        public void BasicExpressionTest_withString_noAtAt()
        {

            SortedList<string, Variable> wfList = new SortedList<string, Variable>();
            wfList.Add("1.kapsch", new Variable("1.kapsch", @"kapsch", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfList.Add("1.bcom", new Variable("1.bcom", @"bcom", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.concat", new Variable("1.concat", @"""kapsch"" + "" businessCom""", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfList.Add("1.concatVars", new Variable("1.concatVars", @"""{{1.kapsch}}"" + "" "" + ""{{1.bcom}}""", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfList.Add("1.firma", new Variable("1.firma", @" ""Ich arbeite in der Firma: "" + {{1.kapsch}} + "" "" + {{1.bcom}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfList.Add("1.singleVar", new Variable("1.singleVar", @"""{{1.bcom}}""", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            //outputWorkflowVariables(wfList);

            WorkflowModel wfModel = new WorkflowModel();
            wfModel.LoadModelXml(@"<workflow id=""WODE_9dbfbd14fc36446182b908a2f8f5cc4a"" dataHelperName="""" name=""EQDE_ADD_CAT2_ONBOARDING"" status=""NotStarted"" demoMode=""true""></workflow>");
            EngineContext engineContext = new EngineContext("intanceid",
                wfModel,
                new ExecutionIteration(new XElement("test")),
                new Activity(@"<activity nr=""1"" id=""Kapsch.IS.ProcessEngine.WFActivity.StartTimed.StartTimedActivity"" instance=""1.StartTimedActivity""></activity>"),
                new Transition(new XElement("test")),
                wfList,
                EnumWorkflowInstanceStatus.Executing,
                Guid.NewGuid()
                );



            wfModel.UpdateWorkflowVariableInXmlModel("0.gustav","ich & du",EnumVariablesDataType.stringType,EnumVariableDirection.both);

            


            EdpFeatures edpfeatures = EdpFeatures.GetInstance();
            Variable result;
            result = edpfeatures.GetProcessedActivityVariable(engineContext, "kapsch");
            Assert.AreEqual("kapsch", result.VarValue);

            result = edpfeatures.GetProcessedActivityVariable(engineContext, "concat");
            Assert.AreEqual("kapsch businessCom", result.VarValue);

            result = edpfeatures.GetProcessedActivityVariable(engineContext, "vars");
            System.Diagnostics.Trace.WriteLine(result.Name + ": " + result.VarValue);
            Assert.AreEqual("kapsch", result.VarValue);

            result = edpfeatures.GetProcessedActivityVariable(engineContext, "concatVars");
            System.Diagnostics.Trace.WriteLine(result.Name + ": " + result.VarValue);
            Assert.AreEqual("kapsch bcom", result.VarValue);

            result = edpfeatures.GetProcessedActivityVariable(engineContext, "firma");
            System.Diagnostics.Trace.WriteLine(result.Name + ": " + result.VarValue);
            Assert.AreEqual("Ich arbeite in der Firma: kapsch bcom", result.VarValue);

            result = edpfeatures.GetProcessedActivityVariable(engineContext, "singleVar");
            System.Diagnostics.Trace.WriteLine(result.Name + ": " + result.VarValue);
            Assert.AreEqual("bcom", result.VarValue);
        }


        [TestMethod, TestCategory("WorkflowExpression")]
        public void Basic_AtAt_ExpressionTest_withString()
        {
            Assert.AreEqual("4.29", "this test does nothing");
        }

        [TestMethod, TestCategory("BoolscheExpressions")]
        public void BoolscheExpression()
        {
            SortedList<string, Variable> wfList = new SortedList<string, Variable>();
            wfList.Add("1.approvalDecision",
                new Variable("1.approvalDecision", @"""approved""", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.approvedString",
                new Variable("1.approvedString", @"""approved""", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.declinedString",
                new Variable("1.declinedString", @"""declined""", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult1",
                new Variable("1.conditionResult1", @"1 = 1", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult2",
                new Variable("1.conditionResult2", @"1 > 3", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult25",
                new Variable("1.conditionResult25", @"1 <> 3", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult3",
                new Variable("1.conditionResult3", @"""left"" = ""right""", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult4",
                new Variable("1.conditionResult4", @"""left"" = ""left""", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult5",
                new Variable("1.conditionResult5", @"{{1.approvalDecision}} = ""approved""", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult6",
                new Variable("1.conditionResult6", @"{{1.approvalDecision}} = {{1.declinedString}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfList.Add("1.conditionResult7",
                new Variable("1.conditionResult7", @"{{1.approvalDecision}} <> {{1.declinedString}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            outputWorkflowVariables(wfList);

            EngineContext engineContext = new EngineContext("intanceid",
                new WorkflowModel(), new ExecutionIteration(new XElement("test")), new Activity(@"<ac nr=""1""></ac>"),
                new Transition(new XElement("test")), wfList, EnumWorkflowInstanceStatus.Executing, Guid.NewGuid());

            EdpFeatures feature = EdpFeatures.GetInstance();
            Variable result;
            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult1");
            Assert.AreEqual("True", result.VarValue);

            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult2");
            Assert.AreEqual("False", result.VarValue);

            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult25");
            Assert.AreEqual("True", result.VarValue);

            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult3");
            Assert.AreEqual("False", result.VarValue);

            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult4");
            Assert.AreEqual("True", result.VarValue);

            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult5");
            Assert.AreEqual("True", result.VarValue);

            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult6");
            Assert.AreEqual("False", result.VarValue);

            result = feature.GetProcessedActivityVariable(engineContext, "conditionResult7");
            Assert.AreEqual("True", result.VarValue);
        }

        private static void outputWorkflowVariables(SortedList<string, Variable> wfList)
        {
            foreach (var item in wfList)
            {
                System.Diagnostics.Trace.WriteLine(item.Key+": "+item.Value.VarValue);
                Console.WriteLine("{0} = {1}", item.Key, item.Value.VarValue);
            }            
        }

        #region helpers
        private Tuple<EMDEmployment, EMDPerson> getMainEmplforPers(int pId)
        {
            PersonHandler persH = new PersonHandler();
            var pers = (EMDPerson) persH.GetPersonByP_Id(pId);

            EmploymentHandler emplH = new EmploymentHandler();

            var mainEmpl = emplH.GetMainEmploymentForPerson(pers.Guid);

            return new Tuple<EMDEmployment, EMDPerson>(mainEmpl, pers);
        }


        #endregion
    }
}
