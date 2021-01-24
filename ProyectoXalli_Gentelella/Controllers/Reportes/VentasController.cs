using Microsoft.Reporting.WebForms;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static ProyectoXalli_Gentelella.Controllers.Busquedas.BusquedasController;

namespace ProyectoXalli_Gentelella.Controllers.Reportes {
    public class VentasController : Controller {
        private DBControl db = new DBControl();

        // GET: Ventas
        public ActionResult VentasMensuales(int anio) {
            var grafico = ventasMesAnio(anio, false);
            var tabla = ventasMesAnio(anio, true);
            var general = infoCuadros(tabla, anio);

            ReportViewer rtp = new ReportViewer();
            rtp.ProcessingMode = ProcessingMode.Local;
            rtp.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"Reports/Ventas.rdlc";
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Ventas", grafico));
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_TablaVentas", tabla));
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Totales", general));
            rtp.SizeToReportContent = true;
            rtp.ShowPrintButton = true;
            rtp.ShowZoomControl = true;
            ViewBag.rpt = rtp;
            ViewBag.datos = tabla.Count;

            return View();
        }

        public List<generalData> infoCuadros(List<OrdenesPorMes> ventas, int anio) {
            double ventaTotal = ventas.Sum(s => s.TotalVentas);
            int ordenTotal = (from obj in db.Ordenes.ToList()
                              join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                              where obj.FechaOrden.Year == anio && t.CodigoTipoOrden == "V01"
                              select obj).Count();

            generalData Datos = new generalData() {
                Anio = anio,
                OrdenesAtendidas = ordenTotal,
                VentasTotalMes = ventaTotal
            };

            return new List<generalData> { Datos };
        }

        public List<OrdenesPorMes> ventasMesAnio(int anio, bool completo) {
            List<OrdenesPorMes> ordenes = new List<OrdenesPorMes>();
            DateTimeFormatInfo mfi = new CultureInfo("es-ES", false).DateTimeFormat;
            var consulta = db.DetallesDeOrden.Where(f => f.Orden.FechaOrden.Year == anio);

            //RECORRO LOS REGISTROS POR DIA DEL MES
            for (int i = 1; i <= 12; i++) {
                OrdenesPorMes item = new OrdenesPorMes();

                //CREO LA FECHA (12)
                item.Fecha = mfi.GetAbbreviatedMonthName(i);
                var fecha = consulta.Where(o => o.Orden.FechaOrden.Month == i);//FILTRO LOS REGISTROS POR DIAS
                item.TotalVentas = fecha.Any() ? fecha.Sum(x => x.CantidadOrden * x.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT

                if (completo || item.TotalVentas != 0) {
                    ordenes.Add(item);//AGREGO A LA LISTA FINAL                
                }
            }

            return ordenes;
        }

        public ActionResult SeleccionarAnio() {
            return View();
        }

        public class generalData {
            public int Anio { get; set; }
            public int OrdenesAtendidas { get; set; }
            public double VentasTotalMes { get; set; }
        }
    }
}