using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine
{
    public class TaskReturn
    {
        public string ReturnValue { get; set; }
        public EnumTaskStatus TaskState { get; set; }

        public TaskReturn (string returnVal, EnumTaskStatus taskState)
        {
            this.ReturnValue = returnVal;
            this.TaskState = taskState;
        }

    }
}
