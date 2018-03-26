using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Shared.Enums
{
    [Obsolete("dupllicate", true)]
    public enum EnumWorkflowInstanceStatus_old
    {
        NotStarted,
        Executing,
        Sleeping,
        Error,
        Aborted,
        Paused,
        Resumed,
        Finish,
        Reset,
        Undefined
    }
}
