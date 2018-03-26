namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WorkflowInstance")]
    public partial class WorkflowInstance
    {
        [Key]
        [StringLength(255)]
        public string WFI_ID { get; set; }

        [Required]
        [StringLength(255)]
        public string WFI_WFD_ID { get; set; }

        [Required]
        public string WFI_Xml { get; set; }

        [StringLength(50)]
        public string WFI_Status { get; set; }

        //public Guid WFI_LockedByProcess { get; set; }

        [StringLength(255)]
        public string WFI_CurrentActivity { get; set; }

        [StringLength(37)]
        public string WFI_ParentWF { get; set; }

        [StringLength(255)]
        public string WFI_NextActivity { get; set; }

        public DateTime? WFI_Created { get; set; }

        public DateTime? WFI_Updated { get; set; }

        public DateTime? WFI_Finished { get; set; }

        public int? WFI_ProcessTime { get; set; }
    }
}
