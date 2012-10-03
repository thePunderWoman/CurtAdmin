using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CurtAdmin {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "SetPrimaryContent",
                url: "Admin_Website/SetPrimaryContent/{id}/{menuid}",
                defaults: new { controller = "Admin_Website", action = "SetPrimaryContent", id = 0, menuid = 0 }
            );

            routes.MapRoute(
                name: "Content",
                url: "Admin_Website/Content",
                defaults: new { controller = "Admin_Website", action = "Index" }
            );

            routes.MapRoute(
                name: "ContentMenu",
                url: "Admin_Website/Content/Menu/{id}",
                defaults: new { controller = "Admin_Website", action = "Index", id = 0 }
            );

            routes.MapRoute(
                name: "ContentEdit",
                url: "Admin_Website/Content/Edit/{id}",
                defaults: new { controller = "Admin_Website", action = "EditContent", id = 0 }
            );

            routes.MapRoute(
                name: "LinkEdit",
                url: "Admin_Website/Link/Edit/{id}",
                defaults: new { controller = "Admin_Website", action = "EditLink", id = 0 }
            );

            routes.MapRoute(
                name: "LinkAdd",
                url: "Admin_Website/Link/Add/{id}",
                defaults: new { controller = "Admin_Website", action = "AddLink", id = 0 }
            );

            routes.MapRoute(
                name: "ContentAdd",
                url: "Admin_Website/Content/Add/{id}",
                defaults: new { controller = "Admin_Website", action = "AddContent", id = 0 }
            );

            routes.MapRoute(
                name: "MenuAdd",
                url: "Admin_Website/Menu/Add",
                defaults: new { controller = "Admin_Website", action = "AddMenu" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}