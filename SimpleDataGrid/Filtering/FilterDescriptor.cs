namespace SimpleDataGrid.Filtering;

public class FilterDescriptor<T>(Func<T, bool> predicate)
{
    public Func<T, bool> Predicate { get; } = predicate ?? throw new ArgumentNullException(nameof(predicate));
}
