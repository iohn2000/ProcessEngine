using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   Every activity (step) must return a status 
    ///             and a return value. </summary>
    ///
    /// <remarks>   Fleckj, 06.02.2015. </remarks>
    #endregion
    public class StepReturn
    {
        public string ReturnValue { get; set; }
        public EnumStepState StepState { get; set; }
        public string DetailedDescription { get; set; }

        public StepReturn(string returnVal, EnumStepState stepState)
        {
            this.ReturnValue = returnVal;
            this.StepState = stepState;
            this.DetailedDescription = "";
        }

        public StepReturn()
        {
            this.ReturnValue = "";
            this.StepState = EnumStepState.Complete;
            this.DetailedDescription = "";
        }
    }
}
