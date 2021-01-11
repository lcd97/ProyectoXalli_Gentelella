using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MenuAPI.Areas.API.Models;
using ProyectoXalli_Gentelella.Areas.API;
using ProyectoXalli_Gentelella.Models;
using ProyectoXalli_Gentelella.Web_Sockets;

namespace MenuAPI.Areas.API.Controllers
{
    [BasicAuthentication]
    public class OrdenesWSController : Controller
    {
        //conexion con la db
        private DBControl db = new DBControl();

        //Obtener las ordenes con estado atendido con fecha igual al dia de hoy
        [HttpGet]
        public JsonResult OrdenesAbiertas()
        {
            var ordenes = (from o in db.Ordenes.Where(x => x.EstadoOrden == 1).ToList()
                           join m in db.Meseros.ToList() on o.MeseroId equals m.Id
                           join me in db.Mesas.ToList() on o.MesaId equals me.Id
                           join c in db.Clientes.ToList() on o.ClienteId equals c.Id
                           join d in db.Datos.ToList() on c.DatoId equals d.Id
                           where o.FechaOrden.ToShortDateString() == DateTime.Today.ToShortDateString()
                           orderby o.FechaOrden descending
                           select new OrdenWS
                           {
                               id = o.Id,
                               codigo = o.CodigoOrden,
                               fechaorden = o.FechaOrden,
                               horaorden = ConvertHour(o.FechaOrden.Hour, o.FechaOrden.Minute),
                               estado = o.EstadoOrden,
                               meseroid = m.Id,
                               clienteid = c.Id,
                               mesaid = me.Id,
                               cliente = c.Dato.PNombre + " " + c.Dato.PApellido,
                               mesero = m.Dato.PNombre + " " + m.Dato.PApellido,
                               mesa = me.DescripcionMesa

                           }).ToList();

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        //Obtener las ordenes con estado sin facturar o cerrado con fecha igual al dia de hoy
        [HttpGet]
        public JsonResult OrdenesCerradas()
        {
            var ordenes = (from o in db.Ordenes.Where(x => x.EstadoOrden == 2).ToList()
                           join m in db.Meseros.ToList() on o.MeseroId equals m.Id
                           join me in db.Mesas.ToList() on o.MesaId equals me.Id
                           join c in db.Clientes.ToList() on o.ClienteId equals c.Id
                           join d in db.Datos.ToList() on c.DatoId equals d.Id
                           where o.FechaOrden.ToShortDateString() == DateTime.Today.ToShortDateString()
                           orderby o.FechaOrden descending
                           select new OrdenWS
                           {
                               id = o.Id,
                               codigo = o.CodigoOrden,
                               fechaorden = o.FechaOrden,
                               horaorden = ConvertHour(o.FechaOrden.Hour, o.FechaOrden.Minute),
                               estado = o.EstadoOrden,
                               meseroid = m.Id,
                               clienteid = c.Id,
                               mesaid = me.Id,
                               cliente = c.Dato.PNombre + " " + c.Dato.PApellido,
                               mesero = m.Dato.PNombre + " " + m.Dato.PApellido,
                               mesa = me.DescripcionMesa

                           }).ToList();

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        //Obtener las ordenes con estado ordenado y sin facturar o cerrado con fecha igual al dia de hoy
        [HttpGet]
        public JsonResult Ordenes()
        {
            var ordenes = (from o in db.Ordenes.Where(x => x.EstadoOrden == 1 || x.EstadoOrden == 2).ToList()
                           join m in db.Meseros.ToList() on o.MeseroId equals m.Id
                           join me in db.Mesas.ToList() on o.MesaId equals me.Id
                           join c in db.Clientes.ToList() on o.ClienteId equals c.Id
                           join d in db.Datos.ToList() on c.DatoId equals d.Id
                           where o.FechaOrden.ToShortDateString() == DateTime.Today.ToShortDateString()
                           orderby o.FechaOrden descending
                           select new OrdenWS
                           {
                               id = o.Id,
                               codigo = o.CodigoOrden,
                               fechaorden = o.FechaOrden,
                               horaorden = ConvertHour(o.FechaOrden.Hour, o.FechaOrden.Minute),
                               estado = o.EstadoOrden,
                               meseroid = m.Id,
                               clienteid = c.Id,
                               mesaid = me.Id,
                               cliente = c.Dato.PNombre + " " + c.Dato.PApellido,
                               mesero = m.Dato.PNombre + " " + m.Dato.PApellido,
                               mesa = me.DescripcionMesa
                           }).ToList();

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// CONVERTIR LA HORA DE 24HRS A HORA LOCAL
        /// </summary>
        /// <returns></returns>
        public string ConvertHour(int Hora, int Minuto)
        {
            //CONVIERTE LA HORA DE FORMATO 24 A FORMATO 12
            int hour = (Hora + 11) % 12 + 1;
            string Meridiano = Hora > 12 ? "PM" : "AM";

            //AGREGAR UN 0 A LA HORA O MINUTO SI EL VALOR ES MENOR A 10
            string horaEnviar = (hour < 10 ? "0" + hour.ToString() : hour.ToString()) + ":" +
                                (Minuto < 10 ? "0" + Minuto.ToString() : Minuto.ToString()) + " " + Meridiano;

            return horaEnviar;
        }


        //buscar el ultimo codigo de orden
        [HttpGet]
        public async Task<JsonResult> UltimoCodigo()
        {
            var num = await (from obj in db.Ordenes
                             select obj.CodigoOrden).DefaultIfEmpty().MaxAsync();

            int codigo = 1;

            if (num != 0)
            {
                codigo = num + 1;
            }

            return Json(codigo, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult OrdenesPorHuesped(int id)
        {
            var ordenes = (from o in db.Ordenes.Where(x => x.EstadoOrden == 1 || x.EstadoOrden == 2).ToList()
                           join m in db.Meseros.ToList() on o.MeseroId equals m.Id
                           join me in db.Mesas.ToList() on o.MesaId equals me.Id
                           join c in db.Clientes.ToList() on o.ClienteId equals c.Id
                           join d in db.Datos.ToList() on c.DatoId equals d.Id
                           where c.Id == id
                           orderby o.FechaOrden descending
                           select new OrdenWS
                           {
                               id = o.Id,
                               codigo = o.CodigoOrden,
                               fechaorden = o.FechaOrden,
                               horaorden = ConvertHour(o.FechaOrden.Hour, o.FechaOrden.Minute),
                               estado = o.EstadoOrden,
                               meseroid = m.Id,
                               mesaid = me.Id,
                               clienteid = c.Id,
                               mesero = m.Dato.PNombre + " " + m.Dato.PApellido,
                               mesa = me.DescripcionMesa
                               

                           }).ToList();

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> CerrarOrden(int id)
        {
            ResultadoWS resultadoWS = new ResultadoWS();
            resultadoWS.Mensaje = "";
            resultadoWS.Resultado = false;

            var Orden = await db.Ordenes.Where(o => o.Id == id).DefaultIfEmpty(null).FirstOrDefaultAsync();

            using (var transact = db.Database.BeginTransaction())
            {
                try
                {
                    if (Orden != null)
                    {
                        Orden.EstadoOrden = 2;
                        db.Entry(Orden).State = EntityState.Modified;

                        if (db.SaveChanges() > 0)
                        {
                            resultadoWS.Mensaje = "Orden Cerrada con exito";
                            resultadoWS.Resultado = true;
                            transact.Commit();
                            AddNewOrder.Preppend(/*obj*/);//CONEXION DE WEBSOCKETS
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                catch (Exception)
                {
                    resultadoWS.Mensaje = "Error al cerrar la orden";
                    resultadoWS.Resultado = false;
                    transact.Rollback();
                }
            }

            return Json(resultadoWS, JsonRequestBehavior.AllowGet);
        }

        //cerrando la db
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