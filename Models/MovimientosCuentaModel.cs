using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class MovimientosCuentaModel
    {
        public int nIdMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public int nIdTipoMovimiento { get; set; }
        public string cTipoMovimiento { get; set; }
        public int nIdCategoria { get; set; }
        public string cCategoria { get; set; }
        public int IdReferencia { get; set; }
        public int nIdMovimientoCancela { get; set; }
        public decimal Cargo { get; set; }
        public decimal Abono { get; set; }
        public decimal Saldo { get; set; }
        public string cObservaciones { get; set; }
    }
}