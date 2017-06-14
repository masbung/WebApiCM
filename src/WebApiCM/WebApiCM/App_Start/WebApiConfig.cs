using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AzureWebApi2FromScratch
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config) {
            // Web API configuration and services
            //config.Formatters.JsonFormatter.SupportedMediaTypes
            //    .Add(new MediaTypeHeaderValue("text/html"));
            ////or you can choose to remove all formatters but Json:
            //GlobalConfiguration.Configuration.Formatters.Clear();
            //GlobalConfiguration.Configuration.Formatters.Add(new JsonMediaTypeFormatter());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}