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
    public class DepartamentosController : ControladorBase
    {
        // GET: api/Departamentos
        [HttpGet]
        [Route("~/api/Departamentos/ObtenerDepartamentos")]
        public IQueryable<CAT_Departamentos> GetCAT_Departamentos()
        {
            return DB.CAT_Departamentos;
        }

        // GET: api/Departamentos/5
        [HttpGet]
        [Route("~/api/Departamentos/ObtenerDepartamento")]
        [ResponseType(typeof(CAT_Departamentos))]
        public async Task<IHttpActionResult> GetCAT_Departamentos(int id)
        {
            CAT_Departamentos cAT_Departamentos = await DB.CAT_Departamentos.FindAsync(id);
            if (cAT_Departamentos == null)
            {
                return NotFound();
            }

            return Ok(cAT_Departamentos);
        }

        // POST: api/Departamentos
        [HttpPost]
        [Route("~/api/Departamentos/GuardarDepartamento")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> PostCAT_Departamentos(CAT_Departamentos departamento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (departamento.cDepartamento == null || departamento.cDepartamento.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre para el departamento.");
            }

            try
            {
                if (departamento.nIdDepartamento == 0)
                {
                    departamento.bActivo = true;
                    departamento.cUsuario_Registro = UsuarioLoguin;
                    departamento.cUsuario_UltimaModificacion = UsuarioLoguin;
                    departamento.dFecha_Registro = DateTime.Now;
                    departamento.dFecha_UltimaModificacion = DateTime.Now;

                    DB.CAT_Departamentos.Add(departamento);
                    
                    await DB.SaveChangesAsync();
                } else
                {
                    await DB.CAT_Departamentos
                            .Where(d => d.nIdDepartamento == departamento.nIdDepartamento)
                            .UpdateFromQueryAsync(u => new CAT_Departamentos()
                            {
                                cDepartamento = departamento.cDepartamento,
                                bActivo = departamento.bActivo,
                                cUsuario_UltimaModificacion = UsuarioLoguin,
                                dFecha_UltimaModificacion = DateTime.Now,
                                cUsuario_Eliminacion = departamento.bActivo ? null : UsuarioLoguin,
                                dFecha_Eliminacion = departamento.bActivo ? (DateTime?)null : DateTime.Now,
                            });

                    //if (departamento.bActivo == false)
                    //{
                    //    await DB.CAT_Departamentos
                    //        .Where(d => d.nIdDepartamento == departamento.nIdDepartamento)
                    //        .UpdateFromQueryAsync(u => new CAT_Departamentos()
                    //        {
                    //            cDepartamento = departamento.cDepartamento,
                    //            bActivo = departamento.bActivo,
                    //            dFecha_UltimaModificacion = DateTime.Now,
                    //            cUsuario_UltimaModificacion = UsuarioLoguin,
                    //            cUsuario_Eliminacion = UsuarioLoguin,
                    //            dFecha_Eliminacion = DateTime.Now
                    //        });
                    //}
                    //else
                    //{
                    //    await DB.CAT_Departamentos
                    //        .Where(d => d.nIdDepartamento == departamento.nIdDepartamento)
                    //        .UpdateFromQueryAsync(u => new CAT_Departamentos()
                    //        {
                    //            cDepartamento = departamento.cDepartamento,
                    //            bActivo = departamento.bActivo,
                    //            cUsuario_UltimaModificacion = UsuarioLoguin,
                    //            dFecha_UltimaModificacion = DateTime.Now,
                    //            cUsuario_Eliminacion = null,
                    //            dFecha_Eliminacion = null
                    //        });
                    //}
                }

                return Ok(departamento.nIdDepartamento);
            }
            catch (DbUpdateException)
            {
                return BadRequest("Ocurrio un problema al guardar el departamento. Por favor intentelo unos minutos mas tarde.");
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
    }
}