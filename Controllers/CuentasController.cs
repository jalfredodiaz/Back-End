using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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


namespace Back_End.Controllers
{
    public class CuentasController : ControladorBase
    {
        [HttpGet]
        [Route("~/api/Cuentas/ObtenerListadoCuentas")]
        public IQueryable<CuentaModel> GET_ObtenerListado()
        {
            return (from c in DB.CAT_Cuentas
                    select new CuentaModel()
                    {
                        nIdCuenta = c.nIdCuenta,
                        cCuenta = c.cCuenta
                    });
        }

        [HttpGet]
        [Route("~/api/Cuentas/ObtenerListadoMovimientosPorCuenta")]
        public async Task<IHttpActionResult> GET_ObtenerListadoMovimientosPorCuenta(int idCuenta, bool esIngreso)
        {
            if (idCuenta <= 0)
            {
                return BadRequest("Cuenta invalida.");
            }

            try
            {
                List<int> tipoMovimientos = new List<int>();

                if (esIngreso)
                {
                    tipoMovimientos.Add(5);
                    tipoMovimientos.Add(6);
                } else
                {
                    tipoMovimientos.Add(7);
                    tipoMovimientos.Add(8);
                }

                var res = await (from m in DB.CAP_MovimientosCuenta
                                        .Include(m => m.MovimientosCuentaCancela)
                                        .Include(m => m.CategoriaGasto)
                                        .Include(m => m.TipoMovimiento)
                                        .Where(m => m.nIdCuenta == idCuenta && tipoMovimientos.Contains(m.nIdTipoMovimiento))
                                 select new RegistroGastosIngresosModel()
                                 {
                                     nIdCuenta = idCuenta,
                                     nIdMovimiento = m.nIdMovimiento,
                                     nIdMovimientoCancela = m.nIdMovimientoCancela ?? 0,
                                     nIdCategoria = m.nIdCategoria,
                                     NombreCategoria = m.CategoriaGasto.cCategoria,
                                     nIdTipoMovimiento = m.nIdTipoMovimiento,
                                     NombreTipoMovimiento = m.TipoMovimiento.cTipoMovimiento,
                                     dFecha_Registro = m.dFecha_Registro,
                                     bActivo = m.bActivo,
                                     cObservaciones = m.cObservaciones,
                                     cRutaDocumento = m.cRutaDocumento,
                                     nImporte = m.nImporte,
                                     Nuevo = false,
                                     Cancelado = m.MovimientosCuentaCancela.Count() > 0
                                 }).OrderByDescending(m => m.dFecha_Registro).ToArrayAsync();

                return Ok(res);
            }
            catch
            {
                return BadRequest("Ocurrio un error al obtener los movimientos por cuenta.");
            }
        }

        [HttpPost]
        [Route("~/api/Cuentas/ObtenerSaldoCuenta")]
        public async Task<IHttpActionResult> POST_ObtenerSaldoCuenta(ParametrosConsultaMovimientosCuentaModel datos)
        {
            CuentaConMovimientosModel resultado = new CuentaConMovimientosModel();

            try
            {
                int idCuenta = datos.idCuenta;
                DateTime fechaIni = datos.fechaIni.Date; // Me aseguro de haber quitado la hora
                DateTime fechaFin = datos.fechaFin.Date;

                // Me aseguro de haber quitado la hora y le coloco la hora de un milisegundo antes de terminar el día
                fechaFin = fechaFin.AddDays(1).AddMilliseconds(-1);

                resultado.SaldoInicial = await DB.CAT_Cuentas.Where(c => c.nIdCuenta == idCuenta).Select(c => c.nSaldo).FirstAsync();

                var movimientos = await DB.CAP_MovimientosCuenta
                                            .Include(m => m.TipoMovimiento)
                                            .Include(m => m.CategoriaGasto)
                                            .Where(m => m.nIdCuenta == idCuenta && m.dFecha_Registro >= fechaIni && m.dFecha_Registro < fechaFin)
                                            .ToListAsync();

                resultado.SaldoInicial += await DB.CAP_MovimientosCuenta
                                                   .Include(m => m.TipoMovimiento)
                                                   .Where(m => m.nIdCuenta == idCuenta && m.dFecha_Registro < fechaIni)
                                                   .SumAsync(m => (decimal?)m.nImporte * (m.TipoMovimiento.bRestar ? -1 : 1)) ?? 0;

                resultado.Movimientos = (from m in movimientos
                                         orderby m.dFecha_Registro
                                        select new MovimientosCuentaModel()
                                        {
                                            nIdMovimiento = m.nIdMovimiento,
                                            Fecha = m.dFecha_Registro,
                                            nIdCategoria = m.nIdCategoria,
                                            cCategoria = m.CategoriaGasto.cCategoria,
                                            nIdTipoMovimiento = m.nIdTipoMovimiento,
                                            cTipoMovimiento = m.TipoMovimiento.cTipoMovimiento,
                                            IdReferencia = m.nIdCorte != null ? (int)m.nIdCorte : m.nIdPrestamo != null ? (int)m.nIdPrestamo : 0,
                                            nIdMovimientoCancela = m.nIdMovimientoCancela == null ? 0: (int)m.nIdMovimientoCancela,
                                            Abono = m.TipoMovimiento.bRestar ? 0 : m.nImporte,
                                            Cargo = m.TipoMovimiento.bRestar ? m.nImporte : 0,
                                            cObservaciones = m.cObservaciones
                                        }).ToList();

                decimal saldo = resultado.SaldoInicial;

                resultado.Movimientos.ForEach(m =>
                {
                    if (m.Abono > 0)
                    {
                        saldo += m.Abono;
                    } else
                    {
                        saldo -= m.Cargo;
                    }

                    m.Saldo = saldo;
                });

                return Ok(resultado);
            }
            catch
            {
                return BadRequest("Ocurrio un problema al obtener el saldo con movimeintos de la cuenta.");
            }
        }

        [HttpPost]
        [Route("~/api/Cuentas/RegistrarGastoIngreso")]
        public async Task<IHttpActionResult> POST_GuardarRegistroGastoIngreso(RegistroGastosIngresosModel datos)
        {
            try
            {
                if (datos == null)
                {
                    return BadRequest("Datos invalidos.");
                }

                if (datos.nIdCuenta <= 0)
                {
                    return BadRequest("Debe ingresar una cuenta.");
                }

                if (datos.nIdTipoMovimiento <= 0)
                {
                    return BadRequest("Debe ingresar un tipo de movimiento.");
                }

                if (datos.nIdCategoria < 0)
                {
                    return BadRequest("Debe ingresar una categoría.");
                }

                if (datos.nImporte <= 0)
                {
                    return BadRequest("El importe debe ser mayor a 0.");
                }

                if (!datos.Nuevo)
                {
                    return BadRequest("No se permiten modificaciones. Debe cancelar el movimiento y registrarlo de nuevo con los cambios requeridos.");
                }

                if (await DB.CAT_TipoMovimiento.AnyAsync(t => t.nIdTipoMovimiento == datos.nIdTipoMovimiento) == false)
                {
                    return BadRequest("El tipo de movimiento no existe.");
                }

                if (await DB.CAT_CategoriasGasto.AnyAsync(c => c.nIdCategoria == datos.nIdCategoria) == false)
                {
                    return BadRequest("La Categoría no es valida.");
                }

                if (await DB.CAT_Cuentas.AnyAsync(c => c.nIdCuenta == datos.nIdCuenta) == false)
                {
                    return BadRequest("La cuenta no es valida.");
                }

                int id = (await DB.CAP_MovimientosCuenta.MaxAsync(m => (int?)m.nIdMovimiento) ?? 0) + 1;

                CAP_MovimientosCuenta movimiento = new CAP_MovimientosCuenta()
                {
                    nIdMovimiento = id,
                    nIdCuenta = datos.nIdCuenta,
                    nIdTipoMovimiento = datos.nIdTipoMovimiento,
                    nIdCategoria = datos.nIdCategoria,
                    nImporte = datos.nImporte,
                    cObservaciones = datos.cObservaciones ?? "",
                    bActivo = true,
                    cUsuario_Registro = UsuarioLoguin,
                    dFecha_Registro = DateTime.Now
                };

                DB.CAP_MovimientosCuenta.Add(movimiento);

                await DB.SaveChangesAsync();

                await Utilerias.CrearCarpetaFTP(AppSettings.DirectorioRegistroGastoIngresos, ObtenerDirectorioLocal(id));

                return Ok(id);
            }
            catch
            {
                return BadRequest("Ocurrio un problema al guardar el registro.");
            }
        }

        [HttpPost]
        [Route("~/api/Cuentas/CancelarGastoIngreso")]
        public async Task<IHttpActionResult> POST_CancelarRegistroGastoIngreso(CancelacionMovimientoModel datos)
        {
            int idMovimiento = datos.IdMovimiento;

            try
            {
                if (idMovimiento <= 0)
                {
                    return BadRequest("Datos invalidos.");
                }

                if (await DB.CAP_MovimientosCuenta.AnyAsync(m => m.nIdMovimiento == idMovimiento) == false)
                {
                    return BadRequest("Movimiento invalido.");
                }

                if (await DB.CAP_MovimientosCuenta.AnyAsync(m => m.nIdMovimientoCancela == idMovimiento))
                {
                    return BadRequest("No se puede cancelar un movimeinto que ya fue cancelado.");
                }

                int id = (await DB.CAP_MovimientosCuenta.MaxAsync(m => (int?)m.nIdMovimiento) ?? 0) + 1;

                await DB.CAP_MovimientosCuenta
                    .Where(m => m.nIdMovimiento == idMovimiento)
                    .InsertFromQueryAsync(nm => new CAP_MovimientosCuenta()
                    {
                        nIdMovimiento = id,
                        nIdCuenta = nm.nIdCuenta,
                        nIdTipoMovimiento = nm.nIdTipoMovimiento == 5 ? 6: 8, // Si es ingreso, coloco el movimiento cancelacion ingreso, si es gasto, cancelacion gasto
                        nIdCategoria = nm.nIdCategoria,
                        nIdMovimientoCancela = nm.nIdMovimiento,
                        nImporte = nm.nImporte,
                        cObservaciones = datos.Observaciones,
                        bActivo = true,
                        cUsuario_Registro = UsuarioLoguin,
                        dFecha_Registro = DateTime.Now
                    });

                await Utilerias.CrearCarpetaFTP(AppSettings.DirectorioRegistroGastoIngresos, ObtenerDirectorioLocal(id));

                return Ok(id);
            }
            catch
            {
                return BadRequest("Ocurrio un problema al guardar el registro.");
            }
        }

        [HttpPost]
        [Route("~/api/Cuentas/GuardarArchivo")]
        public async Task<IHttpActionResult> POST_GuardarArchivo(int idMovimiento)
        {
            using (var transaccion = DB.Database.BeginTransaction())
            {
                try
                {
                    // Check if the request contains multipart/form-data.
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    var provider = await Request.Content.ReadAsMultipartAsync<InMemoryMultipartFormDataStreamProvider>(new InMemoryMultipartFormDataStreamProvider());
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
                    string directorioSolicitud = AppSettings.DirectorioRegistroGastoIngresos + "/" + ObtenerDirectorioLocal(idMovimiento);

                    file1.Dispose();

                    //return Ok();

                    bool archivoGuardado = await Utilerias.SubirArchivoFTP(archivo, directorioSolicitud, nombreArchivo);

                    //archivo.Close();
                    //archivo.Dispose();

                    if (archivoGuardado)
                    {
                        string URL = AppSettings.URLArchivos + "/" + directorioSolicitud + '/' + nombreArchivo;

                        await DB.CAP_MovimientosCuenta
                                .Where(d => d.nIdMovimiento == idMovimiento)
                                .UpdateFromQueryAsync(u => new CAP_MovimientosCuenta()
                                {
                                    cRutaDocumento = URL
                                });

                        transaccion.Commit();
                        return Ok(URL);
                    }
                    else
                    {
                        transaccion.Rollback();
                        return BadRequest("Ocurrio un problema al guardar el archivo.");
                    }
                }
                catch (IOException iex)
                {
                    transaccion.Rollback();
                    //return BadRequest("Ocurrio un error al guardar el archivo.");
                    return BadRequest(iex.Message);
                }
                catch(Exception ex)
                {
                    transaccion.Rollback();
                    //return BadRequest("Ocurrio un error al guardar el archivo.");
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("~/api/Cuentas/BorrarArchivo")]
        public async Task<IHttpActionResult> POST_BorrarArchivo(int idMovimiento, string nombreArchivo)
        {
            using(var transaccion = DB.Database.BeginTransaction())
            {
                try
                {
                    string directorioSolicitud = AppSettings.DirectorioRegistroGastoIngresos + "/" + ObtenerDirectorioLocal(idMovimiento);

                    if (!await DB.CAP_MovimientosCuenta.AnyAsync(m => m.nIdMovimiento == idMovimiento && m.cRutaDocumento.Contains(nombreArchivo)))
                    {
                        return BadRequest("No existe el archivo en el folio de registro seleccionado.");
                    }

                    await DB.CAP_MovimientosCuenta
                            .Where(m => m.nIdMovimiento == idMovimiento)
                            .UpdateFromQueryAsync(m => new CAP_MovimientosCuenta()
                            {
                                cRutaDocumento = null
                            });

                    bool borrado = await Utilerias.BorrarArchivoFTP(directorioSolicitud, nombreArchivo);

                    if (borrado)
                    {
                        transaccion.Commit();
                        return Ok();
                    } else
                    {
                        transaccion.Rollback();
                        return BadRequest("No se pudo borrar el archivo.");
                    }
                }
                catch (IOException iex)
                {
                    transaccion.Rollback();
                    return BadRequest(iex.Message);
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    return BadRequest(ex.Message);
                }
            }
        }

        private string ObtenerDirectorioLocal(int idMovimiento)
        {
            //return Path.Combine(this.ObtenerDirectorioLoca(), "Solicitudes\\" + idSolicitudPrestamo.ToString().PadLeft(6, '0'));
            return idMovimiento.ToString().PadLeft(6, '0');
        }
    }
}
