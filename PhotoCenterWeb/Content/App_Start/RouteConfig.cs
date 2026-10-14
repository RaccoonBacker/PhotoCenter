using System.Web.Mvc;
using System.Web.Routing;

namespace PhotoCenterWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "React",
                url: "{*url}",
                defaults: new
                {
                    controller = "React",
                    action = "Index"
                }
            );
        }
    }
}