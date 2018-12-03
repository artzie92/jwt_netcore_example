using JWTExample.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTExample.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserIdentity userIdentity;
        private readonly JWTConfiguration configuration;

        public AuthController(UserIdentity userIdentity, JWTConfiguration configuration)
        {
            this.userIdentity = userIdentity;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("token")]
        public AccessToken GenerateToken([FromBody]UserIdentity credentials)
        {
            string md5Password = string.Empty;
            using (MD5 md5Hash = MD5.Create())
            {
                md5Password = GetMd5Hash(md5Hash, credentials.Password);
            }

            if (userIdentity.Password != md5Password || userIdentity.Login != credentials.Login)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new AccessToken { Success = false };
            }

            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, credentials.Login),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiredOn = DateTime.Now.AddMinutes(configuration.TokenExpirationTime);
            var token = new JwtSecurityToken(configuration.ValidIssuer,
                  configuration.ValidAudience,
                  claims,
                  expires: expiredOn,
                  signingCredentials: creds);

            return new AccessToken
            {
                ExpireOnDate = token.ValidTo,
                Success = true,
                ExpiryIn = configuration.TokenExpirationTime,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}