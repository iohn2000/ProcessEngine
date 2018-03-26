using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kapsch.IS.EDP.Core.Report;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using System.Collections.Generic;
using Kapsch.IS.Util.DataFile;
using System.Data;
using Kapsch.IS.Util.ErrorHandling;
using System.Text;
using System.Text.RegularExpressions;

namespace EDP20Core_UnitTest
{
    [TestClass]
    public class ReportTests
    {
        [TestMethod]
        [TestCategory("ReportTests")]
        public void Test_EquipmentReport()
        {
            //get an equipment to test report
            List<IEMDObject<EMDEquipmentDefinition>> eqdeList = new EquipmentDefinitionHandler().GetObjects<EMDEquipmentDefinition, EquipmentDefinition>();

            Random rnd = new Random();

            EMDEquipmentDefinition eqde1 = (EMDEquipmentDefinition)eqdeList[rnd.Next(0, eqdeList.Count)];

            EquipmentReport er = new EquipmentReport()
                .SetEquipmentGuid(eqde1.Guid)               
                .Query();                

            try
            {           
                if (er.QueryResult.Columns.Count > 0 && er.QueryResult.Rows.Count > 0)
                {

                    String exportFileName = "Export_" + eqde1.Name;
                    exportFileName = Regex.Replace(exportFileName, @"\s+", "_");
                    exportFileName += ".csv";

                    CSVFile exportFile = new CSVFile();
                    exportFile.FileEncoding = Encoding.UTF8;
                    exportFile.FixedLength = false;
                    exportFile.IncludeHeader = true;
                    exportFile.TextQualifier = null;

                    exportFile.StoreToFileSystem(exportFileName, er.QueryResult);                    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine ("Exception thrown while exporting dataset to csv.", ex);
                throw new BaseException(ErrorCodeHandler.E_JOB_EXECUTION_GENERAL);
            }                           
        }

        [TestMethod]
        [TestCategory("ReportTests")]
        public void Test_OrgUnitLevels()
        {
            OrgUnitLevels orgUnitLevels = new OrgUnitLevels()
                .Query();

            try
            {
                if (orgUnitLevels.QueryResult.Columns.Count > 0 && orgUnitLevels.QueryResult.Rows.Count > 0)
                {

                    String exportFileName = "OrgUnitLevels";
                    exportFileName = Regex.Replace(exportFileName, @"\s+", "_");
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
    }
}
