using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tomouh.Application.Common.Behavior;

namespace Tomouh.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(msc =>
        {
            msc.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection));
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EventsHandlerBehavior<,>));

        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        ////services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(CacheingBehavior<,,>));
        //services.AddCachingBehavior();



        return services;
    }
}
