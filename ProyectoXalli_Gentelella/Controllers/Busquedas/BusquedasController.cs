using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.Busquedas {
    public class BusquedasController : Controller {

        private DBControl db = new DBControl();

        // GET: Busquedas
        public ActionResult BuscarCliente() {
            return View();
        }

        /// <summary>
        /// OBTIENE UNA LISTA DE TODOS LOS CLIENTES CON FILTRO DE NOMBRE Y APELLIDO
        /// </summary>
        /// <param name="Nombres"></param>
        /// <param name="Apellidos"></param>
        /// <returns></returns>
        public ActionResult BusquedaCliente(string Nombres = "", string Apellidos = "") {

            //var clientes = (from obj in db.Datos
            //                join c in db.Clientes on obj.Id equals c.DatoId
            //                where obj.PNombre.Trim() == Nombres.Trim() || obj.PApellido.Trim() == Apellidos.Trim()
            //                select new {
            //                    Identificacion = obj.Cedula.Trim() != null ? obj.Cedula.Trim() : c.PasaporteCliente.Trim(),
            //                    RUC = obj.RUC,
            //                    Cliente = obj.PNombre.Trim() + " " + obj.PApellido.Trim()
            //                }).ToList();


            var clientes = (from obj in db.Datos
                            join c in db.Clientes on obj.Id equals c.DatoId
                            where (obj.PNombre.Trim().Contains(Nombres.Trim()) || obj.PApellido.Trim().Contains(Apellidos.Trim())) &&
                            c.EstadoCliente == true
                            select new {
                                Id = c.Id,
                                Identificacion = obj.Cedula.Trim() != null ? obj.Cedula.Trim() : c.PasaporteCliente.Trim(),
                                RUC = obj.RUC,
                                Cliente = obj.PNombre.Trim() + " " + obj.PApellido.Trim()
                            }).ToList();

            return Json(clientes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductosFavoritos(string Role) {
            /*
             SELECT Top 5 m.Id,
                		sum(dt.CantidadOrden)
            FROM Menu.Menus m
            INNER JOIN Ord.DetallesDeOrden dt
            ON m.Id = dt.MenuId
            inner join ord.Ordenes o
            on o.Id = dt.OrdenId
            WHERE DATEPART(wk, o.FechaOrden) = DATEPART(wk, '2020/09/20')
            GROUP BY m.Id             
             */

            //var producto = (from obj in db.Menus.ToList()
            //                join dt in db.DetallesDeOrden.ToList() on obj.Id equals dt.MenuId
            //                join ord in db.Ordenes.ToList() on dt.OrdenId equals ord.Id
            //                //where SqlFunctions.DatePart("week", ord.FechaOrden) == SqlFunctions.DatePart("week", DateTime.Parse("26/09/2020"))
            //                select obj).Take(3);

            //SELECT Top 3 m.DescripcionMenu, sum(dt.CantidadOrden) as Cantidad FROM Menu.Menus m INNER JOIN Ord.DetallesDeOrden dt ON m.Id = dt.MenuId inner join ord.Ordenes o on o.Id = dt.OrdenId WHERE DATEPART(wk, o.FechaOrden) = DATEPART(wk, getdate()) GROUP BY m.DescripcionMenu order by cantidad desc
            string where = "";

            if (Role == "Cocinero") {
                where = "WHERE b.Id = 2 AND t.CodigoTipoOrden='V01' ";
            } else if (Role == "Bartender") {
                where = "WHERE b.Id = 1 AND t.CodigoTipoOrden='V01' ";
            } else {
                where = "";
            }


            string qwerty = "SELECT Top 3 m.DescripcionMenu, sum(dt.CantidadOrden) as Cantidad " +
                        "FROM Menu.Menus m " +
                        "INNER JOIN Ord.DetallesDeOrden dt " +
                        "ON m.Id = dt.MenuId " +
                        "inner join ord.Ordenes o " +
                        "on o.Id = dt.OrdenId " +
                        "inner join Menu.CategoriasMenu c " +
                        "on m.CategoriaMenuId = c.Id " +
                        "inner join inv.Bodegas b " +
                        "on b.Id = c.BodegaId " +
                        "inner join ord.TiposDeOrden t " +
                        "on t.Id = o.TipoOrdenId " + where +
                        "GROUP BY m.DescripcionMenu " +
                        "order by cantidad desc";

            //STRING FORMAT DE CONSULTA SQL PARA OBTENER LOS PRIMEROS 3 PLATILLOS/BEBIDAS MAS PEDIDOS DEL MENU
            var producto = db.Database.SqlQuery<MenuProducto>(qwerty);

            return Json(producto, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// CLASE SERIALIZABLE PARA ALMACENAR LA CONSULTA DE LOS PRODUCTOS MAS SOLICITADOS
        /// </summary>
        [Serializable]
        public class MenuProducto {
            public string DescripcionMenu { get; set; }
            public int Cantidad { get; set; }

        }

        /// <summary>
        /// CONSULTA PARA PONER DE LETRERO EN EL DASHBOARD
        /// </summary>
        /// <returns></returns>
        public ActionResult DashboardSign() {
            //ORDENES ACTIVAS DE LAS VENTAS REALIZADAS
            var ordenesActivas = (from obj in db.Ordenes.ToList()
                                  join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                                  where obj.EstadoOrden == 1 && t.CodigoTipoOrden == "V01"
                                  select obj).Count();

            //ORDENES TOTALES DE LAS VENTAS REALIZADAS
            var ordenesTotales = (from obj in db.Ordenes.ToList()
                                  join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                                  where (obj.FechaOrden).Month == (DateTime.Now).Month && t.CodigoTipoOrden == "V01"
                                  select obj).Count();

            /*
             select 	
	                SUM(do.CantidadOrden*do.PrecioOrden) as facturado
                from ord.Ordenes o 
                inner join ord.DetallesDeOrden do
                on o.Id = do.OrdenId
                WHERE datepart(month,o.FechaOrden)=datepart(month,GETDATE())
             */

            //TOTAL DE LAS VENTAS REALIZADAS
            var ventasTotales = (from obj in db.Ordenes.ToList()
                                 join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                                 join dt in db.DetallesDeOrden.ToList() on obj.Id equals dt.OrdenId
                                 where (obj.FechaOrden).Month == (DateTime.Now).Month && t.CodigoTipoOrden == "V01"
                                 select (dt.CantidadOrden * dt.PrecioOrden)).Sum();

            return Json(new { ordenesActivas, ordenesTotales, ventasTotales }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ventasDiaMes() {

            //var qwerty = db.Database.SqlQuery<OrdenesPorDia>("select " +
            //        "CONVERT(varchar, o.FechaOrden,3) as Fecha," +
            //        "SUM(do.CantidadOrden*do.PrecioOrden) as TotalVentas " +
            //    "from ord.Ordenes o " +
            //    "inner join ord.DetallesDeOrden do " +
            //    "on o.Id = do.OrdenId " +
            //    "WHERE datepart(month,o.FechaOrden)=datepart(month,GETDATE()) " +
            //    "group by CONVERT(varchar, o.FechaOrden,3)");

            //return Json(qwerty, JsonRequestBehavior.AllowGet);

            List<OrdenesPorDia> ordenes = new List<OrdenesPorDia>();
            var mes = DateTime.Now.Month;//OBTENGO EL MES ACTUAL
            var anio = DateTime.Now.Year;//OBTENGO EL AÑO ACTUAL

            DateTimeFormatInfo monthName = new DateTimeFormatInfo();//FORMATO DE FECHA (ES)

            //SE REALIZA LA CONSULTA DONDE DEVUELVE REGISTROS DEL MES ACTUAL
            var consulta = db.DetallesDeOrden.Where(f => f.Orden.FechaOrden.Month == DateTime.Now.Month);

            //RECORRO LOS REGISTROS POR DIA DEL MES
            for (int i = 1; i <= DateTime.DaysInMonth(anio, mes); i++) {
                var item = new OrdenesPorDia();

                //CREO LA FECHA (31/12)
                item.Fecha = i < 10 ? ("0" + i).ToString() + "/" + monthName.GetAbbreviatedMonthName(mes) : i + "/" + monthName.GetAbbreviatedMonthName(mes);
                var fecha = consulta.Where(o => o.Orden.FechaOrden.Day == i);//FILTRO LOS REGISTROS POR DIAS

                item.TotalVentas = fecha.Any() ? fecha.Sum(x => x.CantidadOrden * x.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT

                ordenes.Add(item);//AGREGO A LA LISTA FINAL
            }

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// OBTENGO LAS ESTADISTICAS DEL EMPLEADO SEGUN SU ROL
        /// </summary>
        /// <param name="Role"></param>
        /// <param name="MeseroId"></param>
        /// <returns></returns>
        public ActionResult StatticsRole(string Role, int MeseroId) {
            List<OrdenesPorMes> ordenes = new List<OrdenesPorMes>();
            var mes = DateTime.Now.Month;//OBTENGO EL MES ACTUAL
            var anio = DateTime.Now.Year;//OBTENGO EL AÑO ACTUAL
            DateTimeFormatInfo mfi = new CultureInfo("es-ES", false).DateTimeFormat;

            double recOrd = 0, porcOrdenes = 0, porcVentas = 0, recVenta = 0;

            if (Role.ToUpper() == "MESERO") {
                //SE REALIZA LA CONSULTA DONDE DEVUELVE REGISTROS DEL AÑO ACTUAL Y ORDENES REALIZADAS POR X MESERO
                var consulta = db.DetallesDeOrden.Include(d => d.OrdenId).Where(f => f.Orden.FechaOrden.Year == anio && f.Orden.MeseroId == MeseroId);

                //RECORRO LOS REGISTROS POR DIA DEL MES
                for (int i = 1; i <= 12; i++) {
                    var item = new OrdenesPorMes();

                    //CREO LA FECHA (12)
                    item.Fecha = mfi.GetAbbreviatedMonthName(i);
                    var fecha = consulta.Where(o => o.Orden.FechaOrden.Month == i);//FILTRO LOS REGISTROS POR DIAS
                    item.TotalVentas = fecha.Any() ? fecha.Sum(x => x.CantidadOrden * x.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT

                    ordenes.Add(item);//AGREGO A LA LISTA FINAL
                }

                //SACAR ESTADISTICAS DE CUADROS
                //((Valor Reciente / Valor Anterior) – 1) x 100 --> CALCULA EL CRECIMIENTO
                recOrd = db.Ordenes.Where(w => w.FechaOrden.Month == mes && w.MeseroId == MeseroId).Count();//CANTIDAD DE ORDENES ATENDIDAS DEL MES ACTUAL
                if ((mes - 1) != 0) {
                    var pasOrd = db.Ordenes.Where(w => w.FechaOrden.Month == (mes - 1) && w.MeseroId == MeseroId).Count();//CANTIDAD DE ORDENES ATENDIDAS DEL MES ANTERIOR
                    porcOrdenes = pasOrd == 0 ? 100 : ((recOrd / pasOrd) - 1) * 100;
                } else {//SI LA FECHA ACTUAL ES EN ENERO
                    var ant = db.DetallesDeOrden.Include(d => d.OrdenId)
                        .Where(f => f.Orden.FechaOrden.Year == (anio - 1) && f.Orden.FechaOrden.Month == 12 && f.Orden.MeseroId == MeseroId)
                        .Count();//SACAMOS LA CANTIDAD DE ORDENES DE DICIEMBRE DEL AÑO ANTERIOR
                    porcOrdenes = ant == 0 ? 100 : ((recOrd / ant) - 1) * 100;
                }

                //SACAR EL TOTAL DE VENTAS DEL MES
                recVenta = ordenes[(mes - 1)].TotalVentas;//AGARRO EL TOTAL DE VENTAS DEL MES ACTUAL (EN LISTA ES DE 0-11)
                if ((mes - 1) < 0) {//
                    var pasVenta = ordenes[(mes - 1) - 1].TotalVentas;
                    porcVentas = pasVenta == 0 ? 100 : ((recVenta / pasVenta) - 1) * 100;
                } else {
                    var ant = db.DetallesDeOrden.Include(d => d.OrdenId)
                        .Where(f => f.Orden.FechaOrden.Year == (anio - 1) && f.Orden.FechaOrden.Month == 12 && f.Orden.MeseroId == MeseroId)
                        .Sum(s => s.CantidadOrden * s.PrecioOrden);//SACAMOS EL TOTAL DE VENTAS DE DICIEMBRE DEL AÑO ANTERIOR
                    porcVentas = ant == 0 ? 100 : ((recVenta / ant) - 1) * 100;
                }
            } else if (Role.ToUpper() == "RECEPCIONISTA" || Role.ToUpper() == "ADMIN") {
                //SE REALIZA LA CONSULTA DONDE DEVUELVE REGISTROS DEL AÑO ACTUAL
                var consulta = db.DetallesDeOrden.Where(f => f.Orden.FechaOrden.Year == anio);

                //RECORRO LOS REGISTROS POR DIA DEL MES
                for (int i = 1; i <= 12; i++) {
                    var item = new OrdenesPorMes();

                    //CREO LA FECHA (12)
                    item.Fecha = mfi.GetAbbreviatedMonthName(i);
                    var fecha = consulta.Where(o => o.Orden.FechaOrden.Month == i);//FILTRO LOS REGISTROS POR DIAS
                    item.TotalVentas = fecha.Any() ? fecha.Sum(x => x.CantidadOrden * x.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT

                    ordenes.Add(item);//AGREGO A LA LISTA FINAL
                }

                //SACAR ESTADISTICAS DE CUADROS
                //((Valor Reciente / Valor Anterior) – 1) x 100 --> CALCULA EL CRECIMIENTO
                recOrd = db.Ordenes.Where(w => w.FechaOrden.Month == mes).Count();
                if ((mes - 1) != 0) {
                    var pasOrd = db.Ordenes.Where(w => w.FechaOrden.Month == (mes - 1)).Count();
                    porcOrdenes = pasOrd == 0 ? 100 : ((recOrd / pasOrd) - 1) * 100;
                } else {//SI LA FECHA ACTUAL ES EN ENERO
                    var ant = db.DetallesDeOrden.Include(d => d.OrdenId)
                        .Where(f => f.Orden.FechaOrden.Year == (anio - 1) && f.Orden.FechaOrden.Month == 12)
                        .Count();//SACAMOS LA CANTIDAD DE ORDENES DE DICIEMBRE DEL AÑO ANTERIOR
                    porcOrdenes = ant == 0 ? 100 : ((recOrd / ant) - 1) * 100;
                }

                //SACAR EL TOTAL DE VENTAS DEL MES
                recVenta = ordenes[(mes - 1)].TotalVentas;
                if ((mes - 1) < 0) {
                    var pasVenta = ordenes[(mes - 1) - 1].TotalVentas;
                    porcVentas = pasVenta == 0 ? 100 : ((recVenta / pasVenta) - 1) * 100;
                } else {
                    var ant = db.DetallesDeOrden.Include(d => d.OrdenId)
                        .Where(f => f.Orden.FechaOrden.Year == (anio - 1) && f.Orden.FechaOrden.Month == 12)
                        .Sum(s => s.CantidadOrden * s.PrecioOrden);//SACAMOS EL TOTAL DE VENTAS DE DICIEMBRE DEL AÑO ANTERIOR
                    porcVentas = ant == 0 ? 100 : ((recVenta / ant) - 1) * 100;
                }
            } else {
                List<OrdenesBodega> ordenesBodegas = new List<OrdenesBodega>();

                //SE REALIZA LA CONSULTA DONDE DEVUELVE REGISTROS DEL AÑO ACTUAL Y SEGUN EL AREA
                //var consCocina = db.DetallesDeOrden.Include(d => d.Menu).Include(m => m.Menu.CategoriaMenu).Include(c => c.Menu.CategoriaMenu.Bodega)
                //    .Where(f => f.Orden.FechaOrden.Year == DateTime.Now.Year && f.Menu.CategoriaMenu.DescripcionCategoriaMenu.ToUpper() == "COCINA");
                //var consBar = db.DetallesDeOrden.Include(d => d.Menu).Include(m => m.Menu.CategoriaMenu).Include(c => c.Menu.CategoriaMenu.Bodega)
                //    .Where(f => f.Orden.FechaOrden.Year == DateTime.Now.Year && f.Menu.CategoriaMenu.DescripcionCategoriaMenu.ToUpper() == "BAR");

                var consCocina = db.Ordenes
                                .Join(db.DetallesDeOrden, ord => ord.Id, det => det.OrdenId, (ord, det) => new { ord, det })
                                .Join(db.Menus, odet => odet.det.MenuId, menu => menu.Id, (odet, menu) => new { odet, menu })
                                .Join(db.CategoriasMenu, men => men.menu.CategoriaMenuId, cat => cat.Id, (men, cat) => new { men, cat })
                                .Join(db.Bodegas, cate => cate.cat.BodegaId, bod => bod.Id, (cate, bod) => new { cate, bod })
                                .Where(w => w.cate.men.odet.ord.FechaOrden.Year == anio && w.bod.DescripcionBodega.ToUpper() == "COCINA")
                                .Select(s => s);

                var consBar = db.Ordenes
                                .Join(db.DetallesDeOrden, ord => ord.Id, det => det.OrdenId, (ord, det) => new { ord, det })
                                .Join(db.Menus, odet => odet.det.MenuId, menu => menu.Id, (odet, menu) => new { odet, menu })
                                .Join(db.CategoriasMenu, men => men.menu.CategoriaMenuId, cat => cat.Id, (men, cat) => new { men, cat })
                                .Join(db.Bodegas, cate => cate.cat.BodegaId, bod => bod.Id, (cate, bod) => new { cate, bod })
                                .Where(w => w.cate.men.odet.ord.FechaOrden.Year == anio && w.bod.DescripcionBodega.ToUpper() == "BAR")
                                .Select(s => s);

                //RECORRO LOS REGISTROS POR DIA DEL MES
                for (int i = 1; i <= 12; i++) {
                    var item = new OrdenesBodega();

                    ////CREO LA FECHA (12)
                    item.Fecha = mfi.GetAbbreviatedMonthName(i);
                    //var fechaCocina = consCocina.Where(o => o.Orden.FechaOrden.Month == i);//FILTRO LOS REGISTROS POR DIAS AREA COCINA
                    //var fechaBar = consBar.Where(o => o.Orden.FechaOrden.Month == i);//FILTRO LOS REGISTROS POR DIAS AREA BAR

                    //item.TVCocina = fechaCocina.Any() ? fechaCocina.Sum(x => x.CantidadOrden * x.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT
                    //item.TVBar = fechaBar.Any() ? fechaBar.Sum(x => x.CantidadOrden * x.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT

                    var cocina = consCocina.Where(o => o.cate.men.odet.ord.FechaOrden.Month == i);
                    var bar = consBar.Where(o => o.cate.men.odet.ord.FechaOrden.Month == i);

                    item.TVCocina = cocina.Any() ? cocina.Sum(x => x.cate.men.odet.det.CantidadOrden * x.cate.men.odet.det.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT
                    item.TVBar = bar.Any() ? bar.Sum(x => x.cate.men.odet.det.CantidadOrden * x.cate.men.odet.det.PrecioOrden) : 0;//SI NO EXISTE EL VALOR PONER 0 DEFAULT

                    ordenesBodegas.Add(item);//AGREGO A LA LISTA FINAL
                }

                return Json(new { ordenes = ordenesBodegas, bodegas = true, recOrd, porcOrdenes, recVenta, porcVentas }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { ordenes, bodegas = false, recOrd, porcOrdenes, recVenta, porcVentas }, JsonRequestBehavior.AllowGet);
        }


        public class OrdenesPorDia {
            public string Fecha { get; set; }
            public double TotalVentas { get; set; }
        }

        public class OrdenesPorMes {
            public string Fecha { get; set; }
            public double TotalVentas { get; set; }
        }

        public class OrdenesBodega {
            public string Fecha { get; set; }
            public double TVCocina { get; set; }
            public double TVBar { get; set; }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}