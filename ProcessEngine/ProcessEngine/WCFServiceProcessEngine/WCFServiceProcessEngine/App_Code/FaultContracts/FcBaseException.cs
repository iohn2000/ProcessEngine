using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;




namespace Kapsch.IS.WsProcessEngine.FaultContracts
{
    /// <summary>
    /// Summary description for FcBaseException
    /// </summary>
    [DataContract]
    public class FcBaseException
    {
        [DataMember]
        /// <summary>
        /// Line Number in Xml Document
        /// </summary>
        public int LineNumberStart { get; set; }

        [DataMember]
        public int LineNumberEnd { get; set; }
    }
}