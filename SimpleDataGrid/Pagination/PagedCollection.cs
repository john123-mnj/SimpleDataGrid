using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleDataGrid.Pagination;

/// <summary>
/// Represents a collection of items that can be paged, filtered, and searched.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class PagedCollection<T> : IPagedCollection, INotifyPropertyChanged
{
    private readonly int _pageSize;
    private int _currentPage;
    /// <summary>
    /// Gets the number of items to display per page.
    /// </summary>
    public int PageSize => _pageSize;
    private IReadOnlyList<T> _source = [];
    private IReadOnlyList<T> _filtered = [];

    private readonly List<Func<T, bool>> _filters = [];
    private readonly List<(Func<T, object> selector, bool ascending)> _sorts = [];
    private Func<T, string>? _searchSelector;
    private string? _searchTerm;
    private bool _useWildcards;

    /// <summary>
    /// Gets a value indicating whether the collection is sorted.
    /// </summary>
    public bool IsSorted => _sorts.Count > 0;

    /// <summary>
    /// Occurs when the sorting changes.
    /// </summary>
    public event EventHandler? SortChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedCollection{T}"/> class.
    /// </summary>
    /// <param name="pageSize">The number of items to display per page.</param>
    public PagedCollection(int pageSize = 50)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        this._pageSize = pageSize;
    }

    /// <summary>
    /// Sets the source collection.
    /// </summary>
    /// <param name="items">The source collection.</param>
    public void SetSource(IReadOnlyList<T> items)
    {
        _source = items ?? throw new ArgumentNullException(nameof(items));
        ApplyFiltering();
    }

    /// <summary>
    /// Adds a filter to the collection.
    /// </summary>
    /// <param name="filter">The filter to add.</param>
    public void AddFilter(Func<T, bool> filter)
    {
        _filters.Add(filter);
        ApplyFiltering();
    }

    /// <summary>
    /// Clears all filters from the collection.
    /// </summary>
    public void ClearFilters()
    {
        _filters.Clear();
        ApplyFiltering();
    }

    /// <summary>
    /// Sets the search criteria for the collection.
    /// </summary>
    /// <param name="selector">A function that returns the string representation of the object to search.</param>
    /// <param name="term">The search term.</param>
    /// <param name="useWildcards">A value indicating whether to use wildcards in the search term.</param>
    public void SetSearch(Func<T, string> selector, string? term, bool useWildcards = false)
    {
        _searchSelector = selector;
        _searchTerm = term;
        _useWildcards = useWildcards;
        ApplyFiltering();
    }

    /// <summary>
    /// Sets the sort criteria for the collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to sort by.</typeparam>
    /// <param name="selector">A function to extract the key for sorting.</param>
    /// <param name="ascending">A value indicating whether to sort in ascending order.</param>
    public void SetSort<TKey>(Func<T, TKey> selector, bool ascending)
    {
        _sorts.Clear();
        _sorts.Add((x => selector(x)!, ascending));
        ApplyFiltering();
        SortChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the sort criteria from the collection.
    /// </summary>
    public void ClearSort()
    {
        _sorts.Clear();
        ApplyFiltering();
        SortChanged?.Invoke(this, EventArgs.Empty);
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
            if (_useWildcards)
            {
                var regex = new Regex(WildcardToRegex(_searchTerm), RegexOptions.IgnoreCase);
                query = query.Where(x => regex.IsMatch(_searchSelector(x)));
            }
            else
            {
                query = query.Where(x =>
                    _searchSelector(x)
                        ?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) == true);
            }
        }

        if (_sorts.Count > 0)
        {
            IOrderedEnumerable<T>? orderedQuery = null;
            foreach (var (selector, ascending) in _sorts)
            {
                if (orderedQuery == null)
                {
                    orderedQuery = ascending ? query.OrderBy(selector) : query.OrderByDescending(selector);
                }
                else
                {
                    orderedQuery = ascending ? orderedQuery.ThenBy(selector) : orderedQuery.ThenByDescending(selector);
                }
            }
            query = orderedQuery ?? query;
        }

        _filtered = [.. query];
        _currentPage = 0;
        RaiseAllChanged();
    }

    private static string WildcardToRegex(string pattern)
        => "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";

    /// <summary>
    /// Gets the items on the current page.
    /// </summary>
    public IReadOnlyList<T> CurrentPageItems =>
        [.. _filtered.Skip(_currentPage * _pageSize).Take(_pageSize)];

    IReadOnlyList<object> IPagedCollection.CurrentPageItems => 
        [..CurrentPageItems.Cast<object>()];

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurrentPage => _currentPage + 1;

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_filtered.Count / _pageSize));

    /// <summary>
    /// Gets the total number of items in the filtered collection.
    /// </summary>
    public int TotalItems => _filtered.Count;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNext => _currentPage < TotalPages - 1;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPrevious => _currentPage > 0;

    /// <summary>
    /// Moves to the next page.
    /// </summary>
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

    /// <summary>
    /// Moves to the previous page.
    /// </summary>
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

    /// <summary>
    /// Moves to a specific page.
    /// </summary>
    /// <param name="page">The page to move to.</param>
    public void GoToPage(int page)
    {
        var newPage = Math.Clamp(page - 1, 0, TotalPages - 1);
        if (newPage != _currentPage)
        {
            _currentPage = newPage;
            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(HasNext));
            OnPropertyChanged(nameof(HasPrevious));
        }
    }

    /// <summary>
    /// Moves to the first page.
    /// </summary>
    public void GoToFirstPage()
    {
        GoToPage(1);
    }

    /// <summary>
    /// Moves to the last page.
    /// </summary>
    public void GoToLastPage()
    {
        GoToPage(TotalPages);
    }

    private void RaiseAllChanged()
    {
        OnPropertyChanged(nameof(CurrentPageItems));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasNext));
        OnPropertyChanged(nameof(HasPrevious));
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}