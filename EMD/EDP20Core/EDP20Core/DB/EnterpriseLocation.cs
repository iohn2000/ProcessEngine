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
    
    public partial class EnterpriseLocation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EnterpriseLocation()
        {
            this.Employment = new HashSet<Employment>();
        }
    
        public string Guid { get; set; }
        public string HistoryGuid { get; set; }
        public string E_Guid { get; set; }
        public string L_Guid { get; set; }
        public System.DateTime ValidFrom { get; set; }
        public System.DateTime ValidTo { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public Nullable<int> L_ID { get; set; }
        public Nullable<int> E_ID { get; set; }
        public string DistList_int { get; set; }
        public string DistList_ext { get; set; }
        public System.DateTime ActiveFrom { get; set; }
        public System.DateTime ActiveTo { get; set; }
        public byte Status { get; set; }
        public string Guid_ModifiedBy { get; set; }
        public string ModifyComment { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employment> Employment { get; set; }
        public virtual Enterprise Enterprise { get; set; }
        public virtual Location Location { get; set; }
        public virtual Person Person { get; set; }
    }
}