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

                if (login(user.Name,user.Pass))
                {
                    return;
                }
            }

            //filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", String.Format("Basic realm=\"{0}\"", BasicRealm ?? "Ryadel"));
            /// thanks to eismanpat for this line: https://www.ryadel.com/en/http-basic-authentication-asp-net-mvc-using-custom-actionfilter/#comment-2507605761
            filterContext.Result = new HttpUnauthorizedResult();
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