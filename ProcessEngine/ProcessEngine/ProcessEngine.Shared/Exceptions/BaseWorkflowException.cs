using System;

namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{
    public abstract class BaseWorkflowException : Exception
    {
        /// <summary>
        /// Line Number in Xml Document
        /// </summary>
        public int LineNumberStart { get; set; }


        public int LineNumberEnd { get; set; }

        public BaseWorkflowException() : base()
        {

        }

        public BaseWorkflowException(string msg) : base(msg)
        {

        }

        public BaseWorkflowException(string msg, Exception innerException) : base(msg, innerException)
        {

        }
    }
}
