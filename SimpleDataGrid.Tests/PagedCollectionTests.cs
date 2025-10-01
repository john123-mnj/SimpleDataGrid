using SimpleDataGrid.Pagination;
using SimpleDataGrid.Example;

namespace SimpleDataGrid.Tests;

[TestClass]
public class PagedCollectionTests
{
    [TestMethod]
    public void SetSource_WithEmptySource_HandlesCorrectly()
    {
        // Arrange
        var source = new List<int>();
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        pagedCollection.SetSource(source);

        // Assert
        Assert.AreEqual(1, pagedCollection.TotalPages); // Should be 1 even if empty
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(0, pagedCollection.CurrentPageItems);
        Assert.IsFalse(pagedCollection.HasNext);
        Assert.IsFalse(pagedCollection.HasPrevious);
        Assert.IsTrue(pagedCollection.IsEmpty);
        Assert.IsFalse(pagedCollection.HasItems);
        Assert.IsTrue(pagedCollection.IsSourceEmpty);
    }

    [TestMethod]
    public void SetSource_WithSingleItemSource_HandlesCorrectly()
    {
        // Arrange
        var source = new List<int> { 1 };
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        pagedCollection.SetSource(source);

        // Assert
        Assert.AreEqual(1, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(1, pagedCollection.CurrentPageItems);
        Assert.IsFalse(pagedCollection.HasNext);
        Assert.IsFalse(pagedCollection.HasPrevious);
        Assert.IsFalse(pagedCollection.IsEmpty);
        Assert.IsTrue(pagedCollection.HasItems);
        Assert.IsFalse(pagedCollection.IsSourceEmpty);
    }

    [TestMethod]
    public void SetSource_ExactPageSize_HandlesCorrectly()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        pagedCollection.SetSource(source);

        // Assert
        Assert.AreEqual(1, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(10, pagedCollection.CurrentPageItems);
        Assert.IsFalse(pagedCollection.HasNext);
        Assert.IsFalse(pagedCollection.HasPrevious);
    }

    [TestMethod]
    public void SetSource_OneItemOverPageSize_HandlesCorrectly()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        pagedCollection.SetSource(source);

        // Assert
        Assert.AreEqual(2, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(10, pagedCollection.CurrentPageItems);
        Assert.IsTrue(pagedCollection.HasNext);
        Assert.IsFalse(pagedCollection.HasPrevious);
    }

    [TestMethod]
    public void GoToPage_ValidPage_MovesToCorrectPage()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        var pagedCollection = new PagedCollection<int>(5);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.GoToPage(2);

        // Assert
        Assert.AreEqual(2, pagedCollection.CurrentPage);
        Assert.HasCount(5, pagedCollection.CurrentPageItems);
        Assert.AreEqual(6, pagedCollection.CurrentPageItems[0]);
    }

    [TestMethod]
    public void GoToPage_PageOutOfBounds_ClampsToValidPage()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        var pagedCollection = new PagedCollection<int>(5);
        pagedCollection.SetSource(source);

        // Act: Go to a page beyond the total pages
        pagedCollection.GoToPage(100);

        // Assert
        Assert.AreEqual(3, pagedCollection.CurrentPage); // Should clamp to last page
        Assert.HasCount(1, pagedCollection.CurrentPageItems);
        Assert.AreEqual(11, pagedCollection.CurrentPageItems[0]);

        // Act: Go to a page before the first page
        pagedCollection.GoToPage(0);

        // Assert
        Assert.AreEqual(1, pagedCollection.CurrentPage); // Should clamp to first page
        Assert.HasCount(5, pagedCollection.CurrentPageItems);
        Assert.AreEqual(1, pagedCollection.CurrentPageItems[0]);
    }

    [TestMethod]
    public void GoToFirstPage_MovesToFirstPage()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        var pagedCollection = new PagedCollection<int>(5);
        pagedCollection.SetSource(source);
        pagedCollection.NextPage(); // Move to second page

        // Act
        pagedCollection.GoToFirstPage();

        // Assert
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.AreEqual(1, pagedCollection.CurrentPageItems[0]);
    }

    [TestMethod]
    public void GoToLastPage_MovesToLastPage()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        var pagedCollection = new PagedCollection<int>(5);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.GoToLastPage();

        // Assert
        Assert.AreEqual(3, pagedCollection.CurrentPage);
        Assert.AreEqual(11, pagedCollection.CurrentPageItems[0]);
    }

    [TestMethod]
    public void AddFilter_WithMultipleFilters_FiltersTheCollectionWithANDLogic()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var pagedCollection = new PagedCollection<int>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.AddFilter(x => x > 3);
        pagedCollection.AddFilter(x => x < 8);

        // Assert
        Assert.AreEqual(1, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(4, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<int> { 4, 5, 6, 7 }, pagedCollection.CurrentPageItems.ToList());
    }

    [TestMethod]
    public void SetFilter_AddsOrUpdatesFilter()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(10);
        pagedCollection.SetSource(source);

        // Act: Add a filter
        pagedCollection.SetFilter("even", x => x % 2 == 0);

        // Assert
        Assert.HasCount(2, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<int> { 2, 4 }, pagedCollection.CurrentPageItems.ToList());

        // Act: Update the filter
        pagedCollection.SetFilter("even", x => x % 2 != 0);

        // Assert
        Assert.HasCount(3, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<int> { 1, 3, 5 }, pagedCollection.CurrentPageItems.ToList());
    }

    [TestMethod]
    public void RemoveFilter_RemovesFilter()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(10);
        pagedCollection.SetSource(source);
        pagedCollection.SetFilter("even", x => x % 2 == 0);

        // Act
        pagedCollection.RemoveFilter("even");

        // Assert
        Assert.HasCount(5, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 4, 5 }, pagedCollection.CurrentPageItems.ToList());
    }

    [TestMethod]
    public void ClearFilters_RemovesAllFilters()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(10);
        pagedCollection.SetSource(source);
        pagedCollection.AddFilter(x => x > 3);
        pagedCollection.SetFilter("even", x => x % 2 == 0);

        // Act
        pagedCollection.ClearFilters();

        // Assert
        Assert.HasCount(5, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 4, 5 }, pagedCollection.CurrentPageItems.ToList());
    }

    [TestMethod]
    public void SetSearch_WithEmptySearchTerm_ReturnsAllItems()
    {
        // Arrange
        var source = new List<string> { "apple", "banana", "cherry" };
        var pagedCollection = new PagedCollection<string>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSearch(x => x, "");

        // Assert
        Assert.HasCount(3, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(source, pagedCollection.CurrentPageItems.ToList());
    }

    [TestMethod]
    public void SetSearch_WithNoMatches_ReturnsEmptyCollection()
    {
        // Arrange
        var source = new List<string> { "apple", "banana", "cherry" };
        var pagedCollection = new PagedCollection<string>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSearch(x => x, "xyz");

        // Assert
        Assert.HasCount(0, pagedCollection.CurrentPageItems);
    }

    [TestMethod]
    public void SetSearch_WithPartialMatch_ReturnsMatchingItems()
    {
        // Arrange
        var source = new List<string> { "apple", "banana", "cherry", "date" };
        var pagedCollection = new PagedCollection<string>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSearch(x => x, "an");

        // Assert
        Assert.HasCount(1, pagedCollection.CurrentPageItems);
        Assert.AreEqual("banana", pagedCollection.CurrentPageItems[0]);
    }

    [TestMethod]
    public void SetSearch_WithWildcardAsterisk_ReturnsMatchingItems()
    {
        // Arrange
        var source = new List<string> { "apple", "banana", "cherry", "date" };
        var pagedCollection = new PagedCollection<string>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSearch(x => x, "*a*", true);

        // Assert
        Assert.HasCount(2, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<string> { "apple", "banana" }, pagedCollection.CurrentPageItems.ToList());
    }

    [TestMethod]
    public void SetSearch_WithWildcardQuestionMark_ReturnsMatchingItems()
    {
        // Arrange
        var source = new List<string> { "apple", "apply", "apricot" };
        var pagedCollection = new PagedCollection<string>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSearch(x => x, "appl?", true);

        // Assert
        Assert.HasCount(2, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<string> { "apple", "apply" }, pagedCollection.CurrentPageItems.ToList());
    }

    [TestMethod]
    public void SetSearch_CaseInsensitive_ReturnsMatchingItems()
    {
        // Arrange
        var source = new List<string> { "Apple", "banana", "Cherry" };
        var pagedCollection = new PagedCollection<string>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSearch(x => x, "apple");

        // Assert
        Assert.HasCount(1, pagedCollection.CurrentPageItems);
        Assert.AreEqual("Apple", pagedCollection.CurrentPageItems[0]);
    }

    [TestMethod]
    public void SetSearch_MultiColumn_ORLogic_ReturnsMatchingItems()
    {
        // Arrange
        var source = new List<Person>
        {
            new() { Name = "Alice", Email = "alice@example.com", Department = "HR" },
            new() { Name = "Bob", Email = "bob@example.com", Department = "IT" },
            new() { Name = "Charlie", Email = "charlie@example.com", Department = "HR" }
        };
        var pagedCollection = new PagedCollection<Person>(10);
        pagedCollection.SetSource(source);

        // Act: Search for "HR" in Name or Department
        pagedCollection.SetSearch([p => p.Name, p => p.Department], "HR");

        // Assert
        Assert.HasCount(2, pagedCollection.CurrentPageItems);
        CollectionAssert.AreEqual(new List<string> { "Alice", "Charlie" }, pagedCollection.CurrentPageItems.Select(p => p.Name).ToList());
    }

    [TestMethod]
    public void SetSource_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => pagedCollection.SetSource(null!));
    }

    [TestMethod]
    public void AddFilter_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => pagedCollection.AddFilter(null!));
    }

    [TestMethod]
    public void SetFilter_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => pagedCollection.SetFilter(null!, _ => true));
    }

    [TestMethod]
    public void SetFilter_WithNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => pagedCollection.SetFilter("key", null!));
    }

    [TestMethod]
    public void RemoveFilter_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => pagedCollection.RemoveFilter(null!));
    }

    [TestMethod]
    public void SetSearchAll_WithNullSelectors_ThrowsArgumentNullException()
    {
        // Arrange
        var pagedCollection = new PagedCollection<string>(10);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => pagedCollection.SetSearchAll(null!, "term"));
    }

    [TestMethod]
    public void SetSort_WithNullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var pagedCollection = new PagedCollection<int>(10);

        // Act
        Assert.ThrowsExactly<ArgumentNullException>(() => pagedCollection.SetSort<int>(null!, true));
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForCurrentPageItems()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.CurrentPageItems))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.NextPage();

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForCurrentPage()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.CurrentPage))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.NextPage();

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForTotalPages()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.TotalPages))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.AddFilter(x => x > 0); // Trigger a change that affects TotalPages

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForHasNext()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.HasNext))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.NextPage();

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForHasPrevious()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.HasPrevious))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.NextPage();

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForIsEmpty()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.IsEmpty))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.ClearFilters(); // Make it empty

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForHasItems()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.HasItems))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.ClearFilters(); // Make it empty

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForIsSourceEmpty()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.IsSourceEmpty))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.SetSource([]); // Make source empty

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForIsSorted()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.IsSorted))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.SetSort(x => x, true);

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void PropertyChanged_IsFiredForIsSearching()
    {
        // Arrange
        var source = new List<string> { "apple", "banana" };
        var pagedCollection = new PagedCollection<string>(2);
        pagedCollection.SetSource(source);
        var eventFired = false;
        pagedCollection.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(pagedCollection.IsSearching))
            {
                eventFired = true;
            }
        };

        // Act
        pagedCollection.SetSearch(x => x, "a", false, 10);
        System.Threading.Thread.Sleep(20); // Allow debounce timer to start

        // Assert
        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void SetSort_SortsTheCollection()
    {
        // Arrange
        var source = new List<Person>
        {
            new() { Id = 1, Name = "Bob", Age = 30 },
            new() { Id = 2, Name = "Alice", Age = 25 },
            new() { Id = 3, Name = "Charlie", Age = 35 }
        };
        var pagedCollection = new PagedCollection<Person>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSort(p => p.Name, true);

        // Assert
        CollectionAssert.AreEqual(new List<string> { "Alice", "Bob", "Charlie" }, pagedCollection.CurrentPageItems.Select(p => p.Name).ToList());
    }

    [TestMethod]
    public void SetSort_SortsTheCollectionDescending()
    {
        // Arrange
        var source = new List<Person>
        {
            new() { Id = 1, Name = "Bob", Age = 30 },
            new() { Id = 2, Name = "Alice", Age = 25 },
            new() { Id = 3, Name = "Charlie", Age = 35 }
        };
        var pagedCollection = new PagedCollection<Person>(10);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSort(p => p.Name, false);

        // Assert
        CollectionAssert.AreEqual(new List<string> { "Charlie", "Bob", "Alice" }, pagedCollection.CurrentPageItems.Select(p => p.Name).ToList());
    }

    [TestMethod]
    public void ClearSort_ClearsSorting()
    {
        // Arrange
        var source = new List<Person>
        {
            new() { Id = 1, Name = "Bob", Age = 30 },
            new() { Id = 2, Name = "Alice", Age = 25 },
            new() { Id = 3, Name = "Charlie", Age = 35 }
        };
        var pagedCollection = new PagedCollection<Person>(10);
        pagedCollection.SetSource(source);
        pagedCollection.SetSort(p => p.Name, true);

        // Act
        pagedCollection.ClearSort();

        // Assert
        CollectionAssert.AreEqual(source.ConvertAll(p => p.Name), pagedCollection.CurrentPageItems.Select(p => p.Name).ToList());
    }
}