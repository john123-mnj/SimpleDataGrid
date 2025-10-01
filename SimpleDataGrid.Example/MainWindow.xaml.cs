using SimpleDataGrid.Controls;
using System.Windows;

namespace SimpleDataGrid.Example;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.PreviousPage();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.NextPage();
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.SetSearch(p => p.Name, SearchTextBox.Text, WildcardCheckBox.IsChecked == true);
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        if (int.TryParse(MinAgeTextBox.Text, out var minAge))
        {
            viewModel.ApplyFilter(minAge);
        }
    }

    private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.ClearFilter();
    }

    private void FirstButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.GoToFirstPage();
    }

    private void LastButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.GoToLastPage();
    }

    private void GoToPageButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        if (int.TryParse(PageTextBox.Text, out var page))
        {
            viewModel.People.GoToPage(page);
        }
    }
}