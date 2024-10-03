using System.Web;
using System.Web.Optimization;

namespace MasterPiece
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/lib/animate/animate.min.css",
                     "~/lib/owlcarousel/assets/owl.carousel.min.css",
                     "~/css/bootstrap.min.css",
                     "~/css/style.css"
                     ));
            // Bundle for JS files in assets folder
            bundles.Add(new ScriptBundle("~/bundles/assets/js").Include(
                "~/Content/src/plugins/datatables/js/jquery.dataTables.min.js",   // Adjusted path
                "~/Content/src/plugins/datatables/js/dataTables.bootstrap4.min.js",   // Adjusted path
                "~/Content/src/plugins/datatables/js/dataTables.responsive.min.js",   // Adjusted path
                "~/Content/src/plugins/datatables/js/responsive.bootstrap4.min.js"    // Adjusted path
            ));
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Content/vendors/scripts/core.js",                  // Adjusted path
                "~/Content/vendors/scripts/script.min.js",            // Adjusted path
                "~/Content/vendors/scripts/process.js",               // Adjusted path
                "~/Content/vendors/scripts/layout-settings.js"        // Adjusted path
            ));
            bundles.Add(new StyleBundle("~/bundles/assets/css").Include(
                "~/Content/vendors/styles/core.css",               // Adjusted path
                "~/Content/vendors/styles/icon-font.min.css"        // Adjusted path
            ));
            bundles.Add(new StyleBundle("~/bundles/cssd").Include(
                "~/Content/vendors/styles/style.css",                 // Adjusted path
                "~/Content/src/plugins/datatables/css/dataTables.bootstrap4.min.css",   // Adjusted path
                "~/Content/src/plugins/datatables/css/responsive.bootstrap4.min.css"    // Adjusted path
            ));
        }
    }
}
