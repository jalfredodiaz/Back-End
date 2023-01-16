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
    
    public partial class CAP_SolicitudPrestamo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CAP_SolicitudPrestamo()
        {
            this.CorteDetalle = new HashSet<CAP_CorteDetalle>();
            this.CAP_MovimientosCuenta = new HashSet<CAP_MovimientosCuenta>();
        }
    
        public int nIdPrestamo { get; set; }
        public int nCodEmpleado { get; set; }
        public int nIdRubro { get; set; }
        public decimal nImporte { get; set; }
        public decimal nSaldo { get; set; }
        public System.DateTime dFechaCobro { get; set; }
        public string cRutaArchivoINE_Frente { get; set; }
        public string cRutaArchivoINE_Atras { get; set; }
        public string cRutaPagare { get; set; }
        public string cRutaCheque { get; set; }
        public short nVersion { get; set; }
        public bool bConCorte { get; set; }
        public bool bActivo { get; set; }
        public string cUsuario_Registro { get; set; }
        public System.DateTime dFecha_Registro { get; set; }
        public string cUsuario_UltimaModificacion { get; set; }
        public System.DateTime dFecha_UltimaModificacion { get; set; }
        public string cUsuario_Eliminacion { get; set; }
        public Nullable<System.DateTime> dFecha_Eliminacion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CAP_CorteDetalle> CorteDetalle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CAP_MovimientosCuenta> CAP_MovimientosCuenta { get; set; }
        public virtual CAT_Empleados Empleado { get; set; }
        public virtual CAT_RubrosPrestamos TipoPrestamo { get; set; }
        public virtual CAT_Usuarios CAT_Usuarios { get; set; }
        public virtual CAT_Usuarios CAT_Usuarios1 { get; set; }
        public virtual CAT_Usuarios CAT_Usuarios2 { get; set; }
    }
}
