using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Assignment.Provider
{
    public class JwtTokenValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var token = GetTokenFromRequestHeader(actionContext.Request);

            // Validate token using JwtSecurityTokenHandler
            if (string.IsNullOrEmpty(token) || !ValidateToken(token))
            {
                // Handle the case where the token is invalid
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }

            // Inspect roles
            var handler = new JwtSecurityTokenHandler();
            var claimsPrincipal = handler.ValidateToken(token, GetTokenValidationParameters(), out _);

            var roles = claimsPrincipal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            // Log or debug print roles

            base.OnActionExecuting(actionContext);
        }

        private string GetTokenFromRequestHeader(HttpRequestMessage request)
        {
            // Check if the token is in the request header
            var headerToken = request.Headers.Authorization?.Parameter;
            if (!string.IsNullOrEmpty(headerToken))
            {
                return headerToken;
            }

            return null;
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = ConfigurationManager.AppSettings["JwtIssuer"],
                ValidAudience = ConfigurationManager.AppSettings["JwtAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["JwtKey"]))
            };
        }

        private bool ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;
                handler.ValidateToken(token, GetTokenValidationParameters(), out validatedToken);

                // Token is valid
                return true;
            }
            catch (SecurityTokenException)
            {
                // Token validation failed
                return false;
            }
        }
    }


}