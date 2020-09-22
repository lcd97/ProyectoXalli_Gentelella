using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using ProyectoXalli_Gentelella.Models;

[assembly: OwinStartupAttribute(typeof(ProyectoXalli_Gentelella.Startup))]
namespace ProyectoXalli_Gentelella
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CrearPermisos();
        }

        private void CrearPermisos() {
            //CREAR LA CONEXION A LA BASE DE DATOS
            ApplicationDbContext contex = new ApplicationDbContext();
            //DBControl db = new DBControl();

            //CREAR INSTANCIAS DE ROLES Y USUARIOS Y MANDARLO AL CONTEXTO DE SEGURIDAD
            var AdmPermisos = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(contex));
            var AdmUsuario = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(contex));
            IdentityRole permiso = new IdentityRole();

            //SI NO EXISTE EL PERMISO ADMINISTRADOR PRINCIPAL
            if (!AdmPermisos.RoleExists("Admin")) {
                //CREAR EL ROL ADMIN
                permiso = new IdentityRole();
                permiso.Name = "Admin";
                AdmPermisos.Create(permiso);

                /*
                 SE CREA UNA INSTANCIA DEL USUARIO ADMIN SUPER USER
                 ESTAS CREDENCIALES NO TENDRA PERMITIDO REALIZAR NINGUNA 
                 ACCION COMO ORDENAR O INGRESAR 
                 */
                var usuario = new ApplicationUser();

                //CRAR EL USUARIO PARA EL ROL ADMIN
                usuario.UserName = "daniela97";
                usuario.Email = "danycordero9@gmail.com";
                usuario.PeopleId = 0;

                var resultado = AdmUsuario.Create(usuario, "Dcl030197..");

                //SI EL USUARIO SE CRE EXISTOSAMENTE
                if (resultado.Succeeded) {

                    //ASIGNAMOS EL ROL AL USUARIO RECIEN CREADO
                    AdmUsuario.AddToRole(usuario.Id, "Admin");
                }
            }

            //SI NO EXISTE EL PERMISO MESERO
            if (!AdmPermisos.RoleExists("Mesero")) {
                permiso = new IdentityRole();
                permiso.Name = "Mesero";
                AdmPermisos.Create(permiso);
            }

            //SI NO EXISTE EL PERMISO COCINERO
            if (!AdmPermisos.RoleExists("Cocinero")) {
                permiso = new IdentityRole();
                permiso.Name = "Cocinero";
                AdmPermisos.Create(permiso);
            }

            //SI NO EXISTE EL PERMISO RESPONSABLE
            if (!AdmPermisos.RoleExists("Recepcionista")) {
                permiso = new IdentityRole();
                permiso.Name = "Recepcionista";
                AdmPermisos.Create(permiso);
            }
        }
    }
}
