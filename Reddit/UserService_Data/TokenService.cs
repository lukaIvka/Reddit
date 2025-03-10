using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UserService_Data
{
    public class TokenService
    {
        private readonly string _securityKey;

        public TokenService(string securityKey)
        {
            _securityKey = securityKey;
        }

        public TokenService()
        {

        }

        public string GenerateJwtToken(User user)
        {
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes("long_secret_key_with_at_least_32_bytes"));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Ime),
            new Claim(ClaimTypes.Email, user.Email),
            
           
            };

            var token = new JwtSecurityToken(
                issuer: "reddit",
                audience: "client",
                claims: claims,
                expires: DateTime.Now.AddHours(1), 
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public bool ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "reddit", // Postavi ovo na svoj Issuer (izdavač)
                ValidAudience = "client", // Postavi ovo na svoju Audience (publiku)
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("long_secret_key_with_at_least_32_bytes"))
            };

            try
            {
                SecurityToken validatedToken;
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return true;
            }
            catch
            {
                // Token nije validan
                return false;
            }
        }


        public string GetUsernameFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

                if (usernameClaim != null && emailClaim != null)
                {
                    string username = usernameClaim.Value;
                    return username;
                }
                else
                {
                    Console.WriteLine("Error: Required claims not found in token.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Greška prilikom dekodiranja tokena
                Console.WriteLine("Error decoding token: " + ex.Message + ex.StackTrace);
                return null;
            }
        }

        public string GetEmailFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

                if (usernameClaim != null && emailClaim != null)
                {
                    string email = emailClaim.Value;
                    return email;
                }
                else
                {
                    Console.WriteLine("Error: Required claims not found in token.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Greška prilikom dekodiranja tokena
                Console.WriteLine("Error decoding token: " + ex.Message + ex.StackTrace);
                return null;
            }
        }

    }
}
