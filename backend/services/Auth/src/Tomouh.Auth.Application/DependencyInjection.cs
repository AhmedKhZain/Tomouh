using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tomouh.Shared.Kernel.CommonBehaviors;
using Tomouh.Shared.Kernel.Extensions;
using Tomouh.Shared.Kernel.Requests;

namespace Tomouh.Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Registered first so it runs as the outermost pipeline behavior,
        // rejecting unauthenticated/unauthorized requests before any work begins.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EventsHandlerBehavior<,>));

        services.AddValidatorsFromAssembly(assembly);

        services.AddTripleGenericBehavior(assembly, typeof(CachingBehavior<,,>), typeof(ICacheableRequest));

        services.AddTripleGenericBehavior(assembly, typeof(IdempotencyBehavior<,,>), typeof(IIdempotentRequest));


        return services;
    }
}
