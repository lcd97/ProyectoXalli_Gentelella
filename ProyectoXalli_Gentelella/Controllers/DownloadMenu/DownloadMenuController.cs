using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Controllers.DownloadMenu {
    [AllowAnonymous]
    public class DownloadMenuController : Controller {
        // GET: DownloadMenu
        [AllowAnonymous]
        public ActionResult Index() {
            return View();
        }
    }
}