using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>   Interface for a workflow step. Use this to implement steps
    ///             for the process engine.</summary>
    ///
    /// <remarks>   Fleckj, 06.02.2015. </remarks>
    #endregion
    public interface IProcessStep
    {
        #region Documentation -----------------------------------------------------------------------------
        /// <summary>   This function is called by the WF engine. Implement this function to 
        ///             excute the code you need for your step to do what it is supposed to do. </summary>
        ///
        /// <param name="engineContext">    Context for the engine. <see cref="EngineContext"/></param>
        ///
        /// <returns>   A StepReturn. </returns>
        #endregion
        StepReturn Execute(EngineContext engineContext);

        /// <summary>
        /// is called first when an activity was instanciated. Here we do 
        /// - read input variables 
        /// - set equivalent internal properties
        /// - do polling when asynchronous
        /// Allowed Stepstates are correctly handled in BaseActivity
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        StepReturn Initialize(EngineContext engineContext);

        /// <summary>
        /// is called after Initialization was done.
        /// - does complete business-logic of the activity
        /// - do NOT read input data from anywhere
        /// - do NOT set output vars.
        /// Allowed Stepstates are correctly handled in BaseActivity
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        StepReturn Run(EngineContext engineContext);

        /// <summary>
        /// is called after Activvity ran.
        /// - do write Output Vars here.
        /// Allowed Stepstates are correctly handled in BaseActivity
        /// </summary>
        /// <param name="engineContext"></param>
        /// <returns></returns>
        StepReturn Finish(EngineContext engineContext);

    }
}
