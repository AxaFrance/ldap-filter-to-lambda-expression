using AxaFrance.LdapFiltersToLambdaExpression.Internal;
using AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;

namespace AxaFrance.LdapFiltersToLambdaExpression.Options;

public sealed class ExpressionOption
{
    internal static readonly ResettableLazy<ExpressionOption> lazy = new(() => new ExpressionOption());

    private ExpressionOption()
    {
        this.LikeFunction(new DefaultLike())
            .ApproximateFunction(new DefaultApproximate())
            .AddNamespaces($"{nameof(System)}",
                $"{nameof(System)}.{nameof(System.Linq)}.{nameof(System.Linq.Expressions)}");
    }

    internal string[] Namespaces { get; private set; } = Array.Empty<string>();

    internal static ExpressionOption Instance => lazy.Value;

    internal IFunction Like { get; private set; }

    internal IFunction Approximate { get; private set; }

    public bool CaseInsensitiveDbActive { get; private set; }

    public ExpressionOption LikeFunction(IFunction likeFunction)
    {
        this.Like = likeFunction;
        return this;
    }

    public ExpressionOption ApproximateFunction(IFunction approximateFunction)
    {
        this.Approximate = approximateFunction;
        return this;
    }

    public ExpressionOption CaseInsensitiveDbIsActive()
    {
        this.CaseInsensitiveDbActive = true;
        return this;
    }

    public ExpressionOption AddNamespaces(params string[] namespaces)
    {
        this.Namespaces = this.Namespaces.Concat(namespaces).ToArray();
        return this;
    }
}
