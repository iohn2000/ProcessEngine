namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WorkflowXmlDataRepository")]
    public partial class WorkflowXmlDataRepository
    {
        [Key]
        [StringLength(37)]
        public string Guid { get; set; }
        [StringLength(37)]
        public string WFI_ID { get; set; }
        public string XML { get; set; }
        public DateTime Created { get; set; }
    }
}
