using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TESTING
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class FleeEvaluator_TEST
    {
        public FleeEvaluator_TEST()
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

        [TestMethod, TestCategory("FleeExpression")]
        public void Nested_CurlyBracket_TEST()
        {
            ExpressionEvaluator flee = new ExpressionEvaluator();
            SortedList<string, Variable> wfVariables = new SortedList<string, Variable>(StringComparer.OrdinalIgnoreCase);

            wfVariables.Add("0.age", new Variable("0.age", "11", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("0.name", new Variable("0.name", "sepp", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("0.Level1", new Variable("0.Level1", "\"Your name is \" + {{0.name}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("0.Level2", new Variable("0.Level2", "\"You are \" + {{0.age}} + \" old. \" + {{0.Level1}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            object oL0 = flee.Evaluate("{{0.age}}", wfVariables);
            Assert.AreEqual("11", oL0.ToString());
            object oL1 = flee.Evaluate("{{0.Level1}}", wfVariables);
            Assert.AreEqual("Your name is sepp", oL1.ToString());
            object oL2 = flee.Evaluate("\"You are \" + {{0.age}} + \" old. \" + {{0.Level1}}", wfVariables);
            Assert.AreEqual("You are 11 old. Your name is sepp", oL2.ToString());
        }

        [TestMethod, TestCategory("FleeExpression")]
        public void VerschachtelteWfVariables_TEST()
        {
            ExpressionEvaluator flee = new ExpressionEvaluator();
            SortedList<string, Variable> wfVariables = new SortedList<string, Variable>(StringComparer.OrdinalIgnoreCase);

            wfVariables.Add("a", new Variable("a", "aa", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("b", new Variable("b", "{{x}} + \" Belle \"  + {{m}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("x", new Variable("x", "{{y}} + \" zzz \"", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("m", new Variable("m", "mm", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("y", new Variable("y", "yy", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("t", new Variable("t", "{{a}} + {{b}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            object o = flee.Evaluate(wfVariables["t"].VarValue, wfVariables);

            Assert.AreEqual("aayy zzz  Belle mm", o.ToString());

        }

        [TestMethod, TestCategory("FleeExpression")]
        public void AtAt_Syntax_TEST()
        {
            ExpressionEvaluator flee = new ExpressionEvaluator();
            SortedList<string, Variable> wfVariables = new SortedList<string, Variable>(StringComparer.OrdinalIgnoreCase);




            wfVariables.Add("0.EffectedPersonEmployment", new Variable("0.EffectedPersonEmployment", "EMPL_asdfasdfadsf", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("0.atat", new Variable("0.atat", "\"FamilyName@@P_Guid@@\" + {{0.EffectedPersonEmployment}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));


            object o = flee.Evaluate("\"FamilyName@@P_Guid@@\" + {{0.EffectedPersonEmployment}}", wfVariables);
            Assert.AreEqual("FamilyName@@P_Guid@@EMPL_asdfasdfadsf", o.ToString());

            object oo = flee.Evaluate("{{0.atat}}", wfVariables);
            Assert.AreEqual("FamilyName@@P_Guid@@EMPL_asdfasdfadsf", oo.ToString());

        }

        [TestMethod, TestCategory("FleeExpression")]
        public void BuildFleeExpression_FromWorkflowActivity_TEST()
        {
            ExpressionEvaluator flee = new ExpressionEvaluator();
            SortedList<string, Variable> wfVariables = new SortedList<string, Variable>(StringComparer.OrdinalIgnoreCase);

            wfVariables.Add("0.requestor", new Variable("0.requestor", "Alex Tischer", EnumVariablesDataType.stringType, EnumVariableDirection.both));
            wfVariables.Add("0.kosten", new Variable("0.kosten", "200", EnumVariablesDataType.doubleType, EnumVariableDirection.both));
            wfVariables.Add("0.faktor", new Variable("0.faktor", "3", EnumVariablesDataType.doubleType, EnumVariableDirection.both));

            string expre = "\"Requestor is \" + {{0.requestor}}";
            object o = flee.Evaluate(expre, wfVariables);
            Assert.AreEqual("Requestor is Alex Tischer", o.ToString());

            expre = "{{0.kosten}} * {{0.faktor}}";
            object oo = flee.Evaluate(expre, wfVariables);
            Assert.AreEqual("600", oo.ToString());

            expre = "{{0.kosten}} * {{0.faktor}}";
        }


        [TestMethod, TestCategory("FleeExpression")]
        public void Dates_TEST()
        {
            ExpressionEvaluator flee = new ExpressionEvaluator();
            SortedList<string, Variable> wfVariables = new SortedList<string, Variable>(StringComparer.OrdinalIgnoreCase);

            wfVariables.Add("0.date",
                new Variable("0.date", "\"2015-01-12 15:33:44\"", EnumVariablesDataType.dateType, EnumVariableDirection.both));

            object r0 = flee.Evaluate(wfVariables["0.date"].VarValue, wfVariables);
            Assert.AreEqual("2015-01-12 15:33:44", r0.ToString());
            Debug.WriteLine("r0 passed");

        }
        [TestMethod, TestCategory("FleeExpression")]
        public void Quotes_TEST()
        {
            ExpressionEvaluator flee = new ExpressionEvaluator();
            SortedList<string, Variable> wfVariables = new SortedList<string, Variable>(StringComparer.OrdinalIgnoreCase);

            wfVariables.Add("0.eqName",
                new Variable("0.eqName", "\"Visio lokal installiert\"", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfVariables.Add("0.requestorOhne",
                new Variable("0.requestorOhne", "\"alex_tischer@kapsch.net\"", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfVariables.Add("0.sender",
                new Variable("0.sender", "{{0.requestorOhne}}", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            wfVariables.Add("0.subject",
                new Variable("0.subject", "{{0.requestorOhne}} + \" hat ein EQ Namens : '\" + {{0.eqName}} + \"' angefordert.\"", EnumVariablesDataType.stringType, EnumVariableDirection.both));

            object r0 = flee.Evaluate(wfVariables["0.requestorOhne"].VarValue, wfVariables);
            Assert.AreEqual("alex_tischer@kapsch.net", r0.ToString());
            Debug.WriteLine("r0 passed");

            object r1 = flee.Evaluate(wfVariables["0.requestorOhne"].VarValue, wfVariables);
            Assert.AreEqual("alex_tischer@kapsch.net", r1.ToString(), "direkt string with quotes");
            Debug.WriteLine("r1 passed");

            object r2 = flee.Evaluate(wfVariables["0.sender"].VarValue, wfVariables);
            Assert.AreEqual("alex_tischer@kapsch.net", r2.ToString(), "variable with email and quotes");
            Debug.WriteLine("r2 passed");

            object r3 = flee.Evaluate(wfVariables["0.subject"].VarValue, wfVariables);
            Assert.AreEqual("alex_tischer@kapsch.net hat ein EQ Namens : 'Visio lokal installiert' angefordert.", r3.ToString(), "expression with strings and variable concat");
            Debug.WriteLine("r3 passed");
        }

        [TestMethod, TestCategory("FleeExpression")]
        public void Boolean_Test()
        {SortedList<string, Variable> wfVariables = new SortedList<string, Variable>(StringComparer.OrdinalIgnoreCase);
            ExpressionEvaluator flee = new ExpressionEvaluator();
            object x = flee.Evaluate("1=1 AND \"hallo\"=\"hallo\"",wfVariables);

        }

    }
}
