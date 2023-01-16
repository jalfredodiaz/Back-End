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
    
    public partial class CAT_Puestos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CAT_Puestos()
        {
            this.Empleados = new HashSet<CAT_Empleados>();
            this.PuestosHijos = new HashSet<CAT_Puestos>();
        }
    
        public int nIdPuesto { get; set; }
        public Nullable<int> nIdPuestoPadre { get; set; }
        public string cPuesto { get; set; }
        public decimal nSueldo { get; set; }
        public int nIdDepartamento { get; set; }
        public bool bActivo { get; set; }
        public string cUsuario_Registro { get; set; }
        public System.DateTime dFecha_Registro { get; set; }
        public string cUsuario_UltimaModificacion { get; set; }
        public Nullable<System.DateTime> dFecha_UltimaModificacion { get; set; }
        public string cUsuario_Eliminacion { get; set; }
        public Nullable<System.DateTime> dFecha_Eliminacion { get; set; }
    
        public virtual CAT_Departamentos Departamento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CAT_Empleados> Empleados { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CAT_Puestos> PuestosHijos { get; set; }
        public virtual CAT_Puestos PuestoPadre { get; set; }
    }
}
