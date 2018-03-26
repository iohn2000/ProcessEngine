using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.ProcessEngine.WFActivity;
using Kapsch.IS.ProcessEngine;
using FluentValidation.Results;
using System.Collections.Generic;

namespace TESTING
{
    [TestClass]
    public class ActivityValidation_TEST
    {
    //    [TestMethod]
    //    [TestCategory("Validation")]
    //    public void TestAuditLogValidationRules()
    //    {
    //        /* BASE
    //        <activity nr="1" id="Kapsch.IS.ProcessEngine.WFActivity.AuditLogActivity" instance="1.AuditLogActivity" maxOutgoingConditions="0" editConditions="false">
	   //         <name>log who requested WF</name>
	   //         <properties>
		  //          <property name="loggText" direction="input"  dataType="stringType"/>
	   //         </properties>
	   //         <transition to="2.DecisionActivity">
    //                <transition>true</transition>
    //            </transition>
    //        </activity>
    //        */

    //        EvaluationDecisionActivityValidator auditValidator = new EvaluationDecisionActivityValidator();

    //        // nr missing, 
    //        // property name wrong
    //        // transistion to empty
    //        // no conditions allowed on outgoing trans
    //        Activity baseA = new Activity("<activity id=\"Kapsch.IS.ProcessEngine.WFActivity.AuditLogActivity\" instance=\"1.AuditLogActivity\"><name>log who requested WF</name><properties><property name=\"loggText\" direction=\"output\"  dataType=\"stringType\"/></properties><transition to=\"\"><condition>true</condition></transition></activity>");

    //       ValidationResult result = auditValidator.Validate(baseA);

    //        if (result.IsValid)
    //            Assert.Fail("failure expected ...");

    //        // wrong parameter direction
    //        baseA = new Activity("<activity nr=\"3\" id=\"Kapsch.IS.ProcessEngine.WFActivity.AuditLogActivity\" instance=\"1.AuditLogActivity\"><name>log who requested WF</name><properties><property name=\"logText\" direction=\"output\" dataType=\"stringType\"/></properties><transition to=\"2.DecisionActivity\" /></activity>");
    //        ValidationResult r2 = auditValidator.Validate(baseA);

    //        if (r2.IsValid)
    //            Assert.Fail("failure wrong direction expected");

    //    }
    }
}
