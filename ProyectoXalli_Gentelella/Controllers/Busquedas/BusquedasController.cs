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
    }
}