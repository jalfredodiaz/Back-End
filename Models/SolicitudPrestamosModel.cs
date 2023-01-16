using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class SolicitudPrestamosModel
    {
        public int nIdPrestamo { get; set; }
        public int nCodEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public int nIdRubro { get; set; }
        public string NombreRubro { get; set; }
        public decimal nImporte { get; set; }
        public decimal nSaldo { get; set; }
        public System.DateTime dFechaCobro { get; set; }
        public System.DateTime dFecha_Registro { get; set; }
        public string cRutaArchivoINE_Frente { get; set; }
        public string cRutaArchivoINE_Atras { get; set; }
        public string cRutaPagare { get; set; }
        public string cRutaCheque { get; set; }
        public bool bActivo { get; set; }
        public short nVersion { get; set; }
        public bool bConCorte { get; set; }
        public bool Nueva { get; set; }
        public int? nIdCorte { get; set; }
    }
}