using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MenuAPI.Areas.API.Models
{
    public class CategoriaWS
    {
        public int id { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public bool estado { get; set; }

    }
}