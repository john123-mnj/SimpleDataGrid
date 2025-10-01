using SimpleDataGrid.Controls;
using System.Windows;
using System.Windows.Controls;

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

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyMultiColumnSearch();
    }

    private void SearchOption_Changed(object sender, RoutedEventArgs e)
    {
        ApplyMultiColumnSearch();
    }

    private void ApplyMultiColumnSearch()
    {
        var viewModel = (MainViewModel)DataContext;
        var selectors = new List<Func<Person, string>>();

        if (viewModel.SearchByName) selectors.Add(p => p.Name);
        if (viewModel.SearchByEmail) selectors.Add(p => p.Email);
        if (viewModel.SearchByDepartment) selectors.Add(p => p.Department);

        viewModel.People.SetSearch(selectors, SearchTextBox.Text, WildcardCheckBox.IsChecked == true, 300);
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

    private void MaxAgeFilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        if (int.TryParse(MaxAgeTextBox.Text, out var maxAge))
        {
            viewModel.ApplyMaxAgeFilter(maxAge);
        }
    }

    private void NamePrefixFilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.ApplyNameFilter(NamePrefixTextBox.Text);
    }

    private void RemoveFilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        if (sender is Button button && button.CommandParameter is string key)
        {
            viewModel.RemoveFilter(key);
        }
    }

    private void AdvancedExamplesButton_Click(object sender, RoutedEventArgs e)
    {
        var advancedExamplesWindow = new AdvancedExamplesWindow();
        advancedExamplesWindow.Show();
    }
}