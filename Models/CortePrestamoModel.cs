using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Back_End.Models
{
    public class CortePrestamoModel
    {
        public int nIdCorte { get; set; }
        public int nIdRubro { get; set; }
        public string NombreRubro { get; set; }
        public System.DateTime dFechaCorte { get; set; }
        public decimal nTotal { get; set; }
        public bool bActivo { get; set; }
        public bool bPagado { get; set; }
        public short nVersion { get; set; }
        public bool Nuevo { get; set; }
        public string cUsuario_Registro { get; set; }
        public System.DateTime dFecha_Registro { get; set; }
        public string cUsuario_UltimaModificacion { get; set; }
        public System.DateTime dFecha_UltimaModificacion { get; set; }
        public string cUsuario_Eliminacion { get; set; }
        public Nullable<System.DateTime> dFecha_Eliminacion { get; set; }
        public List<SolicitudPrestamosModel> Prestamos { get; set; }
    }
}