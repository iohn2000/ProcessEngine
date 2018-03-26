using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.WsProcessEngine.Entities
{
    [DataContract]
    public class ActivityItem
    {
        [DataMember]
        public string Id { get; set; }


        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public EnumActivityType ActivityType { get; set; }
    }
}
