using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella.Areas.API
{
    public class BasicAuthenticationAttribute : ActionFilterAttribute
    {
        public SignInStatus SingnInStatus { get; private set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            var auth = req.Headers["Authorization"];

            if (!String.IsNullOrEmpty(auth))
            {
                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(auth.Substring(6))).Split(':');
                var user = new { Name = cred[0], Pass = cred[1] };

                if (login(user.Name, user.Pass))
                {
                    return;
                }
                else
                {

                }
            }

            filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", "Basic");
            filterContext.HttpContext.Response.StatusCode = 403;
            filterContext.HttpContext.Response.StatusDescription = "Acceso denegado, Debe Authenticarse";
            filterContext.Result = new JsonResult
            {
                Data = new { Success = false, Data = "UnAuthorized" },
                ContentEncoding = System.Text.Encoding.UTF8,
                ContentType = "application/json",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            base.OnActionExecuting(filterContext);
        }

        private bool login(string user, string pass)
        {
            var resul = System.Web.HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>().PasswordSignIn(user, pass, false, false);

            if (resul == SignInStatus.Success)
            {
                return true;
            }

            return false;
        }


    }
}