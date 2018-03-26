namespace Kapsch.IS.ProcessEngine.DataLayer
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ActivityDefinition")]
    public partial class ActivityDefinition
    {
        [Key]
        [StringLength(255)]
        public string WFAD_ID { get; set; }
        [StringLength(255)]
        public string WFAD_Name { get; set; }
        public string WFAD_Description { get; set; }
        public string WFAD_ConfigTemplate { get; set; }
        public int? WFAD_HostLoad { get; set; }
        public DateTime WFAD_Created { get; set; }
        public DateTime? WFAD_Updated { get; set; }
        public DateTime WFAD_ValidFrom { get; set; }
        public DateTime WFAD_ValidTo { get; set; }
        public bool WFAD_IsStartActivity { get; set; }
        public int WFAD_Type { get; set; }
    }
}
