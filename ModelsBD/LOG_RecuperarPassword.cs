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
    
    public partial class LOG_RecuperarPassword
    {
        public System.Guid cID { get; set; }
        public string cLogin { get; set; }
        public System.DateTime dFechaVencimiento { get; set; }
        public System.DateTime dFechaGeneracion { get; set; }
        public bool bActivo { get; set; }
    
        public virtual CAT_Usuarios Usuario { get; set; }
    }
}
