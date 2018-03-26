namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WorkflowDefinition")]
    public partial class WorkflowDefinition
    {
        [Key]
        public Guid Guid {get; set; }

        [StringLength(255)]
        public string WFD_ID { get; set; }

        [Required]
        [StringLength(255)]
        public string WFD_Name { get; set; }

        [Required]
        public string WFD_Definition { get; set; }

        [StringLength(255)]
        public string WFD_Description { get; set;}

        [Required]
        public int WFD_Version { get; set; }

        public string WFD_CheckedOutBy { get; set; }

        public DateTime WFD_Created { get; set; }

        public DateTime? WFD_ValidFrom { get; set; }

        public DateTime? WFD_ValidTo { get; set; }
    }
}
