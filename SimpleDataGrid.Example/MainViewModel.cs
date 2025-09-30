
using SimpleDataGrid.Pagination;
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

    private List<Person> GetPeople()
    {
        var people = new List<Person>();
        for (int i = 1; i <= 100; i++)
        {
            people.Add(new Person { Id = i, Name = $"Person {i}", Age = 20 + (i % 50) });
        }
        return people;
    }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
