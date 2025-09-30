# <img src="https://github.com/DerekGooding/SimpleInjection/blob/main/Icon.png" width=50 height=50/> SimpleDataGrid 

A simple DataGrid for WPF that supports pagination, filtering, and searching.

## Features

*   **Pagination:** Easily page through large datasets.
*   **Filtering:** Filter data based on custom criteria.
*   **Searching:** Search for data using strings or wildcards.

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
            <TextBox x:Name="SearchTextBox" Width="200" Margin="5" />
            <Button Content="Search" Click="SearchButton_Click" Margin="5" />
            <CheckBox x:Name="WildcardCheckBox" Content="Use Wildcards" VerticalAlignment="Center" Margin="5" />

            <TextBox x:Name="MinAgeTextBox" Width="50" Margin="5" />
            <Button Content="Filter by Min Age" Click="FilterButton_Click" Margin="5" />
            <Button Content="Clear Filter" Click="ClearFilterButton_Click" Margin="5" />
        </StackPanel>

        <local:PersonPagedDataGrid x:Name="PagedDataGrid" Grid.Row="1" PagedSource="{Binding People}" AutoGenerateColumns="True" />

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
            people.Add(new Person { Id = i, Name = $"Person {i}", Age = 20 + (i % 50) });
        }
        return people;
    }

    public void ApplyFilter(int minAge)
    {
        People.ClearFilters();
        People.AddFilter(p => p.Age >= minAge);
    }

    public void ClearFilter()
    {
        People.ClearFilters();
    }
}
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
