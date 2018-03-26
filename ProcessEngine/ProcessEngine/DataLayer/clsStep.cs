using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   The cls step represents a definition of a process step. 
    ///             inlcuding namespace, dll, returnvalues, etc ...</summary>
    ///
    /// <remarks>   Fleckj, 06.02.2015. </remarks>
    #endregion
    public class clsStep
    {
        public int StepId { get; set; }
        public string StepType { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set;}
        public string StepNamespace { get; set; }
        public string DLLPath { get; set; }
    }
}
