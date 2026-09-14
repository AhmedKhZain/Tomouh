using Scalar.AspNetCore;
using Tomouh.Auth.Application;
using Tomouh.Auth.Infrastructure;
using Tomouh.Auth.Infrastructure.Persistence;

namespace Tomouh.Auth.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);



        builder.Services.AddAuthApplication()
            .AddAuthInfrastructure(builder.Configuration)
            .AddAuthPresentation(builder.Configuration);



        var app = builder.Build();

        await app.UseMongoInitializationAsync();


        if (app.Environment.IsDevelopment())
        {
        }

        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tomouh API v1");
        });

        app.MapScalarApiReference(options =>
        {
            options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
        });

        app.MapOpenApi();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
