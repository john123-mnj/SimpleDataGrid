namespace SimpleDataGrid.Searching;

public class SearchDescriptor<T>(Func<T, string> selector, string? term)
{
    public Func<T, string> Selector { get; } = selector ?? throw new ArgumentNullException(nameof(selector));
    public string? Term { get; } = term;
}
