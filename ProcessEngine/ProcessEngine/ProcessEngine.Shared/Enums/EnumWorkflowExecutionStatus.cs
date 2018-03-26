using System;

namespace Kapsch.IS.ProcessEngine.Shared.Enums
{
    /// <summary>
    /// NotStarted = 0,
    /// Executing = 1,
    /// Sleeping = 2,
    /// Error = 3,     --> set by activity or process engine, WF with this status can be restarted
    /// Aborted = 4,   --> this status can only be set by user
    /// Paused = 5,    --> WF can be restarted
    /// Resumed = 6,
    /// Finish = 7,
    /// Reset = 8,
    /// Undefined = 9, 
    /// StopError = 10 --> set by activity or process engine - WF cannot be restarted
    /// </summary>
    public enum EnumWorkflowInstanceStatus
    {
        NotStarted = 0,
        Executing = 1,
        Sleeping = 2,
        Error = 3,
        Aborted = 4, 
        Paused = 5,
        Resumed = 6,
        Finish = 7,
        Reset = 8,
        Undefined = 9,
        StopError = 10 
    }
}
