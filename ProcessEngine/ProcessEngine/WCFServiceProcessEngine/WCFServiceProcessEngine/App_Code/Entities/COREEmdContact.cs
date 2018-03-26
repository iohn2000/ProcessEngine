using System;
using System.Runtime.Serialization;

namespace Kapsch.IS.WsProcessEngine.Entities
{
    
    [DataContract]
    public class COREEmdContact
    {
        [DataMember]
        public string CT_Guid { get; set; }
        [DataMember]
        public int C_CT_ID { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public bool VisiblePhone { get; set; }
        [DataMember]
        public bool VisibleKatce { get; set; }
        [DataMember]
        public bool ACDDisplay { get; set; }
    }
}
