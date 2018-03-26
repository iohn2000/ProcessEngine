using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Logic.Filter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace EDP20Core_UnitTest
{
    public class DBHelper
    {
        private static SqlConnection SQLCon_Target;
        public static void initDB()
        {
            SQLCon_Target = new SqlConnection(ConfigurationManager.ConnectionStrings["EMD_Direct"].ConnectionString);
        }
        
        public static void executeCommandInTargetDB(string CommandText)
        {
            SqlCommand SQLCom = new SqlCommand(CommandText, SQLCon_Target);
            SQLCom.Connection.Open();
            SQLCom.ExecuteNonQuery();
            SQLCom.Connection.Close();
        }
                    
    }
}
