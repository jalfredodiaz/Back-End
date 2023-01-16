using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Back_End.ModelsBD;

namespace Back_End.Controllers
{
    public class TipoPrestamoController : ControladorBase
    {
        [HttpGet]
        [Route("~/api/TipoPrestamo/ObtenerTipoPrestamo")]
        public IQueryable<CAT_RubrosPrestamos> GET_ObtenerTipoPrestamos()
        {
            return DB.CAT_RubrosPrestamos;
        }

        [HttpPost]
        [Route("~/api/TipoPrestamo/GuardarTipoPrestamo")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> POST_Guardar(CAT_RubrosPrestamos model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.cRubro == null || model.cRubro.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre para el tipo de prestamo.");
            }

            // Corte Mensual
            if (model.bMensual)
            {
                if (model.nDiaCorte <= 0)
                {
                    return BadRequest("Debe seleccionar un día de corte.");
                }

                if (model.nDiaCorte > 28)
                {
                    return BadRequest("El día de corte no debe ser mayor a 28 días.");
                }
            }

            // Corte Anual
            if (!model.bMensual)
            {
                if (model.nMesCorte <= 0)
                {
                    return BadRequest("Debe ingresar un mes.");
                }

                if (model.nMesCorte > 12)
                {
                    return BadRequest("Debe ingresar un mes valido.");
                }
            }

            try
            {
                if (DB.CAT_RubrosPrestamos.Any(p => p.cRubro.ToUpper() == model.cRubro.ToUpper()))
                {
                    return BadRequest("Ya existe un tipo de prestamo con el mismo nombre.");
                }

                model.bActivo = true;
                model.cUsuario_Registro = UsuarioLoguin;
                model.cUsuario_UltimaModificacion = UsuarioLoguin;
                model.dFecha_Registro = DateTime.Now;
                model.dFecha_UltimaModificacion = DateTime.Now;

                DB.CAT_RubrosPrestamos.Add(model);

                await DB.SaveChangesAsync();

                return Ok(model.nIdRubro);
            }
            catch (Exception)
            {
                return BadRequest("Ocurrio un problema al guardar el tipo de prestamo. Por favor intentelo unos minutos mas tarde.");
            }
        }

        [HttpPost]
        [Route("~/api/TipoPrestamo/ModificarTipoPrestamo")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> POST_Modificar(CAT_RubrosPrestamos model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.cRubro == null || model.cRubro.Trim().Length == 0)
            {
                return BadRequest("Debe ingresar un nombre para el tipo de prestamo.");
            }

            // Corte Mensual
            if (model.bMensual)
            {
                if (model.nDiaCorte <= 0)
                {
                    return BadRequest("Debe seleccionar un día de corte.");
                }

                if (model.nDiaCorte > 28)
                {
                    return BadRequest("El día de corte no debe ser mayor a 28 días.");
                }

                model.nMesCorte = 0;
            }

            // Corte Anual
            if (!model.bMensual)
            {
                if (model.nMesCorte <= 0)
                {
                    return BadRequest("Debe ingresar un mes.");
                }

                if (model.nMesCorte > 12)
                {
                    return BadRequest("Debe ingresar un mes valido.");
                }
            }

            try
            {
                if (DB.CAT_RubrosPrestamos.Any(p => p.nIdRubro != model.nIdRubro && p.cRubro.ToUpper() == model.cRubro.ToUpper()))
                {
                    return BadRequest("Ya existe un tipo de prestamo con el mismo nombre;");
                }

                await DB.CAT_RubrosPrestamos
                    .Where(d => d.nIdRubro == model.nIdRubro)
                    .UpdateFromQueryAsync(u => new CAT_RubrosPrestamos()
                    {
                        cRubro = model.cRubro,
                        bMensual = model.bMensual,
                        nMesCorte = model.nMesCorte,
                        nDiaCorte = model.nDiaCorte,
                        bActivo = model.bActivo,
                        cUsuario_UltimaModificacion = UsuarioLoguin,
                        dFecha_UltimaModificacion = DateTime.Now,
                        cUsuario_Eliminacion = model.bActivo == true ? null : UsuarioLoguin,
                        dFecha_Eliminacion = model.bActivo == true ? (DateTime?)null : DateTime.Now
                    });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Ocurrio un problema al guardar el tipo de prestamo. Por favor intentelo unos minutos mas tarde.");
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
