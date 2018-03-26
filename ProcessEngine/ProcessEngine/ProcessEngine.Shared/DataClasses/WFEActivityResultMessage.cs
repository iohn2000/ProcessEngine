using System;

namespace Kapsch.IS.ProcessEngine.Shared.DataClasses
{
    public class WFEActivityResultMessage
    {
        public Guid ARM_ID { get; set; }
        public string ARM_Woin { get; set; }
        public string ARM_ActivityInstanceId { get; set; }
        public string ARM_ResultMessage { get; set; }
        public DateTime ARM_Created { get; set; }
    }
}