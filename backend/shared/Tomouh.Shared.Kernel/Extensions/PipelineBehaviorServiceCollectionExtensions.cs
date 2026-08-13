using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Shared.Kernel.Extensions;

public static class PipelineBehaviorServiceCollectionExtensions
{
    /// <summary>
    /// Scans the target assembly for all requests returning <see cref="ResultOf{T}"/> 
    /// and registers the specified open generic behavior with 3 generic parameters.
    /// </summary>
    /// <param name="services">The service collection instance.</param>
    /// <param name="targetAssembly">The assembly containing requests to scan.</param>
    /// <param name="behaviorOpenGenericType">The open generic behavior type (e.g., typeof(CachingBehavior<,,>)).</param>

    /// <param name="requestConstraintType">Optional interface constraint that the request must implement (e.g., typeof(ICacheableRequest)).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddTripleGenericBehavior(
        this IServiceCollection services,
        Assembly targetAssembly,
        Type behaviorOpenGenericType,
        Type? requestConstraintType = null)
    {
        ArgumentNullException.ThrowIfNull(targetAssembly);
        ArgumentNullException.ThrowIfNull(behaviorOpenGenericType);

        if (!behaviorOpenGenericType.IsGenericTypeDefinition || behaviorOpenGenericType.GetGenericArguments().Length != 3)
        {
            throw new ArgumentException("The behavior type must be an open generic with exactly 3 generic arguments.", nameof(behaviorOpenGenericType));
        }

        var candidateTypes = targetAssembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => requestConstraintType is null || requestConstraintType.IsAssignableFrom(t))
            .Select(t => new
            {
                QueryType = t,
                RequestInterface = t.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
            })
            .Where(x => x.RequestInterface != null)
            .ToList();

        foreach (var item in candidateTypes)
        {
            var requestType = item.QueryType;
            var responseType = item.RequestInterface!.GetGenericArguments()[0];

            // Inspect if Response inherits from or implements ResultOf<TValue>
            Type? baseType = responseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ResultOf<>))
                {
                    var tValue = baseType.GetGenericArguments()[0];

                    var closedBehaviorType = behaviorOpenGenericType.MakeGenericType(requestType, responseType, tValue);
                    var closedPipelineInterface = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

                    services.AddTransient(closedPipelineInterface, closedBehaviorType);
                    break;
                }
                baseType = baseType.BaseType;
            }
        }

        return services;
    }
}
