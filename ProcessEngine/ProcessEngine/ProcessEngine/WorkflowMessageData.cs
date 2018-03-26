using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine
{
    /// <summary>
    /// Holds a list of WorkflowMessageVariables.
    /// Is used to transfer Variables from Core/Gui to ProcessEngine
    /// </summary>
    [Serializable]
    public class WorkflowMessageData
    {
        /// <summary>
        /// workflow definition id 
        /// </summary>
        public string WorkflowDefinitionID { get; set; }

        /// <summary>
        /// collection of workflow variables; can also be serialized XElements
        /// </summary>
        public List<WorkflowMessageVariable> WorkflowVariables { get; set; }

        /// <summary>
        /// additional data in xml format. 
        /// This is a simple storing and viewing, no business logic or similar will be apllied to it
        /// </summary>
        public string XmlData { get; set; }

        /// <summary>
        /// empty ctor for serialization
        /// </summary>
        public WorkflowMessageData()
        {
            this.WorkflowVariables = new List<WorkflowMessageVariable>();
        }


    }
}
