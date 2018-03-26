using System;

namespace Kapsch.IS.EDP.Core.Entities
{
    public class EMDObjectFlag : EMDObject<EMDObjectFlag>
    {
        public override String Prefix { get { return "OBFL"; } }
        public string Obj_Guid { get; set; }
        public string FlagType { get; set; }
        
        public EMDObjectFlag(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDObjectFlag()
        { }
    }
}