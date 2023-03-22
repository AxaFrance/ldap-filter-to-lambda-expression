using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;

public sealed class DefaultLike : IFunction
{
    private static readonly char[] RegexSpecialChars
        = { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\' };

    private static readonly string EscapeRegexCharsPattern = string.Join("|", RegexSpecialChars.Select(c => $@"\{c}"));

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(1000.0);

    public DefaultLike()
    {
        this.DeclaringType = this.GetType();
        this.MethodInfo = this.DeclaringType.GetMethod(nameof(Do), new[] { typeof(string), typeof(string) })!;
        this.ExpressionPattern = $"{typeof(DefaultLike).FullName}.{this.MethodInfo.Name}({{0}}, {{1}})";
    }

    public MethodInfo MethodInfo { get; }

    public Type DeclaringType { get; }

    public string ExpressionPattern { get; }

    public static bool Do(string matchExpression, string pattern)
    {
        if (string.IsNullOrEmpty(matchExpression) || string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        if (matchExpression.Equals(pattern, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var regexPattern = Regex.Replace(pattern, EscapeRegexCharsPattern, c => $@"\{c}", default, RegexTimeout);
        var stringBuilder = new StringBuilder();
        foreach (var c in regexPattern)
        {
            switch (c)
            {
                case '_':
                {
                    stringBuilder.Append('.');
                    break;
                }
                case '%':
                {
                    stringBuilder.Append(".*");
                    break;
                }
                default:
                {
                    stringBuilder.Append(c);
                    break;
                }
            }
        }

        regexPattern = stringBuilder.ToString();
        return Regex.IsMatch(
            matchExpression,
            $@"\A{regexPattern}\s*\z",
            RegexOptions.IgnoreCase | RegexOptions.Singleline,
            RegexTimeout);
    }
}
