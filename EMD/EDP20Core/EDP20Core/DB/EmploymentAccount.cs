//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kapsch.IS.EDP.Core.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmploymentAccount
    {
        public string Guid { get; set; }
        public string HistoryGuid { get; set; }
        public System.DateTime ValidFrom { get; set; }
        public System.DateTime ValidTo { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public string EP_Guid { get; set; }
        public string AC_Guid { get; set; }
        public int EPA_ID { get; set; }
        public int EP_ID { get; set; }
        public int AC_ID { get; set; }
        public short Main { get; set; }
        public Nullable<short> Percent { get; set; }
        public System.DateTime ActiveFrom { get; set; }
        public System.DateTime ActiveTo { get; set; }
        public string Guid_ModifiedBy { get; set; }
        public string ModifyComment { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual Employment Employment { get; set; }
        public virtual Person Person { get; set; }
    }
}