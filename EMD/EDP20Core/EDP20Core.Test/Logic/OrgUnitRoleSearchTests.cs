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

namespace EDP20Core.Test.Logic
{
    [TestFixture, Category("Logic")]
    public class OrgUnitRoleSearchTests
    {
        [TestCase("EMPL_d34eb2699a1f4f8fa6642f95fe2d77f0", "WIE", true)]
        [TestCase("EMPL_c7afb9cb39804387a9b95e7d127610d1","WIE", true)]
        [TestCase("EMPL_d34eb2699a1f4f8fa6642f95fe2d77f0", "WIEE", false)]
        public void SearchSupervisor_TEST(string emplGuid, string supervisorUserId , bool expectedResult)
        {
            OrgUnitRoleSearch search = new OrgUnitRoleSearch();
            EmploymentHandler emplHandler = new EmploymentHandler();
            PersonHandler persHandler = new PersonHandler();

            List<string> supervisors = search.SearchOrgUnitRoleForEmployment(10500, emplGuid);
            bool expectedSupervisorFound = false;
            foreach (string sup in supervisors)
            {

                EMDEmployment empl = (EMDEmployment)emplHandler.GetObject<EMDEmployment>(sup);
                EMDPerson pers = (EMDPerson)persHandler.GetObject<EMDPerson>(empl.P_Guid);
                Console.WriteLine(pers.FamilyName + " " + pers.FirstName);
                if (pers.UserID.ToUpper() == supervisorUserId.ToUpper())
                {
                    expectedSupervisorFound = true;
                }
            }
            Assert.AreEqual(expectedSupervisorFound, expectedResult);
        }

        [TestCase()]
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
