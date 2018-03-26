using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{

    public enum WorkflowInstanceErrorType
    {
        InvalidWorkflowStatusToWakeup,
        CannotSave,
        CannotLoad
    }

    public class WorkflowInstanceException : BaseWorkflowException
    {
        public WorkflowInstanceErrorType ErrorType { get; set; }
        
        public WorkflowInstanceException() : base()
        {

        }

        public WorkflowInstanceException(string msg) : base(msg)
        {

        }
        public WorkflowInstanceException(string msg, Exception innerException) : base(msg, innerException)
        {

        }

        public WorkflowInstanceException(string msg, Exception innerException, WorkflowInstanceErrorType errType) : base(msg, innerException)
        {
            this.ErrorType = errType;
        }


    }
}
