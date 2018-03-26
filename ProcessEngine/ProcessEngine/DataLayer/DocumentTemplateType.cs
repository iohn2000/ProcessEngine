using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.DataLayer
{
    [Table("DocumentTemplateType")]
    public partial class DocumentTemplateType
    {
        public Guid DTYP_ID { get; set; }

        [Key]
        [StringLength(50)]
        public string DTYP_Name { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DTYP_Created { get; set; }
    }
}
