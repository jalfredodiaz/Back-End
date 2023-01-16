using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class CambioContraseñaModel
    {
        public string Usuario { get; set; }
        public string PasswordActual { get; set; }
        public string PasswordNuevo { get; set; }
    }
}