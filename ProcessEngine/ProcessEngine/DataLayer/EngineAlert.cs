namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("EngineAlerts")]
    public partial class EngineAlert
    {
        [Key]
        public int EA_ID { get; set; }

        [StringLength(255)]
        public string EA_WFI_ID { get; set; }

        [StringLength(255)]
        public string EA_StartActivity { get; set; }

        public string EA_InputParameters { get; set; }
        
        public DateTime EA_Created	{ get; set;}
        public DateTime EA_Updated	{ get; set;}
        
        [StringLength(50)]
        public string EA_Status	{ get; set;}

        [StringLength(255)]
        public string EA_CallbackID { get; set; }

        [StringLength(255)]
        public string EA_Type { get; set; }

        public DateTime? EA_LastPolling { get; set; }
        public int? EA_PollingIntervalSeconds { get; set; }

        public Guid? EA_LockedByProcess { get; set; }

        [StringLength(50)]
        public string EA_ProcessEngineInstance { get; set; }
    }
}
