using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace EDP20Core.Test
{
    [SetUpFixture]
    public class TestSetup
    {

        [OneTimeSetUp]
        public void Setup()
        {
            //Database.Backup();

            //Database.CreateTestDb();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            //Database.DropTestDb();

            //SqlConnectionStringBuilder con = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["TestMaster"].ConnectionString);
            //string server = @"\\" + con.DataSource;

            //string backupPath = server + @".kapsch.co.at\" + Database.BACKUP_PATH + Database.BACKUP_NAME + ".bak";
            //backupPath = backupPath.Replace("E:", "e$");

            //FileInfo backupFile = new FileInfo(backupPath);
            //backupFile.Delete();
        }
    }
}