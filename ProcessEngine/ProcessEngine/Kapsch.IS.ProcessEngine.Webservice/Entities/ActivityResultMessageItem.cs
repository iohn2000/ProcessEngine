using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
namespace Kapsch.IS.ProcessEngine.Webservice.Entities
{
    [DataContract]
    public class ActivityResultMessageItem
    {
        [DataMember]
        public Guid ARM_ID { get; set; }
        [DataMember]
        public string ARM_Woin { get; set; }
        [DataMember]
        public string ARM_ActivityInstanceId { get; set; }
        [DataMember]
        public string ARM_ResultMessage { get; set; }
        [DataMember]
        public DateTime ARM_Created { get; set; }

        public static ActivityResultMessageItem Map(ActivityResultMessage item)
        {
            return new ActivityResultMessageItem()
            {
                ARM_ActivityInstanceId = item.ARM_ActivityInstanceId,
                ARM_Created = item.ARM_Created,
                ARM_ID = item.ARM_ID,
                ARM_ResultMessage = item.ARM_ResultMessage,
                ARM_Woin = item.ARM_Woin
            };
        }

        public static List<ActivityResultMessageItem> Map(List<ActivityResultMessage> items)
        {
            List<ActivityResultMessageItem> activityResultMessages = new List<ActivityResultMessageItem>();


            foreach (var item in items)
            {
                activityResultMessages.Add(Map(item));
            }

            return activityResultMessages;
        }
    }
}