using System.Text.Json.Serialization;

namespace Dometrain.EFCore.API.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [JsonIgnore]
    public byte[] ConcurrencyToken { get; set; } = new byte[0];
    
    [JsonIgnore]
    public ICollection<Movie> Movies { get; set; } = new HashSet<Movie>();
}