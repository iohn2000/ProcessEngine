using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity.TaskDecision
{
    public class AsyncTaskRequestResult : BaseAsyncRequestResult
    {
        public AsyncTaskRequestResult(string returnVal, ProcessEngine.Shared.Enums.EnumTaskStatus taskState)
        {
            this.ReturnValue = returnVal;
            this.TaskState = taskState;
            this.DetailedMessage = "";
        }
        public DateTime? NextReminderDate { get; set; }
        public string ReturnValue { get; set; }
        public ProcessEngine.Shared.Enums.EnumTaskStatus TaskState { get; set; }
        public string DetailedMessage { get; set; }
        /// <summary>
        /// the task guid; if multiple tasks are linked, any of the task guids will suffice
        /// </summary>
        public string OneTaskGuid { get; internal set; }
    }
}
