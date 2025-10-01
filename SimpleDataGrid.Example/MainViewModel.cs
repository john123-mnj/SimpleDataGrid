using SimpleDataGrid.Pagination;
using System;
using System.Collections.Generic;

namespace SimpleDataGrid.Example;

/// <summary>
/// The main view model for the SimpleDataGrid example application.
/// </summary>
public class MainViewModel
{
    /// <summary>
    /// Gets the paged collection of people.
    /// </summary>
    public PagedCollection<Person> People { get; }
    /// <summary>
    /// Gets a list of available page sizes.
    /// </summary>
    public List<int> PageSizes { get; } = [10, 25, 50, 100];

    /// <summary>
    /// Gets or sets a value indicating whether to search by name.
    /// </summary>
    public bool SearchByName { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether to search by email.
    /// </summary>
    public bool SearchByEmail { get; set; } = false;
    /// <summary>
    /// Gets or sets a value indicating whether to search by department.
    /// </summary>
    public bool SearchByDepartment { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
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

    /// <summary>
    /// Applies a minimum age filter to the people collection.
    /// </summary>
    /// <param name="minAge">The minimum age to filter by.</param>
    public void ApplyFilter(int minAge)
    {
        People.SetFilter("minAge", p => p.Age >= minAge);
    }

    /// <summary>
    /// Applies a maximum age filter to the people collection.
    /// </summary>
    /// <param name="maxAge">The maximum age to filter by.</param>
    public void ApplyMaxAgeFilter(int maxAge)
    {
        People.SetFilter("maxAge", p => p.Age <= maxAge);
    }

    /// <summary>
    /// Applies a name prefix filter to the people collection.
    /// </summary>
    /// <param name="namePrefix">The name prefix to filter by.</param>
    public void ApplyNameFilter(string namePrefix)
    {
        People.SetFilter("namePrefix", p => p.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Removes a filter from the people collection.
    /// </summary>
    /// <param name="key">The key of the filter to remove.</param>
    public void RemoveFilter(string key)
    {
        People.RemoveFilter(key);
    }

    /// <summary>
    /// Gets a read-only collection of the active filter keys.
    /// </summary>
    public IReadOnlyCollection<string> ActiveFilters => People.GetActiveFilters();

    /// <summary>
    /// Clears all filters from the people collection.
    /// </summary>
    public void ClearFilter()
    {
        People.ClearFilters();
    }
}

/// <summary>
/// Represents a person with basic information.
/// </summary>
public class Person
{
    /// <summary>
    /// Gets or sets the unique identifier of the person.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the name of the person.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the age of the person.
    /// </summary>
    public int Age { get; set; }
    /// <summary>
    /// Gets or sets the email address of the person.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the department of the person.
    /// </summary>
    public string Department { get; set; } = string.Empty;
}
