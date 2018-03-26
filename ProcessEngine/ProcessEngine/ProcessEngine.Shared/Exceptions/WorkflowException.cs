using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using System;
using System.Collections.Generic;

namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{
    public enum WorkflowErrorType
    {
        NotWellformed,
        WorkflowNotFound,
        RuleViolation,
        WorkflowNotCheckedOut
    }

    public class WorkflowException : BaseWorkflowException
    {
        public List<WorkflowError> errorMessages = new List<WorkflowError>();

        public WorkflowException() : base()
        {

        }

        public WorkflowException(string msg) : base(msg)
        {

        }
        public WorkflowException(string msg, Exception innerException) : base(msg, innerException)
        {

        }
        public WorkflowException(string msg, Exception innerException, WorkflowErrorType errType) : base(msg, innerException)
        {
            this.ErrorType = errType;
        }

        public WorkflowException(string msg, List<WorkflowError> errorMessages,  WorkflowErrorType errType) : base (msg)
        {
            this.ErrorType = errType;
            this.errorMessages = errorMessages;
        }


        public WorkflowErrorType ErrorType { get; set; }
    }
}
