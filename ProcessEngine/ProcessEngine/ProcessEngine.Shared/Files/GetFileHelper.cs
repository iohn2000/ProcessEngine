using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Shared.Files
{
    public static class GetFileHelper
    {
        private const string workflowSchemaRelativePath = "Files/workflowDefinition.xsd";

        public static string WorkflowValidationSchema(bool isWebService = false)
        {
            return File.ReadAllText(GetSchemaFilePath(isWebService));
        }
        public static string GetWorkflowValidationSchemaPath(bool isWebService = false)
        {
            return GetSchemaFilePath(isWebService);
        }

        private static string GetSchemaFilePath(bool isWebservice)
        {
            if (isWebservice)
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", workflowSchemaRelativePath);
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, workflowSchemaRelativePath);
        }
    }
}
