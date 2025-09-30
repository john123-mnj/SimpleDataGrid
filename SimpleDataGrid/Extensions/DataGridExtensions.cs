using SimpleDataGrid.Pagination;
using System.Windows.Controls;

namespace SimpleDataGrid.Extensions;

public static class DataGridExtensions
{
    public static void BindPagedSource<T>( this DataGrid grid, PagedCollectionView<T> pagedCollection)
        => grid.ItemsSource = pagedCollection.GetCurrentPage();

    public static void RefreshPagedSource<T>(
        this DataGrid grid,
        PagedCollectionView<T> pagedCollection)
    {
        grid.ItemsSource = null;
        grid.ItemsSource = pagedCollection.GetCurrentPage();
    }
}
