using System;

namespace Kapsch.IS.ProcessEngine.Shared.DataClasses
{
    public class WFEAsyncWaitItem
    {
            public int AWI_ID { get; set; }

            public string AWI_InstanceID { get; set; }

            public string AWI_ActivityInstanceID { get; set; }

            public string AWI_Status { get; set; }

            public DateTime? AWI_StartDate { get; set; }

            public DateTime? AWI_DueDate { get; set; }

            public DateTime? AWI_CompletedDate { get; set; }

            public string AWI_Config { get; set; }
    }
}
