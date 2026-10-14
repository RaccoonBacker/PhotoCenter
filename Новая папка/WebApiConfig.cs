using System.Web.Http;
using PhotoCenterWeb.Helpers;

namespace PhotoCenterWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    id = RouteParameter.Optional
                }
            );

            config.MessageHandlers.Add(
                new JwtAuthenticationHandler()
            );

            config.Formatters.JsonFormatter.SerializerSettings
                .ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }
}