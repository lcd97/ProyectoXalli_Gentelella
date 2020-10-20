using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ProyectoXalli_Gentelella.Areas.API.Models;
using System.Data.Entity;

namespace ProyectoXalli_Gentelella.Areas.API.Controllers
{
    [BasicAuthentication]
    public class ClientesWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

        //Obteniendo todos los Clientes
        [HttpGet]
        public async Task<JsonResult> Clientes()
        {
            var clientes = await (from c in db.Clientes.Where(c => c.EstadoCliente == true)
                                  join d in db.Datos on c.DatoId equals d.Id
                                  select new ClienteWS
                                  {

                                      id = c.Id,
                                      identificacion = d.Cedula == null ? c.PasaporteCliente : d.Cedula,
                                      nombre = d.PNombre,
                                      apellido = d.PApellido

                                  }).ToListAsync();

            return Json(clientes, JsonRequestBehavior.AllowGet);
        }

        //obteniendo un cliente
        [HttpGet]
        public async Task<JsonResult> Cliente(int id)
        {
            //Consultando los clientes de la DB
            var cliente = await (from c in db.Clientes.Where(c => c.EstadoCliente == true)
                                  join d in db.Datos on c.DatoId equals d.Id
                                  where c.Id == id
                                  select new ClienteWS
                                  {
                                      id = c.Id,
                                      identificacion = d.Cedula == null ? c.PasaporteCliente : d.Cedula,
                                      nombre = d.PNombre,
                                      apellido = d.PApellido

                                  }).DefaultIfEmpty().FirstOrDefaultAsync();

            return Json(cliente, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> ClientesConComanda()
        {
            var clientes = await (from c in db.Clientes.Where(c => c.EstadoCliente == true)
                                  join d in db.Datos on c.DatoId equals d.Id
                                  join o in db.Ordenes on c.Id equals o.ClienteId
                                  where o.EstadoOrden == 1 || o.EstadoOrden == 2 && c.Id != 2
                                  group c by new {c.Id, d.Cedula, c.PasaporteCliente, d.PNombre, d.PApellido } into g
                                  select new ClienteWS
                                  {

                                      id = g.Key.Id,
                                      identificacion = g.Key.Cedula == null ? g.Key.PasaporteCliente : g.Key.Cedula,
                                      nombre = g.Key.PNombre,
                                      apellido = g.Key.PApellido

                                  }).ToListAsync();

            return Json(clientes, JsonRequestBehavior.AllowGet);
        }



        //Cerrar la db
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




    }
}