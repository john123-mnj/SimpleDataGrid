using System.ComponentModel;

namespace SimpleDataGrid.Pagination;

/// <summary>
/// Represents a collection of items that can be paged.
/// </summary>
public interface IPagedCollection : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the items on the current page.
    /// </summary>
    IReadOnlyList<object> CurrentPageItems { get; }

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    int CurrentPage { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    bool HasNext { get; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    bool HasPrevious { get; }

    /// <summary>
    /// Moves to the next page.
    /// </summary>
    void NextPage();

    /// <summary>
    /// Moves to the previous page.
    /// </summary>
    void PreviousPage();
}