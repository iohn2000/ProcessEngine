using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Entities;
using System.Collections.Generic;
using Kapsch.IS.EDP.Core.DB;
using System.Linq;
using System.Data;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class TransactionTest
    {
        [TestMethod, TestCategory("Transaction")]
        public void TestSimpleTransaction()
        {
            CoreTransaction trans = new CoreTransaction();
            UserDomainHandler userDomainHandler = new UserDomainHandler(null, "SimpleTransactiontest");
            string usdoGuid = (string)userDomainHandler.GetObjects<EMDUserDomain, UserDomain>("Name = \"kapsch.co.at\"").FirstOrDefault().Guid;


            trans.Begin();

            int updated = 0;

            using (UserHandler userHandler = new UserHandler(trans, null, "SimpleTransactiontest"))
            {
                IEnumerable<EMDUser> allUsers = userHandler.GetObjects<EMDUser, User>().Cast<EMDUser>();
                List<EMDUser> usersFromDB = (from usr in allUsers where usr.Username != "*keine" select usr).ToList();

                foreach (EMDUser emdUser in usersFromDB)
                {
                    if (emdUser.USDO_Guid != usdoGuid)
                    {
                        emdUser.USDO_Guid = usdoGuid;
                        userHandler.UpdateObject(emdUser);
                        updated++;
                        if (updated >= 20)
                        {
                            break;
                        }
                    }
                }
            }

            trans.Rollback();

            Assert.IsTrue(updated >= 20);
        }


        [TestMethod, TestCategory("Transaction")]
        public void TestMultipleTransactionRead()
        {
            CoreTransaction transWrite = new CoreTransaction();
            CoreTransaction transRead = new CoreTransaction();
            UserDomainHandler userDomainHandler = new UserDomainHandler(null, "SimpleTransactiontest");
            string usdoGuid = (string)userDomainHandler.GetObjects<EMDUserDomain, UserDomain>("Name = \"kapsch.co.at\"").FirstOrDefault().Guid;

            Exception handledException = null;
            int updated = 0;
            try
            {
                UserHandler userHandlerReadOnly = new UserHandler(transRead);
                transRead.Begin();
                transWrite.Begin();

                using (UserHandler userHandler = new UserHandler(transWrite, null, "SimpleTransactiontest"))
                {
                    IEnumerable<EMDUser> allUsers = userHandler.GetObjects<EMDUser, User>().Cast<EMDUser>();
                    List<EMDUser> usersFromDB = (from usr in allUsers where usr.Username != "*keine" select usr).ToList();

                    foreach (EMDUser emdUser in usersFromDB)
                    {
                        EMDUser readUser = (EMDUser)userHandlerReadOnly.GetObject<EMDUser>(emdUser.Guid);

                        updated++;
                        if (updated >= 20)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                handledException = ex;
            }

            transRead.Commit();
            transWrite.Rollback();

            if (handledException != null)
            {
                Assert.Fail("Exception thrown: {0}", handledException.StackTrace);
            }
            else
            {
                Assert.IsTrue(updated >= 20);
            }
        }



        [TestMethod, TestCategory("Transaction")]
        public void TestMultipleTransactionReadWithoutLocalTrans()
        {
            UserDomainHandler userDomainHandler = new UserDomainHandler(null, "SimpleTransactiontest");
            string usdoGuid = (string)userDomainHandler.GetObjects<EMDUserDomain, UserDomain>("Name = \"kapsch.co.at\"").FirstOrDefault().Guid;

            Exception handledException = null;
            int updated = 0;
            try
            {
                UserHandler userHandlerReadOnly = new UserHandler();


                using (UserHandler userHandler = new UserHandler(null, null, "SimpleTransactiontest"))
                {
                    IEnumerable<EMDUser> allUsers = userHandler.GetObjects<EMDUser, User>().Cast<EMDUser>();
                    List<EMDUser> usersFromDB = (from usr in allUsers where usr.Username != "*keine" select usr).ToList();

                    foreach (EMDUser emdUser in usersFromDB)
                    {
                        EMDUser readUser = (EMDUser)userHandlerReadOnly.GetObject<EMDUser>(emdUser.Guid);

                        updated++;
                        if (updated >= 20)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                handledException = ex;
            }


            if (handledException != null)
            {
                Assert.Fail("Exception thrown: {0}", handledException.StackTrace);
            }
            else
            {
                Assert.IsTrue(updated >= 20);
            }
        }

        [TestMethod, TestCategory("Transaction")]
        public void TestOneTransactionReadWithGlobalTrans()
        {
            CoreTransaction transRead = new CoreTransaction();
            UserDomainHandler userDomainHandler = new UserDomainHandler(transRead, "SimpleTransactiontest");
            string usdoGuid = (string)userDomainHandler.GetObjects<EMDUserDomain, UserDomain>("Name = \"kapsch.co.at\"").FirstOrDefault().Guid;

            Exception handledException = null;
            List<EMDUser> users = null;

            try
            {
                UserHandler userHandlerReadOnly = new UserHandler(transRead);
                users = userHandlerReadOnly.GetObjects<EMDUser, User>().Cast<EMDUser>().ToList();


            }
            catch (Exception ex)
            {
                handledException = ex;
            }


            if (handledException != null)
            {
                Assert.Fail("Exception thrown: {0}", handledException.StackTrace);
            }
            else
            {
                Assert.IsTrue(users != null);
            }
        }

        [TestMethod, TestCategory("Transaction")]
        public void TestGlobalTransaction()
        {
            int updatedCounter = 0;
            Exception handledException = null;

            using (PackageTransaction trans = new PackageTransaction())
            using (UserHandler userHandler = new UserHandler(trans, null, "Job - EDP_SyncUsersWithAD"))
            using (UserDomainHandler userDomainHandler = new UserDomainHandler(null, "Job - EDP_SyncUsersWithAD"))
            {
                string usdoGuid = (string)userDomainHandler.GetObjects<EMDUserDomain, UserDomain>("Name = \"kapsch.co.at\"").FirstOrDefault().Guid;

                try
                {

                    // EMD_Entities DB_Context = new EMD_Entities();
                    trans.Begin();
                    IEnumerable<EMDUser> allUsers = userHandler.GetObjects<EMDUser, User>().Cast<EMDUser>();
                    List<EMDUser> usersFromDB = (from usr in allUsers where usr.Username != "*keine" select usr).ToList().Cast<EMDUser>().Take(10).ToList();




                    // Debugging
                    // usersFromDB = (from usr in usersFromDB where usr.Username.IndexOf("MUELLERA", StringComparison.OrdinalIgnoreCase) >= 0 select usr).ToList();

                    int itemCounter = usersFromDB.Count();


                    System.Diagnostics.Debug.WriteLine(string.Format("Found {0} users ", usersFromDB.Count));
                    foreach (EMDUser emdUser in usersFromDB)
                    {
                        EMDUser user = (EMDUser)userHandler.UpdateObject(emdUser);
                        System.Diagnostics.Debug.WriteLine(string.Format("Updated user: {0}", user.Guid));
                        updatedCounter++;
                    } //foreach

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    handledException = ex;
                    trans.Rollback();
                }
            }



            if (handledException != null)
            {
                Assert.Fail("Exception thrown: {0}", handledException.StackTrace);
            }
            else
            {
                Assert.IsTrue(updatedCounter == 10);
            }
        }
    }


    public class PackageTransaction
      : CoreTransaction
      , IDisposable
    {

        private int counter;
        private int maximum;

        public PackageTransaction()
            : this(100000)
        {
        }

        public PackageTransaction(int maximum)
        {
            this.counter = 0;
            this.Maximum = maximum; //make sure to use property to perform validity-checks
            base.SetAutoDetectChanges(false);
            base.SetValidationOnSave(false); //Core ensures that only valid entities are written to db.
        }

        public int Maximum
        {
            get { return this.maximum; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("Maximum must greater than 0.");
                if (this.maximum != value)
                {
                    base.saveChanges();
                    this.counter = 0;
                    this.maximum = value;
                }
            }
        }

        public override void saveChanges()
        {
            if (this.counter >= this.Maximum)
            {
                base.saveChanges();
                this.counter = 0;
            }
            else
            {
                this.counter++;
            }
        }

        public void SaveChangesNow()
        {
            base.saveChanges();
            this.counter = 0;
        }

        public override void Rollback()
        {
            this.counter = 0;
            base.Rollback();
        }
        public override void Commit()
        {
            this.SaveChangesNow();
            this.counter = 0;
            base.Commit();
        }

        protected override void Dispose(bool disposing)
        {
            this.counter = 0;
            this.SetAutoDetectChanges(true);
            this.SetValidationOnSave(true);
            base.Dispose(disposing);
        }
    }
}


