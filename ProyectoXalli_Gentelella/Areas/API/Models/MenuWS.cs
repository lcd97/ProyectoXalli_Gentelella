using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MenuAPI.Areas.API.Models
{
    public class MenuWS
    {
        public int id { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public string tiempoestimado { get; set; }
        public double precio { get; set; }
        public bool estado { get; set; }
        public string ruta { get; set; }
        public int idcategoria { get; set; }
    }
}