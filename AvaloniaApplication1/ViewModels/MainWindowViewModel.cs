using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;
using DynamicData.Alias;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }
    
    private int? _id;
    public int? Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }
    
    private string? _name;
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    
    private int? _age;
    public int? Age
    {
        get => _age;
        set => this.RaiseAndSetIfChanged(ref _age, value);
    }

    public ObservableCollection<Person> Persons { get; } = [];
    
    private Person? _selectedPerson;
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            var person = this.RaiseAndSetIfChanged(ref _selectedPerson, value);
            if (person == null) return;
            
            Id = person.Id;
            Name = person.Name;
            Age = person.Age;
        } 
    }
    
    public ReactiveCommand<Unit, Unit> CommandSearch { get; }
    public ReactiveCommand<Unit, Unit> CommandSave { get; }
    public ReactiveCommand<Unit, Unit> CommandDelete { get; }
    public ReactiveCommand<Unit, Unit> CommandClear { get; }

    public MainWindowViewModel()
    {
        LoadPersons();

        var isSearchTextValid = this.WhenAnyValue(
            x => x.SearchText,
            p1 => !string.IsNullOrWhiteSpace(p1));

        var isInputsValid = this.WhenAnyValue(
            p1 => p1.Id,
            p2 => p2.Name,
            p3 => p3.Age,
            (p1, p2, p3) => p1 is not null && !string.IsNullOrWhiteSpace(p2) && p3 is not null);
        
        var isInputsEmpty = this.WhenAnyValue(
            p1 => p1.Id,
            p2 => p2.Name,
            p3 => p3.Age,
            (p1, p2, p3) => p1 is not null || !string.IsNullOrWhiteSpace(p2) || p3 is not null);
        
        CommandSearch = ReactiveCommand.CreateFromTask(Search, isSearchTextValid);
        
        CommandSave = ReactiveCommand.CreateFromTask(Save, isInputsValid);
        CommandDelete = ReactiveCommand.CreateFromTask(Delete, isInputsValid);
        CommandClear = ReactiveCommand.Create(Clear, isInputsEmpty);
    }
    
    private async void LoadPersons()
    {
        await using var file = File.OpenRead("data.json");
        var persons = JsonSerializer.DeserializeAsyncEnumerable<Person>(file);

        await foreach (var person in persons)
        {
            Persons.Add(person);
        }
    }
    
    private Task Search()
    {
        var persons = Persons
            .Where(x => x.Name.StartsWith(SearchText, System.StringComparison.CurrentCultureIgnoreCase))
            .ToList();
        
        Persons.Clear();

        foreach (var person in persons)
        {
            Persons.Add(person);
        }

        return Task.CompletedTask;
    }
    
    private async Task Save()
    {
        if (SelectedPerson == null)
        {
            Persons.Add(new Person
            {
                Id = Id!.Value,
                Name = Name!,
                Age = Age!.Value
            });
        }
        
        await SaveToFile();
    }

    private async Task Delete()
    {
        Persons.Remove(SelectedPerson!);

        await SaveToFile();
    }

    private void Clear()
    {
        Id = null;
        Name = null;
        Age = null;
    }

    private async Task SaveToFile()
    {
        await using var file = new FileStream("data.json", FileMode.Truncate, FileAccess.Write);
        await JsonSerializer.SerializeAsync(file, Persons);
        
        Clear();
        
        LoadPersons();
    }
}