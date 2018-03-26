namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("AsyncWaitItem")]
    public partial class AsyncWaitItem
    {
        [Key]
        public int AWI_ID { get; set; }
        [StringLength(255)]
        public string AWI_InstanceID { get; set; }
        [StringLength(255)]
        public string AWI_ActivityInstanceID { get; set; }
        [StringLength(50)]
        public string AWI_Status { get; set; }
        public DateTime? AWI_StartDate { get; set; }
        public DateTime? AWI_DueDate { get; set; }
        public DateTime? AWI_CompletedDate { get; set; }
        public string AWI_Config { get; set; }
        public DateTime AWI_Created { get; set; }
        public DateTime? AWI_Modified { get; set; }
    }
}
