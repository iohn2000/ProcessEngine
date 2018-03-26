using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Kapsch.IS.WsProcessEngine.FaultContracts
{
    [DataContract]
    public class FcVariableException : FcBaseException
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int ErrorType { get; set; }

    }
}
