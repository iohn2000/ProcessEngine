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
    
    public partial class ProcessMapping
    {
        public string Guid { get; set; }
        public string HistoryGuid { get; set; }
        public string TypePrefix { get; set; }
        public string EntityGuid { get; set; }
        public string Method { get; set; }
        public string WorkflowID { get; set; }
        public string WorkflowVariables { get; set; }
        public Nullable<System.DateTime> ValidFrom { get; set; }
        public Nullable<System.DateTime> ValidTo { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public System.DateTime ActiveFrom { get; set; }
        public System.DateTime ActiveTo { get; set; }
        public string Guid_ModifiedBy { get; set; }
        public string ModifyComment { get; set; }
    
        public virtual Person Person { get; set; }
        public virtual Person Person1 { get; set; }
    }
}