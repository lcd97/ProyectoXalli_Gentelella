using System.Web;
using System.Web.Mvc;

namespace ProyectoXalli_Gentelella {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
