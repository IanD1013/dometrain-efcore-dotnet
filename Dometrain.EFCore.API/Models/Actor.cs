namespace Dometrain.EFCore.API.Models;

public class Actor
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public List<Movie> Movies { get; set; } = new(); // navigation property to go from actors to movies
}