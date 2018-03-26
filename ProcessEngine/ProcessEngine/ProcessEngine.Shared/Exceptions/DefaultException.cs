using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{
    public enum DefaultErrorType
    {
        GeneralError,
        NameAlreadyExists
    }

    public class DefaultException : BaseWorkflowException
    {
        public DefaultErrorType ErrorType { get; set; }

        public DefaultException() : base()
        {

        }

        public DefaultException(string msg) : base(msg)
        {

        }
        public DefaultException(string msg, Exception innerException) : base(msg, innerException)
        {

        }

        public DefaultException(string msg, Exception innerException, DefaultErrorType errType) : base(msg, innerException)
        {
            this.ErrorType = errType;
        }

        public DefaultException(string msg, DefaultErrorType errType) : base(msg)
        {
            this.ErrorType = errType;
        }
    }
}
