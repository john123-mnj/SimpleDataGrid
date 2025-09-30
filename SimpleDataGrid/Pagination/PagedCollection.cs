using System.ComponentModel;

namespace SimpleDataGrid.Pagination;

public class PagedCollection<T> : INotifyPropertyChanged
{
    private readonly int _pageSize;
    private int _currentPage;
    private IReadOnlyList<T> _source = [];
    private IReadOnlyList<T> _filtered = [];

    private readonly List<Func<T, bool>> _filters = [];
    private Func<T, string>? _searchSelector;
    private string? _searchTerm;

    public PagedCollection(int pageSize = 50)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        this._pageSize = pageSize;
    }

    public void SetSource(IReadOnlyList<T> items)
    {
        _source = items ?? throw new ArgumentNullException(nameof(items));
        ApplyFiltering();
    }

    public void AddFilter(Func<T, bool> filter)
    {
        _filters.Add(filter);
        ApplyFiltering();
    }

    public void ClearFilters()
    {
        _filters.Clear();
        ApplyFiltering();
    }

    public void SetSearch(Func<T, string> selector, string? term)
    {
        _searchSelector = selector;
        _searchTerm = term;
        ApplyFiltering();
    }

    private void ApplyFiltering()
    {
        IEnumerable<T> query = _source;

        foreach (var filter in _filters)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(_searchTerm) && _searchSelector != null)
        {
            query = query.Where(x =>
                _searchSelector(x)
                    ?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) == true);
        }

        _filtered = [.. query];
        _currentPage = 0;
        RaiseAllChanged();
    }

    public IReadOnlyList<T> CurrentPageItems =>
        [.. _filtered.Skip(_currentPage * _pageSize).Take(_pageSize)];

    public int CurrentPage => _currentPage + 1;

    public int TotalPages => (int)Math.Ceiling((double)_filtered.Count / _pageSize);

    public bool HasNext => _currentPage < TotalPages - 1;
    public bool HasPrevious => _currentPage > 0;

    public void NextPage()
    {
        if (HasNext)
        {
            _currentPage++;
            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(HasNext));
            OnPropertyChanged(nameof(HasPrevious));
        }
    }

    public void PreviousPage()
    {
        if (HasPrevious)
        {
            _currentPage--;
            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(HasNext));
            OnPropertyChanged(nameof(HasPrevious));
        }
    }

    private void RaiseAllChanged()
    {
        OnPropertyChanged(nameof(CurrentPageItems));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasNext));
        OnPropertyChanged(nameof(HasPrevious));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}