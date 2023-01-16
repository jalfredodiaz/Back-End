using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

using Back_End.Clases;
using Back_End.Clases.Seguridad;
using Back_End.ModelsBD;

namespace Back_End.Controllers
{
    [Authorize]
    [MyCorsPolicy]
    public class ControladorBase : ApiController
    {
        private readonly IEnumerable<Claim> claims;
        private ConexionBD _BD;
        private Utilerias _utilerias;
        private AppSettings _appSettings;
        private BackEndEntities _db;

        public ControladorBase()
        {
            if (User.Identity.IsAuthenticated)
            {
                this.claims = (User.Identity as ClaimsIdentity).Claims;
            }
        }

        protected Utilerias Utilerias
        {
            get
            {
                if (this._utilerias == null)
                {
                    this._utilerias = new Utilerias();
                }

                return this._utilerias;
            }
        }

        protected AppSettings AppSettings
        {
            get
            {
                if (this._appSettings == null)
                {
                    this._appSettings = new AppSettings();
                }

                return this._appSettings;
            }
        }

        public string UsuarioLoguin
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return this.Utilerias.Desencryptar(this.claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
                }

                return "";
            }
        }

        public string Nombre
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return this.claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                }

                return "";
            }
        }

        public string NombreCorto
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return this.Utilerias.Desencryptar(this.claims.FirstOrDefault(c => c.Type == "NombreCorto")?.Value);
                }

                return "";
            }
        }

        public string Email
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return this.Utilerias.Desencryptar(this.claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value);
                }

                return "";
            }
        }

        public string IdSesion
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return this.Utilerias.Desencryptar(this.claims.FirstOrDefault(c => c.Type == "ID")?.Value);
                }

                return "";
            }
        }

        public Clases.Enumeradores.RolesEnum Rol
        {
            get
            {
                if (User.Identity.IsAuthenticated)
                {
                    return (Clases.Enumeradores.RolesEnum)int.Parse(this.claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
                }

                return Clases.Enumeradores.RolesEnum.SIN_ROL;
            }
        }

        protected ConexionBD BD
        {
            get
            {
                if (this._BD == null)
                {
                    this._BD = new ConexionBD(this);
                }

                return this._BD;
            }
        }
        protected BackEndEntities DB
        {
            get
            {
                if (_db == null)
                {
                    _db = new BackEndEntities();
                    _db.Configuration.LazyLoadingEnabled = false;
                }

                return _db;
            }
        }
    }
}