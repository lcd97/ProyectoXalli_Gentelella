using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MenuAPI.Areas.API.Models
{
    public class OrdenWS
    {
        public int id { get; set; }
        public int codigo { get; set; }
        public DateTime fechaorden { get; set; }
        public DateTime tiempoorden { get; set; }
        public int estado { get; set; }
        public int meseroid { get; set; }
        public int clienteid { get; set; }
        public string cliente { get; set; }
        public string mesero { get; set;}

    }
}