using Microsoft.Reporting.WebForms;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Reportes {
    public class ExistenciasController : Controller {
        private DBControl db = new DBControl();

        // GET: Existencias
        public ActionResult Index() {
            var ext = GetExistencias();
            var prod = GetProductos(ext);
            var fecha = DateTime.Today.ToShortDateString();

            ReportViewer rtp = new ReportViewer();
            rtp.ProcessingMode = ProcessingMode.Local;
            rtp.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"Reports/Existencias.rdlc";
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Existencia", prod));
            rtp.LocalReport.DataSources.Add(new ReportDataSource("ds_Fecha", fecha.ToList()));
            rtp.SizeToReportContent = true;
            rtp.ShowPrintButton = true;
            rtp.ShowZoomControl = true;
            ViewBag.rpt = rtp;
            ViewBag.datos = ext.Count;

            return View();
        }

        public List<Existencias> GetProductos(List<int> existencias) {
            List<Existencias> prod = new List<Existencias>();

            foreach (var item in existencias) {
                Producto producto = db.Productos.Find(item);
                var unidad = producto.PresentacionProducto + " " + db.UnidadesDeMedida.Find(producto.UnidadMedidaId).AbreviaturaUM;

                var ext = new Existencias {
                    CodigoProducto = producto.CodigoProducto,
                    Producto = producto.NombreProducto + " " + producto.MarcaProducto + " " + unidad
                };

                prod.Add(ext);
            }

            return prod;
        }

        //si el menuid es de bar o no 
        public bool esDeBar(int id) {
            string Bodega = (from c in db.CategoriasMenu
                             join b in db.Bodegas on c.BodegaId equals b.Id
                             join m in db.Menus on c.Id equals m.CategoriaMenuId
                             where m.Id == id
                             select b.CodigoBodega).DefaultIfEmpty().FirstOrDefault();

            if (Bodega == "B01") {
                return true;
            }

            return false;
        }

        public List<int> GetExistencias() {
            double entradas = 0;
            int salidas = 0;
            double existencia = 0;

            //LISTAR TODOS LOS PRODUCTOS INVENTARIADOS DEL BAR
            var menu = (from men in db.Menus
                        join cat in db.CategoriasMenu on men.CategoriaMenuId equals cat.Id
                        join bod in db.Bodegas on cat.BodegaId equals bod.Id
                        where men.Inventariado == true && bod.CodigoBodega.Trim().ToUpper() == "B01" && men.EstadoMenu == true
                        select men.Id).ToList();

            List<int> prod = new List<int>();

            //BUSCAR INGREDIENTES QUE CONFORMAN 
            for (int i = 0; i < menu.Count; i++) {
                int menuId = menu[i];

                //SI LA CATEGORIA DEL PLATILLO PERTENECE A BAR
                if (esDeBar(menu[i])) {
                    //LISTAR TODOS LOS PRODUCTOS QUE CONFORMAN EL MENU
                    var idProd = (from m in db.Menus.ToList()
                                  join ing in db.Ingredientes.ToList() on m.Id equals ing.MenuId
                                  where ing.MenuId == menuId
                                  select ing.ProductoId).ToList();

                    //EN ESTA CONDICION SE SACARA LA EXISTENCIA EN NUMEROS-EN CASO QUE SEA
                    if (idProd.Count == 1) {
                        ExistEntrada(idProd[0], ref entradas);//OBTENEMOS LA ENTRADA DEL PRODUCTO

                        if (entradas == 0) {
                            //NO HAY EXISTENCIAS- AGREGAMOS PROD
                            prod.Add(idProd[0]);
                        } else {
                            //BUSCAR LA BEBIDA PARA SABER SI ES TRAGO O BOTELLA

                            //SI HAY ENTRADAS BUSCAR LAS SALIDAS
                            ExistSalidas(idProd[0], ref salidas);//OBTENEMOS LAS SALIDAS DEL PRODUCTO                    

                            existencia = (int)entradas - salidas;//CALCULO DE LA EXISTENCIA
                                                                 //SI NO HAY EXISTENCIA AGREGAMOS EL PRODUCTO
                            if (existencia <= 0) {
                                prod.Add(idProd[0]);
                            }
                        }
                    }
                }//FIN IF DE BAR
            }//FIN FOR

            return prod;
        }

        public bool ExistEntrada(int prodId, ref double entradas) {
            bool esBar = false;

            //SE COMPRUEBA QUE HAYAN ENTRADAS EN EL BAR            
            var entradaProd = (from obj in db.Entradas
                               join de in db.DetallesDeEntrada on obj.Id equals de.EntradaId
                               join b in db.Bodegas on obj.BodegaId equals b.Id
                               where de.ProductoId == prodId && b.CodigoBodega == "B01"
                               group new { de, b } by new { b.CodigoBodega } into grouped
                               select new {
                                   Entradas = grouped.Sum(s => s.de.CantidadEntrada),//SUMAR TODAS LAS ENTRADAS
                                   Bar = grouped.Key.CodigoBodega == "B01" ? true : false//DETERMINAR SI LAS ENTRADAS SON DE BAR O BODEGA
                               }).FirstOrDefault();

            //SI CONTIENE AL MENOS UN ELEMENTO EL PRODUCTO TIENE EXISTENCIA
            if (entradaProd != null) {
                entradas = (double)entradaProd.Entradas;
                esBar = entradaProd.Bar;
            }

            return esBar;
        }

        public void ExistSalidas(int item, ref int salidas) {
            //BUSCAR TODOS LOS PLATILLOS DEL AREA DE BAR QUE TENGAN EL INGREDIENTE            
            var salidasProd = (from obj in db.Menus
                               join i in db.Ingredientes on obj.Id equals i.MenuId
                               join c in db.CategoriasMenu on obj.CategoriaMenuId equals c.Id
                               join b in db.Bodegas on c.BodegaId equals b.Id
                               join d in db.DetallesDeOrden on obj.Id equals d.MenuId
                               where i.ProductoId == item && b.CodigoBodega == "B01" && obj.Inventariado == true
                               select (int?)d.CantidadOrden).Sum();

            //SI CONTIENE AL MENOS UN ELEMENTO EL PRODUCTO TIENE EXISTENCIA
            if (salidasProd != null) {
                salidas = (int)salidasProd;
            }
        }


        public class Existencias {
            public string Producto { get; set; }
            public string CodigoProducto { get; set; }
        }
    }
}