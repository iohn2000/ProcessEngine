namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ActivityResultMessage")]
    public partial class ActivityResultMessage
    {
        [Key]
        public Guid ARM_ID { get; set; }
        [StringLength(255)]
        public string ARM_Woin { get; set; }
        [StringLength(150)]
        public string ARM_ActivityInstanceId { get; set; }
        [StringLength(4000)]
        public string ARM_ResultMessage { get; set; }
        public DateTime ARM_Created { get; set; }
    }
}