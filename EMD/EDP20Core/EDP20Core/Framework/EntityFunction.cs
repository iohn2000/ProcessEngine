using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework
{
    /// <summary>
    /// EntityFunctions, also called "@@Functions" are functions to be called directly within "@@Queries" implemented in @ref:EntityQuery
    /// </summary>
    public abstract class EntityFunction<S> where S : EntityFunction<S>
    {
        public string EntityGuid { get; private set; }
        public string Parameter { get; private set; }
        public string Result { get; internal set; }

        /// <summary>
        /// This is the method to be called and doing the work. 
        /// It is implemented in the Function class derived from EntityFunction
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected abstract S Worker();

        /// <summary>
        /// Constructor for this specific entityfunction containing the EMDObject to be handled for standard behaviour
        /// </summary>
        protected EntityFunction() 
        {

        }

        /// <summary>
        /// method which is called from EntityQuery
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public S Call(string entityGuid, string parameter) {
            this.EntityGuid = entityGuid;
            this.Parameter = parameter;
            try
            {
                this.Worker();
            } catch (Exception e)
            {
                throw new EntityFunctionException("EntityFunction class "+((S)this).GetType()+ToString()+"threw Exception", e);
            }
            return (S)this;

        }

    }
}
