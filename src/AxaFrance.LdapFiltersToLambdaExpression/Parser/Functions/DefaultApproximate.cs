using System.Globalization;
using System.Reflection;
using System.Text;

namespace AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;

public sealed class DefaultApproximate : IFunction
{
    public DefaultApproximate()
    {
        this.DeclaringType = this.GetType();
        this.MethodInfo = this.DeclaringType.GetMethod(nameof(Do), new[] { typeof(string) })!;
        this.ExpressionPattern = $"{typeof(DefaultApproximate).FullName}.{this.MethodInfo.Name}({{0}})";
    }

    public MethodInfo MethodInfo { get; }

    public Type DeclaringType { get; }

    public string ExpressionPattern { get; }

    public static string Do(string value)
    {
        var result = new StringBuilder();
        if (value is not { Length: > 0 })
        {
            return result.ToString().PadRight(4, '0');
        }

        result.Append(char.ToUpper(value[0]));
        (char @char, string code) previous = (default, string.Empty);
        var notFrench = CultureInfo.CurrentCulture.Name.ToLower() != "fr-fr";
        foreach (var (currentChar, currentCode) in value.ToUpper().Skip(1).Select(c => (c, EncodeChar(c, notFrench))))
        {
            if (currentChar != previous.@char && currentCode != previous.code)
            {
                result.Append(currentCode);
            }

            if (result.Length == 4)
            {
                break;
            }

            previous = (currentChar, currentCode);
        }

        return result.ToString().PadRight(4, '0');
    }

    private static string EncodeChar(char c, bool notFrench)
    {
        return c switch
        {
            'B' or 'P' => "1",
            'F' or 'V' when notFrench => "1",
            'C' or 'K' or 'Q' => "2",
            'G' or 'J' or 'S' or 'X' or 'Z' when notFrench => "2",
            'D' or 'T' => "3",
            'L' => "4",
            'M' or 'N' => "5",
            'R' => "6",
            'G' or 'J' => "7",
            'S' or 'X' or 'Z' => "8",
            'F' or 'V' => "9",
            _ => string.Empty
        };
    }
}
