using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDSecurityAction : EMDObject<EMDSecurityAction>
    {
        public string ObjectType { get; set; }
        public string Action { get; set; }
        public string ROLE_Guid { get; set; }
        public override String Prefix { get { return "SEAC"; } }

        public EMDSecurityAction(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDSecurityAction()
        { }
    }
}
