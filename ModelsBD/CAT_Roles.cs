//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Back_End.ModelsBD
{
    using System;
    using System.Collections.Generic;
    
    public partial class CAT_Roles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CAT_Roles()
        {
            this.CAT_Usuarios = new HashSet<CAT_Usuarios>();
        }
    
        public int nIdRol { get; set; }
        public string cNombre { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CAT_Usuarios> CAT_Usuarios { get; set; }
    }
}