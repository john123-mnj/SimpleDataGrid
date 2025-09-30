using SimpleDataGrid.Pagination;

namespace SimpleDataGrid.Tests;

[TestClass]
public class PagedCollectionTests
{
    [TestMethod]
    public void SetSource_WithValidSource_InitializesCollection()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);

        // Act
        pagedCollection.SetSource(source);

        // Assert
        Assert.AreEqual(3, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(2, pagedCollection.CurrentPageItems);
        Assert.AreEqual(1, pagedCollection.CurrentPageItems[0]);
        Assert.AreEqual(2, pagedCollection.CurrentPageItems[1]);
    }

    [TestMethod]
    public void AddFilter_WithFilter_FiltersTheCollection()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.AddFilter(x => x > 3);

        // Assert
        Assert.AreEqual(1, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(2, pagedCollection.CurrentPageItems);
        Assert.AreEqual(4, pagedCollection.CurrentPageItems[0]);
        Assert.AreEqual(5, pagedCollection.CurrentPageItems[1]);
    }

    [TestMethod]
    public void SetSearch_WithSearchTerm_SearchesTheCollection()
    {
        // Arrange
        var source = new List<string> { "apple", "banana", "cherry", "date" };
        var pagedCollection = new PagedCollection<string>(2);
        pagedCollection.SetSource(source);

        // Act
        pagedCollection.SetSearch(x => x, "an");

        // Assert
        Assert.AreEqual(1, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.HasCount(1, pagedCollection.CurrentPageItems);
        Assert.AreEqual("banana", pagedCollection.CurrentPageItems[0]);
    }

    [TestMethod]
    public void Pagination_MovesCorrectlyBetweenPages()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6 };
        var pagedCollection = new PagedCollection<int>(2);
        pagedCollection.SetSource(source);

        // Assert initial state
        Assert.AreEqual(3, pagedCollection.TotalPages);
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.IsTrue(pagedCollection.HasNext);
        Assert.IsFalse(pagedCollection.HasPrevious);
        CollectionAssert.AreEqual(new List<int> { 1, 2 }, pagedCollection.CurrentPageItems.ToList());

        // Act: Move to next page
        pagedCollection.NextPage();

        // Assert after moving to next page
        Assert.AreEqual(2, pagedCollection.CurrentPage);
        Assert.IsTrue(pagedCollection.HasNext);
        Assert.IsTrue(pagedCollection.HasPrevious);
        CollectionAssert.AreEqual(new List<int> { 3, 4 }, pagedCollection.CurrentPageItems.ToList());

        // Act: Move to next page again (last page)
        pagedCollection.NextPage();

        // Assert after moving to last page
        Assert.AreEqual(3, pagedCollection.CurrentPage);
        Assert.IsFalse(pagedCollection.HasNext);
        Assert.IsTrue(pagedCollection.HasPrevious);
        CollectionAssert.AreEqual(new List<int> { 5, 6 }, pagedCollection.CurrentPageItems.ToList());

        // Act: Try to move next beyond last page (should do nothing)
        pagedCollection.NextPage();
        Assert.AreEqual(3, pagedCollection.CurrentPage); // Should still be on page 3

        // Act: Move to previous page
        pagedCollection.PreviousPage();

        // Assert after moving to previous page
        Assert.AreEqual(2, pagedCollection.CurrentPage);
        Assert.IsTrue(pagedCollection.HasNext);
        Assert.IsTrue(pagedCollection.HasPrevious);
        CollectionAssert.AreEqual(new List<int> { 3, 4 }, pagedCollection.CurrentPageItems.ToList());

        // Act: Move to previous page again (first page)
        pagedCollection.PreviousPage();

        // Assert after moving to first page
        Assert.AreEqual(1, pagedCollection.CurrentPage);
        Assert.IsTrue(pagedCollection.HasNext);
        Assert.IsFalse(pagedCollection.HasPrevious);
        CollectionAssert.AreEqual(new List<int> { 1, 2 }, pagedCollection.CurrentPageItems.ToList());

        // Act: Try to move previous beyond first page (should do nothing)
        pagedCollection.PreviousPage();
        Assert.AreEqual(1, pagedCollection.CurrentPage); // Should still be on page 1
    }
}