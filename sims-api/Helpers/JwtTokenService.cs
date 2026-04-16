using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using sims.Models;

namespace sims.Helpers
{
    public static class JwtTokenService
    {
        public static string GenerateToken(User user, IConfiguration config)
        {
            //Old usage of Jwt Key from "plain" Docker compose
            // var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
            byte[] keyBytes;
            const string dockerSecretPath = "/run/secrets/Jwt__Key";
            if (File.Exists(dockerSecretPath))
            {
                keyBytes = File.ReadAllBytes(dockerSecretPath);
            }
            else
            {
               
                var keyString = config["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(keyString))
                    throw new Exception("JWT key not configured.");

                keyBytes = System.Text.Encoding.UTF8.GetBytes(keyString);
            }
            //Vulnerability : "Secret Handling"
            //Here the inserted secret of the docker compose was previously inserted. now it gets inserted from the Docker secret. (see dockerSecretPath)
           // var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
           var signingKey = new SymmetricSecurityKey(keyBytes);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(
                    signingKey, 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}