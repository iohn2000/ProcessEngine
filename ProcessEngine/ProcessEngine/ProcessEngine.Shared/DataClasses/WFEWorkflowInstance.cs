using System;

namespace Kapsch.IS.ProcessEngine.Shared.DataClasses
{
    public class WFEWorkflowInstance
    {
        public string WFI_ID { get; set; }

        public string WFI_WFD_ID { get; set; }

        public string WFI_Xml { get; set; }

        public string WFI_Status { get; set; }

        public string WFI_CurrentActivity { get; set; }

        public string WFI_NextActivity { get; set; }

        public DateTime? WFI_Created { get; set; }

        public DateTime? WFI_Updated { get; set; }

        public DateTime? WFI_Finished { get; set; }

        public int? WFI_ProcessTime { get; set; }

        public string WFI_ParentWF { get; set; }
    }
}
