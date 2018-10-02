using System.Web;
using System.Web.Optimization;

namespace AdobeScheduler
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/adobe/login").Include(
                "~/Scripts/App/login.js"
            ));

            bundles.Add(new ScriptBundle("~/adobe/app").Include(
               "~/Scripts/App/adobeCalendar.js"
           ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/Scripts/jquery.searchit.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/calendar").Include(
                "~/Scripts/fullcalendar.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            
            bundles.Add(new StyleBundle("~/Content/calendar").Include(
                "~/Content/fullcalendar.css"));

            bundles.Add(new ScriptBundle("~/app").Include(
                "~/Scripts/moment.min.js",
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui.js",
                "~/Scripts/chosen.jquery.js",
                "~/Scripts/jquery.signalR-{version}.js",
                // "~/Scripts/jquery.pnotify.js",
                "~/Scripts/pnotify.custom.min.js",
                "~/Scripts/fullcalendar.js",               
                "~/Scripts/App/adobeCalendar.js",                
                 "~/Scripts/jquery-ui-timepicker-addon.js"
                ));

            bundles.Add(new ScriptBundle("~/auth").Include(
               "~/Scripts/moment.min.js",
                "~/Scripts/jquery-{version}.js",
               "~/Scripts/jquery-ui.js",
               "~/Scripts/chosen.jquery.js",
               "~/Scripts/jquery.signalR-{version}.js",
               //"~/Scripts/jquery.pnotify.js",
               "~/Scripts/pnotify.custom.min.js",
               "~/Scripts/fullcalendar.js",               
               "~/Scripts/App/adobeCalendar.js",               
               "~/Scripts/jquery-ui-timepicker-addon.js",               
               "~/Scripts/App/login.js"              
               ));

            bundles.Add(new StyleBundle("~/content/css").Include(
                 "~/Content/chosen.min.css",
                 "~/Content/fullcalendar.css",
                 "~/Content/datepickercss",
                 "~/Content/Site.css",
                 "~/Content/pnotify.custom.min.css"/*,
                 "~/Content/jquery.pnotify.default.icons.css"*/
                ));
            
            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
                        
        }
    }
}