# SimpleDataGrid

[![Build Status](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/tests.yml/badge.svg)](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/tests.yml)
[![Open Issues](https://img.shields.io/github/issues/DerekGooding/SimpleDataGrid)](https://github.com/DerekGooding/SimpleDataGrid/issues)
[![NuGet Version](https://img.shields.io/nuget/v/SimpleDataGrid)](https://www.nuget.org/packages/SimpleDataGrid/)
[![API Docs](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/api-docs.yml/badge.svg)](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/api-docs.yml)

A simple DataGrid for WPF that supports pagination, filtering, and searching.

## Features

*   **Pagination:** Easily page through large datasets, with navigation controls and page size selection.
*   **Filtering:** Filter data based on custom criteria, including named filters for easy management.
*   **Searching:** Search for data using strings, wildcards, and multi-column search with debouncing.
*   **Sorting:** Sort data by clicking on column headers.
*   **Empty State Support:** Provides clear feedback when no items are found after filtering or searching.
*   **Observability Events:** Exposes events for page changes, filter changes, search changes, and sort changes.

## Installation

You can install the `SimpleDataGrid` NuGet package using either the .NET CLI or the NuGet Package Manager.

### .NET CLI

```bash
dotnet add package SimpleDataGrid
```

### NuGet Package Manager

```powershell
Install-Package SimpleDataGrid
```

## Usage

1.  Install the `SimpleDataGrid` NuGet package.
2.  Add the `PagedDataGrid` control to your XAML.
3.  Create a `PagedCollection` and bind it to the `PagedSource` property of the `PagedDataGrid`.

```xaml
<Window x:Class="SimpleDataGrid.Example.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:SimpleDataGrid.Example"
        xmlns:controls="clr-namespace:SimpleDataGrid.Controls;assembly=SimpleDataGrid"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">
    <Window.DataContext>
        <local:MainViewModel />
    </Window.DataContext>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="5">
            <TextBox x:Name="SearchTextBox" Width="200" Margin="5" TextChanged="SearchTextBox_TextChanged"/>
            <Button Content="Search" Click="SearchButton_Click" Margin="5" />
            <CheckBox x:Name="WildcardCheckBox" Content="Use Wildcards" VerticalAlignment="Center" Margin="5" />
            <Button Content="Advanced Examples" Click="AdvancedExamplesButton_Click" Margin="5"/>
        </StackPanel>

        <controls:PagedDataGrid x:Name="PagedDataGrid" Grid.Row="1" PagedSource="{Binding People}" AutoGenerateColumns="True" />

        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Center">
            <Button Content="Previous" Click="PreviousButton_Click" Margin="5" />
            <TextBlock Text="{Binding People.CurrentPage}" VerticalAlignment="Center" Margin="5" />
            <TextBlock Text="/" VerticalAlignment="Center" />
            <TextBlock Text="{Binding People.TotalPages}" VerticalAlignment="Center" Margin="5" />
            <Button Content="Next" Click="NextButton_Click" Margin="5" />
        </StackPanel>
    </Grid>
</Window>
```

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

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
        viewModel.People.SetSearch(p => p.Name, SearchTextBox.Text, WildcardCheckBox.IsChecked == true, 300);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (MainViewModel)DataContext;
        viewModel.People.SetSearch(p => p.Name, SearchTextBox.Text, WildcardCheckBox.IsChecked == true);
    }

    private void AdvancedExamplesButton_Click(object sender, RoutedEventArgs e)
    {
        var advancedExamplesWindow = new AdvancedExamplesWindow();
        advancedExamplesWindow.Show();
    }
}

public class MainViewModel
{
    public PagedCollection<Person> People { get; }

    public MainViewModel()
    {
        People = new PagedCollection<Person>(10);
        People.SetSource(GetPeople());
    }

    private static List<Person> GetPeople()
    {
        var people = new List<Person>();
        for (var i = 1; i <= 100; i++)
        {
            people.Add(new Person { Id = i, Name = $"Person {i}", Age = 20 + (i % 50), Email = $"person{i}@example.com", Department = (i % 2 == 0) ? "Sales" : "Marketing" });
        }
        return people;
    }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime HireDate { get; set; }
}

## Example Project

The `SimpleDataGrid.Example` project demonstrates various features of the `SimpleDataGrid` library. It includes a basic `MainWindow` for quick usage and an `AdvancedExamplesWindow` for showcasing more complex functionalities.

To run the example project:

```bash
dotnet run --project SimpleDataGrid.Example
```

## Performance

For detailed information on performance characteristics and best practices, refer to the [PERFORMANCE.md](PERFORMANCE.md) document.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
