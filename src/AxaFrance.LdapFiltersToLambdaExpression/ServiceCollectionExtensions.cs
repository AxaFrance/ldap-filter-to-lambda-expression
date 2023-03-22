using AxaFrance.LdapFiltersToLambdaExpression.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection LdapFiltersToLambdaConfigure(this IServiceCollection services,
        Action<ExpressionOption> configure)
    {
        configure(ExpressionOption.Instance);
        return services;
    }
}
