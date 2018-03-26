using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDObjectContainerContent : EMDObject<EMDObjectContainerContent>
    {
        public string OC_Guid { get; set; }
        public string ObjectGuid { get; set; }

        public override String Prefix { get { return "OBCC"; } }
        
        public EMDObjectContainerContent(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDObjectContainerContent()
        { }
    }
}
