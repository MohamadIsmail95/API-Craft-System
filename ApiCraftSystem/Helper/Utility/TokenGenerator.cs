using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiCraftSystem.Helper.Utility
{
    public static class TokenGenerator
    {
        public static string GenerateFakeToken(string? userId, string? userName, string? role, string? tenant)
        {



            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKey123!YourSuperSecretKey123YourSuperSecretKey123")); // use a long random key
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                       new Claim(JwtRegisteredClaimNames.Sub,userId),         // User ID
                       new Claim(JwtRegisteredClaimNames.Email, userName),
                       new Claim("role",string.IsNullOrEmpty(role) ? "" :role),
                       new Claim("tenant",string.IsNullOrEmpty(tenant) ? "" :  tenant)
                 };

            var token = new JwtSecurityToken(
                issuer: "MyApp",
                audience: "MyAppClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);



        }
    }
}
