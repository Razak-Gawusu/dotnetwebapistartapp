using System.Text;
using Innout.Persistence.Database;
using Innout.Services.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Innout.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register application services here, e.g.:
        // services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddScoped<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(configuration[DbConstants.DefaultConnectionStringPath]!));

        return services;
    }

    public static IServiceCollection AddRedis(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = configuration[DbConstants.RedisDefaultConnectionStringPath];
            option.InstanceName = configuration["AppName"];
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
                        ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.")))
                };
            });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Innout",
                Version = "v1",
                Description = "API for managing resources"
            });
            AddJwtSwaggerSecurity(options);
        });

        return services;
    }

    public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services)
    {
        return services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });
    }

    private static void AddJwtSwaggerSecurity(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    }
}