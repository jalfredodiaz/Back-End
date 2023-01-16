using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class RegistroGastosIngresosModel
    {
        public int nIdMovimiento { get; set; }
        public int nIdCuenta { get; set; }
        public int nIdTipoMovimiento { get; set; }
        public int nIdMovimientoCancela { get; set; }
        public string NombreTipoMovimiento { get; set; }
        public int nIdCategoria { get; set; }
        public string NombreCategoria { get; set; }
        public DateTime dFecha_Registro { get; set; }
        public decimal nImporte { get; set; }
        public string cObservaciones { get; set; }
        public string cRutaDocumento { get; set; }
        public bool bActivo { get; set; }
        public bool Nuevo { get; set; }
        public bool Cancelado { get; set; }
    }
}