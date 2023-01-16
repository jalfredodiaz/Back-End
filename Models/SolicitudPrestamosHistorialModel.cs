using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class SolicitudPrestamosHistorialModel
    {
        public int IdPrestamo { get; set; }
        public int IdRubro { get; set; }
        public string NombreRubro { get; set; }
        public decimal Importe { get; set; }
        public System.DateTime FechaCobro { get; set; }
        public System.DateTime FechaRegistro { get; set; }
        public bool Pagado { get; set; }
        public bool Activo { get; set; }
    }
}