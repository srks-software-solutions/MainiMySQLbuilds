//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Production_Status_UAT
{
    using System;
    using System.Collections.Generic;
    
    public partial class unitworkccs_tblroles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public unitworkccs_tblroles()
        {
            this.unitworkccs_tbloperatorlogindetails = new HashSet<unitworkccs_tbloperatorlogindetails>();
            this.unitworkccs_tbloperatorlogindetails1 = new HashSet<unitworkccs_tbloperatorlogindetails>();
            this.unitworkccs_tblrolemodulelink = new HashSet<unitworkccs_tblrolemodulelink>();
            this.unitworkccs_tblusers = new HashSet<unitworkccs_tblusers>();
            this.unitworkccs_tblusers1 = new HashSet<unitworkccs_tblusers>();
        }
    
        public int Role_ID { get; set; }
        public string RoleName { get; set; }
        public string RoleDisplayName { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string RoleDesc { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tbloperatorlogindetails> unitworkccs_tbloperatorlogindetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tbloperatorlogindetails> unitworkccs_tbloperatorlogindetails1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblrolemodulelink> unitworkccs_tblrolemodulelink { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblusers> unitworkccs_tblusers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblusers> unitworkccs_tblusers1 { get; set; }
    }
}
