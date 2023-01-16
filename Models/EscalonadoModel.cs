using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class EscalonadoModel
    {
        public int CodigoEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public int CodigoPuesto { get; set; }
        public string NombrePuesto { get; set; }
        public int CodigoDepartamento { get; set; }
        public string NombreDepartamento { get; set; }
        public DateTime FechaIngreso { get; set; }
    }
}