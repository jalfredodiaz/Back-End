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
    
    public partial class CAP_CorteDetalle
    {
        public int nIdCorte { get; set; }
        public int nIdPrestamo { get; set; }
        public decimal nImporte { get; set; }
    
        public virtual CAP_Corte Corte { get; set; }
        public virtual CAP_SolicitudPrestamo SolicitudPrestamo { get; set; }
    }
}
