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
    public class TabuladorPrimaVacacionalController : ControladorBase
    {
        // GET: api/abuladorPrimaVacacional
        [HttpGet]
        [Route("~/api/TabuladorPrimaVacacional/ObtenerTabuladorVacaciones")]
        public IQueryable<CAT_TabuladorPrimaVacacional> GetCAT_TabuladorPrimaVacacional()
        {
            return DB.CAT_TabuladorPrimaVacacional;
        }

        // GET: api/abuladorPrimaVacacional/5
        [HttpGet]
        [Route("~/api/TabuladorPrimaVacacional/ObtenerTabuladorVacacionesPorId")]
        [ResponseType(typeof(CAT_TabuladorPrimaVacacional))]
        public async Task<IHttpActionResult> GetCAT_TabuladorPrimaVacacional(int id)
        {
            CAT_TabuladorPrimaVacacional cAT_TabuladorPrimaVacacional = await DB.CAT_TabuladorPrimaVacacional.FindAsync(id);
            if (cAT_TabuladorPrimaVacacional == null)
            {
                return NotFound();
            }

            return Ok(cAT_TabuladorPrimaVacacional);
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