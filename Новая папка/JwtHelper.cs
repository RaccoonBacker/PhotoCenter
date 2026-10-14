using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PhotoCenterWeb.Helpers
{
    public static class JwtHelper
    {
        private const string SecretKey =
            "PhotoCenterSecretKey_2026_MISPIS_JWT_ChangeThisKey";

        private const int ExpirationMinutes = 120;

        public static string GenerateToken(
            int userId,
            string login,
            string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(SecretKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    login
                ),

                new Claim(
                    ClaimTypes.Role,
                    role
                )
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    ExpirationMinutes
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(SecretKey)
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = key,

                    ValidateIssuer = false,

                    ValidateAudience = false,

                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.FromMinutes(1)
                };

            try
            {
                SecurityToken validatedToken;

                var principal = tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out validatedToken
                );

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}