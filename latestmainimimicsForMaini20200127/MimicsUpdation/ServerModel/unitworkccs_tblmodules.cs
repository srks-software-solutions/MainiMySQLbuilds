//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MimicsUpdation.ServerModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class unitworkccs_tblmodules
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public unitworkccs_tblmodules()
        {
            this.unitworkccs_tblrolemodulelink = new HashSet<unitworkccs_tblrolemodulelink>();
        }
    
        public int ModuleId { get; set; }
        public string Module { get; set; }
        public string ModuleDesc { get; set; }
        public string ModuleDispName { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<int> InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> IsDeleted { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblrolemodulelink> unitworkccs_tblrolemodulelink { get; set; }
    }
}
