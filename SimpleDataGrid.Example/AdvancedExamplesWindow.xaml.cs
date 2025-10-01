using System.Windows;
using System.Windows.Controls;

namespace SimpleDataGrid.Example;

public partial class AdvancedExamplesWindow : Window
{
    public AdvancedExamplesWindow() => InitializeComponent();

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        viewModel.People.PreviousPage();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        viewModel.People.NextPage();
    }

    private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => await ApplyMultiColumnSearch();

    private async void SearchOption_Changed(object sender, RoutedEventArgs e) => await ApplyMultiColumnSearch();

    private async Task ApplyMultiColumnSearch()
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        var selectors = new List<Func<Person, string>>();

        if (viewModel.SearchByName) selectors.Add(p => p.Name);
        if (viewModel.SearchByEmail) selectors.Add(p => p.Email);
        if (viewModel.SearchByDepartment) selectors.Add(p => p.Department);

        if (selectors.Count != 0)
        {
            await viewModel.People.SetSearchAsync(
                selectors,
                SearchTextBox.Text,
                WildcardCheckBox.IsChecked == true,
                300
            );
        }
        else
        {
            await viewModel.People.ClearSearchAsync();
        }
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        if (int.TryParse(MinAgeTextBox.Text, out var minAge))
        {
            viewModel.ApplyFilter(minAge);
        }
    }

    private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        viewModel.ClearFilter();
    }

    private void FirstButton_Click(object sender, RoutedEventArgs e)
        => ((AdvancedExamplesViewModel)DataContext).People.GoToFirstPage();

    private void LastButton_Click(object sender, RoutedEventArgs e)
        => ((AdvancedExamplesViewModel)DataContext).People.GoToLastPage();

    private void GoToPageButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        if (int.TryParse(PageTextBox.Text, out var page))
        {
            viewModel.People.GoToPage(page);
        }
    }

    private void MaxAgeFilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        if (int.TryParse(MaxAgeTextBox.Text, out var maxAge))
        {
            viewModel.ApplyMaxAgeFilter(maxAge);
        }
    }

    private void NamePrefixFilterButton_Click(object sender, RoutedEventArgs e)
        => ((AdvancedExamplesViewModel)DataContext).ApplyNameFilter(NamePrefixTextBox.Text);

    private void RemoveFilterButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (AdvancedExamplesViewModel)DataContext;
        if (sender is Button button && button.CommandParameter is string key)
        {
            viewModel.RemoveFilter(key);
        }
    }

    private void ApplyCustomFilterButton_Click(object sender, RoutedEventArgs e)
        => ((AdvancedExamplesViewModel)DataContext).ApplyCustomFilter();

    private void ExportToCsvButton_Click(object sender, RoutedEventArgs e)
        => ((AdvancedExamplesViewModel)DataContext).ExportCurrentPageToCsv();
}