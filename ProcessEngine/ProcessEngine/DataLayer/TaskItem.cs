namespace Kapsch.IS.ProcessEngine.DataLayer
{

    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TaskItems")]
    public partial class TaskItem
    {
        [Key]
        public int TSK_ID { get; set; }

        [StringLength(255)]
        public string TSK_ProcessName { get; set; }

        [StringLength(255)]
        public string TSK_TaskTitle { get; set; }

        [StringLength(255)]
        public string TSK_EffectedPerson_ID { get; set; }

        [StringLength(255)]
        public string TSK_RequestorID { get; set; }

        [StringLength(255)]
        public string TSK_ToDo { get; set; }

        [StringLength(255)]
        public string TSK_DecisionOptions { get; set; }

        public string TSK_Information { get; set; }

        public int? TSK_NotesHistory_ID { get; set; }

        public DateTime? TSK_Duedate { get; set; }

        public DateTime? TSK_DateRequested { get; set; }

        [StringLength(255)]
        public string TSK_Decision { get; set; }

        [StringLength(255)]
        public string TSK_LinkedTasks_ID { get; set; }
    }
}
