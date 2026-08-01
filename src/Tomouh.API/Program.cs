using Scalar.AspNetCore;
using Tomouh.API;
using Tomouh.Application;
using Tomouh.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddInfrastructure(builder.Configuration)
    .AddApplication(builder.Configuration)
    .AddPresentation(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 1. توليد ملف الـ JSON المعتاد من Swashbuckle
    app.UseSwagger();

    // 2. تشغيل واجهة Swagger UI التقليدية على المسار /swagger
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tomouh API v1");
    });

    // 3. تشغيل واجهة Scalar الحديثة على المسار /scalar/v1
    app.MapScalarApiReference(options =>
    {
        // بنربط Scalar بمسار الـ JSON اللي بيتولّد من SwaggerGen فوق
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}
app.MapOpenApi();

app.UseHttpsRedirection();

app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();

app.MapControllers();


app.Run();

