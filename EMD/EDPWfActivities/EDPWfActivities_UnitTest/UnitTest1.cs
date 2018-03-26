using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDPWfActivities_UnitTest.NavTicketSrvTEST;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.WFActivity.JIRA;
using System.Configuration;
using RestSharp;
using RestSharp.Authenticators;

namespace EDPWfActivities_UnitTest
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod, TestCategory("Onboarding")]
        public void checkOrgunitRole_TEST()
        {
            string emplGuid = "EMPL_03dc3347164049178880975070451bec"; // fleckj
            OrgUnitRoleHandler orroH = new OrgUnitRoleHandler();

            string query = string.Format("R_ID = {0} AND EP_Guid = \"{1}\"", OrgUnitRoleHandler.ROLE_ID_PERSON, emplGuid);
            var orroList = orroH.GetObjects<EMDOrgUnitRole, OrgUnitRole>(query);
            if (orroList == null || orroList.Count == 0)
            {
                //this.orro = orroH.AddOrgUnitRoleToEmployment(emplGuid.VarValue, orgUnit.VarValue, OrgUnitRoleHandler.ROLE_ID_PERSON);
            }
            else
            {
                // do nothing, org unit already exists
            }

        }


        [TestMethod, TestCategory("JIRA")]
        public void TestCreateJiraIssue()
        {
            CreateJIRATicketActivity testJira = new CreateJIRATicketActivity();
            
            testJira.TicketBody = "{\"serviceDeskId\": \"1\"," + 
                "\"requestTypeId\": \"2\"," +
                //"\"raiseOnBehalfOf\": \"sysu_ktc_jira_edp_sy\"," +
                "\"requestFieldValues\":{" +
                         "\"summary\": \"NEDAP Onboarding Task – New User: edp_test\"," +
                         "\"description\":\"A new user has been onboarded, if applicable, please - create a new nedap user - print a nedap badge - assign the nedap badge.*Display Name*: Wolfgang Stagl*Username*: STAGL*First working day*: 4/1/2014 12:00:00 AM*OrgUnit*: SW Development & Project Management*Cost Center*: Informationssysteme & Ablaufberatung*Profile Link*: http://gis-edp.kapsch.co.at/PersonProfile/Profile/PERS_b13887d80d9744f8b179c0d5cf02a4d1*Location*: AT / Vienna / Wienerbergstrasse*Picture Link*: http://gis-edp.kapsch.co.at/images/STAGL.jpg\"," + 
                         "\"customfield_10304\" : {\"value\" : \"User Management\",\"child\": {\"value\":\"Physical Access Control\"}}}}";

            testJira.ClientReferenceID = testJira.CreateJiraTicket("UNIT test: ");

            Assert.IsTrue(testJira.ClientReferenceID.StartsWith("KSDT"));

            // now simluate the call JIRA would make when ticket is finished
            // this.TestSimluateJiraReturnCall_EDPLite(testJira.ClientReferenceID);
        }

        [TestMethod, TestCategory("JIRA")]
        public void TestSimluateJiraReturnCall_EDPLite()
        {
            CreateJIRATicketActivity testJira = new CreateJIRATicketActivity();

            testJira.TestSimluateJiraReturnCall_EDPLite("null", "KSDT-357", "UNIT test:");
        }

        //[TestMethod, TestCategory("NavisionTicket")]
        //public void createHardcodedTicket_TEST()
        //{
        //    KSMP_GenV1SoapClient service = new KSMP_GenV1SoapClient();
        //    TicketRequestTicketRequest request = new TicketRequestTicketRequest();
        //    TicketRequestResponseTicketResponse response = new TicketRequestResponseTicketResponse();

        //    List<object> paramList = new List<object>();

        //    request.Quelle = "EMD-ENGINE";

        //    paramList.Add(CreateStringVal("AuftraggeberName", "auftrags-fleckj"));
        //    paramList.Add(CreateStringVal("AuftraggeberTelefon", "telefon-fleckj"));
        //    paramList.Add(CreateStringVal("Auftragsbeschreibung", "auftragsbeschreibung"));
        //    paramList.Add(CreateStringVal("Einsatzart", "PLAN"));
        //    paramList.Add(CreateStringVal("EMail", "fleckj@kapsch"));
        //    paramList.Add(CreateIntVal("Prioritaet", 5));
        //    paramList.Add(CreateStringVal("EinsatzartBeschreibung", "EinsatzartBeschreibung"));
        //    paramList.Add(CreateStringVal("PartnerTicketID", "XD_ID-RQT_ID"));
        //    paramList.Add(CreateStringVal("Geschaeftsfall", "IT"));
        //    paramList.Add(CreateDateTimeVal("AuftraggeberAm", DateTime.Now.Date));
        //    paramList.Add(CreateDateTimeVal("AuftraggeberUm", DateTime.Now));
        //    paramList.Add(CreateStringVal("LieferungAnPLZ", "LieferungAnPLZ"));
        //    paramList.Add(CreateStringVal("LieferungAnLand", "LieferungAnLand"));
        //    paramList.Add(CreateStringVal("Auftragsbeschreibung3", "Auftragsbeschreibung3"));
        //    paramList.Add(CreateStringVal("QuellsystemNummer", "X_KSMP_User"));
        //    paramList.Add(CreateStringVal("LocationID", "Nav_L_ID_And_E_ID"));

        //    request.Items = paramList.ToArray();

        //    string BöserFehler = "";
        //    string Err = "";
        //    try
        //    {
        //        response = service.TicketRequest(request);
        //        if (response.success == true)
        //        {
        //            //Log("Navision Ticket created - no Error from Webservice", true);
        //            //Log("EMALC - Message: " + Nav_Response.errorMsg.ToString(), true);
        //        }
        //        else
        //        {
        //            Err = "Navision Ticket could not be created - Error: " + response.errorMsg.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        BöserFehler = "woahh das ist ein umlaut drinnen!" + ex.ToString();
        //        throw;
        //    }

        //}




        private TicketRequestTicketRequestStringVal CreateStringVal(string aName, String aValue)
        {
            TicketRequestTicketRequestStringVal sVal1 = new TicketRequestTicketRequestStringVal();
            sVal1.Name = aName;
            sVal1.Value = aValue;

            return sVal1;

        }
        private TicketRequestTicketRequestDateTimeVal CreateDateTimeVal(string aName, DateTime aValue)
        {
            TicketRequestTicketRequestDateTimeVal sVal1 = new TicketRequestTicketRequestDateTimeVal();
            sVal1.Name = aName;
            sVal1.Value = aValue;

            return sVal1;

        }
        private TicketRequestTicketRequestIntVal CreateIntVal(string aName, int aValue)
        {
            TicketRequestTicketRequestIntVal sVal1 = new TicketRequestTicketRequestIntVal();
            sVal1.Name = aName;
            sVal1.Value = aValue;

            return sVal1;
        }
        private TicketRequestTicketRequestAttachment CreateAttachementValue(string fileId, string fileId2, string fileName, long size)
        {
            TicketRequestTicketRequestAttachment att = new TicketRequestTicketRequestAttachment();
            att.FileID = fileId;
            att.FileID2 = fileId2;
            att.FileName = fileName;
            att.FileSize = size;

            return att;
        }
        private TicketRequestTicketRequestAttachment CreateAttachementValue(string fileName, byte[] contents)
        {
            TicketRequestTicketRequestAttachment att = new TicketRequestTicketRequestAttachment();
            att.FileID = "";
            att.FileID2 = "";
            att.FileName = fileName;
            att.FileSize = contents.Length;
            att.FileContents = contents;

            return att;
        }

    }


}
