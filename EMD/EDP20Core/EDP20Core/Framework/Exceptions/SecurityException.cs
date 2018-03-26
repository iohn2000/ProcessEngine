using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class EdpSecurityException : BaseException
    {
        public EdpEnumSecurityError SecurityError { get; private set; }

        public string EntityClassName { get; set; }

        public EdpSecurityException(int errorCode) : base(errorCode)
        {

        }

        public EdpSecurityException(int errorCode, string message) : base(errorCode, message)
        {

        }

        public EdpSecurityException(string entityClassName, EdpEnumSecurityError securityError, int errorCode, string message) : base(errorCode, message)
        {
            this.EntityClassName = entityClassName;
            this.SecurityError = securityError;
        }
    }

    public enum EdpEnumSecurityError
    {
        OnlyAdmin,
        OnlyPrime
    }
}
