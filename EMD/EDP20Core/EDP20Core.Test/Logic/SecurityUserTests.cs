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
    [TestFixture, Category("Logic"), Category("Security")]
    public class SecurityUserTests
    {
        [TestCase("WIE","MAYERR", true)]
        [TestCase("WIE", "FLADISCH", true)]
        [TestCase("WIE", "WALA", false)]
        public void SecurityUser_IsLineManager_TEST(string viewingUserId, string viewedUserId, bool expectedResult)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(viewingUserId);
            EmploymentManager emplMngr = new EmploymentManager();
            PersonManager persMngr = new PersonManager();

            EMDPerson persViewed = (EMDPerson)persMngr.GetPersonByUserId(viewedUserId);
            EMDEmployment emplViewed = emplMngr.GetMainEploymentForPerson(persViewed.Guid);
            bool result = (secUser.IsLineManager(emplViewed));

            Assert.That(result,Is.EqualTo(expectedResult));
        }

        [TestCase("WIE", "MAYERR", true)]
        [TestCase("WIE", "FLADISCH", true)]
        [TestCase("WIE", "WALA", false)]
        public void SecurityUser_IsCostcenterManager_TEST(string viewingUserId, string viewedUserId, bool expectedResult)
        {
            EmploymentManager emplMngr = new EmploymentManager();
            PersonManager persMngr = new PersonManager();
            EMDPerson persViewing = (EMDPerson)persMngr.GetPersonByUserId(viewingUserId);
            SecurityUser securityUserViewing = SecurityUser.NewSecurityUser(viewingUserId);

            EMDPerson persViewed = (EMDPerson)persMngr.GetPersonByUserId(viewedUserId);
            EMDEmployment emplViewed = emplMngr.GetMainEploymentForPerson(persViewed.Guid);
            bool result = (securityUserViewing.IsCostcenterManager(persViewing, emplViewed));

            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("ANDERLR", true)]
        [TestCase("WIESER", true)]
        [TestCase("FLADISCH", false)]
        [TestCase("MAYERR", false)]
        [TestCase("RIEDINGE", false)]
        public void SecurityUser_IsEmploymentAssistence_TEST(string userId, bool expectedResult)
        {
            EmploymentManager emplMngr = new EmploymentManager();
            PersonManager persMngr = new PersonManager();
            EMDPerson pers = (EMDPerson)persMngr.GetPersonByUserId(userId);
            EMDEmployment empl = emplMngr.GetMainEploymentForPerson(pers.Guid);
            SecurityUser securityUserViewing = SecurityUser.NewSecurityUser(userId);

            //bool result = false; (securityUserViewing.IsEmpoymentAssistence(empl.Guid));
            //Assert.AreEqual(expectedResult, result);
            Assert.Fail("see commented code");
        }

        [TestCase("ANDERLR", "MAYERR", true)]
        [TestCase("ANDERLR", "FLADISCH", true)]
        [TestCase("ANDERLR", "WALA", false)]
        public void SecurityUser_IsAssistence_TEST(string viewingUserId, string viewedUserId, bool expectedResult)
        {
            EmploymentManager emplMngr = new EmploymentManager();
            PersonManager persMngr = new PersonManager();
            EMDPerson persViewing = (EMDPerson)persMngr.GetPersonByUserId(viewingUserId);
            SecurityUser securityUserViewing = SecurityUser.NewSecurityUser(viewingUserId);

            EMDPerson persViewed = (EMDPerson)persMngr.GetPersonByUserId(viewedUserId);
            EMDEmployment emplViewed = emplMngr.GetMainEploymentForPerson(persViewed.Guid);
            bool result = (securityUserViewing.IsAssistence(persViewing, emplViewed));
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("WIESER", "MAYERR", true)]
        [TestCase("WIESER", "FLADISCH", true)]
        [TestCase("WIESER", "WALA", false)]
        public void SecurityUser_IsAllowedPerson_TEST(string viewingUserId, string viewedUserId, bool expectedResult)
        {
            bool retVal = false;
            SecurityUser secUser = SecurityUser.NewSecurityUser(viewingUserId);
            PersonManager persManager = new PersonManager();

            EMDPerson personViewed = persManager.GetPersonByUserId(viewedUserId);
            retVal = secUser.IsAllowedPerson(personViewed.Guid, SecurityPermission.PersonManagement_View_Manage);
            Assert.AreEqual(expectedResult, retVal);

        }

        [TestCase("KUESTERR", "WIE", true)]
        [TestCase("KUESTER", "MAYERR", true)]
        [TestCase("KUESTER", "WALA", false)]
        public void SecurityUserAllowedEmployments_TEST(string viewingUserId, string viewedUserId, bool expectedResult)
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser(viewingUserId);
            List<EMDPersonEmployment> avEmployment = secUser.AllowedEmploymentsForEnterprises();
            bool userIdFound = false;
            foreach (EMDPersonEmployment item in avEmployment)
            {
                if (item.Pers.UserID.Trim().ToUpper() == viewedUserId)
                {
                    userIdFound = true;
                }
            }
            Assert.AreEqual(userIdFound, expectedResult);
        }

    }
}
