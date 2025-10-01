using SimpleDataGrid.Pagination;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SimpleDataGrid.Example;

/// <summary>
/// The view model for the advanced examples window.
/// </summary>
public class AdvancedExamplesViewModel : INotifyPropertyChanged
{
    public PagedCollection<Person> People { get; }
    public List<int> PageSizes { get; } = [10, 25, 50, 100];

    public bool SearchByName { get; set; } = true;
    public bool SearchByEmail { get; set; } = false;
    public bool SearchByDepartment { get; set; } = false;

    public AdvancedExamplesViewModel()
    {
        People = new PagedCollection<Person>(10);
        People.SetSource(GetPeople());
    }

    private static List<Person> GetPeople()
    {
        var people = new List<Person>();
        for (var i = 1; i <= 1000; i++)
        {
            people.Add(new Person
            {
                Id = i,
                Name = $"Person {i}",
                Age = 20 + (i % 50),
                Email = $"person{i}@example.com",
                Department = (i % 3 == 0) ? "Sales" : (i % 3 == 1) ? "Marketing" : "Engineering",
                IsActive = (i % 2 == 0),
                HireDate = DateTime.Now.AddDays(-i)
            });
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
