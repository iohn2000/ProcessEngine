using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Framework;
using Kapsch.IS.EDP.Core.Report;
using System.Text.RegularExpressions;
using Kapsch.IS.Util.DataFile;
using System.Reflection;
using System.IO;

namespace EDP20Core.Test.Reports
{
    [TestFixture(),Category("Reports")]
    public class ReportTests
    {
        [TestCase()]
        public void Test_EquipmentReport()
        {
            bool hasError = false;
            try
            {
                //get an equipment to test report
                List<IEMDObject<EMDEquipmentDefinition>> eqdeList = new EquipmentDefinitionHandler().GetObjects<EMDEquipmentDefinition, EquipmentDefinition>();

                Random rnd = new Random();

                EMDEquipmentDefinition eqde1 = (EMDEquipmentDefinition)eqdeList[rnd.Next(0, eqdeList.Count)];

                EquipmentReport er = new EquipmentReport()
                .SetEquipmentGuid(eqde1.Guid)
                .Query();

                if (er.QueryResult.Columns.Count > 0 && er.QueryResult.Rows.Count > 0)
                {
                    String exportFileName = "Export_" + eqde1.Name;
                    exportFileName = Regex.Replace(ReportsDirectory + exportFileName, @"\s+", "_");
                    exportFileName += ".csv";

                    CSVFile exportFile = new CSVFile();
                    exportFile.FileEncoding = Encoding.UTF8;
                    exportFile.FixedLength = false;
                    exportFile.IncludeHeader = true;
                    exportFile.TextQualifier = null;

                    exportFile.StoreToFileSystem(exportFileName, er.QueryResult);

                    FileStream fs = File.OpenRead(exportFileName);

                }
            }
            catch (Exception ex)
            {
                hasError = true;
                System.Diagnostics.Debug.WriteLine("Exception thrown while exporting dataset to csv.", ex);
            }




            Assert.AreEqual(hasError, false);
        }


        [TestCase()]
        public void Test_OrgUnitLevels()
        {
            OrgUnitLevels orgUnitLevels = new OrgUnitLevels()
                .Query();

            try
            {
                if (orgUnitLevels.QueryResult.Columns.Count > 0 && orgUnitLevels.QueryResult.Rows.Count > 0)
                {

                    String exportFileName = "OrgUnitLevels";
                    exportFileName = Regex.Replace(ReportsDirectory + exportFileName, @"\s+", "_");
                    exportFileName += ".csv";

                    CSVFile exportFile = new CSVFile();
                    exportFile.FileEncoding = Encoding.UTF8;
                    exportFile.FixedLength = false;
                    exportFile.IncludeHeader = true;
                    exportFile.TextQualifier = null;

                    exportFile.StoreToFileSystem(exportFileName, orgUnitLevels.QueryResult);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception thrown while exporting dataset to csv.", ex);
                throw new BaseException(ErrorCodeHandler.E_JOB_EXECUTION_GENERAL);
            }
        }


        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string ReportsDirectory
        {
            get
            {
                DirectoryInfo di = Directory.GetParent(AssemblyDirectory);
                return di.Parent.FullName + "\\Reports\\Testfiles\\";
            }
        }
    }
}
