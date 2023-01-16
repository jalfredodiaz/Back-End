using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back_End.Models
{
    public class OpcionMenuModel
    {
        public int Rama { get; set; }
        public int RamaPadre { get; set; }
        public string NombreRama { get; set; }
        public string Ruta { get; set; }
        public int Orden { get; set; }
        public List<OpcionMenuModel> Opciones { get; set; }
    }
}
