using Asp.Versioning;
using Common.DataConvrters;
using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using Tomouh.API.Filters;
using Tomouh.API.Services;

namespace Tomouh.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<CurrentUser>(sp => sp.GetRequiredService<ICurrentUserProvider>().GetCurrentUser());

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", policy =>
            {
                policy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });


        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }
        ).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new NonFlagsEnumConverterFactory());
            });

        services.AddEndpointsApiExplorer();

        // نسجل الفلتر بتاعك عادي
        services.AddScoped<IdempotencyHeaderFilter>();

        // إعدادات SwaggerGen الأصلية بتاعتك
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Tomouh API",
                Version = "v1"
            });

            options.OperationFilter<IdempotencyHeaderFilter>();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token"
            });

            options.AddSecurityRequirement(document =>
            {
                var requirement = new OpenApiSecurityRequirement();
                requirement.Add(
                    new OpenApiSecuritySchemeReference("Bearer", document, null),
                    new List<string>());
                return requirement;
            });
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });



        services.AddOpenApi();

        return services;
    }

}