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
using System.Net.Http.Headers;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;
using Back_End.Models;
using Back_End.ModelsBD;

namespace Back_End.Controllers
{
    public class CortePrestamoController : ControladorBase
    {
        [HttpGet]
        [Route("~/api/CortePrestamo/ObtenerCortePrestamoListado")]
        public IQueryable<CortePrestamoModel> GET_ObtenerListado()
        {
            return (from c in DB.CAP_Corte.Include(c => c.Rubro)
                    where c.nIdCorte > 0
                    orderby c.nIdCorte descending
                    select new CortePrestamoModel()
                    {
                        nIdCorte = c.nIdCorte,
                        nIdRubro = c.nIdRubro,
                        NombreRubro = c.Rubro.cRubro,
                        dFechaCorte = c.dFechaCorte,
                        nTotal = c.nTotal,
                        bPagado = c.bPagado,
                        bActivo = c.bActivo,
                        cUsuario_Registro = c.cUsuario_Registro,
                        dFecha_Registro = c.dFecha_Registro,
                        cUsuario_UltimaModificacion = c.cUsuario_UltimaModificacion,
                        dFecha_UltimaModificacion = c.dFecha_UltimaModificacion,
                        cUsuario_Eliminacion = c.cUsuario_Eliminacion,
                        dFecha_Eliminacion = c.dFecha_Eliminacion,
                        Nuevo = false,
                        nVersion = c.nVersion
                    });
        }

        [HttpGet]
        [Route("~/api/CortePrestamo/ObtenerCortePrestamoListadoFiltrado")]
        public IQueryable<CortePrestamoModel> GET_ObtenerListadoFiltrado(DateTime fechaIni, DateTime fechaFin, short estatusCorte, int idRubro)
        {
            fechaIni = fechaIni.Date;
            fechaFin = fechaFin.Date.AddDays(1).AddMilliseconds(-1);

            return (from c in DB.CAP_Corte
                        .Include(c => c.Rubro)
                    where c.nIdCorte > 0 && c.nIdRubro == (idRubro > 0 ? idRubro: c.nIdRubro) &&
                          c.bPagado == (estatusCorte == 2 ? c.bPagado: estatusCorte != 0) &&
                          c.dFecha_Registro >= fechaIni && c.dFecha_Registro <= fechaFin
                    orderby c.nIdCorte descending
                    select new CortePrestamoModel()
                    {
                        nIdCorte = c.nIdCorte,
                        nIdRubro = c.nIdRubro,
                        NombreRubro = c.Rubro.cRubro,
                        dFechaCorte = c.dFechaCorte,
                        nTotal = c.nTotal,
                        bPagado = c.bPagado,
                        bActivo = c.bActivo,
                        cUsuario_Registro = c.cUsuario_Registro,
                        dFecha_Registro = c.dFecha_Registro,
                        cUsuario_UltimaModificacion = c.cUsuario_UltimaModificacion,
                        dFecha_UltimaModificacion = c.dFecha_UltimaModificacion,
                        cUsuario_Eliminacion = c.cUsuario_Eliminacion,
                        dFecha_Eliminacion = c.dFecha_Eliminacion,
                        Nuevo = false,
                        nVersion = c.nVersion
                    });
        }

        [HttpGet]
        [Route("~/api/CortePrestamo/ObtenerCortePrestamo")]
        [ResponseType(typeof(CAP_Corte))]
        public async Task<IHttpActionResult> GET_ObtenerPorId(int idCorte)
        {
            CAP_Corte cAP_Corte = await DB.CAP_Corte
                .Include(c => c.Rubro)
                .FirstOrDefaultAsync(c => c.nIdCorte == idCorte);

            if (cAP_Corte == null)
            {
                return NotFound();
            }

            return Ok(new CortePrestamoModel() {
                    nIdCorte = cAP_Corte.nIdCorte,
                    nIdRubro = cAP_Corte.nIdRubro,
                    NombreRubro = cAP_Corte.Rubro.cRubro,
                    dFechaCorte = cAP_Corte.dFechaCorte,
                    nTotal = cAP_Corte.nTotal,
                    bPagado = cAP_Corte.bPagado,
                    bActivo = cAP_Corte.bActivo,
                    cUsuario_Registro = cAP_Corte.cUsuario_Registro,
                    dFecha_Registro = cAP_Corte.dFecha_Registro,
                    cUsuario_UltimaModificacion = cAP_Corte.cUsuario_UltimaModificacion,
                    dFecha_UltimaModificacion = cAP_Corte.dFecha_UltimaModificacion,
                    cUsuario_Eliminacion = cAP_Corte.cUsuario_Eliminacion,
                    dFecha_Eliminacion = cAP_Corte.dFecha_Eliminacion,
                    Nuevo = false,
                    nVersion = cAP_Corte.nVersion
                });
        }

        [HttpGet]
        [Route("~/api/CortePrestamo/ObtenerPDFCortePrestamo")]
        public async Task<HttpResponseMessage> GET_ObtenerPDFCortePrestamo(int idCorte)
        {
            try
            {
                var rpt = new Reportes.RptCorte();

                rpt.SetParameterValue(0, idCorte);

                TableLogOnInfo myLogin;

                string conexion = DB.Database.Connection.ConnectionString;
                string usuario = string.Empty;
                string password = string.Empty;

                if (conexion.Contains("user id"))
                {
                    usuario = conexion.Substring(conexion.LastIndexOf("user id")).Substring(8);
                    password = conexion.Substring(conexion.LastIndexOf("password")).Substring(9);
                    usuario = usuario.Substring(0, usuario.IndexOf(';'));
                    password = password.Substring(0, password.IndexOf(';'));
                }

                foreach (Table myTable in rpt.Database.Tables)
                {
                    myLogin = myTable.LogOnInfo;
                    myLogin.ConnectionInfo.ServerName = DB.Database.Connection.DataSource; 
                    myLogin.ConnectionInfo.DatabaseName = DB.Database.Connection.Database;
                    myLogin.ConnectionInfo.UserID = usuario;
                    myLogin.ConnectionInfo.Password = password;
                    myLogin.ConnectionInfo.Type = ConnectionInfoType.SQL;
                    myLogin.ConnectionInfo.AllowCustomConnection = true;
                    myLogin.ConnectionInfo.IntegratedSecurity = string.IsNullOrWhiteSpace(usuario);

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

                nombreArchivo = "Corte_" + idCorte.ToString().PadLeft(6, '0') + ".pdf";

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

        [HttpPost]
        [Route("~/api/CortePrestamo/GuardarCorte")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> POST_Guardar(CortePrestamoModel datos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int[] idPrestamos = datos.Prestamos.Select(p => p.nIdPrestamo).ToArray();

                var prestamos = await DB.CAP_SolicitudPrestamo
                                    .Where(p => idPrestamos.Contains(p.nIdPrestamo))
                                    .ToListAsync();

                var r = from p in prestamos
                        join p2 in datos.Prestamos on p.nIdPrestamo equals p2.nIdPrestamo
                        where p.nVersion != p2.nVersion
                        select p;

                if (r.Count() > 0)
                {
                    return BadRequest("Hay solicitudes de prestamos que fueron modificadas despues de haber sido leidas. Por favor vuelva a cargar de nuevo la información de los prestamos.");
                }


                int id = (await DB.CAP_Corte.MaxAsync(c => (int?)c.nIdCorte) ?? 0) + 1;

                CAP_Corte corte = new CAP_Corte()
                {
                    nIdCorte = id,
                    nIdRubro = datos.nIdRubro,
                    dFechaCorte = datos.dFechaCorte,
                    nTotal = datos.nTotal,
                    bPagado = false,
                    bActivo = true,
                    cUsuario_Registro = UsuarioLoguin,
                    dFecha_Registro = DateTime.Now,
                    cUsuario_UltimaModificacion = UsuarioLoguin,
                    dFecha_UltimaModificacion = DateTime.Now
                };

                DB.CAP_Corte.Add(corte);

                prestamos.ForEach(p =>
                {
                    DB.CAP_CorteDetalle.Add(new CAP_CorteDetalle()
                    {
                        nIdCorte = id,
                        nIdPrestamo = p.nIdPrestamo,
                        nImporte = p.nImporte
                    });

                    p.bConCorte = true;
                });

                await DB.SaveChangesAsync();

                return Ok(id);
            }
            catch
            {
                return BadRequest("Ocurrio un problema al guardar el corte.");
            }
        }

        [HttpPost]
        [Route("~/api/CortePrestamo/CancelarCorte")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Cancelar(int idCorte, int version)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                DB.Configuration.LazyLoadingEnabled = true;

                CAP_Corte corte = await DB.CAP_Corte.FindAsync(idCorte);

                if (corte == null)
                {
                    return BadRequest("El ID del corte no es valido.");
                }

                if (corte.bPagado)
                {
                    return BadRequest("No se puede cancelar un corte ya PAGADO.");
                }

                if (corte.nVersion != version)
                {
                    return BadRequest("El corte fue modificado despues de haberlo consultado. Limpie la pantalla y vuelva a consultarlo.");
                }

                corte.bActivo = false;
                corte.cUsuario_UltimaModificacion = UsuarioLoguin;
                corte.dFecha_UltimaModificacion = DateTime.Now;
                corte.cUsuario_Eliminacion = UsuarioLoguin;
                corte.dFecha_Eliminacion = DateTime.Now;

                corte.CorteDetalle.ToList().ForEach(d => d.SolicitudPrestamo.bConCorte = false);

                await DB.SaveChangesAsync();

                return Ok();
            }
            catch
            {
                return BadRequest("Ocurrio un problema al guardar el corte.");
            }
        }

        [HttpPost]
        [Route("~/api/CortePrestamo/PagarCorte")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Pagar(int idCorte, int version)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (DbContextTransaction transaccion = DB.Database.BeginTransaction())
            {
                try
                {
                    //DB.Configuration.LazyLoadingEnabled = true;

                    CAP_Corte corte = await DB.CAP_Corte.FindAsync(idCorte);

                    if (corte == null)
                    {
                        transaccion.Rollback();

                        return BadRequest("El ID del corte no es valido.");
                    }

                    if (corte.bPagado)
                    {
                        transaccion.Rollback();

                        return BadRequest("No se puede pagar un corte ya PAGADO.");
                    }

                    if (corte.nVersion != version)
                    {
                        transaccion.Rollback();

                        return BadRequest("El corte fue modificado despues de haberlo consultado. Limpie la pantalla y vuelva a consultarlo.");
                    }

                    if (corte.bActivo == false)
                    {
                        transaccion.Rollback();

                        return BadRequest("No se puede pagar un corte cancelado.");
                    }

                    corte.bPagado = true;
                    corte.cUsuario_UltimaModificacion = UsuarioLoguin;
                    corte.dFecha_UltimaModificacion = DateTime.Now;

                    var prestamos = (from d in DB.CAP_CorteDetalle
                                     join p in DB.CAP_SolicitudPrestamo on d.nIdPrestamo equals p.nIdPrestamo
                                     where d.nIdCorte == idCorte
                                     select p);

                    await prestamos.ForEachAsync(p => p.nSaldo = 0);

                    int idMovimiento = (await DB.CAP_MovimientosCuenta.MaxAsync(m => (int?)m.nIdMovimiento) ?? 0) + 1;

                    CAP_MovimientosCuenta movimiento = new CAP_MovimientosCuenta()
                    {
                        nIdMovimiento = idMovimiento,
                        nIdCuenta = 1, // Cuenta de prestamos
                        nIdCategoria = 0,
                        nIdTipoMovimiento = 3, // PAGO PRESTAMOS
                        nIdPrestamo = null,
                        nIdCorte = idCorte,
                        nIdMovimientoCancela = null,
                        nImporte = corte.nTotal,
                        cObservaciones = "PAGO CORTE PRESTAMOS",
                        cRutaDocumento = null,
                        bActivo = true,
                        cUsuario_Registro = UsuarioLoguin,
                        dFecha_Registro = DateTime.Now
                    };

                    DB.CAP_MovimientosCuenta.Add(movimiento);

                    await DB.SaveChangesAsync();

                    transaccion.Commit();

                    return Ok();
                }
                catch
                {
                    transaccion.Rollback();

                    return BadRequest("Ocurrio un problema al guardar el corte.");
                }
            }
        }

        [HttpPost]
        [Route("~/api/CortePrestamo/CancelarPagoCorte")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_CanelarPago(int idCorte, int version)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            using (DbContextTransaction transaccion = DB.Database.BeginTransaction())
            {
                try
                {
                    CAP_Corte corte = await DB.CAP_Corte.FindAsync(idCorte);

                    if (corte == null)
                    {
                        transaccion.Rollback();

                        return BadRequest("El ID del corte no es valido.");
                    }

                    if (corte.bPagado == false)
                    {
                        transaccion.Rollback();

                        return BadRequest("No se puede cancelar un pago de un corte que no esta pagado.");
                    }

                    if (corte.nVersion != version)
                    {
                        transaccion.Rollback();

                        return BadRequest("El corte fue modificado despues de haberlo consultado. Limpie la pantalla y vuelva a consultarlo.");
                    }

                    if (corte.bActivo == false)
                    {
                        transaccion.Rollback();

                        return BadRequest("No se puede pagar un corte cancelado.");
                    }

                    corte.bPagado = false;
                    corte.cUsuario_UltimaModificacion = UsuarioLoguin;
                    corte.dFecha_UltimaModificacion = DateTime.Now;

                    var prestamos = (from d in DB.CAP_CorteDetalle
                                     join p in DB.CAP_SolicitudPrestamo on d.nIdPrestamo equals p.nIdPrestamo
                                     where d.nIdCorte == idCorte
                                     select p);

                    await prestamos.ForEachAsync(p => p.nSaldo = p.nImporte);

                    int idMovimiento = (await DB.CAP_MovimientosCuenta.MaxAsync(m => (int?)m.nIdMovimiento) ?? 0) + 1;
                    int idMovimientoCancela = await DB.CAP_MovimientosCuenta.Where(mov => mov.nIdCorte == idCorte && mov.bActivo == true && mov.nIdTipoMovimiento == 3).MaxAsync(m => m.nIdMovimiento);

                    CAP_MovimientosCuenta movimiento = new CAP_MovimientosCuenta()
                    {
                        nIdMovimiento = idMovimiento,
                        nIdCuenta = 1, // Cuenta de prestamos
                        nIdCategoria = 0,
                        nIdTipoMovimiento = 4, // PAGO PRESTAMOS
                        nIdPrestamo = null,
                        nIdCorte = idCorte,
                        nIdMovimientoCancela = idMovimientoCancela,
                        nImporte = corte.nTotal,
                        cObservaciones = "CANCELACION PAGO CORTE PRESTAMOS",
                        cRutaDocumento = null,
                        bActivo = true,
                        cUsuario_Registro = UsuarioLoguin,
                        dFecha_Registro = DateTime.Now
                    };

                    DB.CAP_MovimientosCuenta.Add(movimiento);

                    await DB.SaveChangesAsync();

                    transaccion.Commit();

                    return Ok();
                }
                catch
                {
                    transaccion.Rollback();

                    return BadRequest("Ocurrio un problema al guardar el corte.");
                }
            }
        }

        // DELETE: api/Corte/5
        [ResponseType(typeof(CAP_Corte))]
        public async Task<IHttpActionResult> DeleteCAP_Corte(int id)
        {
            CAP_Corte cAP_Corte = await DB.CAP_Corte.FindAsync(id);
            if (cAP_Corte == null)
            {
                return NotFound();
            }

            DB.CAP_Corte.Remove(cAP_Corte);
            await DB.SaveChangesAsync();

            return Ok(cAP_Corte);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DB.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}