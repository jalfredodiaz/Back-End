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
    
    public partial class CAT_RubrosPrestamos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CAT_RubrosPrestamos()
        {
            this.CAP_Corte = new HashSet<CAP_Corte>();
            this.CAP_SolicitudPrestamo = new HashSet<CAP_SolicitudPrestamo>();
        }
    
        public int nIdRubro { get; set; }
        public string cRubro { get; set; }
        public bool bMensual { get; set; }
        public int nMesCorte { get; set; }
        public int nDiaCorte { get; set; }
        public bool bAguinaldo { get; set; }
        public bool bActivo { get; set; }
        public string cUsuario_Registro { get; set; }
        public System.DateTime dFecha_Registro { get; set; }
        public string cUsuario_UltimaModificacion { get; set; }
        public System.DateTime dFecha_UltimaModificacion { get; set; }
        public string cUsuario_Eliminacion { get; set; }
        public Nullable<System.DateTime> dFecha_Eliminacion { get; set; }
        public string cNombreCorto { get; set; }
        public bool bFunerario { get; set; }
        public decimal nImporteMaximo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CAP_Corte> CAP_Corte { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CAP_SolicitudPrestamo> CAP_SolicitudPrestamo { get; set; }
    }
}
