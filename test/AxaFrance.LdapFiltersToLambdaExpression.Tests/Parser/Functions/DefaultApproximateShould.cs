using System.Globalization;
using AxaFrance.LdapFiltersToLambdaExpression.Parser.Functions;
using FluentAssertions;

namespace AxaFrance.LdapFiltersToLambdaExpression.Tests.Parser.Functions;

[TestFixture]
public class DefaultApproximateShould
{
    [TestCase("", "0000")]
    [TestCase("a", "A000")]
    [TestCase("B", "B000")]
    [TestCase("c", "C000")]
    [TestCase("D", "D000")]
    [TestCase("e", "E000")]
    [TestCase("F", "F000")]
    [TestCase("g", "G000")]
    [TestCase("H", "H000")]
    [TestCase("i", "I000")]
    [TestCase("J", "J000")]
    [TestCase("k", "K000")]
    [TestCase("L", "L000")]
    [TestCase("m", "M000")]
    [TestCase("N", "N000")]
    [TestCase("o", "O000")]
    [TestCase("P", "P000")]
    public void TheFirstLetterOfCodeIsTheFirstUppercaseLetterOfWord(string value, string code)
    {
        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().StartWithEquivalentOf(value.ToUpper());
    }

    [TestCase("aA", "A000")]
    [TestCase("Be", "B000")]
    [TestCase("cH", "C000")]
    [TestCase("Di", "D000")]
    [TestCase("eO", "E000")]
    [TestCase("Fu", "F000")]
    [TestCase("gW", "G000")]
    [TestCase("Hy", "H000")]
    public void A_E_H_I_O_U_W_Y_AreNotTranslated(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-EN");


        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("aB", "A100")]
    [TestCase("Bf", "B100")]
    [TestCase("cP", "C100")]
    [TestCase("Dv", "D100")]
    public void B_F_P_V_AreTranslatedToOneInCodeForEnglishCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-EN");


        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("aC", "A200")]
    [TestCase("Bg", "B200")]
    [TestCase("cJ", "C200")]
    [TestCase("Dk", "D200")]
    [TestCase("aQ", "A200")]
    [TestCase("Bs", "B200")]
    [TestCase("cX", "C200")]
    [TestCase("Dz", "D200")]
    public void C_G_J_K_Q_S_X_Z_AreTranslatedToTwoInCodeForEnglishCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-EN");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("ad", "A300")]
    [TestCase("BT", "B300")]
    public void D_T_AreTranslatedToThreeInCodeForEnglishCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-EN");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("aL", "A400")]
    public void L_IsTranslatedToFourInCodeForEnglishCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-EN");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("aM", "A500")]
    [TestCase("an", "A500")]
    public void M_N_IsTranslatedToFiveInCodeForEnglishCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-EN");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("aB", "A100")]
    [TestCase("aP", "A100")]
    public void B_P_IsTranslatedToOneInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("Bc", "B200")]
    [TestCase("cK", "C200")]
    [TestCase("Dq", "D200")]
    public void C_K_Q_AreTranslatedToTwoInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cD", "C300")]
    [TestCase("Dt", "D300")]
    public void D_T_AreTranslatedToThreeInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cL", "C400")]
    public void L_IsTranslatedToFourInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cM", "C500")]
    [TestCase("cn", "C500")]
    public void M_N_AreTranslatedToFiveInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cR", "C600")]
    public void R_AreTranslatedToSixInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cG", "C700")]
    [TestCase("cj", "C700")]
    public void G_J_AreTranslatedToSevenInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cx", "C800")]
    [TestCase("cZ", "C800")]
    [TestCase("cs", "C800")]
    public void X_Z_S_AreTranslatedToEightInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cf", "C900")]
    [TestCase("cV", "C900")]
    public void F_V_AreTranslatedToNineInCodeForFrenchCulture(string value, string code)
    {
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cbBb", "C100")]
    [TestCase("cbpb", "C100")]
    public void NotTranslateSameLetterFollowingEachOther(string value, string code)
    {
        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [TestCase("cbcdl", "C123")]
    [TestCase("Redmond", "R355")]
    public void TheCodeHaveForCaracteresMax(string value, string code)
    {
        var actualValue = DefaultApproximate.Do(value);

        actualValue.Should().Be(code);
    }

    [Test]
    public void DeclaringTypeIsEqualToDefaultApproximate()
    {
        var approximate = new DefaultApproximate();

        approximate.DeclaringType.Should().Be(typeof(DefaultApproximate));
    }

    [Test]
    public void MethodInfoIsEqualToDoMethodOfDefaultApprocimate()
    {
        var approximate = new DefaultApproximate();
        var expectedMethodInfo =
            approximate.DeclaringType.GetMethod(nameof(DefaultApproximate.Do), new[] { typeof(string) })!;

        approximate.MethodInfo.Should().BeSameAs(expectedMethodInfo);
    }

    [Test]
    public void ExpressionPatternIsEqualToPathOfDoMethodOfDefaultApproximate()
    {
        var approximate = new DefaultApproximate();
        var expectedExpressionPattern = $"{typeof(DefaultApproximate).FullName}.{approximate.MethodInfo.Name}({{0}})";

        approximate.ExpressionPattern.Should().Be(expectedExpressionPattern);
    }
}
