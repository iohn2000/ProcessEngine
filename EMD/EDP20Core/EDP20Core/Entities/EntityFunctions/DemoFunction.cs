using Kapsch.IS.EDP.Core.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities.EntityFunctions
{
    public class DemoFunction : EntityFunction<DemoFunction>
    {
        /// <summary>
        /// implementation of Call for NullFunction
        /// </summary>
        /// <param name="myParameter"></param>
        /// <returns></returns>
        protected override DemoFunction Worker()
        {
            if (String.IsNullOrEmpty(this.Parameter))
            {
                throw new EntityFunctionException("the given Parameter for this EntityFunction must not be null");
            }
            base.Result = "worked on " + this.Parameter + " with Guid " + this.EntityGuid;
            return this;
        }
    }
}
