namespace SimpleDataGrid.Pagination;

public class PagedCollectionView<T>
{
    private readonly IEnumerable<T> _items;

    public int PageSize { get; }
    public int CurrentPage { get; private set; }
    public int TotalItems => _items.Count();
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public PagedCollectionView(IEnumerable<T> items, int pageSize = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        _items = items ?? throw new ArgumentNullException(nameof(items));
        PageSize = pageSize;
        CurrentPage = 1;
    }

    public IEnumerable<T> GetCurrentPage() => _items
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);

    public bool MoveNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            return true;
        }
        return false;
    }

    public bool MovePreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            return true;
        }
        return false;
    }

    public void MoveToPage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > TotalPages)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        CurrentPage = pageNumber;
    }
}
