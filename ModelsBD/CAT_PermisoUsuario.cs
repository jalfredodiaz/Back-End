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
    
    public partial class CAT_PermisoUsuario
    {
        public string cLogin { get; set; }
        public int nRama { get; set; }
        public bool bActivo { get; set; }
    
        public virtual CAT_Navegador Navegador { get; set; }
        public virtual CAT_Usuarios CAT_Usuarios { get; set; }
    }
}
