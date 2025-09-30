using System.Windows;
using System.Windows.Controls;
using SimpleDataGrid.Pagination;

namespace SimpleDataGrid.Controls;

public class PagedDataGrid : DataGrid
{
    public static readonly DependencyProperty PagedSourceProperty =
        DependencyProperty.Register(nameof(PagedSource), typeof(object), typeof(PagedDataGrid),
            new PropertyMetadata(null, OnPagedSourceChanged));

    public object? PagedSource
    {
        get => GetValue(PagedSourceProperty);
        set => SetValue(PagedSourceProperty, value);
    }

    private static void OnPagedSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PagedDataGrid grid && e.NewValue is IPagedSource paged)
        {
            grid.ItemsSource = paged.CurrentPageItems;
        }
    }
}

public interface IPagedSource
{
    object CurrentPageItems { get; }
}
