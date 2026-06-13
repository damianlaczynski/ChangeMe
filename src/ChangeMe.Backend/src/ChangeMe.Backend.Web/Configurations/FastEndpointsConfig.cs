using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag;

namespace ChangeMe.Backend.Web.Configurations;

public static class FastEndpointsConfig
{
  public static IServiceCollection AddFastEndpointsWithSwagger(this IServiceCollection services)
  {
    services.ConfigureHttpJsonOptions(options =>
    {
      options.SerializerOptions.PropertyNameCaseInsensitive = true;
      options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddFastEndpoints()
            .SwaggerDocument(o =>
            {
              o.ShortSchemaNames = true;
              o.EnableJWTBearerAuth = false;
              o.DocumentSettings = settings =>
                  {
                    settings.AddAuth("Bearer", new OpenApiSecurityScheme
                    {
                      Type = OpenApiSecuritySchemeType.Http,
                      Scheme = JwtBearerDefaults.AuthenticationScheme,
                      BearerFormat = "JWT",
                    });
                  };
            });
    return services;
  }

  public static WebApplication UseFastEndpointsWithSwagger(this WebApplication app)
  {
    app.UseFastEndpoints(config =>
    {
      config.Endpoints.RoutePrefix = "api";
      config.Serializer.Options.PropertyNameCaseInsensitive = true;
      config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    }).UseSwaggerGen();
    return app;
  }
}
