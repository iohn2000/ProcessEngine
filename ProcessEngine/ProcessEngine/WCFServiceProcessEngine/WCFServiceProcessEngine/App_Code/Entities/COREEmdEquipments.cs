using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for COREEmdEquipments
/// </summary>
[DataContract]
public class COREEmdEquipments
{
    /// <summary>
    /// Equipment definition to create an equipment from 
    /// </summary>
    [DataMember]
    public string EqdeGuid { get; set; }
    /// <summary>
    /// where did the equipment come from? employmentEQ, fromPackage, ...
    /// </summary>
    [DataMember]
    public string OrtyGuid { get; set; }
    /// <summary>
    /// name of object container (package) where eq is in (empl guid for single eq requests)
    /// </summary>
    [DataMember]
    public string FromTemplateGuid { get; set; }

    public COREEmdEquipments()
    {
    }
}