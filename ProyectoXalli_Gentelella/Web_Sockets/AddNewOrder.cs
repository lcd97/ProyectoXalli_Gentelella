using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Web_Sockets {
    public static class AddNewOrder {

        public static void Preppend(object orden) {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<RowAdd>();

            if (hubContext !=null ) {
                hubContext.Clients.All.nuevaFila(orden);
            }
        }
    }
}