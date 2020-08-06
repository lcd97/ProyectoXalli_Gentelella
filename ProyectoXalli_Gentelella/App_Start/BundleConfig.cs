using System.Web;
using System.Web.Optimization;

namespace ProyectoXalli_Gentelella {
    public class BundleConfig {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles) {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery-ui-dist.js"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información. De este modo, estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // JS DE LA PLANTILLA GENTELLA
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/jquery.min.js",
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/fastclick.js",
                      "~/Scripts/switchery.js",
                      "~/Scripts/custom.min.js"));

            // CSS DE LA PLANTILLA GENTELLA
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/custom.min.css"));

            // JS DE LA PLANTILLA GENTELLA
            bundles.Add(new ScriptBundle("~/bundles/datatableJS").Include(
                      "~/Scripts/datatable/jquery.dataTables.js",
                      "~/Scripts/datatable/dataTables.bootstrap.js"));

            // CSS DE LA PLANTILLA GENTELLA
            bundles.Add(new StyleBundle("~/Content/datatableCSS").Include(
                      "~/Content/datatable/dataTables.bootstrap.css"));
        }
    }
}
