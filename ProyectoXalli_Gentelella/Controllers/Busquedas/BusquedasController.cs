using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
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

        public ActionResult ProductosFavoritos() {
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
            //STRING FORMAT DE CONSULTA SQL PARA OBTENER LOS PRIMEROS 3 PLATILLOS/BEBIDAS MAS PEDIDOS DEL MENU
            var producto = db.Database.SqlQuery<MenuProducto>("SELECT Top 3 m.DescripcionMenu, sum(dt.CantidadOrden) as Cantidad FROM Menu.Menus m INNER JOIN Ord.DetallesDeOrden dt ON m.Id = dt.MenuId inner join ord.Ordenes o on o.Id = dt.OrdenId GROUP BY m.DescripcionMenu order by cantidad desc");

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

            var ventasTotales = (from obj in db.Ordenes.ToList()
                                 join dt in db.DetallesDeOrden.ToList() on obj.Id equals dt.OrdenId
                                 //where (obj.FechaOrden).Month == (DateTime.Now).Month
                                 select dt.PrecioOrden).Sum();

            return Json(new { ordenesActivas, ordenesTotales, ventasTotales }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}