using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace AxaFrance.LdapFiltersToLambdaExpression.Tests.UseEFCore.DbStore;

public partial class SampleDbContext
{
    [DbFunction(Name = "SOUNDEX", IsBuiltIn = true)]
    public static string Soundex(string value)
    {
        throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Soundex)));
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(typeof(SampleDbContext).GetMethod(nameof(Soundex))!)
            .HasTranslation(arguments => new SqlFunctionExpression("SOUNDEX", arguments, false,
                arguments.Select(a => false), typeof(string), null));
    }
}
