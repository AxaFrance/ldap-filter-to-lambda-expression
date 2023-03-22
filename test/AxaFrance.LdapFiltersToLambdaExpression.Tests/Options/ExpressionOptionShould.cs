using AxaFrance.LdapFiltersToLambdaExpression.Options;
using AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;
using FluentAssertions;

namespace AxaFrance.LdapFiltersToLambdaExpression.Tests.Options;

[TestFixture]
public class ExpressionOptionShould
{
    [Test]
    public void ReturnDefaultApproximateImplementationWhenCallGetWithoutOverride()
    {
        var instanceApproximate = ExpressionOption.Instance.Approximate;

        instanceApproximate.Should().BeOfType<DefaultApproximate>();
    }

    [Test]
    public void ReturnDefaultLikeImplementationWhenCallGetWithoutOverride()
    {
        var instanceLike = ExpressionOption.Instance.Like;

        instanceLike.Should().BeOfType<DefaultLike>();
    }

    [Test]
    public void AddNamespaceToOptionsWhenCallAddNamespaces()
    {
        var expectedNamespaces = new[]
        {
            $"{nameof(System)}",
            $"{nameof(System)}.{nameof(System.Linq)}.{nameof(System.Linq.Expressions)}",
            $"{nameof(Microsoft)}.{nameof(Microsoft.EntityFrameworkCore)}"
        };

        var namespaces = ExpressionOption.Instance
            .AddNamespaces($"{nameof(Microsoft)}.{nameof(Microsoft.EntityFrameworkCore)}").Namespaces;

        namespaces.Should().OnlyContain(@namespace => expectedNamespaces.Contains(@namespace));
    }
    
    [Test]
    public void CaseInsensitiveDbActiveEqualToTrueWhenCallCaseInsensitiveDbIsActive()
    {
        var caseInsensitiveDbActive = ExpressionOption.Instance.CaseInsensitiveDbIsActive().CaseInsensitiveDbActive;

        caseInsensitiveDbActive.Should().BeTrue();
    }
}
