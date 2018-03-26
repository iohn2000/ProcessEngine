using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.EDPExports.Entities;
using Kapsch.IS.EDP.EDPExports;

namespace EDPExports_Test
{
    [TestClass]
    public class EDPDataForITTest
    {
        public string UserID = "MAYERRX";

        [TestMethod]
        public void CreateItem()
        {
            EDPDataForITHandler handler = new EDPDataForITHandler("EMD_Export");
            EDPDataForIT newItem = new EDPDataForIT();
            newItem.Status = "active";
            newItem.UserID = this.UserID;
            newItem.UserType = "ADUserFullAccount";
            newItem.UserStatus = "InUse";
            newItem.FirstName = "RobertTest";
            newItem.FamilyName = "MayerTest";
            newItem.DisplayName = "MayerTest RobertTest";
            newItem.ObjID = 4;
            newItem.CompanyShortName = "KTC-AT";
            newItem.EmploymentTypeID = 3;
            newItem.Direct = "1234";
            newItem.Mobile = "0664 628 1234";
            newItem.Phone = "50 811 1234";
            newItem.EFax = "50 811 99 1234";
            newItem.Room = "1.44";
            newItem.PersonalNr = "65482";
            newItem.PersonID = 5555;
            newItem.EmploymentID = 2005555;
            newItem.Gender = "M";
            newItem.created = DateTime.Now;
            handler.CreateItem(newItem);
            Assert.AreEqual(handler.ItemExists(newItem.UserID), true);
        }


        [TestMethod]
        public void UpdateItem()
        {
            EDPDataForITHandler handler = new EDPDataForITHandler("EMD_Export");
            EDPDataForIT newItem = new EDPDataForIT();
            newItem.Status = "active";
            newItem.UserID = this.UserID;
            newItem.UserType = "ADUserFullAccount";
            newItem.UserStatus = "Reserved";
            newItem.FirstName = "RobertTest";
            newItem.FamilyName = "MayerTest";
            newItem.DisplayName = "MayerTest RobertTest";
            newItem.ObjID = 4;
            newItem.CompanyShortName = "KTC-AT";
            newItem.EmploymentTypeID = 3;
            newItem.Direct = "1234";
            newItem.Mobile = "0664 628 1234";
            newItem.Phone = "50 811 1234";
            newItem.EFax = "50 811 99 1234";
            newItem.Room = "1.44";
            newItem.PersonalNr = "65482";
            newItem.PersonID = 5555;
            newItem.EmploymentID = 2005555;
            newItem.Gender = "M";
            newItem.created = DateTime.Now;
            handler.UpdateItem(newItem);
            Assert.AreEqual(handler.ItemExists(newItem.UserID), true);
        }

        [TestMethod]
        public void ItemExists()
        {
            EDPDataForITHandler handler = new EDPDataForITHandler("EMD_Export");
            bool itemExists = handler.ItemExists(this.UserID);
            Assert.AreEqual(itemExists, true);

        }

        [TestMethod]
        public void ItemExistsStatic()
        {
            string itemExists =  StaticInterface.EDPDataForIT_ItemExists(this.UserID);
            Assert.AreEqual(itemExists, "True");

            itemExists = StaticInterface.EDPDataForIT_ItemExists(this.UserID+"XXXTTEED");
            Assert.AreEqual(itemExists, "False");

        }

        [TestMethod]
        public void CreateOrUpdateItem()
        {
            EDPDataForITHandler handler = new EDPDataForITHandler("EMD_Export");
            EDPDataForIT newItem = new EDPDataForIT();
            newItem.Status = "active";
            newItem.UserID = this.UserID + "Y";
            newItem.UserType = "ADUserFullAccount";
            newItem.UserStatus = "InUse";
            newItem.FirstName = "RobertTest";
            newItem.FamilyName = "MayerTest";
            newItem.DisplayName = "MayerTest RobertTest";
            newItem.ObjID = 4;
            newItem.CompanyShortName = "KTC-AT";
            newItem.EmploymentTypeID = 3;
            newItem.Direct = "1234";
            newItem.Mobile = "0664 628 1234";
            newItem.Phone = "50 811 1234";
            newItem.EFax = "50 811 99 1234";
            newItem.Room = "1.44";
            newItem.PersonalNr = "65482";
            newItem.PersonID = 5555;
            newItem.EmploymentID = 2005555;
            newItem.Gender = "M";
            newItem.created = DateTime.Now;
            handler.CreateOrUpdateItem(newItem);
            Assert.AreEqual(handler.ItemExists(newItem.UserID), true);
        }
    }

    
}
