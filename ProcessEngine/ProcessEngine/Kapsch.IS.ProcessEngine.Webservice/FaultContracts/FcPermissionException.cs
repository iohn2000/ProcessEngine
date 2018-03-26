using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Kapsch.IS.ProcessEngine.Webservice.FaultContracts
{
    [DataContract]
    public class FcPermissionException
    {
        [DataMember]
        public bool IsCheckedOutByAnotherUser { get; set; }

        [DataMember]
        public bool IsNotCheckedOut { get; set; }

    }
}
