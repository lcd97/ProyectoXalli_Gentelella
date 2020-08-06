using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProyectoXalli_Gentelella.Startup))]
namespace ProyectoXalli_Gentelella
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
