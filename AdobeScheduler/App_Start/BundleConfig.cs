using System.Web;
using System.Web.Optimization;

namespace AdobeScheduler
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));            

            bundles.Add(new ScriptBundle("~/app").Include(
                "~/Scripts/moment.min.js",
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui.js",
                "~/Scripts/chosen.jquery.js",
                "~/Scripts/jquery.signalR-{version}.js",                
                "~/Scripts/pnotify.custom.min.js",
                "~/Scripts/fullcalendar.js",               
                "~/Scripts/App/adobeCalendar.js",                
                 "~/Scripts/jquery-ui-timepicker-addon.js"
                ));

            bundles.Add(new ScriptBundle("~/auth").Include(               
                "~/Scripts/jquery-{version}.js",
               "~/Scripts/jquery-ui.js",                             
               "~/Scripts/pnotify.custom.min.js",                   
               "~/Scripts/App/login.js",
               "~/Scripts/jquery.unobtrusive*",
               "~/Scripts/jquery.validate*"
               ));

            bundles.Add(new StyleBundle("~/content/css").Include(
                 "~/Content/chosen.min.css",
                 "~/Content/fullcalendar.css",
                 "~/Content/datepickercss",
                 "~/Content/Site.css",
                 "~/Content/pnotify.custom.min.css"
                ));            
        }
    }
}