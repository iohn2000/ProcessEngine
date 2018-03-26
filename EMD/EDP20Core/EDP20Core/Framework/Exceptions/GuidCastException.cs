using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class GuidCastException : BaseException
    {
        public string MismatchedGuid { get; }

        public GuidCastException(int error, string mismatchedGuid) : base(error)
        {
            MismatchedGuid = mismatchedGuid;
        }

        public GuidCastException(int error, string mismatchedGuid, string msg) : base(error, msg)
        {
            MismatchedGuid = mismatchedGuid;
        }
    }
}
