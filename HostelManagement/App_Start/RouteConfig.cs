using System.Web.Mvc;
using System.Web.Routing;

namespace HostelManagement
{
    /// <summary>
    /// Class to configure the route
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Method to register the routes
        /// </summary>
        /// <param name="routes">the route collection</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                 name: "Default",
                 url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "HostelManagement.Controllers" }
            );
        }
    }
}
