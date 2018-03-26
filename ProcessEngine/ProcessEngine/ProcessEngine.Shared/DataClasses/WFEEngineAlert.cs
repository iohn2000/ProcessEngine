using System;

namespace Kapsch.IS.ProcessEngine.Shared.DataClasses
{

    public class WFEEngineAlert
    {
        public int EA_ID { get; set; }

        public string EA_WFI_ID { get; set; }

        public string EA_StartActivity { get; set; }

        public string EA_InputParameters { get; set; }

        public Guid EA_ProcessedByServer { get; set; }

        public DateTime EA_Created { get; set; }

        public DateTime EA_Updated { get; set; }

        public string EA_Status { get; set; }

        public string EA_CallbackID { get; set; }

        public string EA_Type { get; set; }

        public DateTime? EA_LastPolling { get; set; }

        public int? EA_PollingIntervalSeconds { get; set; }

        public Guid? EA_LockedByProcess { get; set; }

        public string EA_ProcessEngineInstance { get; set; }
    }
}

