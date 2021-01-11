using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProyectoXalli_Gentelella.Models;

namespace ProyectoXalli_Gentelella.Controllers {
    [Authorize]
    public class AccountController : Controller {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext context = new ApplicationDbContext();

        private DBControl db = new DBControl();
        private string mensaje = "";

        public AccountController() {
        }

        public ActionResult comprobarUser(string userName) {
            var checkUser = UserManager.FindByName(userName);

            bool existe = checkUser != null ? true : false;

            return Json(existe, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// RETORNA LA VISTA DE PERFIL DE USUARIO
        /// </summary>
        /// <returns></returns>
        public ActionResult UserProfile() {
            return View();
        }

        /// <summary>
        /// CONSULTA PARA VER EL TIPO DE ROL DE USUARIO
        /// </summary>
        /// <param name="empleado"></param>
        /// <returns></returns>
        public ActionResult ColaboradorRole(string empleado) {

            var result = (from tb1 in context.Users
                          from tb2 in tb1.Roles
                          join tb3 in context.Roles on tb2.RoleId equals tb3.Id
                          where tb1.Id == empleado
                          orderby tb1.UserName, tb3.Name
                          select new {
                              Role = tb3.Name,
                              ColaboradorId = tb1.PeopleId
                          }).FirstOrDefault();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// RECUPERA LA INFORMACION DEL COLABORADOR LOGGEADO
        /// </summary>
        /// <param name="empleado"></param>
        /// <returns></returns>
        public ActionResult userProfileData(string empleado) {
            var colaborador = (from tb1 in context.Users//NETUSER
                               from tb2 in tb1.Roles//NETROLES
                               join tb3 in context.Roles on tb2.RoleId equals tb3.Id
                               where tb1.Id == empleado
                               orderby tb1.UserName, tb3.Name
                               select new {
                                   UserId = tb1.Id,
                                   Role = tb3.Name,
                                   ColaboradorId = tb1.PeopleId,
                                   Correo = tb1.Email,
                                   UserName = tb1.UserName
                               }).FirstOrDefault();

            var dataProfile = (from obj in db.Datos.ToList()
                               join col in db.Meseros.ToList() on obj.Id equals col.DatoId
                               where col.Id == colaborador.ColaboradorId
                               select new {
                                   ColaboradorId = col.Id,
                                   Nombre = obj.PNombre,
                                   Apellido = obj.PApellido,
                                   Cedula = obj.Cedula,
                                   INSS = col.INSS,
                                   RUC = obj.RUC,
                                   Entrada = col.HoraEntrada,
                                   Salida = col.HoraSalida
                               }).FirstOrDefault();

            return Json(new { colaborador, dataProfile }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditProfile(int colaboradorId, string userId, string nombreCol, string apellidoCol, string correo, string ruc) {

            if (ruc != "") {
                if (ruc.Length != 14) {
                    mensaje = "El número RUC debe ser de 14 dígitos";
                    return Json(new { success = false, message = mensaje }, JsonRequestBehavior.AllowGet);
                }
            }

            //BUSCAMOS EL OBJETO DEL COLABORADOR
            IdentityResult result = new IdentityResult();
            Mesero mesero = db.Meseros.DefaultIfEmpty(null).FirstOrDefault(m => m.Id == colaboradorId);
            string nombre = "";

            //SI ENCUENTRA EL MESERO EDITAR DATOS
            if (mesero != null) {
                Dato dato = db.Datos.DefaultIfEmpty(null).FirstOrDefault(d => d.Id == mesero.DatoId);

                dato.PNombre = nombreCol;
                dato.PApellido = apellidoCol;
                dato.RUC = ruc != "" ? ruc : null;

                nombre = dato.PNombre + " " + dato.PApellido;

                db.Entry(dato).State = EntityState.Modified;
                if (db.SaveChanges() > 0) {
                    //BUSCAMOS EL USUARIO EN SEGURIDAD
                    var usuario = UserManager.FindById(userId);

                    if (usuario != null) {
                        usuario.Email = correo != "" ? correo : null;

                        result = UserManager.Update(usuario);
                        mensaje = result.Succeeded ? "Editado correctamente" : "Error al editar";
                    }
                }
            }

            return Json(new { success = result.Succeeded, message = mensaje, Nombre = nombre }, JsonRequestBehavior.AllowGet);
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager {
            get {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string mensaje = "") {

            if (mensaje != "")
                ViewBag.mensaje = mensaje;

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            //VALIDAR QUE LA CUENTA NO ESTE BLOQUEDA
            var userLogin = UserManager.Find(model.Username, model.Password);

            if (userLogin != null) {
                if (!userLogin.LockoutEnabled) {
                    ModelState.AddModelError("", "Intento de inicio de sesión no válido.");
                    return View(model);
                }
            }

            // No cuenta los errores de inicio de sesión para el bloqueo de la cuenta
            // Para permitir que los errores de contraseña desencadenen el bloqueo de la cuenta, cambie a shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result) {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Intento de inicio de sesión no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe) {
            // Requerir que el usuario haya iniciado sesión con nombre de usuario y contraseña o inicio de sesión externo
            if (!await SignInManager.HasBeenVerifiedAsync()) {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            // El código siguiente protege de los ataques por fuerza bruta a los códigos de dos factores. 
            // Si un usuario introduce códigos incorrectos durante un intervalo especificado de tiempo, la cuenta del usuario 
            // se bloqueará durante un período de tiempo especificado. 
            // Puede configurar el bloqueo de la cuenta en IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result) {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código no válido.");
                    return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        //
        // GET: /Account/Register
        public ActionResult Register() {
            ViewBag.RoleList = new SelectList(context.Roles.ToList(), "Name", "Name");

            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(/*RegisterViewModel model*/string Username, string Password, string PasswordConfirmed, string ConfirmPassword, string RoleName, int PeopleId) {
            bool almacenado = false;

            //if (ModelState.IsValid)
            //{
            var user = new ApplicationUser { UserName = Username, PeopleId = PeopleId /*, Email = model.Username*/ };
            var result = await UserManager.CreateAsync(user, Password);
            if (result.Succeeded) {
                //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false); INICIA SESION AUTOMATICAMENTE

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar correo electrónico con este vínculo
                // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                // await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", "Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                //SE REGISTRA EL ROL ASIGNADO AL USUARIO
                await this.UserManager.AddToRoleAsync(user.Id, RoleName);

                almacenado = true;//CAMBIOS REALIZADOS

                //return RedirectToAction("Index", "Home");
                return Json(almacenado, JsonRequestBehavior.AllowGet);

            }
            AddErrors(result);
            //}

            //ViewBag.RoleList = new SelectList(context.Roles.ToList(), "Name", "Name");

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            //return View(model);


            return Json(almacenado, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code) {
            if (userId == null || code == null) {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword() {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null /*|| !(await UserManager.IsEmailConfirmedAsync(user.Id))*/) {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar correo electrónico con este vínculo
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");
                var body= "Restablecer contraseña\nPara restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>";
                sendEmail(model.Email, body);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        public void sendEmail(string Email, string body) {
            string emailHotel = "proyectoshotel2020@gmail.com";
            string passwordHotel = "bxqalmibjgxzjqux";
            //string passwordHotel = "Calabazas#sin.Nombre2020";

            try {
                MailMessage correo = new MailMessage();
                correo.From = new MailAddress(emailHotel);
                correo.To.Add(Email);
                correo.Subject = "Cambio de contraseña Xalli Hotel";
                correo.Body = body;
                correo.IsBodyHtml = true;
                correo.Priority = MailPriority.Normal;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(emailHotel, passwordHotel);

                smtp.Send(correo);

                mensaje = "Correo enviado correctamente";

            } catch (Exception ex) {
                mensaje = ex.Message;
            }
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> changePassReset(string userId) {
            string pass = "xalli2021";
            IdentityResult resultado = new IdentityResult();

            var usuario = await UserManager.FindByIdAsync(userId);

            if (usuario != null) {

                if (!string.IsNullOrEmpty(pass)) {
                    IdentityResult validacionClave = await UserManager.PasswordValidator.ValidateAsync(pass);

                    if (validacionClave.Succeeded) {
                        usuario.PasswordHash = UserManager.PasswordHasher.HashPassword(pass);
                        resultado = await UserManager.UpdateAsync(usuario);
                        if (resultado.Succeeded) {
                            mensaje = "Cambio exitoso. Nueva clave: " + pass;
                        } else {
                            mensaje = "Error al modificar. Intentelo de nuevo";
                        }
                    }
                } else {
                    mensaje = "La clave no puede estar vacia.";
                }
            } else {
                mensaje = "Usuario no encontrado";
            }

            return Json(new { success = resultado.Succeeded, message = mensaje }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetData() {
            var result = (from tb1 in context.Users
                          from tb2 in tb1.Roles
                          join tb3 in context.Roles on tb2.RoleId equals tb3.Id
                          where tb1.PeopleId != 0
                          orderby tb1.UserName, tb3.Name
                          select new {
                              Id = tb1.Id,
                              Role = tb3.Name,
                              UserName = tb1.UserName,
                              Desactivado = tb1.LockoutEnabled
                          }).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResetByAdmin() {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code) {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null) {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded) {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            //AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation() {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl) {
            // Solicitar redireccionamiento al proveedor de inicio de sesión externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe) {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null) {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model) {
            if (!ModelState.IsValid) {
                return View();
            }

            // Generar el token y enviarlo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider)) {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl) {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null) {
                return RedirectToAction("Login");
            }

            // Si el usuario ya tiene un inicio de sesión, iniciar sesión del usuario con este proveedor de inicio de sesión externo
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result) {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Si el usuario no tiene ninguna cuenta, solicitar que cree una
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl) {
            if (User.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid) {
                // Obtener datos del usuario del proveedor de inicio de sesión externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null) {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded) {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded) {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff() {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure() {
            return View();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_userManager != null) {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null) {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Asistentes
        // Se usa para la protección XSRF al agregar inicios de sesión externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager {
            get {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null) {
            }

            public ChallengeResult(string provider, string redirectUri, string userId) {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context) {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null) {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}