using System.Linq.Expressions;
using AxaFrance.LdapFiltersToLambdaExpression.Parser;

// ReSharper disable once CheckNamespace
namespace System.Linq;

public static class QueryableExtensions
{
    public static Task<IQueryable<TSource>> ByLdapFilterAsync<TSource>(this IQueryable<TSource> source,
        string filter)
    {
        return source.ByLdapFilterAsync(filter, Queryable.Where);
    }

    public static async Task<TResult> ByLdapFilterAsync<TSource, TResult>(this IQueryable<TSource> source,
        string filter,
        Func<IQueryable<TSource>, Expression<Func<TSource, bool>>, TResult> search)
    {
        var predicate = await LdapFilterParser.ParseAsync<TSource>(filter);
        return search(source, predicate);
    }
}
