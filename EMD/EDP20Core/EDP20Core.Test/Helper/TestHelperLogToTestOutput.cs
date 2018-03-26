using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDP20Core.Test.Helper
{
    class TestHelperLogToTestOutput
    {
        // Test output - Success message
        public static void SuccessMessageToOutput()
        {
            Console.WriteLine("NUnit test executed successfully.");
        }

        // Test output - Fail message with Exception
        public static void FailMessageToOutput(Exception ex)
        {
            Console.WriteLine("NUnit test failed:");
            Console.WriteLine(ex);
        }
    }
}
