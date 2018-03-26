using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class EntityNotAllowedException : BaseException
    {
        public EnumEntityNotAllowedError EntityNotAllowedExceptionType { get; private set; }

        public string EntityClassName { get; set; }

        public EntityNotAllowedException(int errorCode) : base(errorCode)
        {

        }

        public EntityNotAllowedException(int errorCode, string message) : base(errorCode, message)
        {

        }

        public EntityNotAllowedException(string entityClassName, EnumEntityNotAllowedError entityNotAllowedExceptionType, int errorCode, string message) : base(errorCode, message)
        {
            this.EntityClassName = entityClassName;
            this.EntityNotAllowedExceptionType = entityNotAllowedExceptionType;
        }
    }

    public enum EnumEntityNotAllowedError
    {
        EntityAllowedOnlyOnceForSelectedParameters,
        EntityAllowedOnlyOnce,
        EntityNotAllowed
    }
}
