using SimpleDataGrid.Pagination;
using System;
using System.Collections.Generic;

namespace SimpleDataGrid.Example;

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
        People.SetFilter("minAge", p => p.Age >= minAge);
    }

    public void ApplyMaxAgeFilter(int maxAge)
    {
        People.SetFilter("maxAge", p => p.Age <= maxAge);
    }

    public void ApplyNameFilter(string namePrefix)
    {
        People.SetFilter("namePrefix", p => p.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveFilter(string key)
    {
        People.RemoveFilter(key);
    }

    public IReadOnlyCollection<string> ActiveFilters => People.GetActiveFilters();

    public void ClearFilter()
    {
        People.ClearFilters();
    }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
