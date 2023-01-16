using Back_End.ModelsBD;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Back_End.Controllers
{
    [Authorize]
    public class CategoriasGastoController : ControladorBase
    {
        [HttpGet]
        [Route("~/api/CategoriasGasto/ObtenerCategorias")]
        public IQueryable<CAT_CategoriasGasto> GET_ObtenerCategorias()
        {
            return DB.CAT_CategoriasGasto;
        }

        [HttpPost]
        [Route("~/api/CategoriasGasto/GuardarCategoria")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> POST_Guardar(CAT_CategoriasGasto categoria)
        {
            if (categoria == null)
            {
                return BadRequest("Datos invalidos.");
            }

            if (categoria.cCategoria == null || categoria.cCategoria.Trim().Length == 0)
            {
                return BadRequest("Falta ingresar un nombre a la categoría.");
            }

            try
            {
                if (categoria.nIdCategoria == 0)
                {
                    int id = DB.CAT_CategoriasGasto.Max(c => c.nIdCategoria) + 1;

                    categoria.nIdCategoria = id;
                    categoria.bActivo = true;
                    categoria.dFecha_Registro = DateTime.Now;
                    categoria.cUsuario_Registro = UsuarioLoguin;
                    categoria.dFecha_UltimaModificacion = DateTime.Now;
                    categoria.cUsuario_UltimaModificacion = UsuarioLoguin;

                    DB.CAT_CategoriasGasto.Add(categoria);

                    await DB.SaveChangesAsync();
                } else
                {
                    await DB.CAT_CategoriasGasto
                            .Where(c => c.nIdCategoria == categoria.nIdCategoria)
                            .UpdateFromQueryAsync(u => new CAT_CategoriasGasto()
                            {
                                cCategoria = categoria.cCategoria,
                                bActivo = categoria.bActivo,
                                dFecha_UltimaModificacion = DateTime.Now,
                                cUsuario_UltimaModificacion = UsuarioLoguin,
                                cUsuario_Eliminacion = categoria.bActivo ? null : UsuarioLoguin,
                                dFecha_Eliminacion = categoria.bActivo ? (DateTime?)null : DateTime.Now
                            });
                }

                return Ok(categoria.nIdCategoria);
            }
            catch
            {
                return BadRequest("Ocurrio un error al guardar los cambios. Intente de nuevo en unos minutos.");
            }
        }
    }
}
