using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using Back_End.Models;
using Back_End.ModelsBD;
using Back_End.Clases.Enumeradores;
using System.Net.Http.Headers;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;

using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Back_End.Controllers
{
    public class SolicitudPrestamoController : ControladorBase
    {
        [HttpGet]
        [Route("~/api/SolicitudPrestamo/ObtenerSolicitudesDePrestamo")]
        public IQueryable<SolicitudPrestamosModel> GET_ObtenerSolicitudesDePrestamo()
        {
            var resultado = (from s in DB.CAP_SolicitudPrestamo
                             join e in DB.CAT_Empleados on s.nCodEmpleado equals e.nCodEmpleado
                             join r in DB.CAT_RubrosPrestamos on s.nIdRubro equals r.nIdRubro
                             where s.nIdPrestamo > 0
                             orderby s.nIdPrestamo descending
                             select new SolicitudPrestamosModel()
                                 {
                                     nIdPrestamo = s.nIdPrestamo,
                                     nCodEmpleado = e.nCodEmpleado,
                                     NombreEmpleado = e.cNombre,
                                     nIdRubro = s.nIdRubro,
                                     NombreRubro = r.cRubro,
                                     dFechaCobro = s.dFechaCobro,
                                     dFecha_Registro = s.dFecha_Registro,
                                     bActivo = s.bActivo,
                                     cRutaArchivoINE_Atras = s.cRutaArchivoINE_Atras,
                                     cRutaArchivoINE_Frente = s.cRutaArchivoINE_Frente,
                                     cRutaCheque = s.cRutaCheque,
                                     cRutaPagare = s.cRutaPagare,
                                     nImporte = s.nImporte,
                                     nVersion = s.nVersion,
                                     nSaldo = s.nSaldo,
                                     bConCorte = s.bConCorte,
                                     Nueva = false
                                 }
                             );

            return resultado;
        }

        [HttpGet]
        [Route("~/api/SolicitudPrestamo/ObtenerSolicitudesDePrestamoPorFechaCorte")]
        public IQueryable<SolicitudPrestamosModel> GET_ObtenerSolicitudesDePrestamoPorFechaCorte(int idRubro, DateTime fechaCorte)
        {
            fechaCorte = fechaCorte.AddDays(1).AddMilliseconds(-1);

            var resultado = (from s in DB.CAP_SolicitudPrestamo
                             join e in DB.CAT_Empleados on s.nCodEmpleado equals e.nCodEmpleado
                             join r in DB.CAT_RubrosPrestamos on s.nIdRubro equals r.nIdRubro
                             where s.nIdRubro == idRubro && s.dFechaCobro <= fechaCorte && s.bConCorte == false && s.bActivo == true
                             select new SolicitudPrestamosModel()
                             {
                                 nIdPrestamo = s.nIdPrestamo,
                                 nCodEmpleado = e.nCodEmpleado,
                                 NombreEmpleado = e.cNombre,
                                 nIdRubro = s.nIdRubro,
                                 NombreRubro = r.cRubro,
                                 dFechaCobro = s.dFechaCobro,
                                 dFecha_Registro = s.dFecha_Registro,
                                 bActivo = s.bActivo,
                                 cRutaArchivoINE_Atras = s.cRutaArchivoINE_Atras,
                                 cRutaArchivoINE_Frente = s.cRutaArchivoINE_Frente,
                                 cRutaCheque = s.cRutaCheque,
                                 cRutaPagare = s.cRutaPagare,
                                 nImporte = s.nImporte,
                                 nVersion = s.nVersion,
                                 nSaldo = s.nSaldo,
                                 bConCorte = s.bConCorte,
                                 Nueva = false
                             }
                             );

            return resultado;
        }

        [HttpGet]
        [Route("~/api/SolicitudPrestamo/ObtenerSolicitudesDePrestamoPorIdCorte")]
        public IQueryable<SolicitudPrestamosModel> GET_ObtenerSolicitudesDePrestamoPorIdCorte(int idCorte)
        {

            var datos = DB.CAP_CorteDetalle
                                .Include(d => d.Corte)
                                .Include(d => d.SolicitudPrestamo)
                                .Include(p => p.SolicitudPrestamo.TipoPrestamo)
                                .Include(p => p.SolicitudPrestamo.Empleado)
                                .Where(d => d.nIdCorte == idCorte);



            return (from d in datos
                    select new SolicitudPrestamosModel() {
                        nIdPrestamo = d.nIdPrestamo,
                        nCodEmpleado = d.SolicitudPrestamo.nCodEmpleado,
                        NombreEmpleado = d.SolicitudPrestamo.Empleado.cNombre,
                        nIdRubro = d.SolicitudPrestamo.nIdRubro,
                        NombreRubro = d.SolicitudPrestamo.TipoPrestamo.cRubro,
                        dFechaCobro = d.SolicitudPrestamo.dFechaCobro,
                        dFecha_Registro = d.SolicitudPrestamo.dFecha_Registro,
                        bActivo = d.SolicitudPrestamo.bActivo,
                        nImporte = d.SolicitudPrestamo.nImporte,
                        nVersion = d.SolicitudPrestamo.nVersion,
                        nSaldo = d.SolicitudPrestamo.nSaldo,
                        cRutaArchivoINE_Atras = d.SolicitudPrestamo.cRutaArchivoINE_Atras,
                        cRutaArchivoINE_Frente = d.SolicitudPrestamo.cRutaArchivoINE_Frente,
                        cRutaCheque = d.SolicitudPrestamo.cRutaCheque,
                        cRutaPagare = d.SolicitudPrestamo.cRutaPagare,
                        bConCorte = true,
                        Nueva = false,
                        nIdCorte = d.nIdCorte
                    });
        }

        [HttpGet]
        [Route("~/api/SolicitudPrestamo/ObtenerHistorial")]
        public IQueryable<SolicitudPrestamosHistorialModel> GET_ObtenerHistorial(int codigoEmpleado)
        {
            var resultado = (from s in DB.CAP_SolicitudPrestamo
                             join r in DB.CAT_RubrosPrestamos on s.nIdRubro equals r.nIdRubro
                             where s.nCodEmpleado == codigoEmpleado && s.nIdPrestamo > 0
                             select new SolicitudPrestamosHistorialModel()
                             {
                                 IdPrestamo = s.nIdPrestamo,
                                 IdRubro = s.nIdRubro,
                                 NombreRubro = r.cRubro,
                                 FechaCobro = s.dFechaCobro,
                                 FechaRegistro = s.dFecha_Registro,
                                 Importe = s.nImporte,
                                 Pagado = s.nSaldo == 0,
                                 Activo = s.bActivo
                             });

            return resultado;
        }

        [HttpGet]
        [Route("~/api/SolicitudPrestamo/ObtenerPDFSolicitud")]
        public async Task<HttpResponseMessage> GET_ObtenerPDFSolicitud(int idSolicitud)
        {
            try
            {
                var rpt = new Reportes.RptSolicitudPrestamo();

                rpt.SetParameterValue(0, idSolicitud);

                TableLogOnInfo myLogin;

                string conexion = DB.Database.Connection.ConnectionString;
                string usuario = conexion.Substring(conexion.LastIndexOf("user id")).Substring(8);
                string password = conexion.Substring(conexion.LastIndexOf("password")).Substring(9);

                usuario = usuario.Substring(0, usuario.IndexOf(';'));
                password = password.Substring(0, password.IndexOf(';'));

                foreach (Table myTable in rpt.Database.Tables)
                {
                    myLogin = myTable.LogOnInfo;
                    myLogin.ConnectionInfo.ServerName = DB.Database.Connection.DataSource;
                    myLogin.ConnectionInfo.DatabaseName = DB.Database.Connection.Database;
                    myLogin.ConnectionInfo.UserID = usuario;
                    myLogin.ConnectionInfo.Password = password;
                    myLogin.ConnectionInfo.Type = ConnectionInfoType.SQL;
                    myLogin.ConnectionInfo.AllowCustomConnection = true;
                    myLogin.ConnectionInfo.IntegratedSecurity = false;

                    myTable.ApplyLogOnInfo(myLogin);
                    myTable.Location = DB.Database.Connection.Database + ".dbo." + myTable.Location;
                }


                var pdf = rpt.ExportToStream(ExportFormatType.PortableDocFormat);
                byte[] archivo = new byte[pdf.Length];

                await pdf.ReadAsync(archivo, 0, (int)pdf.Length);

                pdf.Close();
                pdf.Dispose();
                rpt.Dispose();

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                string nombreArchivo;

                nombreArchivo = "Solicitud_" + idSolicitud.ToString().PadLeft(6, '0') + ".pdf";

                response.Content = new ByteArrayContent(archivo);

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = nombreArchivo
                };

                response.Content.Headers.Add("x-filename", nombreArchivo);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                return response;
            }
            catch
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);

                return response;
            }
        }

        [HttpGet]
        [Route("~/api/SolicitudPrestamo/ObtenerCreditoUtilizado")]
        public async Task<IHttpActionResult> GET_CreditoUtilizado(int idSolicitud, int codigoEmpleado, int idRubro)
        {
            try
            {
                return Ok(await ObtenerCreditoUtilizado(idSolicitud, codigoEmpleado, idRubro));
            }
            catch
            {
                return BadRequest("Ocurrio un problema al consultar el crédito utilizado.");
            }
        }

        [HttpGet]
        [Route("~/api/SolicitudPrestamo/ObtenerCreditoMaximo")]
        public async Task<IHttpActionResult> GET_ObtenerCreditoMaximo(int codigoEmpleado, int idRubro)
        {
            try
            {
                return Ok(await CreditoMaximo(codigoEmpleado, idRubro));
            }
            catch
            {
                return BadRequest("Ocurrio un problema al consultar el crédito maximo.");
            }
        }


        /// <summary>  
        /// Upload Document.....  
        /// </summary>        
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/SolicitudPrestamo/GuardarArchivo")]
        public async Task<IHttpActionResult> POST_GuardarArchivo(int idSolicitud, int tipoArchivo)
        {
            DbContextTransaction transaction = null;

            try
            {
                // Check if the request contains multipart/form-data.  
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());
                //access form data  
                NameValueCollection formData = provider.FormData;
                //access files  
                IList<HttpContent> files = provider.Files;

                HttpContent file1 = files[0];
                var thisFileName = file1.Headers.ContentDisposition.FileName.Trim('\"');
                string extencion = thisFileName.Substring(thisFileName.LastIndexOf('.'));
                string nombreArchivo = Guid.NewGuid() + extencion;
                //Stream archivo = await file1.ReadAsStreamAsync();
                byte[] archivo = await file1.ReadAsByteArrayAsync();
                string directorioSolicitud = AppSettings.DirectorioSolicitudPrestamo + "/" + ObtenerDirectorioLocalSolicitud(idSolicitud);
                string URL = AppSettings.URLArchivos + "/" + directorioSolicitud + '/' + nombreArchivo;

                file1.Dispose();

                transaction = DB.Database.BeginTransaction();

                switch ((TipoArchivoSolicitudEnum)tipoArchivo)
                {
                    case TipoArchivoSolicitudEnum.INE_F:
                        await DB.CAP_SolicitudPrestamo
                        .Where(d => d.nIdPrestamo == idSolicitud)
                        .UpdateFromQueryAsync(u => new CAP_SolicitudPrestamo()
                        {
                            cRutaArchivoINE_Frente = URL
                        });
                        break;
                    case TipoArchivoSolicitudEnum.INE_A:
                        await DB.CAP_SolicitudPrestamo
                        .Where(d => d.nIdPrestamo == idSolicitud)
                        .UpdateFromQueryAsync(u => new CAP_SolicitudPrestamo()
                        {
                            cRutaArchivoINE_Atras = URL
                        });
                        break;
                    case TipoArchivoSolicitudEnum.PAGARE:
                        await DB.CAP_SolicitudPrestamo
                        .Where(d => d.nIdPrestamo == idSolicitud)
                        .UpdateFromQueryAsync(u => new CAP_SolicitudPrestamo()
                        {
                            cRutaPagare = URL
                        });
                        break;
                    case TipoArchivoSolicitudEnum.CHEQUE:
                        await DB.CAP_SolicitudPrestamo
                        .Where(d => d.nIdPrestamo == idSolicitud)
                        .UpdateFromQueryAsync(u => new CAP_SolicitudPrestamo()
                        {
                            cRutaCheque = URL
                        });
                        break;
                    default:
                        break;
                }

                bool archivoGuardado = await Utilerias.SubirArchivoFTP(archivo, directorioSolicitud, nombreArchivo);

                if (archivoGuardado)
                {
                    SolicitudPrestamoGuardarArchivoRespuestaModel respuesta = new SolicitudPrestamoGuardarArchivoRespuestaModel
                    {
                        URL = URL,
                        NuevaVersion = await DB.CAP_SolicitudPrestamo.Where(s => s.nIdPrestamo == idSolicitud).Select(s => s.nVersion).FirstAsync()
                    };

                    transaction.Commit();

                    return Ok(respuesta);
                }
                else
                {
                    transaction.Rollback();

                    return BadRequest("Ocurrio un problema al guardar el archivo.");
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                return BadRequest("Ocurrio un error al guardar el archivo.");
            }
        }

        [HttpPost]
        [Route("~/api/SolicitudPrestamo/BorrarArchivo")]
        public async Task<IHttpActionResult> POST_BorrarArchivo(int idSolicitud, string nombreArchivo, int tipoArchivo)
        {
            DbContextTransaction transaction = null;

            try
            {
                string directorioSolicitud = AppSettings.DirectorioSolicitudPrestamo + "/" + ObtenerDirectorioLocalSolicitud(idSolicitud);

                switch (tipoArchivo)
                {
                    case 1:
                        if (!await DB.CAP_SolicitudPrestamo.AnyAsync(m => m.nIdPrestamo == idSolicitud && m.cRutaArchivoINE_Frente.Contains(nombreArchivo)))
                        {
                            return BadRequest("Nombre del archivo invalido.");
                        }

                        break;
                    case 2:
                        if (!await DB.CAP_SolicitudPrestamo.AnyAsync(m => m.nIdPrestamo == idSolicitud && m.cRutaArchivoINE_Atras.Contains(nombreArchivo)))
                        {
                            return BadRequest("Nombre del archivo invalido.");
                        }

                        break;
                    case 3:
                        if (!await DB.CAP_SolicitudPrestamo.AnyAsync(m => m.nIdPrestamo == idSolicitud && m.cRutaPagare.Contains(nombreArchivo)))
                        {
                            return BadRequest("Nombre del archivo invalido.");
                        }

                        break;
                    case 4:
                        if (!await DB.CAP_SolicitudPrestamo.AnyAsync(m => m.nIdPrestamo == idSolicitud && m.cRutaCheque.Contains(nombreArchivo)))
                        {
                            return BadRequest("Nombre del archivo invalido.");
                        }

                        break;
                    default:
                        break;
                }

                transaction = DB.Database.BeginTransaction();

                await DB.CAP_SolicitudPrestamo
                        .Where(m => m.nIdPrestamo == idSolicitud)
                        .UpdateFromQueryAsync(m => new CAP_SolicitudPrestamo()
                        {
                            cRutaArchivoINE_Frente = tipoArchivo == 1 ? null: m.cRutaArchivoINE_Frente,
                            cRutaArchivoINE_Atras = tipoArchivo == 2 ? null : m.cRutaArchivoINE_Atras,
                            cRutaPagare = tipoArchivo == 3 ? null : m.cRutaPagare,
                            cRutaCheque = tipoArchivo == 4 ? null : m.cRutaCheque
                        });

                bool borrado = await Utilerias.BorrarArchivoFTP(directorioSolicitud, nombreArchivo);

                if (borrado)
                {
                    int version;

                    version = await DB.CAP_SolicitudPrestamo.Where(s => s.nIdPrestamo == idSolicitud).Select(s => s.nVersion).FirstAsync();

                    transaction.Commit();

                    return Ok(version);
                }
                else
                {
                    transaction.Rollback();
                    return BadRequest("No se pudo borrar el archivo.");
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                return BadRequest("Ocurrio un error al borrar el archivo.");
            }
        }

        [HttpPost]
        [Route("~/api/SolicitudPrestamo/GuardarSolicitudPrestamo")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> POST_Guardar(CAP_SolicitudPrestamo solicitud)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (solicitud.nIdRubro <= 0)
            {
                return BadRequest("Debe ingresar un rubro.");
            }

            if (solicitud.nCodEmpleado <= 0)
            {
                return BadRequest("Debe ingresar un empleado.");
            }

            if (solicitud.nImporte == 0)
            {
                return BadRequest("Debe ingresar un importe.");
            }

            if (solicitud.dFechaCobro <= DateTime.Today)
            {
                return BadRequest("Debe ingresar una fecha de cobro mayor al día de hoy.");
            }

            try
            {
                CAT_RubrosPrestamos rubro = await DB.CAT_RubrosPrestamos.FindAsync(solicitud.nIdRubro);

                if (rubro == null)
                {
                    return BadRequest("El rubro no es valido.");
                }

                CAT_Empleados empleado = await DB.CAT_Empleados.Include(e => e.Puesto).FirstOrDefaultAsync(e => e.nCodEmpleado == solicitud.nCodEmpleado);

                if (empleado == null)
                {
                    return BadRequest("El empleado no es valido.");
                }

                decimal creditoUtilizado = await ObtenerCreditoUtilizado(0, solicitud.nCodEmpleado, solicitud.nIdRubro);
                decimal maximo = await CreditoMaximo(solicitud.nCodEmpleado, solicitud.nIdRubro);

                if (creditoUtilizado + solicitud.nImporte > maximo)
                {
                    return BadRequest("Exedio el crédito maximo permitido.");
                }

                int idPrestamo = (await DB.CAP_SolicitudPrestamo.MaxAsync(p => (int?)p.nIdPrestamo) ?? 0) + 1;

                solicitud.nIdPrestamo = idPrestamo;
                solicitud.nSaldo = solicitud.nImporte;
                solicitud.bActivo = true;
                solicitud.cUsuario_Registro = UsuarioLoguin;
                solicitud.cUsuario_UltimaModificacion = UsuarioLoguin;
                solicitud.dFecha_Registro = DateTime.Now;
                solicitud.dFecha_UltimaModificacion = DateTime.Now;

                DB.CAP_SolicitudPrestamo.Add(solicitud);

                CAP_MovimientosCuenta movimiento = new CAP_MovimientosCuenta();

                int idMovimiento = (await DB.CAP_MovimientosCuenta.MaxAsync(m => (int?)m.nIdMovimiento) ?? 0) + 1;

                movimiento.nIdMovimiento = idMovimiento;
                movimiento.nIdCuenta = 1; // Cuenta de prestamos
                movimiento.nIdCategoria = 0;
                movimiento.nIdTipoMovimiento = 1; // Prestamos
                movimiento.nIdPrestamo = idPrestamo;
                movimiento.nImporte = solicitud.nImporte;
                movimiento.cObservaciones = "Captura en solicitud de prestamos";
                movimiento.cRutaDocumento = null;
                movimiento.bActivo = true;
                movimiento.cUsuario_Registro = UsuarioLoguin;
                movimiento.dFecha_Registro = DateTime.Now;


                DB.CAP_MovimientosCuenta.Add(movimiento);

                await DB.SaveChangesAsync();

                var respuesta = await Utilerias.CrearCarpetaFTP(AppSettings.DirectorioSolicitudPrestamo, ObtenerDirectorioLocalSolicitud(solicitud.nIdPrestamo));

                return Ok(solicitud.nIdPrestamo);
            }
            catch (Exception)
            {
                return BadRequest("Ocurrio un problema al guardar el empleado. Por favor intentelo dentro de unos minutos.");
            }
        }

        [HttpPost]
        [Route("~/api/SolicitudPrestamo/ModificarSolicitudPrestamo")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Modificar(CAP_SolicitudPrestamo solicitud)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (solicitud.nIdPrestamo <= 0)
            {
                return BadRequest("Solicitud invalida.");
            }

            if (solicitud.nIdRubro <= 0)
            {
                return BadRequest("Debe ingresar un rubro.");
            }

            if (solicitud.nCodEmpleado <= 0)
            {
                return BadRequest("Debe ingresar un empleado.");
            }

            if (solicitud.nImporte == 0)
            {
                return BadRequest("Debe ingresar un importe.");
            }

            if (solicitud.dFechaCobro <= DateTime.Today)
            {
                return BadRequest("Debe ingresar una fecha de cobro mayor al día de hoy.");
            }

            if (solicitud.bConCorte)
            {
                return BadRequest("El prestamo ya se encuentra en un corte.");
            }

            using (DbContextTransaction transaccion = DB.Database.BeginTransaction())
            {
                try
                {
                    if (!DB.CAT_RubrosPrestamos.Any(p => p.nIdRubro == solicitud.nIdRubro))
                    {
                        transaccion.Rollback();

                        return BadRequest("El rubro no es valido.");
                    }

                    if (!DB.CAT_Empleados.Any(e => e.nCodEmpleado == solicitud.nCodEmpleado))
                    {
                        transaccion.Rollback();

                        return BadRequest("El empleado no es valido.");
                    }

                    if (DB.CAP_SolicitudPrestamo.Any(s => s.nIdPrestamo != solicitud.nIdPrestamo && s.nCodEmpleado == solicitud.nCodEmpleado && s.nSaldo > 0 && s.nIdRubro == solicitud.nIdRubro && s.bActivo == true))
                    {
                        transaccion.Rollback();

                        return BadRequest("El empleado ya cuenta con un prestamo del mismo rubro activo y con deuda.");
                    }

                    var solicitudBD = DB.CAP_SolicitudPrestamo.FirstOrDefault(s => s.nIdPrestamo == solicitud.nIdPrestamo);

                    if (solicitudBD == null)
                    {
                        transaccion.Rollback();

                        return BadRequest("La solicitud no existe.");
                    }

                    if (solicitudBD.nVersion != solicitud.nVersion)
                    {
                        transaccion.Rollback();

                        return BadRequest("La solicitud de prestamo sufrio cambios despues de haberla consultado. Por favor limpie la pantalla y vuelva a consultarla.");
                    }

                    if (solicitudBD.nSaldo != solicitudBD.nImporte)
                    {
                        transaccion.Rollback();

                        return BadRequest("La solicitud ya tiene pagos, no puede ser modificada.");
                    }

                    if (solicitudBD.bConCorte)
                    {
                        transaccion.Rollback();

                        return BadRequest("El prestamo ya se encuentra en un corte.");
                    }

                    await DB.CAP_SolicitudPrestamo
                        .Where(d => d.nIdPrestamo == solicitud.nIdPrestamo)
                        .UpdateFromQueryAsync(u => new CAP_SolicitudPrestamo()
                        {
                            nImporte = solicitud.nImporte,
                            nSaldo = solicitud.nImporte,
                            dFechaCobro = solicitud.dFechaCobro,
                            cRutaArchivoINE_Frente = solicitud.cRutaArchivoINE_Frente,
                            cRutaArchivoINE_Atras = solicitud.cRutaArchivoINE_Atras,
                            cRutaPagare = solicitud.cRutaPagare,
                            cRutaCheque = solicitud.cRutaCheque,
                            bActivo = solicitud.bActivo,
                            cUsuario_UltimaModificacion = UsuarioLoguin,
                            dFecha_UltimaModificacion = DateTime.Now,
                            cUsuario_Eliminacion = solicitud.bActivo ? null : UsuarioLoguin,
                            dFecha_Eliminacion = solicitud.bActivo ? (DateTime?)null : DateTime.Now
                        });

                    string directorioSolicitud = AppSettings.DirectorioSolicitudPrestamo + "/" + ObtenerDirectorioLocalSolicitud(solicitud.nIdPrestamo);

                    if (solicitud.cRutaArchivoINE_Frente != solicitudBD.cRutaArchivoINE_Frente)
                    {
                        string ruta = solicitudBD.cRutaArchivoINE_Frente;

                        if (ruta != null)
                        {
                            string nombreArchivo = ruta.Substring(ruta.LastIndexOf("/") + 1);

                            await Utilerias.BorrarArchivoFTP(directorioSolicitud, nombreArchivo);
                        }
                    }

                    if (solicitud.cRutaArchivoINE_Atras != solicitudBD.cRutaArchivoINE_Atras)
                    {
                        string ruta = solicitudBD.cRutaArchivoINE_Atras;

                        if (ruta != null)
                        {
                            string nombreArchivo = ruta.Substring(ruta.LastIndexOf("/") + 1);

                            await Utilerias.BorrarArchivoFTP(directorioSolicitud, nombreArchivo);
                        }
                    }

                    if (solicitud.cRutaPagare != solicitudBD.cRutaPagare)
                    {
                        string ruta = solicitudBD.cRutaPagare;

                        if (ruta != null)
                        {
                            string nombreArchivo = ruta.Substring(ruta.LastIndexOf("/") + 1);

                            await Utilerias.BorrarArchivoFTP(directorioSolicitud, nombreArchivo);
                        }
                    }

                    if (solicitud.cRutaCheque != solicitudBD.cRutaCheque)
                    {
                        string ruta = solicitudBD.cRutaCheque;

                        if (ruta != null)
                        {
                            string nombreArchivo = ruta.Substring(ruta.LastIndexOf("/") + 1);

                            await Utilerias.BorrarArchivoFTP(directorioSolicitud, nombreArchivo);
                        }
                    }

                    await DB.CAP_MovimientosCuenta
                            .Where(m => m.nIdPrestamo == solicitud.nIdPrestamo && m.nIdTipoMovimiento == 1 && m.bActivo == true)
                            .UpdateFromQueryAsync(m => new CAP_MovimientosCuenta()
                            {
                                nImporte = solicitud.nImporte
                            });

                    transaccion.Commit();

                    return Ok();
                }
                catch (Exception)
                {
                    transaccion.Rollback();

                    return BadRequest("Ocurrio un problema al guardar el empleado. Por favor intentelo dentro de unos minutos.");
                }
            }
        }
    
        [HttpPost]
        [Route("~/api/SolicitudPrestamo/CancelarSolicitudPrestamo")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Cancelar(int idSolicitud, short version)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (idSolicitud == 0)
            {
                return BadRequest("Falta el ID de la solicitud de presrtamo.");
            }

            using (DbContextTransaction transaccion = DB.Database.BeginTransaction())
            {
                try
                {
                    var solicitud = DB.CAP_SolicitudPrestamo.FirstOrDefault(s => s.nIdPrestamo == idSolicitud);

                    if (solicitud == null)
                    {
                        transaccion.Rollback();

                        return BadRequest("ID invalido.");
                    }

                    if (solicitud.nVersion != version)
                    {
                        transaccion.Rollback();

                        return BadRequest("La solicitud sufrio cambios despues de haberla obtenido. Refresque los datos en su pantalla con la tecla F5 o actualizar y consulte de nuevo la solicitud de prestamo.");
                    }

                    await DB.CAP_SolicitudPrestamo
                            .Where(s => s.nIdPrestamo == idSolicitud)
                            .UpdateFromQueryAsync(s => new CAP_SolicitudPrestamo()
                            {
                                bActivo = false,
                                cUsuario_UltimaModificacion = UsuarioLoguin,
                                dFecha_UltimaModificacion = DateTime.Now,
                                cUsuario_Eliminacion = UsuarioLoguin,
                                dFecha_Eliminacion = DateTime.Now
                            });

                    int idMovimiento = (await DB.CAP_MovimientosCuenta.MaxAsync(m => (int?)m.nIdMovimiento) ?? 0) + 1;

                    await DB.CAP_MovimientosCuenta
                            .Where(m => m.nIdPrestamo == solicitud.nIdPrestamo && m.nIdTipoMovimiento == 1 && m.bActivo == true)
                            .InsertFromQueryAsync(mov => new CAP_MovimientosCuenta()
                            {
                                nIdMovimiento = idMovimiento,
                                nIdCuenta = 1, // Cuenta de prestamos
                                nIdCategoria = 0,
                                nIdTipoMovimiento = 2, // CANCELACION PRESTAMOS
                                nIdPrestamo = mov.nIdPrestamo,
                                nIdMovimientoCancela = mov.nIdMovimiento,
                                nImporte = mov.nImporte,
                                cObservaciones = "CANELACION SOLICITUD DE PRESTAMOS",
                                cRutaDocumento = null,
                                bActivo = true,
                                cUsuario_Registro = UsuarioLoguin,
                                dFecha_Registro = DateTime.Now
                            });

                    transaccion.Commit();

                    return Ok();
                }
                catch
                {
                    transaccion.Rollback();

                    return BadRequest("Ocurrio un problema al cancelar la solicitud de prestamo. Intentelo mas tarde.");
                }
            }
        }

        private string ObtenerDirectorioLocalSolicitud(int idSolicitudPrestamo)
        {
            //return Path.Combine(this.ObtenerDirectorioLoca(), "Solicitudes\\" + idSolicitudPrestamo.ToString().PadLeft(6, '0'));
            return idSolicitudPrestamo.ToString().PadLeft(6, '0');
        }

        private async Task<decimal> ObtenerCreditoUtilizado(int idSolicitud, int codigoEmpleado, int idRubro)
        {
            DateTime fechaRegistro = await DB.CAP_SolicitudPrestamo.Where(s => s.nIdPrestamo == idSolicitud).MaxAsync(s => (DateTime?)s.dFecha_Registro) ?? DateTime.Now;

            decimal prestado = await DB.CAP_SolicitudPrestamo
                                        .Where(s => s.nIdPrestamo != idSolicitud && s.nCodEmpleado == codigoEmpleado && s.nIdRubro == idRubro && s.dFecha_Registro < fechaRegistro && s.bActivo == true)
                                        .Select(s => new
                                        {
                                            prestamo = s,
                                            corteActivoPagado = s.CorteDetalle.Where(c => c.Corte.bPagado == true && c.Corte.bActivo == true).Count() > 0
                                        })
                                        .Where(s => s.corteActivoPagado == false)
                                        .Select(s => (decimal?)s.prestamo.nImporte)
                                        .SumAsync() ?? 0;

            return prestado;
        }

        private async Task<decimal> CreditoMaximo(int codigoEmpleado, int idRubro)
        {
            CAT_RubrosPrestamos rubro = await DB.CAT_RubrosPrestamos.FindAsync(idRubro);

            if (rubro == null)
            {
                return 0;
            }

            CAT_Empleados empleado = await DB.CAT_Empleados.Include(e => e.Puesto).FirstOrDefaultAsync(e => e.nCodEmpleado == codigoEmpleado);

            if (empleado == null)
            {
                return 0;
            }

            decimal maximo = 0;
            string años = ((DateTime.Today - empleado.dFechaIngreso).Days / 365.4).ToString();
            int inicioDecimales = años.LastIndexOf('.');
            
            años = años.Substring(0, inicioDecimales <= 1 ? 1: inicioDecimales);

            if (rubro.bAguinaldo)
            {
                CAT_TabuladorAguinaldo tab = await DB.CAT_TabuladorAguinaldo.FindAsync(int.Parse(años));

                if (tab == null)
                {
                    return 0;
                }

                maximo = Math.Round((empleado.Puesto.nSueldo * tab.nDiasSalario) / 2, 2);
            }
            else if (rubro.bFunerario)
            {
                maximo = rubro.nImporteMaximo;
            }
            else
            {
                CAT_TabuladorPrimaVacacional tab = await DB.CAT_TabuladorPrimaVacacional.FindAsync(int.Parse(años));

                if (tab == null)
                {
                    return 0;
                }

                maximo = Math.Round((empleado.Puesto.nSueldo * tab.nDiasSalario) * (tab.nPorcPrima / 100), 2);
            }

            return maximo;
        }
    }
}
