using SimpleDataGrid.Pagination;
using System.Windows;
using System.Windows.Controls;

namespace SimpleDataGrid.Controls;

/// <summary>
/// A DataGrid that supports pagination through a <see cref="PagedCollection{T}"/>.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class PagedDataGrid<T> : DataGrid
{
    /// <summary>
    /// Identifies the <see cref="PagedSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PagedSourceProperty =
        DependencyProperty.Register(nameof(PagedSource), typeof(PagedCollection<T>), typeof(PagedDataGrid<T>),
            new PropertyMetadata(null, OnPagedSourceChanged));

    /// <summary>
    /// Gets or sets the paged collection that this grid displays.
    /// </summary>
    public PagedCollection<T>? PagedSource
    {
        get => (PagedCollection<T>)GetValue(PagedSourceProperty);
        set => SetValue(PagedSourceProperty, value);
    }

    private static void OnPagedSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PagedDataGrid<T> grid && e.NewValue is PagedCollection<T> paged)
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