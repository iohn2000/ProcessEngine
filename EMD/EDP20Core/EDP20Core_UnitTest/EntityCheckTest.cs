using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Logic;
using System.Linq;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.Framework;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class EntityCheckTest
    {
        [TestMethod, TestCategory("EntityCheck")]
        public void CreateEmploymentEntityCheckAndGetDates()
        {
            CoreTransaction transaction = new CoreTransaction();
            EmploymentManager manager = new EmploymentManager(transaction);
            EntityCheckManager entityCheckManager = new EntityCheckManager(transaction, null, "createdByUnitTest");
            EMDEmployment employment = null;
            List<EMDEmploymentType> employmentTypes = new EmploymentTypeHandler(transaction).GetObjects<EMDEmploymentType, EmploymentType>().Cast<EMDEmploymentType>().ToList();
            EMDEmploymentType foundEmploymentType = (from a in employmentTypes where a.CheckIntervalInDays > 0 select a).FirstOrDefault();
            if (foundEmploymentType != null)
            {
                employment = manager.GetList(string.Format("ET_Guid = \"{0}\"", foundEmploymentType.Guid)).FirstOrDefault();

            }


            if (employment != null)
            {
                transaction.Begin();

                EMDEntityCheck createdEntityCheck = entityCheckManager.AddEntity(employment);

                IEntityCheck entityCheckObject = entityCheckManager.GetEMDEntityWithIntervals(createdEntityCheck.Guid);
                Assert.IsTrue(foundEmploymentType.CheckIntervalInDays == entityCheckObject.CheckIntervalInDays && (foundEmploymentType.ReminderIntervalInDays.HasValue ? foundEmploymentType.ReminderIntervalInDays.Value : 0) == entityCheckObject.ReminderIntervalInDays);

                entityCheckObject = entityCheckManager.UpdateIntervals(employment);

                int reminderIntervalInDays = entityCheckObject.ReminderIntervalInDays;
                DateTime reminderDate = DateTime.Now.AddDays(reminderIntervalInDays);
                // set overdue
                int overdueIntervalInDays = entityCheckObject.CheckIntervalInDays + 1;


                DateTime dateReminderCheck = DateTime.Now;
                dateReminderCheck = dateReminderCheck.AddDays((entityCheckObject.CheckIntervalInDays / 2) + 1);

                Assert.IsTrue(reminderDate < dateReminderCheck);


                entityCheckManager.Reset(employment);

                transaction.Rollback();
            }
            else
            {
                Assert.Fail("No Employment found with a configured CheckInterval");
            }

        }
    }
}
