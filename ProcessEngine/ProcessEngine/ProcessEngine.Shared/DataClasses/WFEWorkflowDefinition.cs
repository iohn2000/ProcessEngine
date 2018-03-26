using System;

namespace Kapsch.IS.ProcessEngine.Shared.DataClasses
{
    public class WFEWorkflowDefinition
    {
        public Guid Guid { get; set; }

        public string WFD_ID { get; set; }

        public string WFD_Name { get; set; }

        public string WFD_Definition { get; set; }

        public string WFD_Description { get; set; }

        public int WFD_Version { get; set; }

        public string WFD_CheckedOutBy { get; set; }

        public DateTime WFD_Created { get; set; }

        public DateTime? WFD_ValidFrom { get; set; }

        public DateTime? WFD_ValidTo { get; set; }
    }
}
