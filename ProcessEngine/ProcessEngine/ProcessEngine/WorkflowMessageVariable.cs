using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine
{
    /// <summary>
    /// is used to transfer variables from core/gui to process engine
    /// </summary>
    [Serializable]
    public class WorkflowMessageVariable
    {
        /// <summary>
        /// name of the variable
        /// </summary>
        public string VarName { get; set; }
        /// <summary>
        /// value of variable
        /// </summary>
        public string VarValue { get; set; }

        /// <summary>
        /// emtpy contructor for serialization
        /// </summary>
        public WorkflowMessageVariable()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">name of variable</param>
        /// <param name="val">value of variable</param>
        public WorkflowMessageVariable(string name, string val)
        {
            this.VarName = name;
            this.VarValue = val;
        }
    }
}
