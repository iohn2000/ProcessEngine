using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDContentTemplate : EMDObject<EMDContentTemplate>
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string OBCO_Guid { get; set; }
        public string RUSE_Guid { get; set; }

        public override String Prefix { get { return "COTE"; } }

        public EMDContentTemplate(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDContentTemplate()
        { }
    }
}
