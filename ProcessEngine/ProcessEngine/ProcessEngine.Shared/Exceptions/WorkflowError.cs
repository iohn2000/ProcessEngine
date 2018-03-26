using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{
    [Serializable]
    public class WorkflowError
    {
        public string Message { get; set; }
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }


        public WorkflowError(string message, int lineNumber, int linePosition)
        {
            this.Message = message;
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;

        }
    }
}
