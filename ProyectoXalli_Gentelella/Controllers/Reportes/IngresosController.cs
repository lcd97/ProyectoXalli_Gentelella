using Microsoft.Reporting.WebForms;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Reportes {
    [Authorize]
    public class IngresosController : Controller {
        private DBControl db = new DBControl();

        [Authorize(Roles = "Admin, Recepcionista")]
        // GET: Ingresos
        public ActionResult Index() {
            ViewBag.BodegaId = new SelectList(db.Bodegas, "Id", "DescripcionBodega");

            return View();
        }

        public ActionResult CargarEntradas(int proveedorId = 0, string fechaInic = "", string fechaFin = "", int bodega = 1) {
            var filtro = (dynamic)null;

            var ent = (from en in db.Entradas.ToList()
                       join bod in db.Bodegas.ToList() on en.BodegaId equals bod.Id
                       join prov in db.Proveedores.ToList() on en.ProveedorId equals prov.Id
                       join dat in db.Datos.ToList() on prov.DatoId equals dat.Id
                       where bod.Id == bodega
                       select new {
                           Id = en.Id,
                           Codigo = en.CodigoEntrada,
                           Fecha = en.FechaEntrada.ToShortDateString(),
                           ProveedorId = prov.Id,
                           Proveedor = prov.Local ? dat.PNombre + " " + dat.PApellido : prov.NombreComercial
                       }).ToList();

            //APLICAR FILTROS
            if (proveedorId != 0) {
                filtro = ent.Where(w => w.ProveedorId == proveedorId);
            } else if (fechaInic != "") {
                filtro = ent.Where(w => DateTime.Parse(w.Fecha) >= DateTime.Parse(fechaInic) && DateTime.Parse(w.Fecha) <= DateTime.Parse(fechaFin));
            }

            return Json(new { data = filtro }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerarEntrada(int Id) {

            var encabezado = getEncabezado(Id);
            var detalle = getDetalleEntrada(Id);
            var calculo = getCalculo(detalle);

            ReportViewer rtp = new ReportViewer();
            rtp.ProcessingMode = ProcessingMode.Local;
            rtp.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"Reports/Entradas.rdlc";
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Encabezado", encabezado));
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_DetalleEntrada", detalle));
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Calculos", calculo));
            rtp.SizeToReportContent = true;
            rtp.ShowPrintButton = true;
            rtp.ShowZoomControl = true;
            ViewBag.rpt = rtp;
            ViewBag.datos = encabezado.Count;

            return View();
        }

        public List<Calculo> getCalculo(List<DetalleEntrada> Detalle) {
            var subtotal = Detalle.Sum(s => s.SubTotal);
            var calculo = new Calculo();

            calculo.TotalEntrada = subtotal;

            return new List<Calculo> { calculo };
        }

        public List<Encabezado> getEncabezado(int entradaId) {
            var entrada = db.Entradas.Find(entradaId);
            var bodega = db.Bodegas.Find(entrada.BodegaId);
            var proveedor = db.Proveedores.Find(entrada.ProveedorId);
            var dato = db.Datos.Find(proveedor.DatoId);
            var tipoEntrada = db.TiposDeEntrada.Find(entrada.TipoEntradaId);

            var encab = new Encabezado();
            encab.FechaEntrada = entrada.FechaEntrada;
            encab.CodigoEntrada = entrada.CodigoEntrada;
            encab.Bodega = bodega.DescripcionBodega;
            encab.Proveedor = proveedor.Local ? dato.PNombre + " " + dato.PApellido : proveedor.NombreComercial;
            encab.TipoEntrada = tipoEntrada.DescripcionTipoEntrada;

            return new List<Encabezado> { encab };
        }

        public List<DetalleEntrada> getDetalleEntrada(int entradaId) {
            List<DetalleEntrada> detalle = (from det in db.DetallesDeEntrada
                                            join prod in db.Productos on det.ProductoId equals prod.Id
                                            join med in db.UnidadesDeMedida on prod.UnidadMedidaId equals med.Id
                                            where det.EntradaId == entradaId
                                            select new DetalleEntrada() {
                                                CantidadEntrada = det.CantidadEntrada,
                                                PrecioEntrada = det.PrecioEntrada,
                                                Producto = prod.MarcaProducto == null ? prod.NombreProducto : prod.NombreProducto + " - " + prod.MarcaProducto,
                                                UnidadMedida = prod.PresentacionProducto == 1 ? med.AbreviaturaUM : prod.PresentacionProducto + " " + med.AbreviaturaUM,
                                                SubTotal = det.PrecioEntrada * det.CantidadEntrada
                                            }).ToList();

            return detalle;
        }

        public class Calculo {
            public double TotalEntrada { get; set; }
        }

        public class DetalleEntrada {
            public double CantidadEntrada { get; set; }
            public double PrecioEntrada { get; set; }
            public string Producto { get; set; }
            public string UnidadMedida { get; set; }
            public double SubTotal { get; set; }
        }

        public class Encabezado {
            public DateTime FechaEntrada { get; set; }
            public string CodigoEntrada { get; set; }
            public string Bodega { get; set; }
            public string Proveedor { get; set; }
            public string TipoEntrada { get; set; }
        }
    }
}