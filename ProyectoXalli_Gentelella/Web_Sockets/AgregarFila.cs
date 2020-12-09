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
            //var orden = await (dynamic)null;

            //if (EmpleadoRol == "Mesero") {
            //    orden = (from obj in db.Ordenes.ToList()
            //             join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
            //             join d in db.Datos.ToList() on c.DatoId equals d.Id
            //             where obj.EstadoOrden == 1 && obj.MeseroId == empleadoId
            //             select new {
            //                 OrdenId = obj.Id,
            //                 CodigoOrden = obj.CodigoOrden,
            //                 HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
            //                 Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
            //                 Mesero = (from o in db.Ordenes
            //                           join m in db.Meseros on o.MeseroId equals m.Id
            //                           join a in db.Datos on m.DatoId equals a.Id
            //                           where obj.Id == o.Id
            //                           select a.PNombre + " " + a.PApellido).FirstOrDefault()
            //             }).LastOrDefault();
            //} else {
            //    orden = (from obj in db.Ordenes.ToList()
            //             join c in db.Clientes.ToList() on obj.ClienteId equals c.Id
            //             join d in db.Datos.ToList() on c.DatoId equals d.Id
            //             where obj.EstadoOrden == 1
            //             select new {
            //                 OrdenId = obj.Id,
            //                 CodigoOrden = obj.CodigoOrden,
            //                 HoraOrden = ConvertHour(obj.FechaOrden.Hour, obj.FechaOrden.Minute),
            //                 Cliente = d.PNombre.ToUpper() != "DEFAULT" ? d.PNombre + " " + d.PApellido : "N/A",
            //                 Mesero = (from o in db.Ordenes
            //                           join m in db.Meseros on o.MeseroId equals m.Id
            //                           join a in db.Datos on m.DatoId equals a.Id
            //                           where obj.Id == o.Id
            //                           select a.PNombre + " " + a.PApellido).FirstOrDefault()
            //             }).LastOrDefault();
            //}

            //return orden;

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