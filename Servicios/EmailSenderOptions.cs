using Back_End.Clases;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Back_End.Servicios
{
    public class EmailSenderOptions
    {
        public int Port { get { return int.Parse(ConfigurationManager.AppSettings["PuertoEmail"]); } }
        public string Email { get { return ConfigurationManager.AppSettings["Email"]; } }
        public string Password { get { return ConfigurationManager.AppSettings["Password"]; } }
        public bool EnableSsl { get { return bool.Parse(ConfigurationManager.AppSettings["EnableSsl"]); } }
        public string Host { get { return ConfigurationManager.AppSettings["Host"]; } }
    }
}
