using System.Text;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Web.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ChangeMe.Backend.Web.Configurations;

public static class AuthConfig
{
  public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, WebApplicationBuilder builder)
  {
    services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

    var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
    var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey = true,
          ValidateLifetime = true,
          ValidIssuer = jwtOptions.Issuer,
          ValidAudience = jwtOptions.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(signingKey),
          ClockSkew = TimeSpan.Zero,
          NameClaimType = JwtRegisteredClaimNames.Sub
        };

        options.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs"))
              context.Token = accessToken;

            return Task.CompletedTask;
          }
        };
      });

    services.AddPermissionAuthorization();

    return services;
  }
}
