using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Entities.Enhanced;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Framework;
using System.Data;
using System.Data.SqlClient;

using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class SecurityUserTests
    {
       
        [TestMethod(), TestCategory("SecurityUser")]
        public void SecurityUser_IsLineManager_TEST()
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser("WIE");
            EmploymentManager emplMngr = new EmploymentManager();
            PersonManager persMngr = new PersonManager();

            EMDPerson persMAYERR = (EMDPerson)persMngr.GetPersonByUserId("MAYERR");
            EMDEmployment emplMAYERR = emplMngr.GetMainEploymentForPerson(persMAYERR.Guid);
            bool result = (secUser.IsLineManager(emplMAYERR));
            Assert.AreEqual(true, result);

            EMDPerson persFLADISCH = (EMDPerson)persMngr.GetPersonByUserId("FLADISCH");
            EMDEmployment emplFLADISCH = emplMngr.GetMainEploymentForPerson(persFLADISCH.Guid);
            result = secUser.IsLineManager(emplFLADISCH);
            Assert.AreEqual(true, result);

            EMDPerson persWALA = (EMDPerson)persMngr.GetPersonByUserId("WALA");
            EMDEmployment emplWALA = emplMngr.GetMainEploymentForPerson(persWALA.Guid);
            result = secUser.IsLineManager(emplWALA);
            Assert.AreEqual(false, result);
        }

        [TestMethod(), TestCategory("SecurityUser")]
        public void SecurityUser_IsCostcenterManager_TEST()
        {
            EmploymentManager emplMngr = new EmploymentManager();
            PersonManager persMngr = new PersonManager();
            EMDPerson persWIE = (EMDPerson)persMngr.GetPersonByUserId("WIE");
            SecurityUser securityUserWie = SecurityUser.NewSecurityUser("WIE");


            EMDPerson persMAYERR = (EMDPerson)persMngr.GetPersonByUserId("MAYERR");
            EMDEmployment emplMAYERR = emplMngr.GetMainEploymentForPerson(persMAYERR.Guid);
            bool result = (securityUserWie.IsCostcenterManager(persWIE, emplMAYERR));
            Assert.AreEqual(true, result);

            EMDPerson persFLADISCH = (EMDPerson)persMngr.GetPersonByUserId("FLADISCH");
            EMDEmployment emplFLADISCH = emplMngr.GetMainEploymentForPerson(persFLADISCH.Guid);
            result = securityUserWie.IsCostcenterManager(persWIE, emplFLADISCH);
            Assert.AreEqual(true, result);

            EMDPerson persWALA = (EMDPerson)persMngr.GetPersonByUserId("WALA");
            EMDEmployment emplWALA = emplMngr.GetMainEploymentForPerson(persWALA.Guid);
            result = securityUserWie.IsCostcenterManager(persWIE, emplWALA);
            Assert.AreEqual(false, result);
        }

        [TestMethod(), TestCategory("SecurityUser")]
        public void SecurityUser_IsAssistence_TEST()
        {
            EmploymentManager emplMngr = new EmploymentManager();
            PersonManager persMngr = new PersonManager();
            EMDPerson persANDERLR = (EMDPerson)persMngr.GetPersonByUserId("ANDERLR");
            SecurityUser securityUserANDERLR = SecurityUser.NewSecurityUser("ANDERLR");


            EMDPerson persMAYERR = (EMDPerson)persMngr.GetPersonByUserId("MAYERR");
            EMDEmployment emplMAYERR = emplMngr.GetMainEploymentForPerson(persMAYERR.Guid);
            bool result = (securityUserANDERLR.IsAssistence(persANDERLR, emplMAYERR));
            Assert.AreEqual(true, result);

            EMDPerson persFLADISCH = (EMDPerson)persMngr.GetPersonByUserId("FLADISCH");
            EMDEmployment emplFLADISCH = emplMngr.GetMainEploymentForPerson(persFLADISCH.Guid);
            result = securityUserANDERLR.IsAssistence(persANDERLR, emplFLADISCH);
            Assert.AreEqual(true, result);

            EMDPerson persWALA = (EMDPerson)persMngr.GetPersonByUserId("WALA");
            EMDEmployment emplWALA = emplMngr.GetMainEploymentForPerson(persWALA.Guid);
            result = securityUserANDERLR.IsAssistence(persANDERLR, emplWALA);
            Assert.AreEqual(false, result);
        }

        [TestMethod(), TestCategory("SecurityUser")]
        public void SecurityUser_IsAllowedPerson_TEST()
        {
            bool retVal = false;
            SecurityUser secUser = SecurityUser.NewSecurityUser("WIESER");
            PersonManager persManager = new PersonManager();

            EMDPerson personToView = persManager.GetPersonByUserId("KOSTIC");
            retVal = secUser.IsAllowedPerson(personToView.Guid, SecurityPermission.PersonManagement_View_Manage);
            Assert.AreEqual(true, retVal);

            personToView = persManager.GetPersonByUserId("WALA");
            retVal = secUser.IsAllowedPerson(personToView.Guid, SecurityPermission.PersonManagement_View_Manage);
            Assert.AreEqual(false, retVal);
        }

        [TestMethod(), TestCategory("SecurityUserReadOnly_TEST")]
        public void SecurityUserAllowedEmployments_TEST()
        {
            SecurityUser secUser = SecurityUser.NewSecurityUser("KUESTER");
            List<EMDPersonEmployment> avEmployment = secUser.AllowedEmploymentsForEnterprises();
            string userIdToFind = "WIE";
            bool userIdFound = false;
            foreach (EMDPersonEmployment item in avEmployment)
            {
                if (item.Pers.UserID.Trim().ToUpper() == userIdToFind)
                {
                    userIdFound = true;
                }
            }
            Assert.AreEqual(userIdFound, true);
        }

    }
}
