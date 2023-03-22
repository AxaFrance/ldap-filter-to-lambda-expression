using System.Linq.Expressions;
using System.Text.RegularExpressions;
using AxaFrance.LdapFiltersToLambdaExpression.Options;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace AxaFrance.LdapFiltersToLambdaExpression.Parser;

internal static class LdapFilterParser
{
    private const string ToDecimalExpression = $"{nameof(Convert)}.{nameof(Convert.ToDecimal)}";

    private static readonly Regex queryPattern =
        new(
            @"(?<condition>(\([&|\||!])*)(?<expression>\((?<property>[\w\d:]+)(?<comparison>>=|<=|~=|>|<|=)(?<value>[*a-z0-9\s]+)\))(\)*)",
            RegexOptions.IgnoreCase,TimeSpan.FromMilliseconds(100));

    private static readonly Regex groupsPattern =
        new(@"\((?<condition>[&|\||!])(?<expression>(\(\{\d+\}\))*)\)*", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

    private static readonly Regex captureIndex = new(@"\(\{(?<index>\d+)\}\)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

    public static Task<Expression<Func<T, bool>>> ParseAsync<T>(string filter)
    {
        const char OpeningParenthesis = '(';
        const char ClosingParenthesis = ')';
        if (!filter.StartsWith(OpeningParenthesis) || !filter.EndsWith(ClosingParenthesis))
        {
            throw new ArgumentException(
                $"le {nameof(filter)} doit commencer par '{OpeningParenthesis}' et finir par '{ClosingParenthesis}'.");
        }

        if (filter.Count(s => s == OpeningParenthesis) != filter.Count(s => s == ClosingParenthesis))
        {
            throw new ArgumentException(
                $"le {nameof(filter)} doit avoir le même nombres de '{OpeningParenthesis}' et de '{ClosingParenthesis}'.");
        }

        var properties = typeof(T).GetProperties().ToDictionary(p => p.Name.ToLower(), p => p.Name);
        filter = filter.ToLower();
        var matches = queryPattern.Matches(filter);
        var matchedFilter = string.Concat(matches.Select(m => m.Value));
        if (matchedFilter != filter)
        {
            throw new ArgumentException($"le format de {nameof(filter)}: [{filter}] n'est pas correct.");
        }

        var groups = matches.Select(m => m.Groups).ToArray();
        var expressions = new Dictionary<int, string>();
        var argumentKey = 0;
        foreach (var group in groups)
        {
            var property = group["property"].Value;
            var comparison = group["comparison"].Value;
            var value = group["value"].Value;
            var condition = group["condition"].Value;
            var origin = group["expression"].Value;
            var oldValue = !condition.Contains(Symbol.Negative) ? origin : $"({Symbol.Negative}{origin})";
            matchedFilter = matchedFilter.Replace(oldValue, $"({{{argumentKey}}})");
            expressions.Add(argumentKey++, ToLambdaCode(properties[property.ToLower()], comparison, value, condition));
        }

        var finalExpression = GenerateLambdaCode(argumentKey, matchedFilter, expressions);
        finalExpression = $"m => {finalExpression}";
        return ToLambdaExpression<T>(finalExpression);
    }

    private static Task<Expression<Func<T, bool>>> ToLambdaExpression<T>(string finalExpression)
    {
        var options = ScriptOptions.Default
            .AddImports(ExpressionOption.Instance.Namespaces)
            .AddReferences(typeof(T).Assembly);
        return CSharpScript.EvaluateAsync<Expression<Func<T, bool>>>(finalExpression, options);
    }

    private static string GenerateLambdaCode(int argumentKey, string reformattedFilter,
        IDictionary<int, string> expressions)
    {
        var initialFilter = reformattedFilter;
        var groups = groupsPattern.Matches(reformattedFilter)
            .Select(m => m.Groups)
            .ToArray();
        if (groups.Length < 1)
        {
            return expressions.Single().Value;
        }

        foreach (var group in groups)
        {
            var expression = group["expression"].Value;
            if (string.IsNullOrWhiteSpace(expression))
            {
                continue;
            }

            var condition = group["condition"].Value;
            var indexes = captureIndex.Matches(expression);
            if (indexes.Count < 2)
            {
                continue;
            }

            var concatenatedCondition = string.Join($" {condition}{condition} ", indexes.Select(m =>
            {
                var name = ToInt(m.Groups["index"].Value);
                var code = expressions[name];
                expressions.Remove(name);
                return code;
            }));

            expressions.Add(argumentKey, $"({concatenatedCondition})");
            reformattedFilter = reformattedFilter.Replace($"{condition}{expression}", $"{{{argumentKey++}}}");
        }

        if (reformattedFilter == initialFilter)
        {
            throw new ArgumentException("Erreur de format du filtre.");
        }

        return GenerateLambdaCode(argumentKey, reformattedFilter, expressions);
    }

    private static int ToInt(string value)
    {
        var _ = int.TryParse(value, out var parsedValue);
        return parsedValue;
    }

    private static string ToLambdaCode(string property, string comparison, string value, string condition)
    {
        var negative = condition.Contains(Symbol.Negative);
        condition = negative ? Symbol.Negative : string.Empty;
        string lambdaCode;
        switch (comparison)
        {
            case Symbol.Equal when value == Symbol.Star:
            {
                lambdaCode = $"m.{property} {(negative ? "==" : "!=")} null";
                break;
            }
            case Symbol.Approximate:
            {
                var approximate = ExpressionOption.Instance.Approximate;
                var approximateExpression = approximate.ExpressionPattern;
                lambdaCode =
                    $"{string.Format(approximateExpression, $"m.{property}")} == {string.Format(approximateExpression, $"\"{value}\"")}";
                break;
            }
            case Symbol.Equal when value.Contains(Symbol.Star):
            {
                var reformattedValue = value.Replace(Symbol.Star, "%");
                var like = ExpressionOption.Instance.Like;
                var likeExpression = like.ExpressionPattern;
                lambdaCode =
                    $"{condition}{string.Format(likeExpression, $"m.{property}", $"\"{reformattedValue}\"")}";
                break;
            }
            case Symbol.LowerOrEqual or Symbol.GreaterOrEqual or Symbol.Lower or Symbol.Greater:
            {
                comparison = comparison switch
                {
                    Symbol.LowerOrEqual when negative => Symbol.Greater,
                    Symbol.GreaterOrEqual when negative => Symbol.Lower,
                    Symbol.Lower when negative => Symbol.GreaterOrEqual,
                    Symbol.Greater when negative => Symbol.LowerOrEqual,
                    _ => comparison
                };

                lambdaCode =
                    $"{ToDecimalExpression}(m.{property}) {comparison} {ToDecimalExpression}(\"{value}\")";
                break;
            }
            default:
            {
                var toLowerExpression = ExpressionOption.Instance.CaseInsensitiveDbActive
                    ? string.Empty
                    : $".{nameof(string.ToLower)}()";
                lambdaCode = $"m.{property}{toLowerExpression} {(negative ? "!=" : "==")} \"{value}\"";
                break;
            }
        }

        return lambdaCode;
    }
}
