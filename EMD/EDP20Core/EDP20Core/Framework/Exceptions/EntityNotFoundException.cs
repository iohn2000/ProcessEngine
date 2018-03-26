using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class EntityNotFoundException : BaseException
    {
        public string EntityClassName { get; set; }

        public EntityNotFoundException(int errorCode) : base(errorCode)
        {

        }

        public EntityNotFoundException(int errorCode, string message) : base(errorCode, message)
        {

        }

        public EntityNotFoundException(string entityClassName, int errorCode, string message) : base(errorCode, message)
        {
            this.EntityClassName = entityClassName;
        }
    }
}
