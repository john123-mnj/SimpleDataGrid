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
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.SetSearchAsync(p => p.Name, SearchTextBox.Text, WildcardCheckBox.IsChecked == true, 300);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.SetSearchAsync(p => p.Name, SearchTextBox.Text, WildcardCheckBox.IsChecked == true);
    }

    private void AdvancedExamplesButton_Click(object sender, RoutedEventArgs e)
    {
        var advancedExamplesWindow = new AdvancedExamplesWindow();
        advancedExamplesWindow.Show();
    }
}