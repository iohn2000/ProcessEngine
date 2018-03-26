using Kapsch.IS.Util.Logging;

namespace Kapsch.IS.ProcessEngine
{
    /// <summary>
    /// class to extract information about workflow instance - finished or in progress
    /// to analyse errors or current status of workflow instance
    /// </summary>
    public class ProcessInstanceAnalyser
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Ideas :
        ///   -) show every step (ExecutionIterations) in a linear list sorted by call order
        ///   -) producte a graph that shows execution path and current activty (if in progress)
        ///   -) re-calculate workflow variables if not available in workflow, like activity properties (vars)
        ///   -) save all data in a data model
        ///   -) use nustache renderer to produce text or html file with infos (e.g. for log viewer)
        /// 
        /// Data Model :
        ///    -) Input Variables of worklflow (where GUIDs are resolved to readable names also, e.g. EffectedPerson Guid and Name)
        ///    -) List of ExecutionIterations
        ///    -) for Async Activities - AsyncWaitItem Daten anzeigen
        /// </summary>
        public ProcessInstanceAnalyser()
        {

        }

    }
}
