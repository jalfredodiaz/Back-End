using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class CuentaConMovimientosModel
    {
        public decimal SaldoInicial { get; set; }
        public List<MovimientosCuentaModel> Movimientos { get; set; }
    }
}