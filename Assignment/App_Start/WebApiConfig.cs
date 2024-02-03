using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Web.Http.Cors;
using System.Configuration;


namespace Assignment
{
    public static class WebApiConfig
    {  
        public static void Register(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute("*", "*", "*"); // You can customize these values
            config.EnableCors(cors);

            // Web API configuration and services
            config.MessageHandlers.Add(new ExceptionHandlingMiddleware());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/users/getuser/{id}",
                defaults: new { controller = "Users", action = "GetUser" }
            );
        }
    }
}
