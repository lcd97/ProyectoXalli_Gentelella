using ProyectoXalli_Gentelella.Areas.API.Models;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Areas.API.Controllers
{

    [BasicAuthentication]
    public class ReportesWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

        //obteniendo las categorias del menu
        [HttpGet]
        public async Task<JsonResult> ProductosMasVendidos(DateTime fecha)
        {
            var productos = await (from d in db.DetallesDeOrden
                                   join m in db.Menus on d.MenuId equals m.Id
                                   join o in db.Ordenes on d.OrdenId equals o.Id
                                   join to in db.TiposDeOrden on o.TipoOrdenId equals to.Id
                                   where o.FechaOrden <= fecha
                                   group d by new { m.DescripcionMenu} into g
                                   orderby g.Count() descending
                                   select new ReporteMasVendidosWS
                                   {
                                       nombre = g.Key.DescripcionMenu,
                                       cantidad = g.Count()
                                    }).Take(5).ToListAsync();

            return Json(productos, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> VentasMensuales(DateTime fecha)
        {
            var ventas = await (from d in db.DetallesDeOrden
                                join o in db.Ordenes on d.OrdenId equals o.Id
                                join to in db.TiposDeOrden on o.TipoOrdenId equals to.Id
                                where o.FechaOrden.Year == fecha.Year && to.CodigoTipoOrden == "V01"
                                group d by new { o.FechaOrden.Month } into g
                                orderby g.Key.Month descending
                                select new ResportesVentasMes
                                {
                                       mes = g.Key.Month,
                                       totalVentas = g.Sum(x=> x.CantidadOrden * x.PrecioOrden)

                                }).ToListAsync();

            return Json(ventas, JsonRequestBehavior.AllowGet);
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