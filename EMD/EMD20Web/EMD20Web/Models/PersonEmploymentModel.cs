using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ReflectionHelper;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class PersonEmploymentModel
    {
        public string Guid { get; set; }
        [StringLength(255), Required(), Display(Name = "Surname")]
        public string FamilyName { get; set; }

        [StringLength(255), Required()]
        public string FirstName { get; set; }
        public string Sex { get; set; }

        [StringLength(25)]
        public string DegreePrefix { get; set; }

        [StringLength(25)]
        public string DegreeSuffix { get; set; }
        public string UserID { get; set; }
        public string MainMail { get; set; }

        public string EMPL_Guid { get; set; }
        public string ENTE_Guid { get; set; }

        public string PersNr { get; set; }
    }
}