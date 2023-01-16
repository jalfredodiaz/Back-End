using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class UsuarioLoguinModel
    {
        public string Login { get; set; }
        public string Nombre { get; set; }
        public string NombreCorto { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public string Token { get; set; }
        public bool Hombre { get; set; }
        public DateTime FechaExpiracion { get; set; }
    }
}