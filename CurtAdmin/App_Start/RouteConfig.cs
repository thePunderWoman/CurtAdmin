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
                url: "Website/SetPrimaryContent/{id}/{menuid}",
                defaults: new { controller = "Website", action = "SetPrimaryContent", id = 0, menuid = 0 }
            );

            routes.MapRoute(
                name: "Content",
                url: "Website/Content",
                defaults: new { controller = "Website", action = "Index" }
            );

            routes.MapRoute(
                name: "ContentMenu",
                url: "Website/Content/Menu/{id}",
                defaults: new { controller = "Website", action = "Index", id = 0 }
            );

            routes.MapRoute(
                name: "ContentEdit",
                url: "Website/Content/Edit/{id}/{revisionID}",
                defaults: new { controller = "Website", action = "EditContent", id = 0, revisionID = 0 }
            );

            routes.MapRoute(
                name: "LinkEdit",
                url: "Website/Link/Edit/{id}",
                defaults: new { controller = "Website", action = "EditLink", id = 0 }
            );

            routes.MapRoute(
                name: "LinkAdd",
                url: "Website/Link/Add/{id}",
                defaults: new { controller = "Website", action = "AddLink", id = 0 }
            );

            routes.MapRoute(
                name: "ContentAdd",
                url: "Website/Content/Add/{id}",
                defaults: new { controller = "Website", action = "AddContent", id = 0 }
            );

            routes.MapRoute(
                name: "MenuAdd",
                url: "Website/Menu/Add",
                defaults: new { controller = "Website", action = "AddMenu" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}