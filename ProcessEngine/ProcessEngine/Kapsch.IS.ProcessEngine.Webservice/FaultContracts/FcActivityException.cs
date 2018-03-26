using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Kapsch.IS.ProcessEngine.Webservice.FaultContracts
{
    [DataContract]
    public class FcActivityException : FcBaseException
    {
        [DataMember]
        public string IdActivityInstance { get; set; }

        [DataMember]
        public int ErrorType { get; set; }

    }
}
