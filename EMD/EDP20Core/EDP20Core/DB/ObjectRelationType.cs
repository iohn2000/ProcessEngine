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
    
    public partial class ObjectRelationType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ObjectRelationType()
        {
            this.ObjectRelation = new HashSet<ObjectRelation>();
        }
    
        public string Guid { get; set; }
        public string RelationName { get; set; }
        public string Object1 { get; set; }
        public string Object2 { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public System.DateTime ValidFrom { get; set; }
        public System.DateTime ValidTo { get; set; }
        public string HistoryGuid { get; set; }
        public System.DateTime ActiveFrom { get; set; }
        public System.DateTime ActiveTo { get; set; }
        public string Guid_ModifiedBy { get; set; }
        public string ModifyComment { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ObjectRelation> ObjectRelation { get; set; }
        public virtual Person Person { get; set; }
    }
}
