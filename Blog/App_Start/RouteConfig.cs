using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Blog
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "NewSlug",
                url: "Blog/{slug}",
                defaults: new
                {
                    controller = "Posts",
                    action = "Details",
                    slug = UrlParameter.Optional
                });

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}",
            //    defaults: new { controller = "Home", action = "Index" }
            //);

            routes.MapRoute(
                name: "Archive",
                url: "{controller}/{action}/{month}/{year}/{category}",
                defaults: new { controller = "Home", action = "Index", month = UrlParameter.Optional, year = UrlParameter.Optional, category = UrlParameter.Optional });

            //routes.MapRoute(
            //    name: "Category",
            //    url: "{controller}/{action}/{category}",
            //    defaults: new { controller = "Home", action = "IndexCat", category = UrlParameter.Optional });
        }
    }
}
