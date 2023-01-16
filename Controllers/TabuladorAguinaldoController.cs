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
    public class TabuladorAguinaldoController : ControladorBase
    {
        // GET: api/TabuladorAguinaldo
        [HttpGet]
        [Route("~/api/TabuladorAguinaldo/ObtenerTabuladorAguinaldo")]
        public IQueryable<CAT_TabuladorAguinaldo> GetCAT_TabuladorAguinaldo()
        {
            return DB.CAT_TabuladorAguinaldo;
        }

        // GET: api/TabuladorAguinaldo/5
        [HttpGet]
        [Route("~/api/TabuladorAguinaldo/ObtenerTabuladorAguinaldoPorId")]
        [ResponseType(typeof(CAT_TabuladorAguinaldo))]
        public async Task<IHttpActionResult> GetCAT_TabuladorAguinaldo(int id)
        {
            CAT_TabuladorAguinaldo cAT_TabuladorAguinaldo = await DB.CAT_TabuladorAguinaldo.FindAsync(id);
            if (cAT_TabuladorAguinaldo == null)
            {
                return NotFound();
            }

            return Ok(cAT_TabuladorAguinaldo);
        }
    }
}