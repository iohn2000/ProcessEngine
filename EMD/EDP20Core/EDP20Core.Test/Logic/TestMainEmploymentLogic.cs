using NUnit.Framework;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Framework;
using System.Collections.Generic;
using System;
using System.Linq;
using EDP20Core.Test.Helper;

namespace EDP20Core.Test.Logic
{
    [TestFixture, Category("Logic")]
    public class TestMainEmploymentLogic
    {
        string ReqeuesterGuid = "PERS_c5ca7e0baf66405f95b5a2410ba895b7";
        string ModifyComment = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua.At vero eos et accusam et justo duo dolores et ea rebum.Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";

        /// <summary>
        /// Testresult:
        /// 1. Old employment mustnt have a main-flag
        /// 2. The persons user must be moved to the new main-empl
        /// 3. Other users must remain on the old employment
        /// </summary>
        public void ChangeMainEmployment()
        {

        }


        [TestCase("EMPL_dc673cc90e1e440fbf779517d510f4ba")]
        public void AreAllEquipmentsActive(string emplGuid)
        {
            OffboardingManager offboardingManager = new OffboardingManager();
            bool areAllActive = offboardingManager.AreAllEquipmentsActive(emplGuid);

            Assert.IsTrue(areAllActive == false, string.Format("The test employment:{0} should have running equipment-processes", emplGuid));
        }


        [TestCase("EMPL_1cf7a98ec7bf4247839fb1d70e0030a3")]
        public void OffboardLastMainEmployment(string emplGuid)
        {
            CoreTransaction transaction = new CoreTransaction();

            OffboardingManager offboardingManager = new OffboardingManager(transaction, ReqeuesterGuid, ModifyComment);
            EmploymentManager employmentManager = new EmploymentManager(transaction, ReqeuesterGuid, ModifyComment);
            UserManager userManager = new UserManager(transaction, ReqeuesterGuid, ModifyComment);

            transaction.Begin();

            offboardingManager.RemoveEmployment(emplGuid);

            bool hasMainEmployment = employmentManager.IsMainEmployment(emplGuid);
            Assert.IsTrue(hasMainEmployment == false, "The offboarding for the last empl must remove the main-flag");

            List<EMDUser> users = userManager.GetEmploymentUsers(emplGuid);
            List<EMDUser> foundusers = users.FindAll(a => a.Status != (int)EnumUserStatus.Reserverd);

            Assert.IsTrue(foundusers.Count == 0, "The offboarding for the last empl must set all users to Status.reserved");

            transaction.Rollback();

        }

        [TestCase("EMPL_38d15e3d5c1e449e8f8abfd359b0fe5b")]
        public void OffboardMainEmploymentWithOtherActiveEmployments(string emplGuid)
        {
            CoreTransaction transaction = new CoreTransaction();

            try
            {                
                OffboardingManager offboardingManager = new OffboardingManager(transaction, ReqeuesterGuid, ModifyComment);
                EmploymentManager employmentManager = new EmploymentManager(transaction, ReqeuesterGuid, ModifyComment);
                UserManager userManager = new UserManager(transaction, ReqeuesterGuid, ModifyComment);

                EMDEmployment mainEmplBefore = employmentManager.GetEmployment(emplGuid);

                transaction.Begin();

                offboardingManager.RemoveEmployment(emplGuid);

                bool hasMainEmployment = employmentManager.IsMainEmployment(emplGuid);
                Assert.IsTrue(hasMainEmployment == false, "The offboarding for the last empl must remove the main-flag");

                EMDEmployment mainEmplAfter = employmentManager.GetMainEploymentForPerson(mainEmplBefore.P_Guid);
                Assert.IsTrue(mainEmplBefore.Guid != mainEmplAfter.Guid, "The main empl must be the same as before");

                List<EMDUser> users = userManager.GetEmploymentUsers(mainEmplAfter.Guid);
                List<EMDUser> foundusers = users.FindAll(a => a.UserType == (int)EnumUserType.ADUserFullAccount && a.Status != (int)EnumUserStatus.InUse);

                Assert.IsTrue(foundusers.Count == 1, "The offboarding for the last empl must set all users to Status.reserved");

                transaction.Rollback();
            }
            catch (Exception ex)
            {
                // Test Output
                TestHelperLogToTestOutput.FailMessageToOutput(ex);

                // Fail and transaction rollback
                Assert.Fail();
                transaction.Rollback();
            }
        }
        

        [Test]
        [Author("Roland Fladischer")]
        // 1 Add person
        // 2 Add employment to person
        // 3 The created Peron should have a Main Employment, which contains 3 user accounts.
        public void OnboardNewMainEmployment()
        {
            // Transaction
            CoreTransaction transaction = new CoreTransaction();

            try
            {
                // Transaction begin                
                transaction.Begin();

                // OnboardingManager
                OnboardingManager onboardingManager = new OnboardingManager(transaction, ReqeuesterGuid, ModifyComment);
                
                // EmploymentManager
                EmploymentManager employmentManager = new EmploymentManager(transaction, ReqeuesterGuid, ModifyComment);

                // UserManager
                UserManager userManager = new UserManager(transaction, ReqeuesterGuid, ModifyComment);

                // Person
                EMDPerson emdPerson = onboardingManager.CreateNewPerson("Testuser", "Hugo", "M");

                // Employment from helper class
                EMDEmployment empl = TestHelperEMDEmployment.EMDEmployment;

                // Prepare onboarding
                TestHelperOnboarding.PrepareOnboarding(onboardingManager, empl, emdPerson);                

                // Check if only one employment has been created.
                List<EMDEmployment> testEMDEmployments = employmentManager.GetEmploymentsForPerson(emdPerson.Guid);
                Assert.IsTrue(testEMDEmployments.Count == 1, "Only one deployment should be created.");

                // Check if the employment was created as Main-Employment.
                ObjectFlagManager objectFlagManager = new ObjectFlagManager(transaction, ReqeuesterGuid, ModifyComment);
                Assert.IsTrue(objectFlagManager.IsMainEmployment(testEMDEmployments.FirstOrDefault().Guid), "Employment should be created as main employment.");

                // Check if Main-Employment has 3 User Accounts.
                Assert.IsTrue(userManager.GetEmploymentUsers(testEMDEmployments.FirstOrDefault().Guid).Count == 3, "Main Employment should have 3 user accounts.");

                // Transaction rollback
                transaction.Rollback();

                // Test Output
                TestHelperLogToTestOutput.SuccessMessageToOutput();
            }
            catch (Exception ex)
            {
                // Test Output
                TestHelperLogToTestOutput.FailMessageToOutput(ex);

                // Fail and transaction rollback
                Assert.Fail();
                transaction.Rollback();
            }
        }

        [Test]
        [Author("Roland Fladischer")]
        // 1 Create person with disabled employment with 3 user accounts. Emplyoment must not be a main employment.
        // 2 Add a new Main Employment to the person. 
        // 3 The new Main Employment gets the AD user from the old employment. // AD-User
        // 4 The admin accounts should remain on deactivated Emplyoment. // 00User & 99User

        //select p.Guid, p.FamilyName, p.FirstName, COUNT(*)
        //from person as p
        //join Employment as e on p.Guid = e.P_Guid
        //where e.Status != 70 and e.ValidTo > GETDATE()
        //GROUP BY
        //p.guid, p.FamilyName, p.FirstName
        //HAVING
        //COUNT(*) > 1 
        public void OnboardMainEmploymentWithExistingRemoved()
        {
            // Transaction
            CoreTransaction transaction = new CoreTransaction();

            try
            {
                // Transaction begin                
                transaction.Begin();

                // OnboardingManager
                OnboardingManager onboardingManager = new OnboardingManager(transaction, ReqeuesterGuid, ModifyComment);

                //OffboardingManager
                OffboardingManager offboardingManager = new OffboardingManager(transaction, ReqeuesterGuid, ModifyComment);

                // EmploymentManager
                EmploymentManager employmentManager = new EmploymentManager(transaction, ReqeuesterGuid, ModifyComment);

                // UserManager
                UserManager userManager = new UserManager(transaction, ReqeuesterGuid, ModifyComment);

                // Person
                EMDPerson emdPerson = onboardingManager.CreateNewPerson("Testuser", "Hugo", "M");

                // Employment from helper class
                EMDEmployment empl = TestHelperEMDEmployment.EMDEmployment;

                // Prepare onboarding
                TestHelperOnboarding.PrepareOnboarding(onboardingManager, empl, emdPerson);

                // Disable employment
                List<EMDEmployment> testEMDEmployments = employmentManager.GetEmploymentsForPerson(emdPerson.Guid);
                EMDEmployment diabledEmployment = testEMDEmployments.FirstOrDefault();
                offboardingManager.RemoveEmployment(diabledEmployment.Guid); 
                

                // Check if the employment is not Main-Employment.
                ObjectFlagManager objectFlagManager = new ObjectFlagManager(transaction, ReqeuesterGuid, ModifyComment);

                Assert.IsFalse(objectFlagManager.IsMainEmployment(diabledEmployment.Guid), "The employment should not be a main employment.");

                // Check if the employment is closed = 70
                Assert.IsTrue(diabledEmployment.Status == 70, "The employment should not be closed.");

                //offboardingManager.PrepareOffboarding()






                //List<EMDEmployment> testEMDEmployments = employmentManager.GetEmploymentsForPerson(emdPerson.Guid);





                //Add new Main Employment to the person. 


                // Transaction rollback
                transaction.Rollback();

                // Test Output
                TestHelperLogToTestOutput.SuccessMessageToOutput();
            }
            catch (Exception ex)
            {
                // Test Output
                TestHelperLogToTestOutput.FailMessageToOutput(ex);

                // Fail and transaction rollback
                Assert.Fail();
                transaction.Rollback();
            }
        }

        /// <summary>
        /// new employment is not the main-empl
        /// </summary>
        public void OnboardMainEmploymentWithExisting()
        {
            //mehrere aktive employments, eine main
            // add employment ohne main hackerl
            // user accounts bleiben am main
        }
    }
}
