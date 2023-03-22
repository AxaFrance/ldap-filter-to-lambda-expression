namespace AxaFrance.LdapFiltersToLambdaExpression.Internal;

internal sealed class ResettableLazy<T>
{
    private Lazy<T> lazy = null!;

    public T Value => this.lazy.Value;

    private readonly Func<T> valueFactory;

    public ResettableLazy(Func<T> valueFactory)
    {
        this.valueFactory = valueFactory;
        this.CreateInstanceOfLazy();
    }

    private void CreateInstanceOfLazy()
    {
        this.lazy = new Lazy<T>(this.valueFactory);
    }

    public void Reset() => this.CreateInstanceOfLazy();
}
