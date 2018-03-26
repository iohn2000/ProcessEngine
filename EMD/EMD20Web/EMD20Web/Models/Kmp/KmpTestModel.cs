using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Kmp
{
    public class KmpTestModel
    {
        public string Status { get; set; }
        public string Error { get; set; }

        public KmpTestModel()
        {
            this.Status = string.Empty;
            this.Error = string.Empty;            
        }
    }
}