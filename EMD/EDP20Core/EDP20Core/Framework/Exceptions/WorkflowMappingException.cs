using Kapsch.IS.EDP.Core.WF.Message;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Framework.Exceptions
{
    public class WorkflowMappingException : BaseException
    {
        /// <summary>
        /// i.e. EMPL
        /// </summary>
        public string Prefix { get; set; }
        public WorkflowAction Method { get; set; }

        public string EntityClassName { get; set; }

        public WorkflowMappingException(int errorCode) : base(errorCode)
        {

        }

        public WorkflowMappingException(string prefix, WorkflowAction method, int errorCode, string message, Exception ex) : base(errorCode, message, ex)
        {
            this.Prefix = prefix;
            this.Method = method;
        }


    }


}
