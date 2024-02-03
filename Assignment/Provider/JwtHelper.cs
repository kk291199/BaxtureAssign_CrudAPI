using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Assignment.Provider
{
    public static class JwtHelper
    {
        private static byte[] SecretKeyBytes => Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["JwtKey"]);

        public static string GenerateToken(string username, string[] roles)
        {
            if (SecretKeyBytes.Length < 32) // 32 bytes = 256 bits
            {
                // Handle error, log, or throw an exception indicating an insufficient key length.
                throw new ApplicationException("JWT key length is insufficient. It must be at least 256 bits.");
            }

            var securityKey = new SymmetricSecurityKey(SecretKeyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                        issuer: ConfigurationManager.AppSettings["JwtIssuer"],
                        audience: ConfigurationManager.AppSettings["JwtAudience"],
                        claims: roles.Select(role => new Claim(ClaimTypes.Role, role.Trim()))
                            .Append(new Claim(ClaimTypes.Name, username)), // Add other claims if needed
                        expires: DateTime.UtcNow.AddMinutes(1),
                        signingCredentials: credentials
                        );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
       
    }

}