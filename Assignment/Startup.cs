using Assignment.Provider;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

[assembly: OwinStartup(typeof(Assignment.Startup))]

namespace Assignment
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // JWT Configuration
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["JwtAudience"];
            var secret = ConfigurationManager.AppSettings["JwtKey"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            };

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                TokenValidationParameters = tokenValidationParameters
            });

            // OAuth Configuration
            OAuthAuthorizationServerOptions oAuthOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"), // https://localhost:44384/token
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                Provider = new AppAuthorizationServerProvider()
            };

            app.UseOAuthAuthorizationServer(oAuthOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }
    }
}
