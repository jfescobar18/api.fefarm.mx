//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace api.fefarm.mx.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class cat_Application_Stages
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public cat_Application_Stages()
        {
            this.cat_Applications = new HashSet<cat_Applications>();
        }
    
        public int Application_Stage_Id { get; set; }
        public string Application_Stage_Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cat_Applications> cat_Applications { get; set; }
    }
}
