using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.HelperExtensions
{
    public class Configuration
    {

        private static string versionNumber;
        private static string coreversionNumber;
        private static string webversionNumber;
        private static string webversionDate;
        private static string database;





        public static string VERSION
        {
            get
            {
                if (versionNumber == null)
                {
                    versionNumber = System.Configuration.ConfigurationManager.AppSettings["EMD20Web.BrowserCacheNumber"];
                }
                return versionNumber;
            }
        }

        public static string WEBVERSION
        {
            get
            {
                if (webversionNumber == null)
                {
                    webversionNumber = AssemblyName.GetAssemblyName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Kapsch.IS.EMD.EMD20Web.dll")).Version.ToString();
                }
                return webversionNumber;
            }
        }

        public static string WEBVERSIONDATE
        {
            get
            {
                if (webversionDate == null)
                {
                    webversionDate = System.IO.File.GetLastWriteTime(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Kapsch.IS.EMD.EMD20Web.dll")).ToShortDateString();
                }
                return webversionDate;
            }
        }

        public static string COREVERSION
        {
            get
            {
                if (coreversionNumber == null)
                {
                    coreversionNumber = AssemblyName.GetAssemblyName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Kapsch.IS.EDP.Core.dll")).Version.ToString();
                }
                return coreversionNumber;
            }
        }

        public static string DATABASENAME
        {
            get
            {
                if (database == null)
                {
                    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["EMD_Entities"].ConnectionString;

                    int startIndex = connectionString.IndexOf("catalog=") + 8;
                    int endIndex = connectionString.IndexOf(';', startIndex);

                    database = connectionString.Substring(startIndex, endIndex - startIndex);
                }
                return database;
            }
        }
    }
}