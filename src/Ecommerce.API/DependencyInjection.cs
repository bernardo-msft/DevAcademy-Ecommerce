using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Ecommerce.Application.Interfaces.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Ecommerce.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddOpenApi();

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret key is not configured.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true, // Ensures token is not expired by default
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var tokenBlacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                    // Get logger instance for this event
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

                    if (string.IsNullOrEmpty(jti))
                    {
                        logger.LogWarning("JTI claim is missing from the token. Cannot check blacklist status.");
                        // Depending on policy, you might fail here: context.Fail("JTI claim is missing.");
                        return;
                    }

                    try
                    {
                        if (await tokenBlacklistService.IsTokenBlacklistedAsync(jti))
                        {
                            logger.LogInformation("Token with JTI {Jti} is blacklisted. Failing authentication.", jti);
                            context.Fail("This token has been blacklisted.");
                            return;
                        }
                        logger.LogDebug("Token with JTI {Jti} is not blacklisted.", jti);
                    }
                    catch (Exception ex) // Catching a broader exception here in case IsTokenBlacklistedAsync itself throws unexpectedly despite its internal try-catch
                    {
                        logger.LogError(ex, "An error occurred while checking token blacklist status for JTI {Jti}. Allowing token (Fail-Open).", jti);
                        // Fail-Open: If any error occurs (even beyond RedisException if IsTokenBlacklistedAsync's catch fails), allow the token.
                        // The internal try-catch in IsTokenBlacklistedAsync should handle RedisExceptions and return false.
                    }
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User", "Admin"));
        });

        return services;
    }
}