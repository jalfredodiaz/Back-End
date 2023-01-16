using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back_End.Models
{
    public class CambiarImagenPerfilModel
    {
        public string UsuarioLogin { get; set; }
        public string ImagenBase64 { get; set; }
    }
}
