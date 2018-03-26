using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDUserDomain : EMDObject<EMDUserDomain>
    {
        public string Name { get; set; }

        public override String Prefix { get { return "USDO"; } }
        
        public EMDUserDomain(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDUserDomain()
        { }
    }
}
