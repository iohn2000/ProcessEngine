using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDUserID : EMDObject<EMDUserID>
    {
        public override String Prefix { get { return "USER"; } }

        public EMDUserID(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        {}

        public EMDUserID(string useridstring, string personGuid, string domain="Kapsch") :
        base()
        {
            this.UserIDText = useridstring;
            this.Domain = domain;
            this.P_Guid = personGuid;
            this.P_ID = 0;
        }

        public EMDUserID(): base()
        {}


        public string P_Guid { get; set; }
        public int USR_ID { get; set; }
        public int P_ID { get; set; }
        public string UserIDText { get; set; }
        public string Domain { get; set; }

    }

    
}
