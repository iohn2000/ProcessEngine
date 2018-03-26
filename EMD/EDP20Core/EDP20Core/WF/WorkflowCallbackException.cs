using System;
using System.Runtime.Serialization;

namespace Kapsch.IS.EDP.Core.WF
{
    [Serializable]
    internal class WorkflowCallbackException : Exception
    {
        public WorkflowCallbackException()
        {
        }

        public WorkflowCallbackException(string message) : base(message)
        {
        }

        public WorkflowCallbackException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WorkflowCallbackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}