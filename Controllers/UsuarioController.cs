using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Back_End.ModelsBD;
using Z.EntityFramework.Extensions;
using Microsoft.SqlServer;
using Back_End.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Http.Cors;
using Back_End.Clases;

namespace Back_End.Controllers
{
    public class UsuarioController : ControladorBase
    {
        // GETS
        public IQueryable<CAT_Usuarios> GetCAT_Usuarios()
        {
            return DB.CAT_Usuarios.SinPassword();
        }

        [ResponseType(typeof(CAT_Usuarios))]
        public async Task<IHttpActionResult> GetCAT_Usuarios(string id)
        {
            CAT_Usuarios cAT_Usuarios = await DB.CAT_Usuarios.FindAsync(id);
            if (cAT_Usuarios == null)
            {
                return NotFound();
            }

            return Ok(cAT_Usuarios.SinPassword());
        }

        [HttpGet]
        [Route("~/api/Usuario/ObtenerMenu")]
        public async Task<IHttpActionResult> GET_ObtenerMenu()
        {
            return Ok((await ObtenerObcionesMenuPorUsuario(this.UsuarioLoguin)));
        }

        [HttpGet]
        [Route("~/api/Usuario/ObtenerPermisos")]
        public async Task<IHttpActionResult> GET_ObtenerPermisos()
        {
            return Ok(await (from p in DB.CAT_PermisoUsuario
                                         .Where(u => u.cLogin == this.UsuarioLoguin && u.bActivo == true)
                                         .Include(m => m.Navegador)
                          select p.Navegador.cOpcion).Distinct().ToListAsync());
        }






        // SE QUITO LA VALIDACIÓN DE SOLO USUARIOS AUTORIZADOS PARA PODER REGISTRAR LOS USUARIOS SIN NECECIDAD DE INICIAR SESION
        // ESTO SOLO CON LA INTENCIÓN DE HACER PRUEBAS.
        // TERMINANDO PRUEBAS QUITAR EL [AllowAnonymous]
        // POSTS
        [HttpPost]
        [Route("~/api/Usuario/GuardarUsuario")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> POST_Guardar(CAT_Usuarios cAT_Usuarios)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Se quito la validación solo para permitir registro de usuarios sin estar autenticado.
            // TERMINANDO PRUEBAS DESCOMENTAR LAS SIGUIENTES LINEAS
            //if (this.Rol != Clases.Enumeradores.RolesEnum.GENERAL)
            //{
            //    return Unauthorized();
            //}

            if (cAT_Usuarios.cEmail == null || cAT_Usuarios.cEmail.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un EMail.");
            }

            if (!this.Utilerias.ValidarCorreo(cAT_Usuarios.cEmail))
            {
                return BadRequest("EMail invalido.");
            }

            if (cAT_Usuarios.cNombre == null || cAT_Usuarios.cNombre.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un Nombre.");
            }

            if (cAT_Usuarios.cLogin == null || cAT_Usuarios.cLogin.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre usuario para Iniciar Sesión.");
            }

            if (cAT_Usuarios.cNombreCorto == null || cAT_Usuarios.cNombreCorto.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un Nombre Corto.");
            }

            if (cAT_Usuarios.cPassword == null || cAT_Usuarios.cPassword.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar una Contraseña");
            }

            if (cAT_Usuarios.nIdRol < 1 || cAT_Usuarios.nIdRol > 4)
            {
                return BadRequest("Rol invalido");
            }

            if (CAT_UsuariosExists(cAT_Usuarios.cLogin))
            {
                return BadRequest("Ya existe un usuario con el nombre de " + cAT_Usuarios.cLogin);
            }

            if (CAT_UsuariosExistsEmail(cAT_Usuarios.cEmail))
            {
                return BadRequest("Ya existe un usuario con el EMail " + cAT_Usuarios.cEmail);
            }


            cAT_Usuarios.dFechaAlta = DateTime.Today;
            cAT_Usuarios.dFechaBloqueo = DateTime.Today;
            cAT_Usuarios.dFechaExpiracion = DateTime.Today.AddDays(30);
            cAT_Usuarios.dFechaUltimoIntento = DateTime.Today;
            cAT_Usuarios.bActivo = true;
            cAT_Usuarios.cPassword = this.Utilerias.Encryptar(cAT_Usuarios.cPassword);

            DB.CAT_Usuarios.Add(cAT_Usuarios);

            await DB.CAT_Navegador
                .Where(n => n.cOpcion.Length > 0)
                .ForEachAsync(n => 
                    DB.CAT_PermisoUsuario.Add(new CAT_PermisoUsuario()
                    {
                        nRama = n.nRama,
                        cLogin = cAT_Usuarios.cLogin,
                        bActivo = true
                    }));

            try
            {
                await DB.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CAT_UsuariosExists(cAT_Usuarios.cLogin))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        [HttpPost]
        [Route("~/api/Usuario/AutenticarUsuario")]
        [AllowAnonymous]
        [ResponseType(typeof(Models.UsuarioLoguinModel))]
        public async Task<IHttpActionResult> POST_Autenticar(Models.AutenticacionModel datos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (datos.Usuario == null || datos.Usuario.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un usuario");
            }

            if (datos.Password == null || datos.Password.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar una contraseña.");
            }


            try
            {
                DB.Configuration.LazyLoadingEnabled = true;

                // La contraseña ya viene encriptada.
                CAT_Usuarios usuario = await DB.CAT_Usuarios.FirstOrDefaultAsync(u => u.cLogin == datos.Usuario);

                // Usuario existente?
                if (usuario == null)
                {
                    return BadRequest("Usuario o contraseña invalidos.");
                }

                // Usuario bloqueado por intentos fallidos de sesión?
                if (usuario.dFechaBloqueo > DateTime.Now)
                {
                    return BadRequest("El usuario se encuentra bloqueado. \nPara evitar este problema les recomendamos cerrar sesión antes de cerrar el explorador.");
                }

                // Usuario activo?
                if (!usuario.bActivo)
                {
                    return BadRequest("Usuario inactivo permanentemente.");
                }

                // Contraseña correcta?
                if (usuario.cPassword != datos.Password)
                {
                    // Ultimo intento con mas de 5 minutos de diferencia?
                    if (usuario.dFechaUltimoIntento.AddMinutes(5) < DateTime.Now)
                    {
                        // Inicializar intentos
                        usuario.nIntentosFallidos = 0;
                    }

                    usuario.dFechaUltimoIntento = DateTime.Now;
                    usuario.nIntentosFallidos++;

                    // Supera los intentos fallidos permitidos?
                    if (usuario.nIntentosFallidos == 5)
                    {
                        // Bloquear usuario
                        usuario.dFechaBloqueo = DateTime.Now.AddMinutes(30);
                    }

                    // Agregar para modificación
                    this.DB.Entry<CAT_Usuarios>(usuario).State = EntityState.Modified;

                    // Guardar cambios.
                    await DB.SaveChangesAsync();

                    return BadRequest("Usuario o contraseña incorectos.");
                }

                // Iniciarlizar fecha ultimo intento.
                usuario.dFechaUltimoIntento = DateTime.Now;
                // Inicializar intentos
                usuario.nIntentosFallidos = 0;

                // Agregar para modificación.
                this.DB.Entry<CAT_Usuarios>(usuario).State = EntityState.Modified;

                // Actualizar datos usuario
                await DB.SaveChangesAsync();


                // Obtener ultima sesion del usuario
                LOG_Sesiones ultimaSesion = await DB.LOG_Sesiones
                                                .Where(s => s.cLogin == usuario.cLogin && s.bActivo == true)
                                                .OrderByDescending(s => s.nIdSesion)
                                                .FirstOrDefaultAsync(s => s.cLogin == usuario.cLogin && s.bActivo == true);

                if (ultimaSesion != null)
                {
                    if (ultimaSesion.dFechaFin > DateTime.Now)
                    {
                        return BadRequest("Ya existe una sesión iniciada. Es importante salir correctamente para evitar estos problemas.");
                    }
                    else
                    {
                        // finalizo la sesión
                        ultimaSesion.bActivo = false;

                        await DB.SaveChangesAsync();
                    }
                }


                var usuarioLog = new UsuarioLoguinModel()
                {
                    Email = usuario.cEmail,
                    Nombre = usuario.cNombre,
                    NombreCorto = usuario.cNombreCorto,
                    Login = datos.Usuario,
                    Rol = usuario.nIdRol.ToString(),
                    Hombre = usuario.bHombre,
                    FechaExpiracion = DateTime.Now.AddHours(AppSettings.Expires)
                };

                // Guardar cambios

                // Nueva sesion
                LOG_Sesiones sesion = new LOG_Sesiones
                {
                    bActivo = true,
                    cLogin = usuario.cLogin,
                    dFechaInicio = DateTime.Now,
                    dFechaFin = DateTime.Now.AddMinutes(30),
                    Usuario = usuario
                };

                DB.LOG_Sesiones.Add(sesion);

                // Crear sesión inactiva, se utiliza para poder obtener el ID de la sesion y agregarla al token
                await DB.SaveChangesAsync();

                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                // Obtener la llave utilizada para encryptar el token
                var key = Encoding.ASCII.GetBytes(AppSettings.Secret);

                // Agregar los datos que tendra el token
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, this.Utilerias.Encryptar(usuarioLog.Login)),
                    new Claim(ClaimTypes.Email, this.Utilerias.Encryptar(usuarioLog.Email)),
                    new Claim("NombreCorto", this.Utilerias.Encryptar(usuarioLog.NombreCorto)),
                    new Claim("Nombre", this.Utilerias.Encryptar(usuarioLog.Nombre)),
                    new Claim("ID", this.Utilerias.Encryptar(sesion.nIdSesion.ToString()))
                    }),
                    Expires = usuarioLog.FechaExpiracion,
                    Issuer = AppSettings.Issuer,
                    Audience = AppSettings.Audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };

                // Crear token
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // Escribir token en texto
                sesion.cToken = tokenHandler.WriteToken(token);
                sesion.bActivo = true;

                // Ecnriptar Token
                usuarioLog.Token = sesion.cToken;

                await DB.SaveChangesAsync();

                return Ok(usuarioLog);
            }

#if DEBUG
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
#else
            catch
            {
                return BadRequest("Hay problemas al validar el usuario. Espere unos minutos e intente de nuevo.");
            }
#endif
    }

        //Envio de correo para la recuperacion de contraseña
        [HttpPost]
        [Route("~/api/Usuario/EnviarCorreoRecuperacion")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> POST_EnviarCorreoDeRecuperacion(ValidarCorreoRecuperacion model)
        {
            if (model == null)
            {
                return BadRequest("Datos invalidos.");
            }

            try
            {
                var correoValidar = Utilerias.ValidarCorreo(model.Usuario);

                CAT_Usuarios usuario;

                // Si es valido como correo, buscar por correo, si no por usuario
                if (correoValidar)
                {
                    usuario = await this.DB.CAT_Usuarios.FirstOrDefaultAsync(u => u.cEmail == model.Usuario && u.bActivo == true);
                }
                else
                {
                    usuario = await this.DB.CAT_Usuarios.FirstOrDefaultAsync(u => u.cLogin == model.Usuario && u.bActivo == true);
                }


                if (usuario != null)
                { // Existe usuario con el email proporcionado
                    //Se obtiene la fecha vencimiento del ID que se generó del usuario que solicitó recuperacion de password
                    var fecVencimiento = await DB.LOG_RecuperarPassword
                                        .Where(u => u.cLogin == usuario.cLogin && u.bActivo == true)
                                        .OrderByDescending(u => u.dFechaVencimiento).FirstOrDefaultAsync();


                    LOG_RecuperarPassword nuevoPass = new LOG_RecuperarPassword();

                    //Configuracion parametrizable
                    var config = await DB.CFG_Configuracion
                                            .Where(u => u.cNombreConfig == "RUTA_LINK")
                                            .FirstOrDefaultAsync();

                    var msjParte1 = await DB.CFG_Configuracion
                                            .Where(u => u.cNombreConfig == "MSJ_RECUPERACION_CONTRASENIA_PARTE1")
                                            .FirstOrDefaultAsync();

                    var msjParte2 = await DB.CFG_Configuracion
                                            .Where(u => u.cNombreConfig == "MSJ_RECUPERACION_CONTRASENIA_PARTE2")
                                            .FirstOrDefaultAsync();

                    var msjTituloCorreo = await DB.CFG_Configuracion
                                                .Where(u => u.cNombreConfig == "TITULO_MENSAJE")
                                                .FirstOrDefaultAsync();

                    CFG_Configuracion minutosLink; minutosLink = await DB.CFG_Configuracion
                        .Where(u => u.cNombreConfig == "MINUTOS_VIDA_LINK")
                        .FirstOrDefaultAsync();

                    string minutosString = minutosLink.cMensajeConfig;

                    int minutos = Convert.ToInt32(minutosString);

                    //Si la fecha vencimiento es menor a la actual se genera contrasenia nueva
                    if (fecVencimiento == null || fecVencimiento.dFechaVencimiento < System.DateTime.Now)
                    {

                        nuevoPass.cID = Guid.NewGuid();
                        nuevoPass.cLogin = usuario.cLogin;
                        nuevoPass.dFechaGeneracion = DateTime.Now;
                        nuevoPass.dFechaVencimiento = nuevoPass.dFechaGeneracion.AddMinutes(minutos);
                        nuevoPass.bActivo = true;

                        this.DB.LOG_RecuperarPassword.Add(nuevoPass);

                        await this.DB.SaveChangesAsync();


                        string link = config.cMensajeConfig + nuevoPass.cID.ToString() + "> Restablecimiento de Contraseña</a>";
                        string msjConfi1 = msjParte1.cMensajeConfig;
                        string msjConfi2 = msjParte2.cMensajeConfig;
                        string tituloMsj = msjTituloCorreo.cMensajeConfig;

                        Servicios.EmailSender emailSender = new Servicios.EmailSender();

                        await emailSender
                                .SendEmailAsync(usuario.cEmail, tituloMsj, msjConfi1 + link + msjConfi2)
                                .ConfigureAwait(false);


                        var mensajeCorrecto = await (from e in DB.CFG_Configuracion
                                                     where e.cNombreConfig == "CORREO_ENVIADO_RECUPERACION_CONTRASENIA"
                                                     select new
                                                     {
                                                         MensajeUsuario = e.cNombreConfig
                                                     }).ToArrayAsync();

                        return Ok(mensajeCorrecto);
                    }
                    else
                    {
                        //si intenta generar otro link para otra contraseña
                        var contraseniaReciente = await (from e in DB.CFG_Configuracion
                                                         where e.cNombreConfig == "CONTRASENIA_ACTIVA"
                                                         select new
                                                         {
                                                             MensajeUsuario = e.cNombreConfig
                                                         }).ToArrayAsync();

                        return Ok(contraseniaReciente);
                    }
                }
                else
                {
                    return Ok("Usuario o EMail no registrados.");
                }
            }
            catch
            {
                return Ok(new { MensajeUsuario = "Ocurrio un problema al mandar el correo." });
            }
        }





        // PUTS
        [HttpPost]
        [Route("~/api/Usuario/ModificarUsuario")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Modificar(string id, CAT_Usuarios cAT_Usuarios)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != cAT_Usuarios.cLogin)
            {
                return BadRequest();
            }



            //DB.Entry(cAT_Usuarios).State = EntityState.Modified;

            try
            {
                //await DB.SaveChangesAsync();
                await DB.CAT_Usuarios
                        .Where(u => u.cLogin == cAT_Usuarios.cLogin)
                        .UpdateFromQueryAsync(u => new CAT_Usuarios()
                        {
                            cNombre = cAT_Usuarios.cNombre,
                            cNombreCorto = cAT_Usuarios.cNombreCorto,
                            cEmail = cAT_Usuarios.cEmail,
                            bHombre = cAT_Usuarios.bHombre
                        });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CAT_UsuariosExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("~/api/Usuario/TerminarSesion")]
        public async Task<IHttpActionResult> POST_TerminarSesionUsuario(string usuario)
        {
            try
            {
                if (usuario == null || usuario.Trim() == "")
                {
                    return BadRequest("Datos no validos.");
                }

                await DB.LOG_Sesiones
                        .Where(u => u.cLogin == usuario && u.bActivo == true)
                        .UpdateFromQueryAsync(u => new LOG_Sesiones()
                        {
                            bActivo = false,
                            dFechaFin = DateTime.Now
                        });

                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest( "Ocurrio un error al cerrar la sesión" );
            }
        }

        [HttpPost]
        [Route("~/api/Usuario/CambiarPassword")]
        public async Task<IHttpActionResult> POST_CambiarContraseña(CambioContraseñaModel datos)
        {
            string usuarioLogin;
            string passwordActual;
            string passwordNuevo;

            try
            {
                usuarioLogin = datos.Usuario;
                passwordActual = datos.PasswordActual;
                passwordNuevo = datos.PasswordNuevo;

                if (usuarioLogin == null || usuarioLogin.Trim() == "" || passwordActual == null || passwordActual.Trim() == "" ||
                    passwordNuevo == null || passwordNuevo.Trim() == "")
                {
                    return BadRequest("Datos no validos.");
                }

                var passwordActualEncryptado = passwordActual; //Utilerias.Encryptar(passwordActual);
                var passwordNuevoEncryptado = passwordNuevo; //Utilerias.Encryptar(passwordNuevo);

                if (passwordActualEncryptado == passwordNuevoEncryptado)
                {
                    return BadRequest("La contraseña nueva no debe ser igual a la actual");
                }


                // Validar que el usuario ingresado sea el mismo que inicio sesión
                if (usuarioLogin != UsuarioLoguin)
                {
                    return BadRequest("El usuario al que intenta cambiar la contraseña no es el mismo con el que inicio sesión.");
                }

                var usuario = await DB.CAT_Usuarios.Where(u => u.cLogin == usuarioLogin).FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return BadRequest("El usuario no es valido");
                }

                if (usuario.cPassword == passwordActualEncryptado)
                {
                    usuario.cPassword = passwordNuevoEncryptado;
                    usuario.dFechaExpiracion = DateTime.Now.AddMonths(1);

                    await DB.SaveChangesAsync();
                }
                else
                {
                    return BadRequest("La contraseña actual no es valida");
                }
            }
            catch
            {
                return BadRequest("Ocurrio un error al cerrar la sesión");
            }

            return Ok();
        }

        [HttpPost]
        [Route("~/api/Usuario/CambiarImagenPerfil")]
        public async Task<IHttpActionResult> POST_CambiarImagenPerfil(CambiarImagenPerfilModel imagenPerfilModel)
        {
            try
            {
                // Validar que no sea nulo
                if (imagenPerfilModel == null)
                {
                    return BadRequest("Datos invalidos.");
                }

                // Validar que el usuario sea el mismo que inicio sesion
                if (this.UsuarioLoguin != imagenPerfilModel.UsuarioLogin)
                {
                    return BadRequest("El usuario es invalido.");
                }

                // Validar que la imagen tenga datos
                if (imagenPerfilModel.ImagenBase64 == null || imagenPerfilModel.ImagenBase64.Length <= 20)
                {
                    return BadRequest("Falta ingresar la imagen.");
                }

                // Validar que el formato de la cadena base 64 sea el correcto.
                if (!imagenPerfilModel.ImagenBase64.Contains("data:image/jpeg;base64,")
                    && !imagenPerfilModel.ImagenBase64.Contains("data:image/png;base64,"))
                {
                    return BadRequest("Datos de la imagen incorrectoos, solo se admiten imagenes JPG y PNG.");
                }

                // Actualizo la imagen del usuario
                await DB.CAT_Usuarios
                        .Where(u => u.cLogin == imagenPerfilModel.UsuarioLogin)
                        .UpdateFromQueryAsync(u => new CAT_Usuarios()
                        {
                            cFotoPerfil = imagenPerfilModel.ImagenBase64
                        });

                return Ok();
            }
            catch
            {
                return BadRequest("Ocurrio un error al actualizar la imagen.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("~/api/Usuario/ActualizarContrasenia")]
        public async Task<IHttpActionResult> POST_ActualizarContraseniaUsuario(ActualizarNuevaContraseniaModel model)
        {
            try
            {
                if (model == null)
                {
                    return Ok(new { Correcto = false, Mensaje = "Datos invalidos" });
                }

                if (model.ID == null || model.ID.Trim().Length == 0)
                {
                    var msjIDInvalido = await (from e in DB.CFG_Configuracion
                                               where e.cNombreConfig == "ID_INVALIDO"
                                               select e.cMensajeConfig).FirstOrDefaultAsync();

                    return Ok(new { Correcto = false, Mensaje = msjIDInvalido });
                }
                if (model.PasswordNueva == null || model.PasswordNueva.Trim().Length < 8)
                {
                    var msjContraseniaInvalida = await (from e in DB.CFG_Configuracion
                                                        where e.cNombreConfig == "CONTRASENIA_INVALIDA"
                                                        select e.cMensajeConfig).FirstOrDefaultAsync();

                    return Ok(new { Correcto = false, Mensaje = msjContraseniaInvalida });
                }

                var idPwd = await DB.LOG_RecuperarPassword.FirstOrDefaultAsync(p => p.cID.ToString() == model.ID);

                if (idPwd == null)
                {
                    var msjIDInvalido = await (from e in DB.CFG_Configuracion
                                               where e.cNombreConfig == "ID_INVALIDO"
                                               select e.cMensajeConfig).FirstOrDefaultAsync();

                    return Ok(new { Correcto = false, Mensaje = msjIDInvalido });
                }
                if (!idPwd.bActivo)
                {
                    var msjIDInactivo = await (from e in DB.CFG_Configuracion
                                               where e.cNombreConfig == "ID_INACTIVO"
                                               select e.cMensajeConfig).FirstOrDefaultAsync();

                    return Ok(new { Correcto = false, Mensaje = msjIDInactivo });
                }
                if (idPwd.dFechaVencimiento < DateTime.Now)
                {
                    var msjIDVencido = await (from e in DB.CFG_Configuracion
                                              where e.cNombreConfig == "ID_VENCIDO"
                                              select e.cMensajeConfig).FirstOrDefaultAsync();

                    return Ok(new { Correcto = false, Mensaje = msjIDVencido });
                }

                var usuario = await DB.CAT_Usuarios.FirstOrDefaultAsync(u => u.cLogin == idPwd.cLogin);

                if (usuario == null)
                {
                    var msjUsuarioNoValido = await (from e in DB.CFG_Configuracion
                                                    where e.cNombreConfig == "USUARIO_NO_VALIDO"
                                                    select e.cMensajeConfig).FirstOrDefaultAsync();

                    return Ok(new { Correcto = false, Mensaje = msjUsuarioNoValido });
                }

                usuario.cPassword = model.PasswordNueva; //passwordEncriptada;
                idPwd.bActivo = false;

                await DB.SaveChangesAsync();

                var msjContraseniaActualizada = await (from e in DB.CFG_Configuracion
                                                       where e.cNombreConfig == "CONTRASENIA_ACTUALIZADA"
                                                       select e.cMensajeConfig).FirstOrDefaultAsync();

                return Ok(new { Correcto = true, Mensaje = msjContraseniaActualizada });
            }
            catch (DbUpdateConcurrencyException)
            {
                var msjErrorContrasenia = await (from e in DB.CFG_Configuracion
                                                 where e.cNombreConfig == "ERROR_ACTUALIZAR_CONTRASENIA"
                                                 select e.cMensajeConfig).FirstOrDefaultAsync();

                //return Ok("ERROR_ACTUALIZAR_CONTRASENIA");
                return Ok(new { Correcto = false, Mensaje = msjErrorContrasenia });
            }
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DB.Dispose();
            }

            base.Dispose(disposing);
        }

        private bool CAT_UsuariosExists(string id)
        {
            return DB.CAT_Usuarios.Count(e => e.cLogin == id) > 0;
        }

        private bool CAT_UsuariosExistsEmail(string email)
        {
            return DB.CAT_Usuarios.Count(e => e.cEmail == email) > 0;
        }

        private async Task<List<OpcionMenuModel>> ObtenerObcionesMenuPorUsuario(string usuario)
        {
            // Esta consulta optiene las opciones de menu finales ( Hijas ) que tiene acceso el usuario.
            // El include es para obtener los datos de la tabla WEB_Navegador y los TheInclude para obtener la rama padre.
            // Se agregaron 9 niveles de padres, si se requiere mas solo se agregaria otro TheInclude despues del ultimo TheInclude
            // Es muy poco probable que se cree un menu con mas de 9 niveles
            //.nRamaPadreNavigation.nRamaPadreNavigation.nRamaPadreNavigation.nRamaPadreNavigation
            DB.Configuration.LazyLoadingEnabled = true;

            var permisosUsuarios = await (
                                            from p in DB.CAT_PermisoUsuario.Where(p => p.cLogin == usuario
                                                                    && p.bActivo == true)
                                               .Include(rp => rp.Navegador)
                                                    .Include(nav => nav.Navegador)
                                                        .Include(nav2 => nav2.Navegador)
                                                            .Include(nav3 => nav3.Navegador)
                                                                .Include(nav4 => nav4.Navegador)
                                                                    .Include(nav5 => nav5.Navegador)
                                                                        .Include(nav6 => nav6.Navegador)
                                                                            .Include(nav7 => nav7.Navegador)
                                                                                .Include(nav8 => nav8.Navegador)
                                                                                    .Include(nav9 => nav9.Navegador)
                                            where p.Navegador.bActivo == true && p.bActivo == true
                                            select p.Navegador
                                          )
                                         .OrderBy(o => o.nOrden)
                                         .ToListAsync();

            // Almacenara todas las opciones agregadas
            var opcionesMenu = new List<OpcionMenuModel>();
            // Almacenara solo las opciones principales, las que no tienen padre
            var opcionesPrincipales = new List<OpcionMenuModel>();

            // Recorrer los permisos del usuario, al recorrerlos se llegara hasta la opcion raiz
            // y a esta se le relacionaran todas las opciones hijas
            foreach (CAT_Navegador menuNivel1 in permisosUsuarios)
            {
                // La funcion CrearOpcion, retorna la opcion raiz ya con sus hijas incluidas.
                var nuevaOpcion = CrearOpcionMenu(menuNivel1, null, ref opcionesMenu);
                // Validar si la opcion raiz ya esta en las opciones principales, si no lo esta se agrega.
                var estaEnOpcionesPrincipales = opcionesPrincipales.Any(p => p.Rama == nuevaOpcion.Rama);

                if (!estaEnOpcionesPrincipales)
                {
                    opcionesPrincipales.Add(nuevaOpcion);
                }
            }

            // Ordernar las opciones segun el valor que tengan en nOrden
            opcionesPrincipales = opcionesPrincipales.OrderBy(o => o.Orden).ToList();

            for (int i = 0; i < opcionesPrincipales.Count; i++)
            {
                OpcionMenuModel opcion = opcionesPrincipales[i];

                this.OrdenarMenu(ref opcion);
            }

            // Limpiar variales
            opcionesMenu.Clear();
            permisosUsuarios.Clear();

            return opcionesPrincipales;
        }

        private OpcionMenuModel CrearOpcionMenu(CAT_Navegador opcion, OpcionMenuModel opcionHijo, ref List<OpcionMenuModel> listOpciones)
        {
            OpcionMenuModel opcionExistente = listOpciones.FirstOrDefault(o => o.Rama == opcion.nRama);
            OpcionMenuModel nuevaOpcion;
            OpcionMenuModel opcionPadre; // Esta variable al final regresara la opcion principal la que ya no tiene padre

            // Validar si ya fue creada la opcion
            if (opcionExistente == null)
            { // Aun no se crea la opcion del Menu
                // Crear opcion Menu
                nuevaOpcion = new OpcionMenuModel()
                {
                    Rama = opcion.nRama,
                    NombreRama = opcion.cNombreRama,
                    //cOpcion = opcion.cOpcion,
                    Ruta = opcion.cRuta,
                    Orden = opcion.nOrden,
                    Opciones = new List<OpcionMenuModel>()
                };

                // Agregar al listado general
                listOpciones.Add(nuevaOpcion);

                // Tiene padre?
                if (opcion.NavegadorPadre != null)
                {
                    // Relacionar a la nueva opcion el codigo de la rama padre
                    nuevaOpcion.RamaPadre = opcion.NavegadorPadre.nRama;
                    // Crear opcion padre
                    opcionPadre = CrearOpcionMenu(opcion.NavegadorPadre, nuevaOpcion, ref listOpciones);

                    if (opcionHijo != null)
                    { // Como el menu tenia hijos pasa a ser padre
                        nuevaOpcion.Opciones.Add(opcionHijo);
                    }
                }
                else
                { // No tiene padre, entonces el es padre
                    opcionPadre = nuevaOpcion;

                    if (opcionHijo != null)
                    {
                        // Como es padre se le agrega el hijo
                        opcionPadre.Opciones.Add(opcionHijo);
                    }
                }
            }
            else
            { // Ya fue creada la opcion
                // Validar que no tenga padre
                if (opcion.NavegadorPadre != null)
                { // Si tiene padre
                    // Como se sabe que tiene padre y la opcion ya fue creada, el llamado a la siguiente funcion solo buscara
                    // el padre
                    opcionPadre = CrearOpcionMenu(opcion.NavegadorPadre, opcionExistente, ref listOpciones);
                }
                else
                {
                    // Retornar la opcion existente como padre
                    opcionPadre = opcionExistente;
                }

                if (opcionHijo != null)
                {
                    var codigoRamaHija = opcionHijo.Rama;
                    var existeEnOpcionesHijas = opcionExistente.Opciones.Any(hija => hija.Rama == codigoRamaHija);

                    if (!existeEnOpcionesHijas)
                    {
                        opcionExistente.Opciones.Add(opcionHijo);
                    }
                }
            }

            return opcionPadre;
        }

        private void OrdenarMenu(ref OpcionMenuModel opcion)
        {
            if (opcion.Opciones.Count > 0)
            {
                opcion.Opciones = opcion.Opciones.OrderBy(o => o.Orden).ToList();

                for (int i = 0; i < opcion.Opciones.Count; i++)
                {
                    OpcionMenuModel opcionMenu = opcion.Opciones[i];

                    this.OrdenarMenu(ref opcionMenu);
                }
            }
        }
    }
}