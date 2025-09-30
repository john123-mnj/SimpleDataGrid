namespace SimpleDataGrid.Filtering;

public class FilterCollection<T>(IEnumerable<T> items)
{
    private readonly ObservableCollection<T> _source = new(items);
    private readonly List<Func<T, bool>> _filters = [];

    public void AddFilter(Func<T, bool> filter) => _filters.Add(filter);

    public void ClearFilters() => _filters.Clear();

    public IEnumerable<T> ApplyFilters()
    {
        IEnumerable<T> query = _source;

        foreach (var filter in _filters)
        {
            query = query.Where(filter);
        }

        return query;
    }

    public void Add(T item) => _source.Add(item);

    public void Remove(T item) => _source.Remove(item);

    public void Clear() => _source.Clear();
}
