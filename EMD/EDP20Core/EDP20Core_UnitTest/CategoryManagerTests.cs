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
    public class CategoryManagerTests
    {
        [TestMethod(), TestCategory("CategoryManager")]
        public void CreateCategoryWithSameNameTest()
        {
            string foundCategoryGuid = string.Empty;
            string categoryName = "Category Test";
            bool throwsException = false;
            CategoryManager catMan = new CategoryManager();
            try
            {


                EMDCategory cat = new EMDCategory();
                cat.Name = categoryName;
                cat.Description = categoryName;
                cat.CategoryType = (int)EnumCategoryType.EquipmentDefinition;
                catMan.Create(cat);

                EMDCategory cat2 = new EMDCategory();
                cat2.Name = categoryName;
                cat2.Description = categoryName;
                cat2.CategoryType = (int)EnumCategoryType.EquipmentDefinition;
                catMan.Create(cat2);
            }
            catch (Exception ex)
            {
                throwsException = true;
                EMDCategory foundCat = catMan.GetCategoryByName(categoryName, EnumCategoryType.EquipmentDefinition);
                if (foundCat != null)
                    catMan.Delete(foundCat.Guid,false);
            }
            Assert.AreEqual(true, throwsException);
        }
    }
}
