using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TESTING.WorkflowService;
using System.Xml.Linq;

namespace TESTING
{
    [TestClass]
    public class WCFServiceProcessEngine_TEST
    {
        #region MyRegion
        //        [TestMethod]
        //        public void DoOnboarding_Test()
        //        {
        //            WorkflowService.ProcessServiceClient c = new WorkflowService.ProcessServiceClient();

        //            COREEmdEmployment coreEmpl = new COREEmdEmployment()
        //            {
        //                Entry = DateTime.Now.AddDays(3),
        //                FirstWorkDay = DateTime.Now.AddDays(3),
        //                Exit = null,
        //                LastDay = null,
        //                PersNr = null
        //            };

        //            COREEmdContact[] cList = new COREEmdContact[2];
        //            COREEmdContact dummy = new COREEmdContact();
        //            dummy.ACDDisplay = false;
        //            dummy.VisibleKatce = true;
        //            dummy.VisiblePhone = true;
        //            dummy.Text = "xx";
        //            dummy.CT_Guid = "COTY_26a1bebc72004b82a87338a67e2e45f7";
        //            dummy.C_CT_ID = 7;
        //            cList[0] = dummy;

        //            dummy = new COREEmdContact();
        //            dummy.ACDDisplay = false;
        //            dummy.VisibleKatce = true;
        //            dummy.VisiblePhone = true;
        //            dummy.Text = "yy";
        //            dummy.CT_Guid = "COTY_9e73f884acb8493191927cd196fba1b3";
        //            dummy.C_CT_ID = 6;
        //            cList[1] = dummy;

        //            XElement xe = XElement.Parse(@"<KCCData>
        //  <Items>
        //    <Item key=""Personnel requisition number"">9876</Item>
        //    <Item key=""No Approval Needed"">False</Item>
        //    <Item key=""Reason for no Approval""></Item>
        //    <Item key=""Simcard"">True</Item>
        //    <Item key=""Datacard"">False</Item>
        //    <Item key=""Software Equipment""></Item>
        //  </Items>
        //</KCCData>");

        //            c.DoOnboarding(
        //                requestingPersonEmplGuid: "EMPL_c90e61959b8544b49de6a129f0c04dd7",
        //                coreEmpl: coreEmpl,
        //                effectedPersonGuid: "PERS_00248d8484814b1098397246d905e94b",
        //                enteGuid: "ENTE_9d1d6d4a3b4a40fdb49c3bae354f8af0",
        //                locaGuid: "LOCA_a56e011a7b9e49e6bc564ff96415d9ee",
        //                accoGuid: "ACCO_e0cc36c5be154d919554ab637adeb925",
        //                orguGuid: "ORGU_0051f54cfe37425bbfaf4f8ae3272935",
        //                emtyGuid: "EMTY_bbef7e324f7a413db35ee7171d437420",
        //                digrGuid: "DIST_76dbad87706241caa8158520450516b3",
        //                emailType: "intern",
        //                contactList: cList,
        //                xmlData: xe,
        //                newEquipments: null
        //                );

        //        } 
        #endregion

        [TestMethod, TestCategory("WebSrv ProcEngine")]
        public void GetDocTemplates()
        {
            ProcessServiceClient s = new ProcessServiceClient();
            var x  = s.GetAllEmailDocumentTemplates();      
        }

        [TestMethod, TestCategory("Worflow Tests")]
        public void DeleteWorkflow__Only_One_VersionExists_TEST()
        {
            const string username = "flecky";
            ProcessServiceClient s = new ProcessServiceClient();
            string uniqueName = ("deleteme__" + Guid.NewGuid().ToString("N"));
            var wf = s.CreateWorkflow(uniqueName, "bla", DateTime.Now, DateTime.Now.AddDays(100));
            s.DeleteWorkflow(wf.Id, true);
            //check if gone

            try
            {
                s.GetWorkflowItem(wf.Id, username);
                Assert.Fail("shouldnt find this workflow. delete didnt work??");
            }
            catch (Exception)
            {

            }
        }

        [TestMethod, TestCategory("Worflow Tests")]
        public void DeleteWorkflow_TEST()
        {
            const string username = "flecky";
            ProcessServiceClient s = new ProcessServiceClient();
            string uniqueName = ("deleteme__" + Guid.NewGuid().ToString("N"));
            var wf = s.CreateWorkflow(uniqueName, "bla", DateTime.Now, DateTime.Now.AddDays(100));

            wf = s.CheckoutWorkflow(wf.Id, username);
            wf.Description = "same new something blue";
            s.SaveWorkflowMetaData(wf.Id, wf.Name, wf.Description, wf.ValidFrom, wf.ValidTo, username);
            s.CheckinWorkflow(wf.Id, username);

            wf = s.CheckoutWorkflow(wf.Id, username);
            wf.Description = "same new something blue, somthing old something new";
            s.SaveWorkflowMetaData(wf.Id, wf.Name, wf.Description, wf.ValidFrom, wf.ValidTo, username);
            s.CheckinWorkflow(wf.Id, username);

            s.DeleteWorkflow(wf.Id, false);
            s.DeleteWorkflow(wf.Id, false);
            s.DeleteWorkflow(wf.Id, false);
        }

        [TestMethod, TestCategory("Worflow Tests")]
        public void IsPackageUsedInActiveWorkflow_TEST()
        {
            ProcessServiceClient s = new ProcessServiceClient();
            string obcoGuid = null;

            // workflow intance erzeugen die ein package als input parameter hat.

            s.IsPackageUsedInActiveWorkflow(obcoGuid);
        }
    }
}
