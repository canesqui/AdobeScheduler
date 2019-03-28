using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AdobeScheduler
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            /*
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            */
            
            
            /*
            routes.MapRoute(
                name: "Reports",
                url: "reports",
                defaults: new { controller = "Dashboard", action = "Report" }
            );*/
            /*
            routes.MapRoute(
                name: "Events",
                url: "events",
                defaults: new { controller = "Dashboard", action = "EventFeed"}
            );*/
            /*
            routes.MapRoute(
                name: "Install",
                url: "install",
                defaults: new { controller = "Install", action = "Start" }
            );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new { controller = "Auth", action = "Logout" }
            );

            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new { controller = "Auth", action = "Login" }
            );
            */
            /*
            routes.MapRoute(
                name: "Rooms",
                url: "{room}",
                defaults: new { controller = "Dashboard", action = "Room" }
            );
            */
            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Dashboard", action = "Index"}
            );
        }
    }
}