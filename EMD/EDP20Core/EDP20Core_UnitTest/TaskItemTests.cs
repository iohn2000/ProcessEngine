using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Entities;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.Logic;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class TaskItemTests
    {
        public const string PersGuidWieserRoland = "PERS_219ff0c4a430482d97d8d0a186f2ce28";
        public const string PersGuidMayerRobert = "PERS_24b9fd4850ef4a448bcbc5a9ecd480fc";
        public string PersGuidLineManager;
        public string PersGuidEmployee;
        public string EmplGuidLineManager;
        public string EmplGuidEmployee;

        public TaskItemTests()
        {
            initUnitTest();
        }

        public void initUnitTest()
        {
            PersGuidEmployee = PersGuidMayerRobert;
            PersGuidLineManager = PersGuidWieserRoland;
            EmploymentManager emplManager = new EmploymentManager();
            EMDEmployment emplEmployee = emplManager.GetMainEploymentForPerson(PersGuidEmployee);
            EMDEmployment emplLineManager = emplManager.GetMainEploymentForPerson(PersGuidLineManager);
            EmplGuidEmployee = emplEmployee.Guid;
            EmplGuidLineManager = emplLineManager.Guid;
        }

        //TODO: Personen dynamisch suchen und Unittest wieder einfügen
        [TestMethod]
        [TestCategory("TaskItem")]
        public void FindTaskApproverForEffectedPerson_TEST()
        {
            // <variable name="4.approver_EmplGUIDs" direction="input" dataType="stringType">EMPL_5aa83bf7345540cf9c0d3d74fca3fd9e</variable> 
            // <variable name="0.EffectedPersonEmploymentGuid" direction="both" dataType="stringType">EMPL_449e9639e0594ca2abb9f044e67d0289</variable>
            TaskItemManager tMgr = new TaskItemManager();
            List<string> apprCodes = new List<string>() { EmplGuidLineManager };
            var result = tMgr.FindTaskApproverForEffectedPerson(apprCodes, EmplGuidEmployee);

            //no exception... passt
        }

        [TestMethod]
        [TestCategory("TaskItem")]
        public void FindTaskApproverFor_CCResponsible_TEST()
        {
            // <variable name="4.approver_EmplGUIDs" direction="input" dataType="stringType">EMPL_5aa83bf7345540cf9c0d3d74fca3fd9e</variable> 
            // <variable name="0.EffectedPersonEmploymentGuid" direction="both" dataType="stringType">EMPL_449e9639e0594ca2abb9f044e67d0289</variable>
            TaskItemManager tMgr = new TaskItemManager();
            List<string> apprCodes = new List<string>() { "KSTL_" + EmplGuidLineManager };
            var result = tMgr.FindTaskApproverForEffectedPerson(apprCodes, EmplGuidEmployee);

            //no exception... passt
        }



        [TestMethod]
        [TestCategory("TaskItem")]
        public void FindTaskApproverFor_RS_TEST()
        {
            // <variable name="4.approver_EmplGUIDs" direction="input" dataType="stringType">EMPL_5aa83bf7345540cf9c0d3d74fca3fd9e</variable> 
            // <variable name="0.EffectedPersonEmploymentGuid" direction="both" dataType="stringType">EMPL_449e9639e0594ca2abb9f044e67d0289</variable>
            TaskItemManager tMgr = new TaskItemManager();
            List<string> apprCodes = new List<string>() { "RS_50360" };
            var result = tMgr.FindTaskApproverForEffectedPerson(apprCodes, EmplGuidEmployee);

            foreach (var item in result)
            {
                System.Diagnostics.Debug.WriteLine(item.Item1 + "-" + item.Item2);
            }


            //no exception... passt
        }
    }
}
