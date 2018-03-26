using System;
using System.Runtime.Serialization;

namespace Kapsch.IS.ProcessEngine.Webservice.Entities
{
    [DataContract]
    public class COREEmdEmployment
    {
        [DataMember]
        public DateTime? Entry { get; set; }
        [DataMember]
        public DateTime? Exit { get; set; }
        [DataMember]
        public DateTime? LastDay { get; set; }
        [DataMember]
        public DateTime? FirstWorkDay { get; set; }
        [DataMember]
        public string PersNr { get; set; }
    }
}
