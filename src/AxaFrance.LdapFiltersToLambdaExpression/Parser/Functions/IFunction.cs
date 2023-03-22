using System.Reflection;

namespace AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;

public interface IFunction
{
    MethodInfo MethodInfo { get; }

    Type DeclaringType { get; }

    string ExpressionPattern { get; }
}
