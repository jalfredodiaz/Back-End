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
using System.Net.Http.Headers;
using Back_End.Models;
using Back_End.ModelsBD;
using Back_End.Clases.Enumeradores;

namespace Back_End.Controllers
{
    public class EmpleadosController : ControladorBase
    {
        [HttpGet]
        [Route("~/api/Empleados/ObtenerEmpleados")]
        public IHttpActionResult GET_ObtenerEmpleados()
        {
            var res = from emp in DB.CAT_Empleados
                      join p in DB.CAT_Puestos on emp.nIdPuesto equals p.nIdPuesto
                      join d in DB.CAT_Departamentos on p.nIdDepartamento equals d.nIdDepartamento
                      where emp.bActivo == true
                      select new
                      {
                          emp.nCodEmpleado,
                          emp.cNombre,
                          p.nIdPuesto,
                          emp.dFechaIngreso,
                          emp.bActivo,
                          p.cPuesto,
                          d.cDepartamento,
                          d.nIdDepartamento,
                          p.nSueldo,
                          emp.nEstatusCrecimiento,
                          emp.cObservaciones
                      };

            return Ok(res);
        }

        [HttpGet]
        [Route("~/api/Empleados/ObtenerArchivosEmpleado")]
        public async Task<IHttpActionResult> GET_ObtenerArchivosEmpleado(int codigoEmpleado)
        {
            try
            {
                var res = await DB.CAT_EmpleadosArchivos
                                .Where(e => e.nCodEmpleado == codigoEmpleado && e.bActivo == true)
                                .Select(e => new EmpleadoArchivosModel()
                                {
                                    Id = e.nIdArchivoEmpleado,
                                    Descripcion = e.cNombre,
                                    Ruta = e.cRutaArchivo
                                }).ToArrayAsync();

                return Ok(res);
            }
            catch
            {
                return BadRequest("Ocurrio un error al consultar los archivos del empleado.");
            }
        }

        [HttpGet]
        [Route("~/api/Empleados/ObtenerEmpleadosSustitutos")]
        public async Task<IHttpActionResult> GET_ObtenerEmpleadosSustitutos(int codigoEmpleado)
        {
            try
            {
                var empleado = await DB.CAT_Empleados
                                .Where(e => e.nCodEmpleado == codigoEmpleado)
                                .Select(e => new {
                                    idPuesto = e.nIdPuesto,
                                    idDepartamento = e.Puesto.nIdDepartamento
                                }).FirstOrDefaultAsync();

                /* ESTATUS NUEVO PUESTO
                 * APTO = 1
                 * NO APTO = 2
                 * CONGELADO = 3
                 * 
                 * SOLO DEBEN MOSTRASE EMPLEADOS APTOS
                 */
                var res = await DB.CAT_Empleados
                                    .Where(e => e.Puesto.nIdPuestoPadre == empleado.idPuesto && e.Puesto.nIdDepartamento == empleado.idDepartamento && e.nEstatusCrecimiento == 1)
                                    .Select(e => new EscalonadoModel()
                                    {
                                        CodigoEmpleado = e.nCodEmpleado,
                                        NombreEmpleado = e.cNombre,
                                        CodigoPuesto = e.nIdPuesto,
                                        NombrePuesto = e.Puesto.cPuesto,
                                        CodigoDepartamento = e.Puesto.nIdDepartamento,
                                        NombreDepartamento = e.Puesto.Departamento.cDepartamento,
                                        FechaIngreso = e.dFechaIngreso
                                    }).OrderBy(e => e.FechaIngreso).ToArrayAsync();

                return Ok(res);
            }
            catch
            {
                return BadRequest("Ocurrio un error al consultar los empleados.");
            }
        }


        [HttpPost]
        [Route("~/api/Empleados/GuardarEmpleado")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> POST_Guardar(CAT_Empleados empleado)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (empleado.nCodEmpleado == 0)
            {
                return BadRequest("Debe ingresar el codigo de empleado.");
            }

            if (empleado.cNombre == null || empleado.cNombre.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre para el puesto.");
            }

            if (empleado.nIdPuesto == 0)
            {
                return BadRequest("Debe ingresar un puesto.");
            }

            if (empleado.dFechaIngreso == new DateTime(1900, 01, 01))
            {
                return BadRequest("Debe ingresar una fecha valida.");
            }

            if (empleado.dFechaIngreso > DateTime.Today)
            {
                return BadRequest("La fecha de ingreso no puede ser mayor a la actual.");
            }

            if ( empleado.nEstatusCrecimiento < 1 || empleado.nEstatusCrecimiento > 3)
            {
                return BadRequest("El estatus para un nuevo puesto no es valido, favor de seleccionar uno.");
            }

            try
            {
                if (!DB.CAT_Puestos.Any(p => p.nIdPuesto == empleado.nIdPuesto))
                {
                    return BadRequest("El puesto no es valido.");
                }

                //empleado.nCodEmpleado = DB.CAT_Empleados.Max(e => e.nCodEmpleado) + 1;
                if (await DB.CAT_Empleados.AnyAsync(e => e.nCodEmpleado == empleado.nCodEmpleado))
                {
                    return BadRequest("Ya existe un empleado con el mismo codigo.");
                }

                empleado.bActivo = true;
                empleado.cUsuario_Registro = UsuarioLoguin;
                empleado.cUsuario_UltimaModificacion = UsuarioLoguin;
                empleado.dFecha_Registro = DateTime.Now;
                empleado.dFecha_UltimaModificacion = DateTime.Now;

                DB.CAT_Empleados.Add(empleado);

                await DB.SaveChangesAsync();

                return Ok(empleado.nCodEmpleado);
            }
            catch (Exception)
            {
                return BadRequest("Ocurrio un problema al guardar el empleado. Por favor intentelo dentro de unos minutos.");
            }
        }

        [HttpPost]
        [Route("~/api/Empleados/ModificarEmpleado")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Modificar(CAT_Empleados empleado)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (empleado.nCodEmpleado == 0)
            {
                return BadRequest("Empleado invalido.");
            }

            if (empleado.cNombre == null || empleado.cNombre.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre para el puesto.");
            }

            if (empleado.nIdPuesto == 0)
            {
                return BadRequest("Debe ingresar un puesto.");
            }

            if (empleado.dFechaIngreso == new DateTime(1900, 01, 01))
            {
                return BadRequest("Debe ingresar una fecha valida.");
            }

            if (empleado.dFechaIngreso > DateTime.Today)
            {
                return BadRequest("La fecha de ingreso no puede ser mayor a la actual.");
            }

            if (empleado.nEstatusCrecimiento < 1 || empleado.nEstatusCrecimiento > 3)
            {
                return BadRequest("El estatus para un nuevo puesto no es valido, favor de seleccionar uno.");
            }

            try
            {
                if (!DB.CAT_Puestos.Any(p => p.nIdPuesto == empleado.nIdPuesto))
                {
                    return BadRequest("El puesto no es valido.");
                }

                await DB.CAT_Empleados
                    .Where(d => d.nCodEmpleado == empleado.nCodEmpleado)
                    .UpdateFromQueryAsync(u => new CAT_Empleados()
                    {
                        cNombre = empleado.cNombre,
                        nIdPuesto = empleado.nIdPuesto,
                        dFechaIngreso = empleado.dFechaIngreso,
                        nEstatusCrecimiento = empleado.nEstatusCrecimiento,
                        cObservaciones = empleado.cObservaciones,
                        bActivo = empleado.bActivo,
                        cUsuario_UltimaModificacion = UsuarioLoguin,
                        dFecha_UltimaModificacion = DateTime.Now,
                        cUsuario_Eliminacion = empleado.bActivo ? null : UsuarioLoguin,
                        dFecha_Eliminacion = empleado.bActivo ? (DateTime?)null : DateTime.Now
                    });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Ocurrio un problema al guardar el puesto. Por favor intentelo dentro de unos minutos.");
            }
        }

        [HttpPost]
        [Route("~/api/Empleados/GuardarArchivo")]
        public async Task<IHttpActionResult> POST_GuardarArchivo(int codigoEmpleado, string descripcion)
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
                byte[] archivo = await file1.ReadAsByteArrayAsync();
                string directorioEmpleado = AppSettings.DirectorioSolicitudEmpleados + "/" + ObtenerDirectorioLocalEmpleado(codigoEmpleado);

                file1.Dispose();

                bool existeDirectorio = await Utilerias.ExisteDirectorioFTP(directorioEmpleado);

                if (!existeDirectorio)
                {
                    bool carpetaCreada = await Utilerias.CrearCarpetaFTP(AppSettings.DirectorioSolicitudEmpleados, ObtenerDirectorioLocalEmpleado(codigoEmpleado));

                    if (!carpetaCreada)
                    {
                        return BadRequest("No se pudo crear el directorio para el empleado.");
                    }
                }

                bool archivoGuardado = await Utilerias.SubirArchivoFTP(archivo, directorioEmpleado, nombreArchivo);

                if (archivoGuardado)
                {
                    string URL = AppSettings.URLArchivos + "/" + directorioEmpleado + "/" + nombreArchivo;
                    CAT_EmpleadosArchivos nuevoArchivo = new CAT_EmpleadosArchivos()
                    {
                        nCodEmpleado = codigoEmpleado,
                        bActivo = true,
                        cNombre = descripcion,
                        cRutaArchivo = URL,
                        cUsuario_Registro = UsuarioLoguin,
                        dFecha_Registro = DateTime.Now
                    };

                    DB.CAT_EmpleadosArchivos.Add(nuevoArchivo);

                    await DB.SaveChangesAsync();

                    return Ok(new EmpleadoArchivosModel { Id = nuevoArchivo.nIdArchivoEmpleado, Descripcion = descripcion, Ruta = URL});
                }
                else
                {
                    return BadRequest("Ocurrio un problema al guardar el archivo.");
                }
            }
            catch
            {
                return BadRequest("Ocurrio un error al guardar el archivo.");
            }
        }

        [HttpPost]
        [Route("~/api/Empleados/BorrarArchivo")]
        public async Task<IHttpActionResult> POST_BorrarArchivo(int codigoEmpleado, int idArchivo)
        {
            DbContextTransaction transaction = null;
            string nombreArchivo;

            try
            {
                string directorioEmpleado = AppSettings.DirectorioSolicitudEmpleados + "/" + ObtenerDirectorioLocalEmpleado(codigoEmpleado);
                CAT_EmpleadosArchivos archivo = await DB.CAT_EmpleadosArchivos
                                .FirstOrDefaultAsync(m => m.nIdArchivoEmpleado == idArchivo);

                if (archivo == null)
                {
                    return BadRequest("Nombre del archivo invalido.");
                }

                if (archivo.nCodEmpleado != codigoEmpleado)
                {
                    return BadRequest("El archivo no pertenece al empleado.");
                }

                if (archivo.bActivo == false)
                {
                    return BadRequest("El archivo ya fue eliminado por otra persona.");
                }

                nombreArchivo = archivo.cRutaArchivo;
                nombreArchivo = nombreArchivo.Substring(nombreArchivo.LastIndexOf('/') + 1);

                transaction = DB.Database.BeginTransaction();

                archivo.bActivo = false;
                archivo.cUsuario_Eliminacion = UsuarioLoguin;
                archivo.dFecha_Eliminacion = DateTime.Now;

                await DB.SaveChangesAsync();

                bool borrado = await Utilerias.BorrarArchivoFTP(directorioEmpleado, nombreArchivo);

                if (borrado)
                {
                    transaction.Commit();
                    return Ok();
                }
                else
                {
                    transaction.Rollback();
                    return BadRequest("No se pudo borrar el archivo.");
                }
            }
            catch
            {
                if (transaction != null) {
                    transaction.Rollback();
                }

                return BadRequest("Ocurrio un error al borrar el archivo.");
            }
        }

        private string ObtenerDirectorioLocalEmpleado(int codigoEmpleado)
        {
            return codigoEmpleado.ToString().PadLeft(8, '0');
        }
    }
}
