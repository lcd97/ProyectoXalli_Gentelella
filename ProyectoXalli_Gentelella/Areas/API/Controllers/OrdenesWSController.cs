using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MenuAPI.Areas.API.Models;
using ProyectoXalli_Gentelella.Models;

namespace MenuAPI.Areas.API.Controllers
{
    public class OrdenesWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();


       //Obtener las ordenes con estado atendido y sin facturar con fecha igual al dia de hoy
       [HttpGet]
        public JsonResult Ordenes()
        {
            var ordenes = (from o in db.Ordenes.Where(x => x.EstadoOrden == 1 || x.EstadoOrden == 2)
                           join m in db.Meseros on o.MeseroId equals m.Id
                           join c in db.Clientes on o.ClienteId equals c.Id
                           join d in db.Datos on c.DatoId equals d.Id
                           //where o.FechaOrden == DateTime.Today
                           orderby o.FechaOrden descending
                           select new OrdenWS
                           {
                               id = o.Id,
                               codigo = o.CodigoOrden,
                               fechaorden = o.FechaOrden,
                               estado = o.EstadoOrden,
                               meseroid = m.Id,
                               clienteid = c.Id,
                               cliente = c.Dato.PNombre +" "+ c.Dato.PApellido,
                               mesero = m.Dato.PNombre + " " + m.Dato.PApellido

                           }).ToList();

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        //buscar el ultimo codigo de orden
        [HttpGet]
        public async Task<JsonResult> UltimoCodigo()
        {
            var num = await (from obj in db.Ordenes
                       select obj.CodigoOrden).DefaultIfEmpty().MaxAsync();

            int codigo = 1;

            if (num != 0)
            {
                codigo = num + 1;
            }

            return Json(codigo, JsonRequestBehavior.AllowGet);
        }

        //cerrando la db
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