using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class NewEquipmentInfo
    {
        /// <summary>
        /// Equipment definition to create an equipment from 
        /// </summary>
        public string EqdeGuid { get; set; }
        /// <summary>
        /// where did the equipment come from? employmentEQ, fromPackage, ...
        /// </summary>
        public string OrtyGuid { get; set; }
        /// <summary>
        /// name of object container (package) where eq is in (empl guid for single eq requests)
        /// </summary>
        public string FromTemplateGuid { get; set; }

        public NewEquipmentInfo()
        {
        }

        public NewEquipmentInfo(string eqdeGuid, string ortyGuid, string fromTemplateguid)
        {
            this.EqdeGuid  = eqdeGuid;
            this.OrtyGuid = ortyGuid;
            this.FromTemplateGuid = fromTemplateguid;
        }
    }
}
