using ProyectoXalli_Gentelella.Areas.API.Models;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Areas.API.Controllers
{
    [BasicAuthentication]
    public class MesasWSController : Controller
    {
        private DBControl db = new DBControl();

        /// <summary>
        /// GENERA UNA LISTA DE LAS MESAS DISPONIBLES
        /// </summary>
        /// <returns></returns>
        //Obtener los detalles de orden de una orden dada
        [HttpGet]
        public async Task<JsonResult> Mesas()
        {
            var mesas = (from m in db.Mesas.ToList()
                         where m.EstadoMesa == true && !(from ord in db.Ordenes.ToList()
                                                         where ord.EstadoOrden == 1
                                                         select ord.MesaId).Contains(m.Id)
                         select new MesaWS
                         {
                             id = m.Id,
                             codigo = m.CodigoMesa,
                             descripcion = m.DescripcionMesa,
                             estado = m.EstadoMesa

                         }).ToList();

            return Json(mesas, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}