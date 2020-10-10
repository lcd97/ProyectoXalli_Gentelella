using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ProyectoXalli_Gentelella.Web_Sockets {

    //CLASE ENDPOIND (REPRESENTACION DEL SERVIDOR PALABRAS TEXTUALES DE FELIS HDEX)
    public class RowAdd : Hub {
        private readonly AgregarFila _not;

        public RowAdd(AgregarFila not) {
            _not = not;
        }

        public RowAdd() : this(AgregarFila.Instance) { }

        //FELIX
        public async Task<object> GetData(int empleadoId, string EmpleadoRol) {
            return await _not.GetData(empleadoId, EmpleadoRol);
        }

    }
}