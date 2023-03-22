using System.Reflection;
using AxaFrance.LdapFiltersToLambdaExpression.Options;
using AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;
using AxaFrance.LdapFiltersToLambdaExpression.Tests.UseEFCore.DbStore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace AxaFrance.LdapFiltersToLambdaExpression.Tests.UseEFCore;

[TestFixture]
public class LdapFilterParserShould
{
    public class EFCoreLike : IFunction
    {
        public MethodInfo MethodInfo => this.DeclaringType.GetMethod(nameof(DbFunctionsExtensions.Like),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

        public Type DeclaringType => typeof(DbFunctionsExtensions);

        public string ExpressionPattern =>
            $"{nameof(DbFunctionsExtensions)}.{this.MethodInfo.Name}({nameof(EF)}.{nameof(EF.Functions)}, {{0}},{{1}})";
    }

    public class EFCoreSoundex : IFunction
    {
        public MethodInfo MethodInfo => this.DeclaringType.GetMethod(nameof(SampleDbContext.Soundex),
            new[] { typeof(string) })!;

        public Type DeclaringType => typeof(SampleDbContext);

        public string ExpressionPattern => $"{this.DeclaringType.FullName}.{this.MethodInfo.Name}({{0}})";
    }
    
    [SetUp]
    public void BeforeEach()
    {
        ExpressionOption.lazy.Reset();
    }
    
    [Test]
    public async Task GenerateSqlQueryByLdapFilterWithToLowerByDefault()
    {
        var services = new ServiceCollection();
        services.LdapFiltersToLambdaConfigure(option =>
        {
            option.LikeFunction(new EFCoreLike())
                .ApproximateFunction(new EFCoreSoundex())
                .AddNamespaces($"{nameof(Microsoft)}.{nameof(Microsoft.EntityFrameworkCore)}");
        }).AddDbContextPool<SampleDbContext>(builder =>
            builder.UseSqlServer("Data Source=fakeDataSource;Initial Catalog=fakeCatalog;"));
        await using var provider = services.BuildServiceProvider();
        await using var dbContext = provider.GetRequiredService<SampleDbContext>();
        var query = await dbContext.Models.ByLdapFilterAsync(
            "(&(whenchanged=*)(|(sn=*andenbussche)(|(&(sn=McClane)(Version=5))(givenname~=J*hn))))",
            Queryable.Where);

#pragma warning disable EF1001
        query.As<EntityQueryable<Model>>().DebugView.Query.Replace(Environment.NewLine, " ")
#pragma warning restore EF1001
            .Should()
            .Be(
                "SELECT [m].[distinguishedname], [m].[civility], [m].[givenname], [m].[job], [m].[sn], [m].[version], [m].[WhenChanged] FROM [dbo].[Model] AS [m] WHERE ([m].[WhenChanged] IS NOT NULL) AND (([m].[sn] LIKE N'%andenbussche') OR (LOWER([m].[sn]) = N'mcclane' AND LOWER([m].[version]) = N'5') OR SOUNDEX([m].[givenname]) = SOUNDEX(N'j*hn'))");
    }
    
    [Test]
    public async Task GenerateSqlQueryByLdapFilterWithoutToLowerWhenCaseInsensitiveDbIsActive()
    {
        var services = new ServiceCollection();
        services.LdapFiltersToLambdaConfigure(option =>
        {
            option.LikeFunction(new EFCoreLike())
                .ApproximateFunction(new EFCoreSoundex())
                .CaseInsensitiveDbIsActive()
                .AddNamespaces($"{nameof(Microsoft)}.{nameof(Microsoft.EntityFrameworkCore)}");
        }).AddDbContextPool<SampleDbContext>(builder =>
            builder.UseSqlServer("Data Source=fakeDataSource;Initial Catalog=fakeCatalog;"));
        await using var provider = services.BuildServiceProvider();
        await using var dbContext = provider.GetRequiredService<SampleDbContext>();
        var query = await dbContext.Models.ByLdapFilterAsync(
            "(&(whenchanged=*)(|(sn=*andenbussche)(|(&(sn=McClane)(Version=5))(givenname~=J*hn))))",
            Queryable.Where);

#pragma warning disable EF1001
        query.As<EntityQueryable<Model>>().DebugView.Query.Replace(Environment.NewLine, " ")
#pragma warning restore EF1001
            .Should()
            .Be(
                "SELECT [m].[distinguishedname], [m].[civility], [m].[givenname], [m].[job], [m].[sn], [m].[version], [m].[WhenChanged] FROM [dbo].[Model] AS [m] WHERE ([m].[WhenChanged] IS NOT NULL) AND (([m].[sn] LIKE N'%andenbussche') OR ([m].[sn] = N'mcclane' AND [m].[version] = N'5') OR SOUNDEX([m].[givenname]) = SOUNDEX(N'j*hn'))");
    }
}
