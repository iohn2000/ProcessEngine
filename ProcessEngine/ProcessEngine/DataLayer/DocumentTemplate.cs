using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.DataLayer
{

    [Table("DocumentTemplate")]
    public partial class DocumentTemplate
    {
        public Guid TMPL_ID { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(100)]
        public string TMPL_Name { get; set; }

        public string TMPL_Description { get; set; }

        [Required]
        public string TMPL_Content { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string TMPL_Category { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime TMPL_Created { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? TMPL_Updated { get; set; }
    }
}
