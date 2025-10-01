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
    /// <summary>
    /// Gets or sets a value indicating whether the person is active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the hire date of the person.
    /// </summary>
    public DateTime HireDate { get; set; }
}