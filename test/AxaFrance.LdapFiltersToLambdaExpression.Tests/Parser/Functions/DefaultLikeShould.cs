using AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;
using FluentAssertions;

namespace AxaFrance.LdapFiltersToLambdaExpression.Tests.Parser.Functions;

[TestFixture]
public class DefaultLikeShould
{
    [TestCase(null)]
    [TestCase("")]
    public void ReturnFalseWhenMatchExpressionIsNullOrEmpty(string matchExpression)
    {
        var actualValue = DefaultLike.Do(matchExpression, "whatever");

        actualValue.Should().BeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    public void ReturnFalseWhenPatternIsNullOrEmpty(string pattern)
    {
        var actualValue = DefaultLike.Do("whatever", pattern);

        actualValue.Should().BeFalse();
    }

    [Test]
    public void ReturnTrueWhenMatchExpressionIsEqualToPattern()
    {
        var actualValue = DefaultLike.Do("equal", "equal");

        actualValue.Should().BeTrue();
    }

    [TestCase("Vandenbussche", "%andenbussche", true)]
    [TestCase("boulanger", "%andenbussche", false)]
    [TestCase("vandenbussche Julien", "Vandenbussche_", false)]
    [TestCase("vandenbussche julien", "Vandenbussche$", false)]
    public void ReturnREsultOfMatchingPatternWhenCallDo(string matchExpression, string pattern, bool expectedResult)
    {
        var actualValue = DefaultLike.Do(matchExpression, pattern);

        actualValue.Should().Be(expectedResult);
    }

    [Test]
    public void DeclaringTypeIsEqualToDefaultLike()
    {
        var approximate = new DefaultLike();

        approximate.DeclaringType.Should().Be(typeof(DefaultLike));
    }

    [Test]
    public void MethodInfoIsEqualToDoMethodOfDefaultLike()
    {
        var approximate = new DefaultLike();
        var expectedMethodInfo =
            approximate.DeclaringType.GetMethod(nameof(DefaultLike.Do), new[] { typeof(string), typeof(string) })!;

        approximate.MethodInfo.Should().BeSameAs(expectedMethodInfo);
    }

    [Test]
    public void ExpressionPatternIsEqualToPathOfDoMethodOfDefaultLike()
    {
        var approximate = new DefaultLike();
        var expectedExpressionPattern = $"{typeof(DefaultLike).FullName}.{approximate.MethodInfo.Name}({{0}}, {{1}})";

        approximate.ExpressionPattern.Should().Be(expectedExpressionPattern);
    }
}
