using System;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class OrgUnitRoleSearchTests
    {

        [TestMethod(), TestCategory("OrgUnitRoleSearch")]
        public void AddOrgUnitRoleToEmployment_TEST()
        {
        }

        [TestMethod(), TestCategory("OrgUnitRoleSearch")]
        public void SearchSupervisor_TEST()
        {
            OrgUnitRoleSearch search = new OrgUnitRoleSearch();
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonHandler persHandler = new PersonHandler();
            List<IEMDObject<EMDEmployment>> listEmplsMAYERR = emplHandler.GetObjects<EMDEmployment, Employment>("EP_ID = 2000505");

            if (listEmplsMAYERR.Count > 0)
            {
                string emplGuidMAYERR = listEmplsMAYERR[0].Guid;
                List<string> supervisors = search.SearchOrgUnitRoleForEmployment(10500, emplGuidMAYERR);
                foreach (string sup in supervisors)
                {

                    EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(sup);
                    EMDPerson pers = (EMDPerson)persHandler.GetObject<EMDPerson>(empl.P_Guid);
                    Console.WriteLine(pers.FamilyName + " " + pers.FirstName);
                    if (pers.UserID.ToUpper() == "WIE")
                    {
                        Assert.AreEqual(pers.UserID.ToUpper(), "WIE");
                    }
                }
            }
        }

        [TestMethod(), TestCategory("OrgUnitRoleSearch")]
        public void GetRootOrgunits()
        {
            OrgUnitHandler handler = new OrgUnitHandler();
            List<EMDOrgUnit> orgunitsRoot = handler.GetAllRootOrgunits(false);

            List<EMDOrgUnit> subOrgunits = null;
            if (orgunitsRoot.Count > 0)
            {
                
                 //   subOrgunits = handler.GetAllSubOrgUnitsFromParent("ORGU_db0c052182ab44bdae8b2d346d086d2c");
                // subOrgunits = handler.GetAllSubOrgUnitsFromParent(orgunitsRoot[0].Guid);
            }

            subOrgunits = handler.GetAllSubOrgUnitsFromParent(string.Empty, false, 2);

            Assert.IsNotNull(subOrgunits);
        }
    }
}
