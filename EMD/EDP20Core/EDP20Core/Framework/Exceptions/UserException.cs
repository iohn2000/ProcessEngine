using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class UserException : BaseException
    {
        public EnumUserExceptionType UserExceptionType { get; set; }

        public UserException(int errorCode, EnumUserExceptionType userExceptionType) : base(errorCode)
        {
            this.UserExceptionType = userExceptionType;
        }

        public UserException(int errorCode, string message, EnumUserExceptionType userExceptionType) : base(errorCode, message)
        {
            this.UserExceptionType = userExceptionType;
        }
    }

    public enum EnumUserExceptionType
    {
        General,
        UserExists,
        UserNotFound
    }
}
