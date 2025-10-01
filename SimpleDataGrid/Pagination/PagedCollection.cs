using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SimpleDataGrid.Pagination;

/// <summary>
/// Represents a collection of items that can be paged, filtered, searched, and sorted.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class PagedCollection<T> : IPagedCollection, IDisposable
{
    private int _pageSize;
    private int _currentPage;

    /// <summary>
    /// Occurs when the page size changes.
    /// </summary>
    public event EventHandler? PageSizeChanged;
    /// <summary>
    /// Gets or sets the number of items to display per page.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => SetPageSize(value);
    }
    private IReadOnlyList<T> _source = [];
    private IReadOnlyList<T> _filtered = [];

    private readonly Dictionary<string, Func<T, bool>> _filters = [];
    private readonly List<(Func<T, object> selector, bool ascending)> _sorts = [];
    private IEnumerable<Func<T, string>> _searchSelectors = [];
    private string? _searchTerm;
    private bool _useWildcards;
    private CancellationTokenSource? _debounceCts;
    private bool _isSearching;

    /// <summary>
    /// Gets a value indicating whether the collection is sorted.
    /// </summary>
    public bool IsSorted => _sorts.Count > 0;

    /// <summary>
    /// Gets a value indicating whether a search operation is currently in progress.
    /// </summary>
    public bool IsSearching
    {
        get => _isSearching;
        private set
        {
            if (_isSearching != value)
            {
                _isSearching = value;
                OnPropertyChanged(nameof(IsSearching));
            }
        }
    }

    /// <summary>
    /// Occurs when the sorting changes.
    /// </summary>
    public event EventHandler? SortChanged;

    /// <summary>
    /// Occurs when the filter changes.
    /// </summary>
    public event EventHandler? FilterChanged;

    /// <summary>
    /// Occurs when the current page changes.
    /// </summary>
    public event EventHandler? PageChanged;

    /// <summary>
    /// Occurs when the source collection changes.
    /// </summary>
    public event EventHandler? SourceChanged;

    /// <summary>
    /// Occurs when the search criteria changes.
    /// </summary>
    public event EventHandler? SearchChanged;

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
        ArgumentNullException.ThrowIfNull(items);
        _source = items;
        ApplyFiltering(false);
        SourceChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Adds a filter to the collection.
    /// </summary>
    /// <param name="filter">The filter to add.</param>
    public void AddFilter(Func<T, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        SetFilter(Guid.NewGuid().ToString(), filter);
    }

    /// <summary>
    /// Adds or updates a filter in the collection.
    /// </summary>
    /// <param name="key">The key of the filter.</param>
    /// <param name="filter">The filter to add or update.</param>
    public void SetFilter(string key, Func<T, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(filter);
        _filters[key] = filter;
        ApplyFiltering(true);
        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes a filter from the collection.
    /// </summary>
    /// <param name="key">The key of the filter to remove.</param>
    public void RemoveFilter(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (_filters.Remove(key))
        {
            ApplyFiltering(true);
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets the keys of the active filters.
    /// </summary>
    /// <returns>A read-only collection of the active filter keys.</returns>
    public IReadOnlyCollection<string> GetActiveFilters() => _filters.Keys;


    /// <summary>
    /// Clears all filters from the collection.
    /// </summary>
    public void ClearFilters()
    {
        _filters.Clear();
        ApplyFiltering(false);
        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the search criteria for the collection using a single selector asynchronously.
    /// </summary>
    /// <param name="selector">A function that returns the string representation of the object to search.</param>
    /// <param name="term">The search term to match.</param>
    /// <param name="useWildcards">Indicates whether wildcards (*) and (?) should be supported in the search term.</param>
    /// <param name="debounceMilliseconds">Optional. Number of milliseconds to debounce the search. If 0, the search is applied immediately.</param>
    /// <returns>A task representing the asynchronous search operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> is null.</exception>
    public Task SetSearchAsync(Func<T, string> selector, string? term, bool useWildcards = false, int debounceMilliseconds = 0)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return SetSearchAsync([selector], term, useWildcards, debounceMilliseconds);
    }

    /// <summary>
    /// Sets the search criteria for the collection across multiple selectors asynchronously.
    /// Performs an OR search across the specified properties.
    /// </summary>
    /// <param name="selectors">A collection of functions that return the string representation of the properties to search.</param>
    /// <param name="term">The search term to match.</param>
    /// <param name="useWildcards">Indicates whether wildcards (*) and (?) should be supported in the search term.</param>
    /// <param name="debounceMilliseconds">Optional. Number of milliseconds to debounce the search. If 0, the search is applied immediately.</param>
    /// <returns>A task representing the asynchronous search operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="selectors"/> is null.</exception>
    public async Task SetSearchAsync(IEnumerable<Func<T, string>> selectors, string? term, bool useWildcards = false, int debounceMilliseconds = 0)
    {
        ArgumentNullException.ThrowIfNull(selectors);
        _searchSelectors = selectors;
        _searchTerm = term;
        _useWildcards = useWildcards;

        if (debounceMilliseconds > 0)
        {
            IsSearching = true;
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            try
            {
                await Task.Delay(debounceMilliseconds, token);
                ApplyFiltering(false);
                SearchChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (TaskCanceledException) { }
            finally
            {
                IsSearching = false;
            }
        }
        else
        {
            ApplyFiltering(false);
            SearchChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Sets the search criteria for the collection across multiple selectors asynchronously.
    /// Performs an AND search, requiring all selectors to match the search term.
    /// </summary>
    /// <param name="selectors">A collection of functions that return the string representation of the properties to search.</param>
    /// <param name="term">The search term to match.</param>
    /// <param name="useWildcards">Indicates whether wildcards (*) and (?) should be supported in the search term.</param>
    /// <param name="debounceMilliseconds">Optional. Number of milliseconds to debounce the search. If 0, the search is applied immediately.</param>
    /// <returns>A task representing the asynchronous search operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="selectors"/> is null.</exception>
    public async Task SetSearchAllAsync(IEnumerable<Func<T, string>> selectors, string? term, bool useWildcards = false, int debounceMilliseconds = 0)
    {
        ArgumentNullException.ThrowIfNull(selectors);
        _searchSelectors = selectors;
        _searchTerm = term;
        _useWildcards = useWildcards;

        if (debounceMilliseconds > 0)
        {
            IsSearching = true;
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            try
            {
                await Task.Delay(debounceMilliseconds, token);
                ApplyFilteringAll(false);
                SearchChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (TaskCanceledException) { }
            finally
            {
                IsSearching = false;
            }
        }
        else
        {
            ApplyFilteringAll(false);
            SearchChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ApplyFilteringAll(bool maintainPosition = false)
    {
        var oldFirstItemIndex = _currentPage * _pageSize;
        IEnumerable<T> query = _source;

        foreach (var filter in _filters.Values)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(_searchTerm) && _searchSelectors.Any())
        {
            var term = _searchTerm;
            Func<string, bool> matches;

            if (_useWildcards)
            {
                var regex = new Regex(WildcardToRegex(term), RegexOptions.IgnoreCase);
                matches = regex.IsMatch;
            }
            else
            {
                matches = s => s.Contains(term, StringComparison.OrdinalIgnoreCase);
            }

            // AND logic for multi-column search
            query = query.Where(item => _searchSelectors.All(selector => matches(selector(item) ?? string.Empty)));
        }

        if (_sorts.Count > 0)
        {
            IOrderedEnumerable<T>? orderedQuery = null;
            foreach (var (selector, ascending) in _sorts)
            {
                orderedQuery = orderedQuery == null
                    ? ascending ? query.OrderBy(selector) : query.OrderByDescending(selector)
                    : ascending ? orderedQuery.ThenBy(selector) : orderedQuery.ThenByDescending(selector);
            }
            query = orderedQuery ?? query;
        }

        _filtered = [.. query];

        _currentPage = maintainPosition && _filtered.Any() ? Math.Clamp(oldFirstItemIndex / _pageSize, 0, TotalPages - 1) : 0;
        RaiseAllChanged();
    }

    /// <summary>
    /// Clears the search criteria from the collection asynchronously.
    /// Resets search selectors, search term, and wildcard usage, and applies filtering.
    /// </summary>
    public async Task ClearSearchAsync(int debounceMilliseconds = 0)
    {
        _searchSelectors = [];
        _searchTerm = null;
        _useWildcards = false;

        if (debounceMilliseconds > 0)
        {
            IsSearching = true;
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            try
            {
                await Task.Delay(debounceMilliseconds, token);
                ApplyFiltering(false);
                SearchChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (TaskCanceledException) { }
            finally
            {
                IsSearching = false;
            }
        }
        else
        {
            ApplyFiltering(false);
            SearchChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Sets the sort criteria for the collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to sort by.</typeparam>
    /// <param name="selector">A function to extract the key for sorting.</param>
    /// <param name="ascending">A value indicating whether to sort in ascending order.</param>
    public void SetSort<TKey>(Func<T, TKey> selector, bool ascending)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _sorts.Clear();
        _sorts.Add((x => selector(x)!, ascending));
        ApplyFiltering(true);

        OnPropertyChanged(nameof(IsSorted));
        SortChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the sort criteria from the collection.
    /// </summary>
    public void ClearSort()
    {
        _sorts.Clear();
        ApplyFiltering(false);

        OnPropertyChanged(nameof(IsSorted));
        SortChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyFiltering(bool maintainPosition = false)
    {
        var oldFirstItemIndex = _currentPage * _pageSize;
        IEnumerable<T> query = _source;

        foreach (var filter in _filters.Values)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(_searchTerm) && _searchSelectors.Any())
        {
            var term = _searchTerm;
            Func<string, bool> matches;

            if (_useWildcards)
            {
                var regex = new Regex(WildcardToRegex(term), RegexOptions.IgnoreCase);
                matches = regex.IsMatch;
            }
            else
            {
                matches = s => s.Contains(term, StringComparison.OrdinalIgnoreCase);
            }

            // OR logic for multi-column search
            query = query.Where(item => _searchSelectors.Any(selector => matches(selector(item) ?? string.Empty)));
        }

        if (_sorts.Count > 0)
        {
            IOrderedEnumerable<T>? orderedQuery = null;
            foreach (var (selector, ascending) in _sorts)
            {
                orderedQuery = orderedQuery == null
                    ? ascending ? query.OrderBy(selector) : query.OrderByDescending(selector)
                    : ascending ? orderedQuery.ThenBy(selector) : orderedQuery.ThenByDescending(selector);
            }
            query = orderedQuery ?? query;
        }

        _filtered = [.. query];

        _currentPage = maintainPosition && _filtered.Any() ? Math.Clamp(oldFirstItemIndex / _pageSize, 0, TotalPages - 1) : 0;
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
        [.. CurrentPageItems.Cast<object>()];

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
    /// Gets a value indicating whether the filtered collection is empty.
    /// </summary>
    public bool IsEmpty => _filtered.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the filtered collection has any items.
    /// </summary>
    public bool HasItems => _filtered.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the original source collection is empty.
    /// </summary>
    public bool IsSourceEmpty => _source.Count == 0;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNext => _currentPage < TotalPages - 1;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPrevious => _currentPage > 0;
    /// <summary>
    /// Gets the keys of the active filters.
    /// </summary>
    public IReadOnlyCollection<string> ActiveFilters => GetActiveFilters();
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
            PageChanged?.Invoke(this, EventArgs.Empty);
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
            PageChanged?.Invoke(this, EventArgs.Empty);
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
            PageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Moves to the first page.
    /// </summary>
    /// <summary>
    /// Moves to the first page.
    /// </summary>
    public void GoToFirstPage() => GoToPage(1);

    /// <summary>
    /// Moves to the last page.
    /// </summary>
    public void GoToLastPage() => GoToPage(TotalPages);

    /// <summary>
    /// Resets the current page to the first page.
    /// </summary>
    public void ResetToFirstPage() => GoToPage(1);

    /// <summary>
    /// Sets the number of items to display per page.
    /// </summary>
    /// <param name="newSize">The new page size.</param>
    public void SetPageSize(int newSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newSize);

        var oldFirstItemIndex = _currentPage * _pageSize;
        _pageSize = newSize;
        _currentPage = oldFirstItemIndex / _pageSize;

        RaiseAllChanged();
        PageSizeChanged?.Invoke(this, EventArgs.Empty);

        OnPropertyChanged(nameof(PageSize));
    }

    private void RaiseAllChanged()
    {
        OnPropertyChanged(nameof(CurrentPageItems));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasNext));
        OnPropertyChanged(nameof(HasPrevious));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(IsSourceEmpty));
        OnPropertyChanged(nameof(TotalItems));
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>
    /// Disposes the resources used by the PagedCollection.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
    }
}