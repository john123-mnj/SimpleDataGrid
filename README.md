# SimpleDataGrid

[![Build Status](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/tests.yml/badge.svg)](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/tests.yml)
[![Open Issues](https://img.shields.io/github/issues/DerekGooding/SimpleDataGrid)](https://github.com/DerekGooding/SimpleDataGrid/issues)
[![NuGet Version](https://img.shields.io/nuget/v/SimpleDataGrid)](https://www.nuget.org/packages/SimpleDataGrid/)
[![API Docs](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/api-docs.yml/badge.svg)](https://github.com/DerekGooding/SimpleDataGrid/actions/workflows/api-docs.yml)

A powerful and simple DataGrid for WPF applications, offering seamless pagination, advanced filtering, and robust searching capabilities. Easily integrate and manage large datasets with a modern UI.

## Features

*   **Pagination:** Easily page through large datasets with comprehensive controls including `NextPage`, `PreviousPage`, `GoToPage`, `GoToFirstPage`, `GoToLastPage`, and `ResetToFirstPage`. Supports configurable page sizes and provides properties like `TotalItems`, `IsEmpty`, `HasItems`, and `IsSourceEmpty` for detailed state management.
*   **Filtering:** Filter data based on custom criteria using `AddFilter`, `SetFilter`, `RemoveFilter`, and `ClearFilters`. Supports named filters for easy management and retrieval of active filters.
*   **Searching:** Robust search functionality with `SetSearchAsync` (OR logic for multiple selectors) and `SetSearchAllAsync` (AND logic for multiple selectors). Supports wildcards (`*` and `?`), debouncing for efficient input handling, and an `IsSearching` property to indicate active search operations.
*   **Sorting:** Sort data by clicking on column headers or programmatically using `SetSort` and `ClearSort`. The `IsSorted` property indicates the current sort status.
*   **Empty State Support:** Provides clear feedback when no items are found after filtering or searching, leveraging `IsEmpty` and `HasItems` properties.
*   **Observability Events:** Exposes a rich set of events including `PageSizeChanged`, `SortChanged`, `FilterChanged`, `PageChanged`, `SourceChanged`, and `SearchChanged` for reactive UI updates.
*   **Resource Management:** Implements `IDisposable` for proper cleanup of resources, particularly for search debouncing mechanisms.

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

## Usage Examples

Here are some examples of how to use the `PagedCollection` for advanced filtering and searching:

### Basic Setup

```csharp
public class MyViewModel
{
    public PagedCollection<MyItem> Items { get; }

    public MyViewModel()
    {
        Items = new PagedCollection<MyItem>(pageSize: 20);
        Items.SetSource(LoadMyItems());
    }

    private List<MyItem> LoadMyItems() => /* ... load your data ... */ new List<MyItem>();
}

public class MyItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### Filtering

```csharp
// Add a filter for items in a specific category
Items.SetFilter("CategoryFilter", item => item.Category == "Electronics");

// Remove a filter
Items.RemoveFilter("CategoryFilter");

// Clear all filters
Items.ClearFilters();
```

### Searching (OR Logic)

```csharp
// Search for a term in Name OR Category, with debouncing
await Items.SetSearchAsync(
    selectors: new Func<MyItem, string>[] { item => item.Name, item => item.Category },
    term: "laptop",
    useWildcards: true,
    debounceMilliseconds: 300
);

// Clear search
await Items.ClearSearchAsync();
```

### Searching (AND Logic)

```csharp
// Search for a term in Name AND Category, with debouncing
await Items.SetSearchAllAsync(
    selectors: new Func<MyItem, string>[] { item => item.Name, item => item.Category },
    term: "gaming",
    useWildcards: false,
    debounceMilliseconds: 300
);
```

### Sorting

```csharp
// Sort by Price in ascending order
Items.SetSort(item => item.Price, ascending: true);

// Clear sorting
Items.ClearSort();
```

### Pagination Controls

```csharp
// Go to the next page
Items.NextPage();

// Go to a specific page
Items.GoToPage(5);

// Change page size
Items.SetPageSize(10);
```

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
