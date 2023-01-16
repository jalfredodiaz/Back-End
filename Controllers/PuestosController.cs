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

namespace Back_End.Controllers
{
    [Authorize]
    public class PuestosController : ControladorBase
    {
        [HttpGet]
        [Route("~/api/Puestos/ObtenerPuestos")]
        public IHttpActionResult GET_ObtenerPuestos()
        {
            var res = from p in DB.CAT_Puestos
                      join p2 in DB.CAT_Puestos on p.nIdPuestoPadre equals p2.nIdPuesto into lpadre
                      from padre in lpadre.DefaultIfEmpty()
                      join d in DB.CAT_Departamentos on p.nIdDepartamento equals d.nIdDepartamento
                      select new
                      {
                          p.nIdPuesto,
                          p.nIdPuestoPadre,
                          p.cPuesto,
                          cPuestoPadre = padre.cPuesto ?? "",
                          p.nIdDepartamento,
                          d.cDepartamento,
                          p.nSueldo,
                          p.bActivo
                      };

            return Ok(res);
        }

        [HttpGet]
        [Route("~/api/Puestos/ObtenerPuestosPorNombre")]
        public IQueryable<CAT_Puestos> GET_ObtenerPuestosPorNombre(string filtro, bool activos, bool inactivos)
        {
            return DB.CAT_Puestos.Where(p => p.cPuesto.Contains(filtro) && ((p.bActivo == true && activos == true) || (p.bActivo == false && inactivos == true)));
        }

        [HttpGet]
        [Route("~/api/Puestos/ObtenerPuesto")]
        public IQueryable<CAT_Puestos> GET_ObtenerPuesto(int codigo)
        {
            return DB.CAT_Puestos.Where(p => p.nIdPuesto == codigo);
        }

        [HttpPost]
        [Route("~/api/Puestos/GuardarPuesto")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> POST_Guardar(CAT_Puestos puesto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (puesto.cPuesto == null || puesto.cPuesto.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre para el puesto.");
            }

            if (puesto.nIdPuestoPadre.GetValueOrDefault() == 0)
            {
                puesto.nIdPuestoPadre = null;
            }

            if (puesto.nIdPuesto == puesto.nIdPuestoPadre)
            {
                return BadRequest("No esta permitido seleccionarse asi mismo como puesto padre.");
            }

            try
            {
                if (DB.CAT_Puestos.Any(p => p.cPuesto.ToUpper() == puesto.cPuesto.ToUpper()))
                {
                    return BadRequest("Ya existe un puesto con el mismo nombre.");
                }

                puesto.bActivo = true;
                puesto.cUsuario_Registro = UsuarioLoguin;
                puesto.cUsuario_UltimaModificacion = UsuarioLoguin;
                puesto.dFecha_Registro = DateTime.Now;
                puesto.dFecha_UltimaModificacion = DateTime.Now;

                DB.CAT_Puestos.Add(puesto);

                await DB.SaveChangesAsync();

                return Ok(puesto.nIdPuesto);
            }
            catch (Exception)
            {
                return BadRequest("Ocurrio un problema al guardar el puesto. Por favor intentelo unos minutos mas tarde.");
            }
        }

        [HttpPost]
        [Route("~/api/Puestos/ModificarPuesto")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Modificar(int codigo, string nombre, bool activo, int codigoPadre, int codigoDepartamento, decimal sueldo)
        {
            int? idPuestoPadre = null;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (nombre == null || nombre.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre para el puesto.");
            }

            if (codigo == codigoPadre)
            {
                return BadRequest("No esta permitido seleccionarse asi mismo como puesto padre.");
            }

            try
            {
                if (codigoPadre > 0)
                {
                    if (DB.CAT_Puestos.Any(p => p.nIdPuesto == codigoPadre))
                    {
                        idPuestoPadre = codigoPadre;
                    }
                    else
                    {
                        return BadRequest("El puesto padre relacionado no existe.");
                    }
                }


                if (!DB.CAT_Departamentos.Any(d => d.nIdDepartamento == codigoDepartamento))
                {
                    return BadRequest("El departamento relacionado no existe.");
                }

                if (DB.CAT_Puestos.Any(p => p.nIdPuesto != codigo && p.cPuesto.ToUpper() == nombre.ToUpper()))
                {
                    return BadRequest("Ya existe un puesto con el mismo nombre;");
                }

                await DB.CAT_Puestos
                    .Where(d => d.nIdPuesto == codigo)
                    .UpdateFromQueryAsync(u => new CAT_Puestos()
                    {
                        cPuesto = nombre,
                        nIdPuestoPadre = idPuestoPadre,
                        nIdDepartamento = codigoDepartamento,
                        nSueldo = sueldo,
                        bActivo = activo,
                        cUsuario_UltimaModificacion = UsuarioLoguin,
                        dFecha_UltimaModificacion = DateTime.Now,
                        cUsuario_Eliminacion = activo == true ? null : UsuarioLoguin,
                        dFecha_Eliminacion = activo == true ? (DateTime?)null : DateTime.Now
                    });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Ocurrio un problema al guardar el puesto. Por favor intentelo unos minutos mas tarde.");
            }
        }

        [HttpPost]
        [Route("~/api/Puestos/AumentarSueldo")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_AumentarSueldo(decimal porcentaje)
        {
            if (porcentaje == 0)
            {
                return BadRequest("Debe ingresar un porcentaje diferente a 0.");
            }

            try
            {
                decimal porcentajeAplicado = (porcentaje / 100) + 1;

                await DB.CAT_Puestos
                        .UpdateFromQueryAsync(p => new CAT_Puestos()
                        {
                            nSueldo = p.nSueldo * porcentajeAplicado,
                            cUsuario_UltimaModificacion = UsuarioLoguin,
                            dFecha_UltimaModificacion = DateTime.Now
                        });

                return Ok();
            }
            catch
            {
                return BadRequest("Ocurrio un error al modificar los sueldos de los puestos.");
            }
        }

        [HttpPost]
        [Route("~/api/Puestos/AumentarSueldoPorDepartamento")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_AumentarSueldoPorDepartamento(int idDepartamento, decimal porcentaje)
        {
            if (porcentaje == 0)
            {
                return BadRequest("Debe ingresar un porcentaje diferente a 0.");
            }

            if (idDepartamento <= 0)
            {
                return BadRequest("Debe ingresar un departamento valido.");
            }

            try
            {
                if (!await DB.CAT_Departamentos.AnyAsync(d => d.nIdDepartamento == idDepartamento)) {
                    return BadRequest("El departamento no existe, seleccione un departamento valido.");
                }

                decimal porcentajeAplicado = (porcentaje / 100) + 1;

                await DB.CAT_Puestos
                        .Where(p => p.nIdDepartamento == idDepartamento)
                        .UpdateFromQueryAsync(p => new CAT_Puestos()
                        {
                            nSueldo = p.nSueldo * porcentajeAplicado,
                            cUsuario_UltimaModificacion = UsuarioLoguin,
                            dFecha_UltimaModificacion = DateTime.Now
                        });

                return Ok();
            }
            catch
            {
                return BadRequest("Ocurrio un error al modificar los sueldos de los puestos.");
            }
        }
    }
}
