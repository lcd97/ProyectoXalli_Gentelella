using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;
using ProyectoXalli_Gentelella.Models;
using System.Threading.Tasks;

namespace ProyectoXalli_Gentelella.Web_Sockets {
    public class AgregarFila {
        private readonly static Lazy<AgregarFila> obj = new Lazy<AgregarFila>(() => new AgregarFila
        (GlobalHost.ConnectionManager.GetHubContext<RowAdd>().Clients));
        private IHubConnectionContext<dynamic> clients;

        private DBControl db = new DBControl();
        private ApplicationDbContext context = new ApplicationDbContext();

        public AgregarFila(IHubConnectionContext<dynamic> clients) {
            this.clients = clients;
        }

        public static AgregarFila Instance {
            get { return obj.Value; }
        }

        //LO QUE QUIERO RECUPERAR Y MOSTRAR EN LA VISTA
        public async Task<object> GetData(int empleadoId, string EmpleadoRol) {
            var orden = (dynamic)null;
            //OBTENGO TODAS LAS ORDENES REALIZADAS POR EL MESERO LOGGEADO
            if (EmpleadoRol == "Mesero") {
                orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where obj.EstadoOrden == 1 && obj.MeseroId == empleadoId && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa
                         }).ToList();
            } else if (EmpleadoRol == "Cocinero") {
                //OBTENGO TODAS LAS ORDENES CON PLATILLO PENDIENTES
                orden = (from obj in db.Ordenes.ToList()
                         join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                         join men in db.Menus.ToList() on det.MenuId equals men.Id
                         join cat in db.CategoriasMenu.ToList() on men.CategoriaMenuId equals cat.Id
                         join bod in db.Bodegas.ToList() on cat.BodegaId equals bod.Id
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == "COCINA" && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa,
                             Mesero = (from o in db.Ordenes
                                       join m in db.Meseros on o.MeseroId equals m.Id
                                       join a in db.Datos on m.DatoId equals a.Id
                                       where obj.Id == o.Id
                                       select a.PNombre + " " + a.PApellido).FirstOrDefault()
                         }).Distinct().ToList();
            } else if (EmpleadoRol == "Bartender") {
                //OBTENGO TODAS LAS ORDENES CON BEBIDAS PENDIENTES
                orden = (from obj in db.Ordenes.ToList()
                         join det in db.DetallesDeOrden.ToList() on obj.Id equals det.OrdenId
                         join men in db.Menus.ToList() on det.MenuId equals men.Id
                         join cat in db.CategoriasMenu.ToList() on men.CategoriaMenuId equals cat.Id
                         join bod in db.Bodegas.ToList() on cat.BodegaId equals bod.Id
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where det.EstadoDetalleOrden == false && bod.DescripcionBodega.ToUpper() == "BAR" && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa,
                             Mesero = (from o in db.Ordenes
                                       join m in db.Meseros on o.MeseroId equals m.Id
                                       join a in db.Datos on m.DatoId equals a.Id
                                       where obj.Id == o.Id
                                       select a.PNombre + " " + a.PApellido).FirstOrDefault()
                         }).Distinct().ToList();
            } else {
                orden = (from obj in db.Ordenes.ToList()
                         join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
                         join d in db.Datos.ToList() on c.DatoId equals d.Id
                         join t in db.TiposDeOrden.ToList() on obj.TipoOrdenId equals t.Id
                         join m in db.Mesas.ToList() on obj.MesaId equals m.Id
                         where obj.EstadoOrden == 1 && t.CodigoTipoOrden == "V01"
                         orderby obj.FechaOrden.ToLongTimeString() ascending
                         select new {
                             OrdenId = obj.Id,
                             CodigoOrden = obj.CodigoOrden,
                             HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
                             Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
                             Mesa = m.DescripcionMesa,
                             Mesero = (from o in db.Ordenes
                                       join m in db.Meseros on o.MeseroId equals m.Id
                                       join a in db.Datos on m.DatoId equals a.Id
                                       where obj.Id == o.Id
                                       select a.PNombre + " " + a.PApellido).FirstOrDefault()
                         }).ToList();
            }

            return true;
        }

        /// <summary>
        /// CONVERTIR LA HORA DE 24HRS A HORA LOCAL
        /// </summary>
        /// <returns></returns>
        public string ConvertHour(int Hora, int Minuto) {
            //CONVIERTE LA HORA DE FORMATO 24 A FORMATO 12
            int hour = (Hora + 11) % 12 + 1;
            string Meridiano = Hora > 12 ? "PM" : "AM";

            //AGREGAR UN 0 A LA HORA O MINUTO SI EL VALOR ES MENOR A 10
            string horaEnviar = (hour < 10 ? "0" + hour.ToString() : hour.ToString()) + ":" +
                                (Minuto < 10 ? "0" + Minuto.ToString() : Minuto.ToString()) + " " + Meridiano;

            return horaEnviar;
        }

        //HACER LA NUEVA ORDEN (FELIX)
        public void NuevaFila(/*object ordenes*/) {
            clients.All.nuevaFila(/*ordenes*/);
        }
    }
}