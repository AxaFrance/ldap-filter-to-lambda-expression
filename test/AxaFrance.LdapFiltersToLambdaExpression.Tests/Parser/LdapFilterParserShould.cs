using AxaFrance.LdapFiltersToLambdaExpression.Options;
using FluentAssertions;

namespace AxaFrance.LdapFiltersToLambdaExpression.Tests.Parser;

[TestFixture]
public class LdapFilterParserShould
{
    [SetUp]
    public void BeforeEach()
    {
        ExpressionOption.lazy.Reset();
    }
    
    private static IQueryable<Model> Populate(params Model[] items)
    {
        var data = new List<Model>(items);
        return data.AsQueryable();
    }

    [TestCase("(WhenChanged=*")]
    [TestCase("WhenChanged=*)")]
    [TestCase("WhenChanged=*")]
    public async Task ThrownArgumentExceptionWhenFilterNotHaveStartByOpeningAndEndByClosingParenthesis(string filter)
    {
        var queryable = Populate(new Model { WhenChanged = DateTime.Now.ToString("yyyyMMddHHmmss.0Z") });
        Func<Task> act = () => queryable.ByLdapFilterAsync(filter);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("le filter doit commencer par '(' et finir par ')'.");
    }

    [TestCase("(&(civility=*)(!(WhenChanged=*))")]
    public async Task ThrownArgumentExceptionWhenFilterNotHaveSameNumberOfOpeningParenthesisAndClosingParenthesis(
        string filter)
    {
        var queryable =
            Populate(new Model { WhenChanged = DateTime.Now.ToString("yyyyMMddHHmmss.0Z"), Civility = "Mr" });
        Func<Task> act = () => queryable.ByLdapFilterAsync(filter);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("le filter doit avoir le même nombres de '(' et de ')'.");
    }

    [TestCase("(civility=)")]
    [TestCase("(|(civility=*)(WhenChanged=))")]
    public async Task ThrownArgumentExceptionWhenFilterIsNotValid(string filter)
    {
        var queryable =
            Populate(new Model { WhenChanged = DateTime.Now.ToString("yyyyMMddHHmmss.0Z"), Civility = "Mr" });
        Func<Task> act = () => queryable.ByLdapFilterAsync(filter);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"le format de filter: [{filter}] n'est pas correct.");
    }

    [Test]
    public async Task ThrownArgumentExceptionWhenFilterFormatIsCorrupted()
    {
        var queryable =
            Populate(new Model { WhenChanged = DateTime.Now.ToString("yyyyMMddHHmmss.0Z"), Civility = "Mr" });
        Func<Task> act = () =>
            queryable.ByLdapFilterAsync(
                "(&(WhenChanged=*)(|(sn=Vandenbussche)(|(&(Sn=McClane)(Version=5)(Givenname=John)))))");
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Erreur de format du filtre.");
    }

    [Test]
    public async Task ApplyNotNullConditionWhenFilterIsEqualStarSymbol()
    {
        var expectedValue = new Model { Civility = "not null" };
        var queryable = Populate(expectedValue, new Model { Civility = null });

        var actualValue = await queryable.ByLdapFilterAsync("(civility=*)", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyNullConditionWhenFilterIsEqualStarAndContainsWithExclamationSymbol()
    {
        var expectedValue = new Model { Civility = null };
        var queryable = Populate(expectedValue, new Model { Civility = "not null" });

        var actualValue = await queryable.ByLdapFilterAsync("(!(Civility=*))", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyStartWithConditionWhenFilterTerminateByStarSymbol()
    {
        var expectedValue = new Model { Job = "Dirigeant Entreprise" };
        var queryable = Populate(expectedValue, new Model { Job = "Developer" });

        var actualValue = await queryable.ByLdapFilterAsync("(job=dirige*)", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyEndWithConditionWhenFilterBeginByStarSymbol()
    {
        var expectedValue = new Model { Job = "Developer" };
        var queryable = Populate(expectedValue, new Model { Job = "Dirigeant Courtage" });

        var actualValue = await queryable.ByLdapFilterAsync("(job=*per)", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyLikeConditionWhenFilterContainsManyStarSymbol()
    {
        var expectedValues = new[]
        {
            new Model
            {
                Sn = "Vandenbussche"
            },
            new Model
            {
                Sn = "Vendenbuche"
            }
        };
        var queyrable = Populate(expectedValues);

        var values = await queyrable.ByLdapFilterAsync("(sn=v*ndenbu*che)", Queryable.Where);

        values.Should().BeEquivalentTo(expectedValues);
    }

    [Test]
    public async Task ApplyApproximateConditionWhenFilterContainsManyApproximateSymbol()
    {
        var expectedValues = new[]
        {
            new Model
            {
                Sn = "vandenbussch"
            },
            new Model
            {
                Sn = "vandenbuch"
            }
        };
        var queyrable = Populate(expectedValues);

        var values = await queyrable.ByLdapFilterAsync("(sn~=vandenbussche)", Queryable.Where);

        values.Should().BeEquivalentTo(expectedValues);
    }

    [Test]
    public async Task ApplyNotEqualConditionWhenFilterContainsOnlyEqualSymbolAndExclamationSymbol()
    {
        var expectedValue = new Model { Sn = "Vandenbusche" };
        var queryable = Populate(expectedValue, new Model { Sn = "Vandenbussche" });

        var actualValue = await queryable.ByLdapFilterAsync("(!(sn=vandenbussche))", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyGreaterThanComparisonWhenFilterContainsGreaterSymbol()
    {
        var expectedValue = new Model { Version = "6" };
        var queryable = Populate(expectedValue, new Model { Version = "5" });

        var actualValue = await queryable.ByLdapFilterAsync("(Version>5)", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyLowerOrEqualComparisonWhenFilterContainsNotGreaterSymbol()
    {
        var expectedValue = new Model { Version = "5" };

        var queryable = Populate(expectedValue, new Model { Version = "6" });

        var actualValue = await queryable.ByLdapFilterAsync("(!(Version>5))", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyGreaterOrEqualThanComparisonWhenFilterContainsGreaterOrEqualSymbol()
    {
        var expectedValues = new[]
        {
            new Model
            {
                Version = "6"
            },
            new Model
            {
                Version = "5"
            }
        };
        var queryable = Populate(expectedValues);

        var actualValues = await queryable.ByLdapFilterAsync("(Version>=5)", Queryable.Where);

        actualValues.Should().BeEquivalentTo(expectedValues);
    }

    [Test]
    public async Task ApplyLowerThanComparisonWhenFilterContainsNotGreaterOrEqualSymbol()
    {
        var expectedValue = new Model { Version = "4" };
        var queryable = Populate(expectedValue, new Model { Version = "5" });

        var actualValues = await queryable.ByLdapFilterAsync("(!(Version>=5))", Queryable.Single);

        actualValues.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyLowerThanComparisonWhenFilterContainsLowerSymbol()
    {
        var expectedValue = new Model { Version = "4" };
        var queryable = Populate(expectedValue, new Model { Version = "5" });

        var actualValue = await queryable.ByLdapFilterAsync("(Version<5)", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyGreaterOrEqualThanComparisonWhenFilterContainsNotLowerSymbol()
    {
        var expectedValue = new Model { Version = "5" };
        var queryable = Populate(expectedValue, new Model { Version = "4" });

        var actualValue = await queryable.ByLdapFilterAsync("(!(Version<5))", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyLowerOrEqualThanComparisonWhenFilterContainsLowerOrEqualSymbol()
    {
        var expectedValues = new[]
        {
            new Model
            {
                Version = "4"
            },
            new Model
            {
                Version = "5"
            }
        };
        var queryable = Populate(expectedValues);

        var actualValues = await queryable.ByLdapFilterAsync("(Version<=5)", Queryable.Where);

        actualValues.Should().BeEquivalentTo(expectedValues);
    }

    [Test]
    public async Task ApplyGreaterThanComparisonWhenFilterContainsNotLowerOrEqualSymbol()
    {
        var expectedValue = new Model { Version = "6" };

        var queryable = Populate(expectedValue, new Model { Version = "5" });

        var actualValue = await queryable.ByLdapFilterAsync("(!(Version<=5))", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyAndConditionWhenFilterContainsAndSymbol()
    {
        var expectedValue = new Model { Version = "6", Sn = "Vandenbussche" };
        var queryable = Populate(expectedValue, new Model { Version = "6", Sn = "Other" });

        var actualValue = await queryable.ByLdapFilterAsync("(&(Version=6)(sn=vandenbussche))", Queryable.Single);

        actualValue.Should().Be(expectedValue);
    }

    [Test]
    public async Task ApplyOrConditionWhenFilterContainsOrSymbol()
    {
        var expectedValues = new[]
        {
            new Model
            {
                Version = "6",
                Sn = "Vandenbussche"
            },
            new Model
            {
                Version = "5",
                Sn = "Other"
            }
        };
        var queryable = Populate(expectedValues);

        var actualValues = await queryable.ByLdapFilterAsync("(|(Version=6)(sn=other))", Queryable.Where);

        actualValues.Should().BeEquivalentTo(expectedValues);
    }

    [Test]
    public async Task ApplyAnd_OrConditionWhenFilterContainsAnd_OrSymbol()
    {
        var now = DateTime.Now.ToString("yyyyMMddHHmmss.0Z");
        var expectedValues = new[]
        {
            new Model
            {
                Sn = "Vandenbussche",
                GivenName = "Julien",
                WhenChanged = now
            },
            new Model
            {
                Sn = "Vandenbussche",
                GivenName = "Aaron",
                WhenChanged = now
            },
            new Model
            {
                Version = "5",
                Sn = "McClane",
                GivenName = "John",
                WhenChanged = now
            },
            new Model
            {
                Version = "6",
                Sn = "Doe",
                GivenName = "John",
                WhenChanged = now
            }
        };

        var queryable = Populate(expectedValues.Append(new Model
            { Version = "5", Sn = "McClane", GivenName = "John", WhenChanged = null }).ToArray());

        var actualValues =
            await queryable.ByLdapFilterAsync(
                "(&(whenchanged=*)(|(sn=Vandenbussche)(|(&(sn=McClane)(Version=5))(givenname=John))))");

        actualValues.Should().BeEquivalentTo(expectedValues);
    }
}
