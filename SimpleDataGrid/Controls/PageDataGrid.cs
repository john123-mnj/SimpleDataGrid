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
}
