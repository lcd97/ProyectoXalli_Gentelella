using Microsoft.AspNet.Identity;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers {
    public class HomeController : Controller {

        private DBControl db = new DBControl();

        [Authorize]
        public ActionResult Index(string mensaje = "", string ordenId = "") {
            ViewBag.Message = mensaje;//MENSAJE DE RECARGO
            ViewBag.OrdenId = ordenId;

            return View();
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}