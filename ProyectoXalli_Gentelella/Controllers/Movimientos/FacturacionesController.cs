using Newtonsoft.Json;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Movimientos {
    public class FacturacionesController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        // GET: Facturaciones
        public ActionResult Index() {
            return View();
        }

        public ActionResult CargarOrdenes(int ClienteId) {

            int cliente = 0;
            if (ClienteId == 0) {
                cliente = db.Clientes.Where(c => c.EmailCliente.Trim() == "defaultuser@xalli.com").Select(c => c.Id).FirstOrDefault();
            } else
                cliente = ClienteId;

            var orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                         where obj.EstadoOrden == 2 && c.Id == cliente//TODAS LAS ORDENES SIN FACTURAR E INACTIVAS
                         group new { obj, d, det } by new { obj.Id, obj.CodigoOrden, obj.FechaOrden, d.PNombre, d.PApellido } into grouped
                         select new {
                             OrdenId = grouped.Key.Id,
                             CodigoOrden = grouped.Key.CodigoOrden,
                             FechaOrden = grouped.Key.FechaOrden.ToShortDateString(),
                             Cliente = grouped.Key.PNombre.ToUpper() != "DEFAULT" ? grouped.Key.PNombre + " " + grouped.Key.PApellido : "N/A",
                             Mesero = (from o in db.Ordenes
                                       join m in db.Meseros on o.MeseroId equals m.Id
                                       join a in db.Datos on m.DatoId equals a.Id
                                       where grouped.Key.Id == o.Id
                                       select a.PNombre + " " + a.PApellido).FirstOrDefault(),
                             SubTotal = grouped.Sum(s => s.det.CantidadOrden * s.det.PrecioOrden)
                         }).ToList();

            return Json(new { orden, ClienteId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CargarSeleccionadas(string OrdenesId) {
            var ordenesId = JsonConvert.DeserializeObject<List<DetalleDeOrden>>(OrdenesId);
            

            return Json(0, JsonRequestBehavior.AllowGet);
        }
    }
}