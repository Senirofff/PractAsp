using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using АСП3курс.Models;
using АСП3курс.Repositories;
using АСП3курс.Services;

namespace АСП3курс.Security;

public sealed record JwtSettings(string Issuer, string Audience, SymmetricSecurityKey Key);

public static class ApiSecurity
{
    public static void AddApiSecurity(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var secret = config["Jwt:SigningKey"];
        if (secret is null && builder.Environment.IsDevelopment())
            secret = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

        if (secret is null || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("Configure Jwt:SigningKey with at least 32 bytes.");

        var jwt = new JwtSettings(
            config["Jwt:Issuer"] ?? throw new InvalidOperationException("Configure Jwt:Issuer."),
            config["Jwt:Audience"] ?? throw new InvalidOperationException("Configure Jwt:Audience."),
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)));

        var services = builder.Services;
        services.AddSingleton(jwt);
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<AuthService>();
        services.AddSingleton<IAuthorizationHandler, ProfileOwnerHandler>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = jwt.Key,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                RoleClaimType = "role",
                NameClaimType = "sub",
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal!;
                    var users = context.HttpContext.RequestServices.GetRequiredService<IRepository<User>>();
                    var role = principal.FindFirst("role")?.Value;
                    if (!int.TryParse(principal.FindFirst("sub")?.Value, out var id) ||
                        !await users.Query().AnyAsync(
                            u => u.IdUser == id && u.IsActive && u.IdRoleNavigation.Title == role,
                            context.HttpContext.RequestAborted))
                    {
                        context.Fail("Account is inactive, missing or has changed role.");
                    }
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Access.Admin, policy => policy.RequireRole(Access.Admin));
            options.AddPolicy(Access.Profile, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new ProfileOwnerRequirement()));
        });

        var origin = config["Cors:FrontendOrigin"];
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var frontendUri) ||
            frontendUri.Scheme is not ("http" or "https") ||
            frontendUri.GetLeftPart(UriPartial.Authority) != origin)
        {
            throw new InvalidOperationException("Configure Cors:FrontendOrigin as one HTTP(S) origin without a trailing slash.");
        }
        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy.WithOrigins(origin).AllowAnyMethod().AllowAnyHeader()
                .WithExposedHeaders("Retry-After", "X-Total-Count")));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(Access.LoginLimit, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
            options.OnRejected = async (context, ct) =>
            {
                var response = context.HttpContext.Response;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var delay))
                    response.Headers.RetryAfter = Math.Ceiling(delay.TotalSeconds)
                        .ToString(System.Globalization.CultureInfo.InvariantCulture);

                await response.WriteAsJsonAsync(new { message = "Слишком много попыток входа." }, ct);
            };
        });

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.OperationFilter<BearerOperationFilter>();
        });
    }
}
