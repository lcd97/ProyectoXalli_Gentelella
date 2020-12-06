using MenuAPI.Areas.API.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using ProyectoXalli_Gentelella.Areas.API.Models;
using ProyectoXalli_Gentelella.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Areas.API.Controllers
{
    [AllowAnonymous]
    public class LoginWSController : Controller
    {
        DBControl db = new DBControl();

        [HttpGet]
        public JsonResult Login(string user, string pass)
        {
            RespuestaLogin respuestaLogin = new RespuestaLogin();
            respuestaLogin.exito = false;

            var resul = System.Web.HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>().PasswordSignIn(user, pass, false, false);

            if (resul == SignInStatus.Success)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                int idcolaborador = UserManager.FindByName(user).PeopleId;
                string id = UserManager.FindByName(user).Id;
                var roles = UserManager.GetRoles(id);

                string nombreCompleto = (from m in db.Meseros.Where(m => m.EstadoMesero == true)
                                         join d in db.Datos on m.DatoId equals d.Id
                                         where m.Id == idcolaborador
                                         select d.PNombre + " " + d.PApellido).DefaultIfEmpty(null).FirstOrDefault();


                if (roles.FirstOrDefault() == "Admin" || roles.FirstOrDefault() == "Mesero")
                {
                    respuestaLogin.id = idcolaborador;
                    respuestaLogin.nombreCompleto = nombreCompleto;
                    respuestaLogin.rol = roles.FirstOrDefault();
                    respuestaLogin.exito = true;
                }
                else
                {
                    respuestaLogin.exito = false;
                    respuestaLogin.nombreCompleto = "Usted no esta autorizado para utilizar la aplicacion movil";
                }
            }
            else
            {
                respuestaLogin.exito = false;
                respuestaLogin.nombreCompleto = "Usuario o Contraseña Incorrectos";
            }

            return Json(respuestaLogin, JsonRequestBehavior.AllowGet);
        }
    }
}