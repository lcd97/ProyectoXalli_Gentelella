using Microsoft.Reporting.WebForms;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Reportes {
    public class FacturasController : Controller {
        private DBControl db = new DBControl();

        // GET: Reportes
        public ActionResult Index() {
            return View();
        }

        public ActionResult GenerarFactura(int Id) {
            var datos = ObtenerDatos(Id);//OBTENEMOS LOS DATOS
            var calculos = Calcular(Id, datos);
            var encabe = GetEncabezado(Id, datos);

            ReportViewer rtp = new ReportViewer();
            rtp.ProcessingMode = ProcessingMode.Local;
            rtp.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"Reports/Facturas.rdlc";
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Factura", datos));
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Calculos", calculos));
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_EncabezadoFact", encabe));
            rtp.SizeToReportContent = true;
            rtp.ShowPrintButton = true;
            rtp.ShowZoomControl = true;
            ViewBag.rpt = rtp;
            ViewBag.datos = datos.Count;

            return View();
        }

        public List<Factura> ObtenerDatos(int Id) {

            return (from ordpago in db.OrdenesPago
                    join orden in db.Ordenes on ordpago.OrdenId equals orden.Id
                    join pago in db.Pagos on ordpago.PagoId equals pago.Id
                    join detalle in db.DetallesDeOrden on orden.Id equals detalle.OrdenId
                    join menu in db.Menus on detalle.MenuId equals menu.Id
                    where pago.Id == Id
                    group new { menu, detalle, orden } by new { menu.DescripcionMenu, detalle.PrecioOrden, detalle.CantidadOrden, orden.Id } into grouped
                    select new Factura {
                        IdOrden = grouped.Key.Id,
                        Cantidad = grouped.Key.CantidadOrden,
                        PrecioUnitario = grouped.Key.PrecioOrden,
                        Menu = grouped.Key.DescripcionMenu,
                        SubTotal = grouped.Key.CantidadOrden * grouped.Key.PrecioOrden
                    }).ToList();
        }

        public List<Calculos> Calcular(int Id, List<Factura> datos) {
            var pago = db.Pagos.Find(Id);
            var reg = new Calculos();
            reg.IVA = double.Parse(pago.IVA.ToString()) / 100;
            reg.Descuento = double.Parse(pago.Descuento.ToString()) / 100;
            reg.Propina = pago.Propina;
            reg.TasaCambio = pago.TipoCambio;

            return new List<Calculos> { reg };
        }

        public List<Encabezado> GetEncabezado(int Id, List<Factura> datos) {
            var pago = db.Pagos.Find(Id);
            var idOrden = datos.FirstOrDefault()?.IdOrden;
            var orden = db.Ordenes.Find(idOrden);
            var datoCliente = db.Clientes.Find(orden?.ClienteId)?.DatoId;
            var datoMesero = db.Meseros.Find(orden?.MeseroId)?.DatoId;
            var cliente = db.Datos.Find(datoCliente);
            var mesero = db.Datos.Find(datoMesero);
            var encabez = new Encabezado();
            encabez.Fecha = pago.FechaPago;
            encabez.Cliente = cliente.PNombre != "DEFAULT" ? cliente.PNombre.ToUpper() + " " + cliente.PApellido.ToUpper() : "VISITANTE";
            encabez.Mesero = mesero.PNombre.ToUpper() + " " + mesero.PApellido.ToUpper();
            var numero = pago.NumeroPago;

            //FORMATEANDO EL CODIGO
            if (numero < 10) {
                encabez.NumeroFact = "000" + numero;
            } else if (numero >= 10 || numero < 100) {
                encabez.NumeroFact = "00" + numero;
            } else if (numero >= 100 || numero < 1000) {
                encabez.NumeroFact = "0" + numero;
            } else {
                encabez.NumeroFact = numero.ToString();
            }

            return new List<Encabezado> { encabez };
        }

        public class Encabezado {
            public DateTime Fecha { get; set; }
            public string Mesero { get; set; }
            public string Cliente { get; set; }
            public string NumeroFact { get; set; }
        }

        public class Calculos {
            public double IVA { get; set; }
            public double Descuento { get; set; }
            public double TasaCambio { get; set; }
            public double Propina { get; set; }
        }

        public class Factura {
            public int IdOrden { get; set; }
            public double PrecioUnitario { get; set; }
            public int Cantidad { get; set; }
            public string Menu { get; set; }
            public double SubTotal { get; set; }
        }

        public ActionResult cargarFacturas(int ClienteId = -1, string fechaInic = "", string fechaFin = "") {
            var filtro = (dynamic)null;

            var fact = (from pago in db.Pagos.ToList()
                        join ordPag in db.OrdenesPago.ToList() on pago.Id equals ordPag.PagoId
                        join ord in db.Ordenes.ToList() on ordPag.OrdenId equals ord.Id
                        join client in db.Clientes.ToList() on ord.ClienteId equals client.Id
                        join dato in db.Datos.ToList() on client.DatoId equals dato.Id
                        select new {
                            Id = pago.Id,
                            FechaFact = pago.FechaPago.ToShortDateString(),
                            NumFact = formatoNum(pago.NumeroPago),
                            ClienteId = client.Id,
                            Cliente = dato.PNombre.ToUpper() != "DEFAULT" ? dato.PNombre + " " + dato.PApellido : "Visitante"
                        }).Distinct().ToList();

            //APLICAR FILTROS
            if (ClienteId != -1) {
                filtro = fact.Where(w => w.ClienteId == ClienteId);
            } else if (fechaInic != "") {
                filtro = fact.Where(w => DateTime.Parse(w.FechaFact) >= DateTime.Parse(fechaInic) && DateTime.Parse(w.FechaFact) <= DateTime.Parse(fechaFin));
            }

            return Json(new { data = filtro }, JsonRequestBehavior.AllowGet);
        }

        public string formatoNum(int numero) {
            string NumeroFact = "";

            // FORMATEANDO EL CODIGO
            if (numero < 10) {
                NumeroFact = "000" + numero;
            } else if (numero >= 10 || numero < 100) {
                NumeroFact = "00" + numero;
            } else if (numero >= 100 || numero < 1000) {
                NumeroFact = "0" + numero;
            } else {
                NumeroFact = numero.ToString();
            }

            return NumeroFact;
        }
    }
}