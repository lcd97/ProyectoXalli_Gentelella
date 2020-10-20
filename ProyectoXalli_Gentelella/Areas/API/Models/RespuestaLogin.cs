using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoXalli_Gentelella.Areas.API.Models
{
    public class RespuestaLogin
    {
        public int id { get; set; }
        public string nombreCompleto { get; set; }
        public string rol { get; set; }
        public bool exito { get; set; }

    }
}