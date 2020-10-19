using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
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
                where = "WHERE b.Id = 2";
            } else if (Role == "Bartender") {
                where = "WHERE b.Id = 1";
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
                        "on b.Id = c.BodegaId " + where +
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
            var ordenesActivas = (from obj in db.Ordenes.ToList()
                                  where obj.EstadoOrden == 1
                                  select obj).Count();

            var ordenesTotales = (from obj in db.Ordenes.ToList()
                                  where (obj.FechaOrden).Month == (DateTime.Now).Month
                                  select obj).Count();

            /*
             select 	
	                SUM(do.CantidadOrden*do.PrecioOrden) as facturado
                from ord.Ordenes o 
                inner join ord.DetallesDeOrden do
                on o.Id = do.OrdenId
                WHERE datepart(month,o.FechaOrden)=datepart(month,GETDATE())
             */

            var ventasTotales = (from obj in db.Ordenes.ToList()
                                 join dt in db.DetallesDeOrden.ToList() on obj.Id equals dt.OrdenId
                                 where (obj.FechaOrden).Month == (DateTime.Now).Month
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
            var mes = DateTime.Now.Month;
            var anio = DateTime.Now.Year;

            DateTimeFormatInfo monthName = new DateTimeFormatInfo();

            var consulta = db.DetallesDeOrden.Where(f => f.Orden.FechaOrden.Month == DateTime.Now.Month);

            for (int i = 1; i <= DateTime.DaysInMonth(anio, mes); i++) {
                var item = new OrdenesPorDia();

                item.Fecha = i < 10 ? ("0" + i).ToString() + "/" + monthName.GetAbbreviatedMonthName(mes) : i + "/" + monthName.GetAbbreviatedMonthName(mes);
                var fecha = consulta.Where(o => o.Orden.FechaOrden.Day == i);

                item.TotalVentas = fecha.Any() ? fecha.Sum(x => x.CantidadOrden * x.PrecioOrden) : 0;

                ordenes.Add(item);
            }

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        public class OrdenesPorDia {
            public string Fecha { get; set; }
            public double TotalVentas { get; set; }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}