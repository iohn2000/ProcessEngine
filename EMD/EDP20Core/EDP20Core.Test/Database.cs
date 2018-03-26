using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDP20Core.Test
{
    public static class Database
    {
        public static readonly string BACKUP_NAME = @"TestMaster";
        public static readonly string BACKUP_PATH = @"E:\MSSQL13.MSSQLSERVER2016_UAT\MSSQL\Backup\";

        public static string backupFile
        {
            get => BACKUP_PATH + BACKUP_NAME + ".bak";
        }

        private static string logicalName = null;

        private static readonly string CMD_LOGICAL_NAME =
            @"USE [master]
              SELECT name FROM sys.master_files 
              WHERE database_id = DB_ID('{0}') and type_desc like 'ROWS'";

        private static readonly string CMD_BACKUP =
            @"USE [master]
              BACKUP DATABASE {0} TO DISK=N'{1}'";

        public static void Backup()
        {
            SqlConnectionStringBuilder conBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["TestMaster"].ConnectionString);
            string db = conBuilder.InitialCatalog;

            string logicalNameCmd = String.Format(CMD_LOGICAL_NAME, db);
            string backupCmd = String.Format(CMD_BACKUP, db, backupFile);


            using (SqlConnection con = new SqlConnection(conBuilder.ConnectionString))
            using (SqlCommand cmd = con.CreateCommand())
            {
                con.Open();
                try
                {
                    cmd.CommandText = logicalNameCmd;
                    string logicalBackupName = cmd.ExecuteScalar() as string;

                    cmd.CommandText = backupCmd;
                    cmd.ExecuteNonQuery();

                    logicalName = logicalBackupName;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static readonly string CMD_CREATE =
            @"USE [master]
              IF NOT EXISTS (SELECT * FROM sys.databases WHERE name like N'{0}')
                BEGIN
                    CREATE DATABASE [{0}]
                END;";

        /// <summary>
        /// 0 -> TestDB name
        /// 1 -> Backup path
        /// 2 -> logical name
        /// </summary>
        public static readonly string CMD_RESTORE =
            @"USE [master]
              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
              
              RESTORE DATABASE [{0}] FROM DISK = N'{1}'
              WITH FILE = 1,
              MOVE N'{2}' TO N'E:\MSSQL13.MSSQLSERVER2016_UAT\MSSQL\Data\{0}.mdf',
              MOVE N'{2}_log' TO N'E:\MSSQL13.MSSQLSERVER2016_UAT\MSSQL\Data\{0}_log.ldf',
              NOUNLOAD, REPLACE, STATS = 5

              ALTER DATABASE [{0}] SET MULTI_USER

              ALTER DATABASE [{0}] SET RECOVERY SIMPLE";

        public static void CreateTestDb()
        {
            if (logicalName == null) throw new InvalidOperationException("No backup");

            EntityConnectionStringBuilder entConStrBuilder = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["EMD_Entities"].ConnectionString);
            SqlConnectionStringBuilder conStrBuilder = new SqlConnectionStringBuilder(entConStrBuilder.ProviderConnectionString);

            string testInstanceDB = conStrBuilder.InitialCatalog;
            conStrBuilder.InitialCatalog = "master";

            string createCmd = String.Format(CMD_CREATE, testInstanceDB);
            string restoreCmd = String.Format(CMD_RESTORE, testInstanceDB, backupFile, logicalName);

            using (SqlConnection con = new SqlConnection(conStrBuilder.ConnectionString))
            using (SqlCommand cmd = con.CreateCommand())
            {
                con.Open();
                try
                {
                    cmd.CommandText = createCmd;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = restoreCmd;
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    con.Close();
                }
            }
        }

        private static readonly string CMD_DROP =
            @"USE [master]
              ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE

              DROP DATABASE [{0}]";

        public static void DropTestDb()
        {
            EntityConnectionStringBuilder entConStrBuilder = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["EMD_Entities"].ConnectionString);
            SqlConnectionStringBuilder conStr = new SqlConnectionStringBuilder(entConStrBuilder.ProviderConnectionString);

            string dropCmd = String.Format(CMD_DROP, conStr.InitialCatalog);
            conStr.InitialCatalog = "master";

            using (SqlConnection con = new SqlConnection(entConStrBuilder.ProviderConnectionString))
            using (SqlCommand cmd = con.CreateCommand())
            {
                con.Open();
                try
                {
                    cmd.CommandText = dropCmd;
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    con.Close();
                }
            }
        }
    }
}
