# SimpleDataGrid Improvement Tasks

## Priority 1: Critical Issues

### Task 1.1: Fix Generic Control XAML Usage
**Problem**: `PagedDataGrid<T>` cannot be easily used in XAML because WPF doesn't support generic types well. Users must create derived classes like `PersonPagedDataGrid` (see `SimpleDataGrid.Example/MainWindow.xaml.cs`).

**Current workaround in example**:
```csharp
public class PersonPagedDataGrid : PagedDataGrid<Person>;
```
Then used as: `<local:PersonPagedDataGrid PagedSource="{Binding People}" />`

**Solution**: Create a non-generic base `PagedDataGrid` class that works with any type through object binding.

**Files to modify**:
- `SimpleDataGrid/Controls/PagedDataGrid.cs`
- `SimpleDataGrid.Example/MainWindow.xaml` (update to use non-generic version)
- `SimpleDataGrid.Example/MainWindow.xaml.cs` (remove PersonPagedDataGrid class)

**Requirements**:
- Create non-generic `PagedDataGrid` class that accepts `IPagedCollection` interface
- Keep type-safe `PagedDataGrid<T>` as optional for code-behind scenarios
- Ensure XAML can bind directly: `<controls:PagedDataGrid PagedSource="{Binding People}" />`
- Update example project to demonstrate simplified usage
- Add interface `IPagedCollection` to enable non-generic binding
- Maintain backward compatibility if possible

---

### Task 1.2: Add Sorting Support
**Problem**: No way to sort columns - essential for DataGrid functionality.

**Solution**: Implement column sorting integrated with WPF DataGrid's built-in sorting.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`
- `SimpleDataGrid/Controls/PagedDataGrid.cs`

**Requirements**:
- Add `SetSort<TKey>(Func<T, TKey> selector, bool ascending)` method
- Add `ClearSort()` method
- Integrate with DataGrid column header clicks
- Support multiple sort levels (primary, secondary, etc.)
- Add `IsSorted` property
- Raise `SortChanged` event

---

## Priority 2: Essential Features

### Task 2.1: Add Page Navigation Enhancements
**Problem**: Limited navigation - can only go next/previous, no way to jump to specific page or see total items. Current example only shows "Page X / Y".

**Solution**: Add comprehensive page navigation methods and properties.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`
- `SimpleDataGrid.Example/MainWindow.xaml` (add jump to page UI and item count display)
- `SimpleDataGrid.Example/MainWindow.xaml.cs` (add GoToPage handler)

**Current example navigation**:
```xml
<Button Content="Previous" Click="PreviousButton_Click" />
<TextBlock Text="{Binding People.CurrentPage}" />
<TextBlock Text="/" />
<TextBlock Text="{Binding People.TotalPages}" />
<Button Content="Next" Click="NextButton_Click" />
```

**Requirements**:
- Add `GoToPage(int page)` method with validation
- Add `GoToFirstPage()` method
- Add `GoToLastPage()` method
- Add `TotalItems` property: `public int TotalItems => _filtered.Count;`
- Add page validation to prevent out-of-range navigation
- Fix edge case where `TotalPages` returns 0 for empty collections (should return 1)
- Update example to show: "Showing items 1-10 of 100" or similar
- Add TextBox + "Go" button for jumping to specific page
- Add "First" and "Last" buttons to example
- Disable Previous/First when on first page, Next/Last when on last page

---

### Task 2.2: Improve Filtering API
**Problem**: Can only add filters, not remove specific ones. No way to identify filters. Current example clears all filters at once.

**Current example usage**:
```csharp
public void ApplyFilter(int minAge)
{
    People.ClearFilters();  // Must clear everything first
    People.AddFilter(p => p.Age >= minAge);
}
```

**Solution**: Implement named filter system for better filter management.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`
- `SimpleDataGrid.Example/MainViewModel.cs` (demonstrate named filters)
- `SimpleDataGrid.Example/MainWindow.xaml` (add UI for multiple filters)
- `SimpleDataGrid.Example/MainWindow.xaml.cs` (add handlers)

**Requirements**:
- Change `_filters` from `List<Func<T, bool>>` to `Dictionary<string, Func<T, bool>>`
- Add `SetFilter(string key, Func<T, bool> filter)` method
- Add `RemoveFilter(string key)` method
- Keep `AddFilter` for anonymous filters (generate GUID key internally)
- Add `GetActiveFilters()` to return list of active filter keys
- Maintain backward compatibility with existing `AddFilter` and `ClearFilters`
- Update example to have multiple filter options:
  - Min Age filter (named "minAge")
  - Max Age filter (named "maxAge")  
  - Name starts with filter (named "namePrefix")
- Add individual "Remove" buttons next to each active filter
- Show list of active filters in UI

---

### Task 2.3: Add Runtime Page Size Changes
**Problem**: Page size is set at construction and cannot be changed. Current example uses fixed 10 items per page.

**Solution**: Allow dynamic page size changes.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`
- `SimpleDataGrid.Example/MainWindow.xaml` (add page size ComboBox)
- `SimpleDataGrid.Example/MainWindow.xaml.cs` (add event handler)
- `SimpleDataGrid.Example/MainViewModel.cs` (expose page size property)

**Requirements**:
- Change `_pageSize` from `readonly int` to `int`
- Add `PageSize` property with getter and setter
- Add `SetPageSize(int newSize)` method with validation
- Recalculate current page position when size changes to maintain approximate scroll position
- Add `PageSizeChanged` event
- Validate new page size (must be > 0)
- Update example project with ComboBox for 10, 25, 50, 100 options
- Bind ComboBox to PageSize property or handle SelectionChanged event

---

### Task 2.4: Add Observability Events
**Problem**: No events for tracking changes beyond PropertyChanged.

**Solution**: Add specific events for better integration.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`

**Requirements**:
- Add `event EventHandler? PageChanged`
- Add `event EventHandler? FilterChanged`
- Add `event EventHandler? SourceChanged`
- Add `event EventHandler? SearchChanged`
- Add `event EventHandler? SortChanged`
- Fire appropriate events when operations occur
- Include event args with relevant data (old/new values)

---

## Priority 3: UX Improvements

### Task 3.1: Add Empty State Support
**Problem**: No indication when filtered results are empty. Example just shows empty grid.

**Solution**: Add properties to detect and communicate empty states, and demonstrate in example.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`
- `SimpleDataGrid.Example/MainWindow.xaml` (add empty state overlay)
- `SimpleDataGrid.Example/MainViewModel.cs` (if needed for binding)

**Current behavior**: 
- When search/filter returns no results, grid is just empty
- No user feedback about why grid is empty

**Requirements**:
- Add `IsEmpty` property: `public bool IsEmpty => _filtered.Count == 0;`
- Add `HasItems` property: `public bool HasItems => _filtered.Count > 0;`
- Add `IsSourceEmpty` property to check original source
- Raise PropertyChanged for these properties when filtering changes
- Update example with empty state overlay:
  - TextBlock with "No items found" message
  - Visibility bound to IsEmpty property
  - Suggest clearing filters/search
  - Style with subtle background and icon if desired

---

### Task 3.2: Add Search Debouncing Support
**Problem**: Search triggers on every keystroke, causing performance issues. Current example uses Button click to trigger search.

**Current example approach**:
```xml
<TextBox x:Name="SearchTextBox" Width="200" />
<Button Content="Search" Click="SearchButton_Click" />
```

**Solution**: Add debounce capability to search functionality and demonstrate in example.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`
- `SimpleDataGrid.Example/MainWindow.xaml` (change to TextChanged event)
- `SimpleDataGrid.Example/MainWindow.xaml.cs` (add debounced search handler)

**Requirements**:
- Add optional `debounceMilliseconds` parameter to `SetSearch` method
- Implement debounce timer using `System.Threading.Timer`
- Cancel previous timer when new search is triggered
- Add `ClearSearch()` method
- Add `IsSearching` property
- Ensure thread-safe PropertyChanged notifications
- Update example to use TextBox.TextChanged instead of Button
- Demonstrate 300ms debounce in example
- Optionally add loading indicator that shows while IsSearching
- Keep Button approach as alternative in AdvancedExamplesWindow

---

### Task 3.3: Add Multi-Column Search
**Problem**: Search only works on one property at a time. Current example only searches Name field.

**Current example usage**:
```csharp
viewModel.People.SetSearch(p => p.Name, SearchTextBox.Text, WildcardCheckBox.IsChecked == true);
```

**Solution**: Allow searching across multiple properties simultaneously.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`
- `SimpleDataGrid.Example/MainWindow.xaml` (add multi-column search demo)
- `SimpleDataGrid.Example/MainWindow.xaml.cs` (update search handler)
- `SimpleDataGrid.Example/MainViewModel.cs` (add Person properties: Email, Department)

**Requirements**:
- Change `_searchSelector` to `_searchSelectors` (list)
- Add `SetSearch(IEnumerable<Func<T, string>> selectors, string term, bool useWildcards = false)`
- Keep single-property `SetSearch` for convenience (backward compatibility)
- Search should match if ANY selector matches the term (OR logic)
- Add `SetSearchAll` variant that requires ALL selectors to match (AND logic)
- Update example Person model to have more searchable fields:
  ```csharp
  public string Email { get; set; }
  public string Department { get; set; }
  ```
- Update example to search across Name, Email, and Department
- Add CheckBoxes to toggle which fields to search
- Show in UI which fields are being searched

---

## Priority 4: Performance & Code Quality

### Task 4.1: Optimize Filtering Performance
**Problem**: `ApplyFiltering()` recreates entire filtered list on every change, using collection expressions that allocate new arrays.

**Solution**: Optimize filtering for large datasets.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`

**Requirements**:
- Use `ToList()` instead of collection expressions `[..]` for better performance
- Cache filter results when possible
- Consider incremental filtering (apply only new filters to existing results)
- Add performance comments for large dataset handling
- Profile and document performance characteristics

---

### Task 4.2: Fix Edge Cases and Null Safety
**Problem**: Several potential null reference and edge case issues.

**Solution**: Improve code robustness.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`

**Requirements**:
- Fix potential null reference in search: `_searchSelector(x)?.Contains(...)`
- Ensure `TotalPages` returns at least 1: `Math.Max(1, (int)Math.Ceiling(...))`
- Validate `_currentPage` stays in range after filtering (try to maintain position or clamp to valid range)
- Add null checks for all public method parameters
- Add argument validation with descriptive error messages
- Handle thread safety for PropertyChanged events

---

### Task 4.3: Improve Current Page Maintenance
**Problem**: `_currentPage` always resets to 0 when filtering, losing user's position.

**Solution**: Maintain current page position when possible.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`

**Requirements**:
- In `ApplyFiltering()`, calculate equivalent page after filtering
- Try to keep the first visible item in view if possible
- If current page becomes invalid, move to last valid page instead of first
- Add `bool maintainPosition = true` parameter to filtering methods
- Add `ResetToFirstPage()` method for explicit reset

---

## Priority 5: Documentation & Examples

### Task 5.1: Add XML Documentation for All Public Members
**Problem**: Some members lack comprehensive documentation.

**Solution**: Add complete XML documentation.

**Files to modify**:
- All files in `SimpleDataGrid/` directory

**Requirements**:
- Add `<summary>` for all public types, methods, properties, events
- Add `<param>` for all parameters with validation notes
- Add `<returns>` for all return values
- Add `<exception>` for all thrown exceptions
- Add `<example>` for complex methods
- Add `<remarks>` for performance considerations

---

### Task 5.2: Create Advanced Usage Examples
**Problem**: README only shows basic usage. The existing Example project is minimal.

**Solution**: Enhance the Example project with comprehensive demonstrations and update README.

**Files to modify**:
- `README.md`
- `SimpleDataGrid.Example/MainWindow.xaml`
- `SimpleDataGrid.Example/MainWindow.xaml.cs`
- `SimpleDataGrid.Example/MainViewModel.cs`
- Create `SimpleDataGrid.Example/AdvancedExamplesWindow.xaml` (new file)
- Create `SimpleDataGrid.Example/AdvancedExamplesWindow.xaml.cs` (new file)

**Current example features**:
- Basic search with wildcards
- Single filter (min age)
- Previous/Next navigation
- Page counter display

**Requirements for enhanced example project**:
- Add second window for advanced examples (AdvancedExamplesWindow)
- Add example: Named filters with add/remove UI
- Add example: Multi-column search across Name, Age, and Id
- Add example: Sorting with column headers
- Add example: Page size selector ComboBox (10, 25, 50, 100 items)
- Add example: "Showing X-Y of Z items" display
- Add example: Debounced search with TextBox.TextChanged
- Add example: Jump to page functionality with TextBox and Go button
- Add example: Handling empty states with message overlay
- Add example: Custom filter expressions builder
- Add example: Export current page to CSV button
- Add button to MainWindow to open AdvancedExamplesWindow
- Expand Person model with more properties (Email, Department, IsActive, HireDate)
- Generate 1000+ sample records for better pagination demonstration

**README updates**:
- Add screenshots or GIFs of example application
- Add "Quick Start" section with minimal example
- Add "Advanced Examples" section linking to example project
- Include code snippets for each advanced feature
- Add section explaining the Example project structure

---

### Task 5.3: Add Performance Guidelines Documentation
**Problem**: No guidance on when/how to use for large datasets.

**Solution**: Document performance characteristics and best practices.

**Files to modify**:
- `README.md` (add Performance section)
- Create `PERFORMANCE.md`

**Requirements**:
- Document time complexity of operations
- Provide guidance for datasets: small (<1K), medium (1K-100K), large (>100K)
- Recommend page sizes for different scenarios
- Explain filtering vs. searching performance
- Document memory usage characteristics
- Provide benchmarks if possible
- Suggest alternatives for very large datasets (database paging, virtualization)

---

## Priority 6: Testing

### Task 6.1: Add Comprehensive Unit Tests
**Problem**: Test coverage is incomplete.

**Solution**: Add thorough unit test suite.

**Files to modify**:
- `SimpleDataGrid.Tests/` directory

**Test data context**:
- Use similar Person model as Example project
- Test with varying dataset sizes (0, 1, 10, 100, 1000 items)
- Current Example uses 100 items with Age range 20-70

**Requirements**:
- Test pagination edge cases:
  - Empty collection
  - Single item
  - Exact page size (10 items, 10 per page = 1 page)
  - One item over page size (11 items, 10 per page = 2 pages)
  - Page boundary navigation (first, last, out of bounds)
- Test filter combinations:
  - No filters
  - Single filter
  - Multiple filters (AND logic)
  - Add/remove filters
  - Clear all filters
- Test search functionality:
  - Empty search term
  - No matches
  - Partial matches
  - Wildcards (* and ?)
  - Case sensitivity
  - Special characters
- Test null handling:
  - Null source
  - Null filter
  - Null search selector
  - Null/empty search term
- Test concurrent modifications (if thread-safe)
- Test PropertyChanged events fire correctly for all properties
- Test page boundary conditions (CurrentPage, TotalPages, HasNext, HasPrevious)
- Test sort stability (once sorting is implemented)
- Aim for >90% code coverage
- Use xUnit, NUnit, or MSTest framework (check existing test structure)

---

### Task 6.2: Add Integration Tests
**Problem**: No tests for WPF control integration.

**Solution**: Add WPF integration tests.

**Files to modify**:
- `SimpleDataGrid.Tests/` directory (add new test class)
- Consider creating `SimpleDataGrid.Tests/IntegrationTests/` subdirectory

**Requirements**:
- Test PagedDataGrid control binding with test data
- Test DataGrid column sorting integration
- Test PropertyChanged propagation to UI
- Test memory leaks (event handler cleanup)
- Test thread safety with UI thread (use Dispatcher)
- Test performance with large datasets (10K+ items) in UI
- Use the Person model from Example project for consistency
- Test the example project scenarios programmatically

---

## Optional Enhancements

### Task 7.1: Add Export Functionality
**Problem**: No way to export data.

**Solution**: Add export methods for current page or all filtered data.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`

**Requirements**:
- Add `ExportCurrentPage()` returning list of items
- Add `ExportAllFiltered()` returning all filtered items
- Add `ExportToCSV(string filePath)` method
- Add `ExportToJSON(string filePath)` method
- Make export methods async
- Add progress reporting for large exports

---

### Task 7.2: Implement ICollectionView Interface
**Problem**: Doesn't integrate seamlessly with WPF's collection view infrastructure.

**Solution**: Implement ICollectionView for better WPF integration.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`

**Requirements**:
- Implement `ICollectionView` interface
- Support WPF's built-in filtering/sorting through ICollectionView
- Maintain backward compatibility
- Update documentation with ICollectionView usage

---

### Task 7.3: Add Async Data Loading Support
**Problem**: Synchronous data loading blocks UI.

**Solution**: Add async loading capabilities.

**Files to modify**:
- `SimpleDataGrid/Pagination/PagedCollection.cs`

**Requirements**:
- Add `SetSourceAsync(Func<Task<IReadOnlyList<T>>> loader)` method
- Add `RefreshAsync()` method
- Add `IsLoading` property
- Add `LoadingChanged` event
- Handle cancellation tokens
- Add error handling and retry logic
- Update example with async loading pattern

---

## Task Execution Notes

**Project Structure Context:**
- Main library: `SimpleDataGrid/` - the NuGet package code
- Tests: `SimpleDataGrid.Tests/` - unit and integration tests
- Example app: `SimpleDataGrid.Example/` - WPF demo application
  - Current example is minimal (100 records, basic features)
  - Uses workaround: `PersonPagedDataGrid : PagedDataGrid<Person>`
  - Good starting point for demonstrating enhancements

**For each task:**
1. Read and understand the current implementation
2. Make changes incrementally
3. Ensure backward compatibility unless noted as breaking change
4. Add/update XML documentation
5. Add/update unit tests in `SimpleDataGrid.Tests/`
6. **Update and test with `SimpleDataGrid.Example/` project**
7. Update README if user-facing changes
8. Test manually with SimpleDataGrid.Example project
9. Consider edge cases and error handling

**Working with Example Project:**
- After implementing features, add demonstrations to Example project
- Example project serves as both manual test and documentation
- Keep MainWindow simple for basic usage, add AdvancedExamplesWindow for complex scenarios
- Expand Person model as needed for realistic demonstrations
- Increase sample data size to test pagination effectively (1000+ records recommended)

**Breaking Changes:**
- Tasks marked as "breaking change" should be grouped together for a major version bump
- Create migration guide for breaking changes
- Consider deprecation warnings before removal
- Update Example project to reflect breaking changes

**Testing:**
- Run all tests after each task: `dotnet test`
- **Run Example project after each task**: `dotnet run --project SimpleDataGrid.Example`
- Test with both .NET 8 and .NET 9 targets
- Test on different screen sizes and DPI settings
- Verify performance with large datasets (10K+ items)

**Documentation:**
- Update CHANGELOG.md with each completed task
- Update version number appropriately (semantic versioning)
- Update NuGet package description if functionality changes significantly
- **Update Example project code to showcase new features**
- Consider recording GIFs/videos of example project for README