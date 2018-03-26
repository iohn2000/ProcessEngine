using System;
using System.Runtime.Serialization;

namespace Kapsch.IS.EDP.Core.Framework
{
    [Serializable]
    public class EntityFunctionException : Exception
    {
        public EntityFunctionException()
        {
        }

        public EntityFunctionException(string message) : base(message)
        {
        }

        public EntityFunctionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityFunctionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}