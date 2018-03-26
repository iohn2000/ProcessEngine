using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using System.Collections.Generic;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void CreateMainUserIDLogic()
        {
            UserHandler userHandler = new UserHandler();
            List<Tuple<string, string>> names = new List<Tuple<string, string>>();
            names.Add(new Tuple<string, string>("Thomas", "Hübler"));
            names.Add(new Tuple<string, string>("Robert", "Mayer"));
            names.Add(new Tuple<string, string>("Hügli", "Thöni"));
            names.Add(new Tuple<string, string>("Übern", "Laurenziberg"));
            names.Add(new Tuple<string, string>("Muströfa", "Ögliputz"));
            names.Add(new Tuple<string, string>("Schwiützer", "Ögelibier"));
            names.Add(new Tuple<string, string>("Nöxter", "Hüpper"));
            names.Add(new Tuple<string, string>("Göhtmä", "Nixön"));
            names.Add(new Tuple<string, string>("Jätzüs", "Abergnöür"));
            names.Add(new Tuple<string, string>("Ěntrǔktes", "Gǚrtělťiǚr"));
            char[] specialCharArray = new[] { 'ä', 'ö', 'ü', 'Ä', 'Ö', 'Ü', 'Ě', 'ǔ', 'ǚ', 'ě', 'ť', 'ǚ' };
            bool foundSpecialCharacterInName = false;
            foreach (Tuple<string, string> name in names)
            {
                String result = userHandler.CreateMainUserIDLogic(name.Item2, name.Item1, string.Empty);
                foreach (char c in specialCharArray)
                {
                    if (result.IndexOf(c) > -1)
                    {
                        foundSpecialCharacterInName = true;
                    }
                }
            }
            Assert.IsFalse(foundSpecialCharacterInName);
        }

        [TestMethod]
        public void GetNewUserName()
        {
            UserManager userManager = new UserManager();
            string userName = userManager.GetNewMainUserName("Woller", "Christian");

            Assert.IsTrue(userName.Contains("WOLLER"));
        }

        [TestMethod]
        public void GetNewUserNameExternal()
        {
            UserManager userManager = new UserManager();
            string userName = userManager.GetNewMainUserName("Woller", "Christian", new EMDEmploymentType() { ET_ID = 11 });

            Assert.IsTrue(userName.Contains("A_"));
        }

        [TestMethod]
        public void CreateNewMainUserName()
        {
            CoreTransaction transaction = new CoreTransaction();

            EMDEmployment employment = new EmploymentManager().GetEmployment("EMPL_1a60ba4d38db4234bce9f491554c3ce7");
            EMDUserDomain emdUserDomain = new UserDomainManager().GetUserDomains().Find(a => a.Name == "kapsch.co.at");

            UserManager userManager = new UserManager(transaction);


            string userName = userManager.GetNewMainUserName("Woller", "Christian");
            transaction.Begin();
            EMDUser emdUser = userManager.CreateMainUserName(employment, userName, emdUserDomain.Guid);
            transaction.Rollback();


            Assert.IsTrue(emdUser.Username == userName && emdUser.USDO_Guid == emdUserDomain.Guid);
        }

        [TestMethod]
        public void ChangeMainUserName()
        {
            CoreTransaction transaction = new CoreTransaction();
            PersonManager personManager = new PersonManager(transaction);


            EMDEmployment employment = new EmploymentManager(transaction).GetEmployment("EMPL_1a60ba4d38db4234bce9f491554c3ce7");
            EMDUserDomain emdUserDomain = new UserDomainManager(transaction).GetUserDomains().Find(a => a.Name == "kapsch.co.at");
            //EMDPerson oldPerson =  personManager.GetPersonByUserId("wollerc");

            UserManager userManager = new UserManager(transaction);

            string userName = userManager.GetNewMainUserName("Woller", "Christian");
            transaction.Begin();
            EMDUser emdUser = userManager.CreateMainUserName(employment, userName, emdUserDomain.Guid);

            EMDPerson person = userManager.SetPersonMainUser(emdUser.Guid, employment.P_Guid);


            transaction.Rollback();


            Assert.IsTrue(person.UserID == emdUser.Username, person.USER_GUID = emdUser.Guid);
        }
    }
}
