using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDMainEmployment : EMDHistorizableObject
    {
        public string EP_Guid { get; set; }
        public string PERS_Guid { get; set; }

        public EMDMainEmployment(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        {
            Prefix = "MAEM";
        }

        public EMDMainEmployment()
        {
            Prefix = "MAEM";
        }
    }
}
