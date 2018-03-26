using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework
{
    [Serializable]
    public class SecurityException : Exception
    {
        private int m_ErrorCode = 0;

        public int ErrorCode { get { return m_ErrorCode; } private set { m_ErrorCode = ErrorCode; } }



        public SecurityException(int ErrorCode) : base(EDPSecurityErrorCodeHandler.GetMessage(ErrorCode)) { init(ErrorCode); }

        protected SecurityException(System.Runtime.Serialization.SerializationInfo serializationInfo,
            System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
            { //TODO implement specific serialization
            m_ErrorCode = ErrorCode;
        }

        private void init(int code)
        {
            m_ErrorCode = code;
            // moved logging to loggerinstance so an Exception is only logged if calling method wants to log.
        }
    }
}
