namespace AxaFrance.LdapFiltersToLambdaExpression.Parser;

internal static class Symbol
{
    internal const string Equal = "=";
    internal const string Star = "*";
    internal const string Negative = "!";
    internal const string Lower = "<";
    internal const string Greater = ">";
    internal const string LowerOrEqual = Lower + Equal;
    internal const string GreaterOrEqual = Greater + Equal;
    internal const string Approximate = "~" + Equal;
}
