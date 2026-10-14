using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PhotoCenterWeb.Helpers
{
    public static class JwtHelper
    {
        private static readonly string SecretKey;
        private static readonly int ExpirationMinutes;

        static JwtHelper()
        {
            SecretKey = ResolveSecretKey();
            ExpirationMinutes = ResolveExpirationMinutes();

            var keyBytes = Encoding.UTF8.GetBytes(SecretKey);

            if (keyBytes.Length < 32)
            {
                throw new ConfigurationErrorsException(
                    "JwtSecretKey слишком короткий. HMAC-SHA256 требует минимум 32 байта."
                );
            }
        }

        private static string ResolveSecretKey()
        {
            // 1. Приоритет: переменная окружения
            var fromEnv = Environment.GetEnvironmentVariable(
                "PHOTOCENTER_JWT_SECRET"
            );

            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv;
            }

            // 2. Fallback: appSettings
            var fromConfig = ConfigurationManager
                .AppSettings["JwtSecretKey"];

            if (!string.IsNullOrWhiteSpace(fromConfig) &&
                fromConfig != "CHANGE_ME")
            {
                return fromConfig;
            }

            throw new ConfigurationErrorsException(
                "JwtSecretKey не задан. Задайте переменную окружения " +
                "PHOTOCENTER_JWT_SECRET или appSettings/JwtSecretKey в Web.config."
            );
        }

        private static int ResolveExpirationMinutes()
        {
            var raw = ConfigurationManager
                .AppSettings["JwtExpirationMinutes"];

            if (int.TryParse(raw, out var minutes) && minutes > 0)
            {
                return minutes;
            }

            return 120;
        }

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