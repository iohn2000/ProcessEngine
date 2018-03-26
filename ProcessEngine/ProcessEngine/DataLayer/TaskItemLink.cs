namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("TaskItemsLinks")]
    public partial class TaskItemLink
    {
        [Key, StringLength(255)]
        public string LTSK_ID { get; set; }


        [StringLength(50)]
        public string LTSK_LinkType { get; set;}
    }
}
