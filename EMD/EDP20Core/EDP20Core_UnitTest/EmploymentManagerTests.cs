using System;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.DB;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Framework;
using System.Linq;
using System.Diagnostics;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class EmploymentManagerTests
    {
        [TestMethod(), TestCategory("EmploymentManager")]
        public void xxx()
        {
            EquipmentManager eqm = new EquipmentManager();
            List<EMDEquipmentDefinition> mylist = eqm.GetEquipmentDefinitionsForClientReferenceSystemForPrice(EnumClientReferenceSystemForPrice.KBCAccountingItem);
            foreach (EMDEquipmentDefinition item in mylist)
            {
                Console.WriteLine(item.Name);
            }
        }


        [TestMethod(), TestCategory("EmploymentManager")]
        public void GetAvailableListOfEquipmentDefinitionsForEmployment_TEST()
        {
            string emplGuid = "EMPL_0b54683e23f14520b9f852d814841f6b"; //miro

            EmploymentManager emplMgr = new EmploymentManager();
            var result = emplMgr.GetAvailableListOfEquipmentDefinitionsForEmployment(emplGuid);
            

            Assert.Equals(1, result.Count());

        }

        [TestMethod(), TestCategory("EmploymentManager")]
        public void GetAvailableEquipementsForEmployment_TEST()
        {
            // original code
            // c packages = empMngr.GetAvailableListOfEquipmentDefinitionsForEmployment(empl_guid);

            string emplGuid = "EMPL_0b54683e23f14520b9f852d814841f6b"; //miro

            Stopwatch sw = new Stopwatch();

            sw.Start();

            List<string> eqdeNames = new List<string>();
            List<string> eqdeNamesNOT = new List<string>();
            List<EMDEquipmentDefinition> availableList = new List<EMDEquipmentDefinition>();

            EmploymentManager emplMgr = new EmploymentManager();
            PackageManager packMgr = new PackageManager();
            ObjectRelationHandler obreH = new ObjectRelationHandler();
            EquipmentManager eqdeMgr = new EquipmentManager();

            // get all eqde allowed according to filter rules
            FilterCriteria filterCriteria = emplMgr.GetFilterCriteriaFromEmployment(emplGuid);
            var allEqdeList = packMgr.GetFilteredListOfEquipmentDefinitions(filterCriteria);


            foreach (EMDEquipmentDefinition eqdeItem in allEqdeList)
            {
                // get count of obres for eqde
                List<EMDObjectRelation> existingEquipmentInstances =
                     obreH.GetObjects<EMDObjectRelation, ObjectRelation>("Object1 = \"" + emplGuid + "\" AND Object2 = \"" + eqdeItem.Guid + "\" AND Status <= 50", null)
                     .Cast<EMDObjectRelation>().ToList();

                // get max allowed for eqde
                EMDEquipmentDefinition equipmentDefinition = eqdeMgr.Get(eqdeItem.Guid);
                EquipmentDefinitionConfig equipmentDefinitionConfig = equipmentDefinition.GetEquipmentDefinitionConfig();


                // check           
                int foundEquipments = existingEquipmentInstances == null ? 0 : existingEquipmentInstances.Count;
                if (foundEquipments < equipmentDefinitionConfig.MaxNumberAllowedEquipments)
                {
                    // allowed
                    eqdeNames.Add(eqdeItem.Name);
                    availableList.Add(eqdeItem);
                }
                else
                {
                    eqdeNamesNOT.Add(eqdeItem.Name);
                }
            }

            long elapsed = sw.ElapsedMilliseconds;
            sw.Stop();
        }

        [TestMethod(), TestCategory("EmploymentManager")]
        public void IsOffboardingAllowed_TEST()
        {
            EmploymentHandler emplH = new EmploymentHandler();

            string testEmplNotAllowed = "EMPL_d10701ecccf94504928942c0ce348fbf";
            string testEmplIsAllowed = "EMPL_03dc3347164049178880975070451bec"; // = fleckj on hotfix


            string result = emplH.IsOffboardingAllowedForEmployment(testEmplNotAllowed, "");
            Assert.AreNotEqual("allowed", result);

            result = emplH.IsOffboardingAllowedForEmployment(testEmplIsAllowed, "");
            Assert.AreEqual("allowed", result);
        }

        [TestMethod(), TestCategory("EmploymentManager")]
        public void GetAssistence_TEST()
        {
            EmploymentManager emplManager = new EmploymentManager();

            EmploymentHandler emplHandler = new EmploymentHandler();

            List<EMDEmployment> empls = emplHandler.GetObjects<EMDEmployment, Employment>().Cast<EMDEmployment>().Take(10).ToList();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (EMDEmployment empl in empls)
            {
                emplManager.GetAssistence(empl.Guid);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            sw.Reset();
            sw.Start();
            foreach (EMDEmployment empl in empls)
            {
                emplManager.GetAssistence(empl.Guid);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            Assert.IsTrue(sw.ElapsedMilliseconds <= 2000);
        }

        [TestMethod(), TestCategory("EmploymentManager")]
        public void DoOffboarding_TEST()
        {
            CoreTransaction transaction = new CoreTransaction();
            OffboardingManager emplManager = new OffboardingManager(transaction);
            transaction.Begin();

            try
            {
                emplManager.RemoveEmployment("EMPL_923c6d9e4d3942108d93421ab27b92b3");


            }
            catch (Exception ex)
            {

            }
            transaction.Rollback();


        }

        [TestMethod(), TestCategory("EmploymentManager")]
        public void PrevEmployment_TEST()
        {

            // get sabrina

            PersonManager pmgr = new PersonManager();
            EMDPerson pers = pmgr.GetPersonByUserId("hota");

            EmploymentManager emgr = new EmploymentManager();

            EMDEmployment sabrinaEmpl = emgr.GetEmploymentsForPerson(pers_guid: pers.Guid, deliverRemoved: false).First();

            EMDEmployment prevSabrina = emgr.GetPreviousEmploymentFromHistory(sabrinaEmpl);


        }
    }
}
