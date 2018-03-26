using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class NotInitializedException : BaseException
    {
        public NotInitializedException(int errorCode) : base(errorCode)
        {

        }

        public NotInitializedException(int errorCode, string message) : base(errorCode, message)
        {

        }
    }
}
