using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Back_End.Clases
{
    public class AppSettings
    {
        public string Secret { get { return ConfigurationManager.AppSettings["ClaveSecreta"]; } }
        public string Issuer { get { return ConfigurationManager.AppSettings["Issuer"]; } }
        public string Audience { get { return ConfigurationManager.AppSettings["Audience"]; } }
        public int Expires { get { return int.Parse(ConfigurationManager.AppSettings["Expires"]); } }
        public string KeyEncryptacion { get { return ConfigurationManager.AppSettings["KeyEncryptacion"]; } }
        public string IVEncryptacion { get { return ConfigurationManager.AppSettings["IVEncryptacion"]; } }
        public string Conexion { get { return ConfigurationManager.ConnectionStrings["BD"].ConnectionString; } }
        public string Origen { get { return ConfigurationManager.AppSettings["Origen"]; } }


        public string URLArchivos { get { return ConfigurationManager.AppSettings["DocsUrl"]; } }



        public string ServidorFTP { get { return ConfigurationManager.AppSettings["servidorFTP"]; } }
        public string UsuarioFPT { get { return ConfigurationManager.AppSettings["usuarioFPT"]; } }
        public string PasswordFTP { get { return ConfigurationManager.AppSettings["passwordFTP"]; } }
        public string DirectorioSolicitudPrestamo { get { return ConfigurationManager.AppSettings["DirectorioSolicitudPrestamo"]; } }
        public string DirectorioRegistroGastoIngresos { get { return ConfigurationManager.AppSettings["DirectorioRegistroGastoIngresos"]; } }
        public string DirectorioSolicitudEmpleados { get { return ConfigurationManager.AppSettings["DirectorioEmpleados"]; } }
    }
}