using SimpleDataGrid.Pagination;
using System.Windows;
using System.Windows.Controls;

namespace SimpleDataGrid.Controls;

/// <summary>
/// A DataGrid that supports pagination through a <see cref="IPagedCollection"/>.
/// </summary>
public class PagedDataGrid : DataGrid
{
    /// <summary>
    /// Identifies the <see cref="PagedSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PagedSourceProperty =
        DependencyProperty.Register(nameof(PagedSource), typeof(IPagedCollection), typeof(PagedDataGrid),
            new PropertyMetadata(null, OnPagedSourceChanged));

    /// <summary>
    /// Gets or sets the paged collection that this grid displays.
    /// </summary>
    public IPagedCollection? PagedSource
    {
        get => (IPagedCollection)GetValue(PagedSourceProperty);
        set => SetValue(PagedSourceProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedDataGrid"/> class.
    /// </summary>
    public PagedDataGrid() => Sorting += OnSorting;


    private static void OnPagedSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PagedDataGrid grid && e.NewValue is IPagedCollection paged)
        {
            grid.ItemsSource = paged.CurrentPageItems;
            paged.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == nameof(paged.CurrentPageItems))
                {
                    grid.ItemsSource = paged.CurrentPageItems;
                }
            };
        }
    }

    /// <summary>
    /// Handles the <see cref="DataGrid.Sorting"/> event.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnSorting(object sender, DataGridSortingEventArgs e) { }
}

/// <summary>
/// A DataGrid that supports pagination through a <see cref="PagedCollection{T}"/>.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class PagedDataGrid<T> : PagedDataGrid
{
    /// <summary>
    /// Gets or sets the paged collection that this grid displays.
    /// </summary>
    public new PagedCollection<T>? PagedSource
    {
        get => (PagedCollection<T>?)base.PagedSource;
        set => base.PagedSource = value;
    }

    /// <summary>
    /// Handles the <see cref="DataGrid.Sorting"/> event.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event data.</param>
    protected override void OnSorting(object sender, DataGridSortingEventArgs e)
    {
        if (PagedSource == null) return;

        var direction = e.Column.SortDirection == System.ComponentModel.ListSortDirection.Ascending
            ? System.ComponentModel.ListSortDirection.Descending
            : System.ComponentModel.ListSortDirection.Ascending;

        e.Column.SortDirection = direction;

        var propertyName = e.Column.SortMemberPath;
        if (string.IsNullOrEmpty(propertyName)) return;

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, propertyName);
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, object>>(System.Linq.Expressions.Expression.Convert(property, typeof(object)), parameter);

        PagedSource.SetSort(lambda.Compile(), direction == System.ComponentModel.ListSortDirection.Ascending);

        e.Handled = true;
    }

}
