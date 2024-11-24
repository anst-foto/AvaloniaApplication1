using System.Text.Json.Serialization;

namespace AvaloniaApplication1.Models;

public class Person
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("age")]
    public int Age { get; set; }
}