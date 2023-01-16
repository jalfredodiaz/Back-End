//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Http;
//using Back_End.Models;
//using Microsoft.IdentityModel.Tokens;

//namespace Back_End.Controllers
//{
//    public class LoginController : ApiController
//    {
//        // POST: api/Login
//        [HttpPost]
//        [AllowAnonymous]
//        public async Task<IHttpActionResult> LoginAsync(UsuarioLogin usuarioLogin)
//        {
//            if (usuarioLogin == null)
//                return BadRequest("Usuario y Contraseña requeridos.");

//            var _userInfo = await AutenticarUsuarioAsync(usuarioLogin.Usuario, usuarioLogin.Password);
//            if (_userInfo != null)
//            {
//                return Ok(new { token = GenerarTokenJWT(_userInfo) });
//            }
//            else
//            {
//                return Unauthorized();
//            }
//        }

//        // COMPROBAMOS SI EL USUARIO EXISTE EN LA BASE DE DATOS 
//        private async Task<UsuarioInfo> AutenticarUsuarioAsync(string usuario, string password)
//        {
//            // AQUÍ LA LÓGICA DE AUTENTICACIÓN //

//            // Supondremos que el usuario existe en la Base de Datos.
//            // Retornamos un objeto del tipo UsuarioInfo, con toda
//            // la información del usuario necesaria para el Token.
//            return new UsuarioInfo()
//            {
//                // Id del Usuario en el Sistema de Información (BD)
//                Id = new Guid("B5D233F0-6EC2-4950-8CD7-F44D16EC878F"),
//                Nombre = "Nombre Usuario",
//                Apellidos = "Apellidos Usuario",
//                Email = "email.usuario@dominio.com",
//                Rol = "Administrador"
//            };

//            // Supondremos que el usuario NO existe en la Base de Datos.
//            // Retornamos NULL.
//            //return null;
//        }

//        // GENERAMOS EL TOKEN CON LA INFORMACIÓN DEL USUARIO
//        private string GenerarTokenJWT(UsuarioInfo usuarioInfo)
//        {
//            // RECUPERAMOS LAS VARIABLES DE CONFIGURACIÓN
//            var _ClaveSecreta = ConfigurationManager.AppSettings["ClaveSecreta"];
//            var _Issuer = ConfigurationManager.AppSettings["Issuer"];
//            var _Audience = ConfigurationManager.AppSettings["Audience"];
//            if (!Int32.TryParse(ConfigurationManager.AppSettings["Expires"], out int _Expires))
//                _Expires = 24;


//            // CREAMOS EL HEADER //
//            var _symmetricSecurityKey = new SymmetricSecurityKey(
//                    Encoding.UTF8.GetBytes(_ClaveSecreta));
//            var _signingCredentials = new SigningCredentials(
//                    _symmetricSecurityKey, SecurityAlgorithms.HmacSha256
//                );
//            var _Header = new JwtHeader(_signingCredentials);

//            // CREAMOS LOS CLAIMS //
//            var _Claims = new[] {
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                new Claim(JwtRegisteredClaimNames.NameId, usuarioInfo.Id.ToString()),
//                new Claim("nombre", usuarioInfo.Nombre),
//                new Claim("apellidos", usuarioInfo.Apellidos),
//                new Claim(JwtRegisteredClaimNames.Email, usuarioInfo.Email),
//                new Claim(ClaimTypes.Role, usuarioInfo.Rol)
//            };

//            // CREAMOS EL PAYLOAD //
//            var _Payload = new JwtPayload(
//                    issuer: _Issuer,
//                    audience: _Audience,
//                    claims: _Claims,
//                    notBefore: DateTime.UtcNow,
//                    // Exipra a la 24 horas.
//                    expires: DateTime.UtcNow.AddHours(_Expires)
//                );

//            // GENERAMOS EL TOKEN //
//            var _Token = new JwtSecurityToken(
//                    _Header,
//                    _Payload
//                );

//            return new JwtSecurityTokenHandler().WriteToken(_Token);
//        }
//    }
//}
