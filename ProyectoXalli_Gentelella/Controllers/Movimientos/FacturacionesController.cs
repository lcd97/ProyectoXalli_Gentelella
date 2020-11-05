using Newtonsoft.Json;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProyectoXalli_Gentelella.tipoCambioBCN;

namespace ProyectoXalli_Gentelella.Controllers.Movimientos {
    public class FacturacionesController : Controller {
        private DBControl db = new DBControl();
        private bool completado = false;
        private string mensaje = "";

        //INSTANCIA DE SERVICIO WEB
        Tipo_Cambio_BCNSoapClient tipoCambio = new Tipo_Cambio_BCNSoapClient();

        public ActionResult CalcularCambioHoy() {
            int dia = DateTime.Now.Day;
            int mes = DateTime.Now.Month;
            int anio = DateTime.Now.Year;

            var cambio = tipoCambio.RecuperaTC_Dia(anio, mes, dia);

            return Json(cambio, JsonRequestBehavior.AllowGet);
        }

        // GET: Facturaciones
        public ActionResult Index() {
            ViewBag.FormaPagoId = new SelectList(db.TiposDePago, "Id", "DescripcionTipoPago");
            ViewBag.MonedaId = new SelectList(db.Monedas, "Id", "DescripcionMoneda");

            return View();
        }

        /// <summary>
        /// CARGA LAS ORDENDES DEL CLIENTE
        /// </summary>
        /// <param name="ClienteId">CLIENTE ID</param>
        /// <returns></returns>
        public ActionResult CargarOrdenes(int ClienteId) {
            int cliente = 0;

            //SI EL ID DEL CLIENTE ES 0 - CARGAR LAS ORDENES DE LOS VISITANTES
            if (ClienteId == 0) {
                cliente = db.Clientes.Where(c => c.EmailCliente.Trim() == "defaultuser@xalli.com").Select(c => c.Id).FirstOrDefault();
            } else
                //SI ES DIF A 0 CARGAR LOS HUESPEDES
                cliente = ClienteId;

            //RECUPERO TODAS LAS ORDENES DE DET ID
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

        /// <summary>
        /// OBTIENE EL DETALLE DE LAS ORDENES SELECCIONADAS DEL HUESPED
        /// </summary>
        /// <param name="ordenIds">RECUPERA TODAS LAS ORDENES SELECCIONADAS DEL CLIENTE</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CargarSeleccionadas(List<int> ordenIds, int clienteId = 0) {
            //CREO LAS INSTANCIA DONDE SE ALMACENARA LAS/LA ORDEN(ES)
            List<detalleCliente> ordenesCliente = new List<detalleCliente>();

            ordenIds.Sort();//ORDENO LA LISTA OBTENIDA

            //RECORRO LA LISTA DE IDS PARA OBTENER SU DETALLE
            foreach (var item in ordenIds) {
                //ALMACENO EL DETALLE EN EL OBJETO
                ordenesCliente.AddRange(IteracionObjeto(item));
            }//FIN FOREACH

            //RECORRO LA LISTA DEL CLIENTE Y AGRUPO ELEMENTOS DE DIFERENTES ORDENES
            var detalleFinal = (from obj in ordenesCliente
                                group new { obj } by new { obj.Id, obj.Cantidad, obj.Precio, obj.Platillo } into grouped
                                select new detalleCliente {
                                    Id = grouped.Key.Id,
                                    Cantidad = grouped.Sum(s => s.obj.Cantidad),
                                    Platillo = grouped.Key.Platillo,
                                    Precio = grouped.Key.Precio
                                }).ToList();

            Cliente cliente = new Cliente();
            var img = (dynamic)null;

            //VERIFICO EL TIPO DE CLIENTE
            //if (clienteId != 0) {
            //    cliente = db.Clientes.Where(c => c.Id == clienteId).Select(c => c).FirstOrDefault();
            //    img = db.Imagenes.DefaultIfEmpty(null).FirstOrDefault(i => i.Id == cliente.ImagenId).Ruta;
            //}

            //bool diplomatico = img == "N/A" || img == null ? false : true;

            return Json(new { detalleFinal, img }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// OBTENGO LA CONSULTA DEL DETALLE DE ORDEN DEL CLIENTE (1X1)
        /// </summary>
        /// <param name="OrdenId">ID ACTUAL</param>
        /// <returns></returns>
        public List<detalleCliente> IteracionObjeto(int OrdenId) {
            //OBTENGO EL DETALLE DE DETERMINADA ORDEN
            List<detalleCliente> ordCliente = (from obj in db.Ordenes.ToList()
                                         join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                                         join menu in db.Menus.ToList() on det.MenuId equals menu.Id
                                         where det.OrdenId == OrdenId
                                         group new { det, menu } by new { menu.DescripcionMenu, det.NotaDetalleOrden, det.PrecioOrden, det.MenuId } into grouped
                                         select new detalleCliente {
                                             Id = grouped.Key.MenuId,
                                             Cantidad = grouped.Sum(s => s.det.CantidadOrden),//SUMO LA CANTIDAD SOLICITADA EN CASO DE QUE SEA EL MISMO PRODUCTO
                                             Platillo = grouped.Key.DescripcionMenu,
                                             Precio = grouped.Key.PrecioOrden
                                         }).ToList();

            return ordCliente;
        }

        /// <summary>
        /// CARGA LA VISTA PARA AGREGAR UN CLIENTE DIPLOMATICO
        /// </summary>
        /// <returns></returns>
        public ActionResult ClienteDiplomatico() {
            return View();
        }

        /// <summary>
        /// CLASE INTERNA DONDE SE ALMACENA EL DETALLE DE LA ORDEN DE CLIENTES
        /// </summary>
        public class detalleCliente {
            public int Id { get; set; }
            public int Cantidad { get; set; }
            public string Platillo { get; set; }
            public double Precio { get; set; }
        }
    }
}