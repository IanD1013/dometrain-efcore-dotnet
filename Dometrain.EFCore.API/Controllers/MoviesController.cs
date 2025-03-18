using Dometrain.EFCore.API.Data;
using Dometrain.EFCore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dometrain.EFCore.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : Controller
{
    private readonly MoviesContext _context;

    public MoviesController(MoviesContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _context.Movies
            .Include(movie => movie.Actors)
            .ToListAsync();
        
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] int id)
    {
        var movie = await _context.Movies
            .Include(m => m.Genre)
            .SingleOrDefaultAsync(m => m.Identifier == id);
        
        return movie == null ? NotFound() : Ok(movie);
    }

    // [HttpGet("by-year/{year:int}")]
    // [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    // public async Task<IActionResult> GetAllByYear([FromRoute] int year)
    // {
    //     // IQueryable<Movie> allMovies = _context.Movies; // define a query but not executed
    //     // IQueryable<Movie> filteredMovies = allMovies.Where(m => m.ReleaseDate.Year == year); // adds a filter on top of it but not executed
    //     
    //     var filteredMovies =
    //         from movie in _context.Movies
    //         where movie.ReleaseDate.Year == year
    //         select movie;
    //     
    //     return Ok(await filteredMovies.ToListAsync()); // trigger database query
    // }    
    
    [HttpGet("by-year/{year:int}")]
    [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllByYear([FromRoute] int year)
    {
        var filteredTitles = await _context.Movies
            .Where(movie => movie.ReleaseDate.Year == year)
            .Select(movie => new MovieTitle { Id = movie.Identifier, Title = movie.Title }) 
            .ToListAsync();
        
        return Ok(filteredTitles); 
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] Movie movie)
    {
        await _context.Movies.AddAsync(movie);
        // movie has no id
        await _context.SaveChangesAsync();
        // movie has an id
        
        return CreatedAtAction(nameof(Get), new { id = movie.Identifier }, movie);
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Movie movie)
    {
        var existingMovie = await _context.Movies.FindAsync(id);
        
        if (existingMovie is null)
            return NotFound();
        
        existingMovie.Title = movie.Title;
        existingMovie.ReleaseDate = movie.ReleaseDate;
        existingMovie.Synopsis = movie.Synopsis;
        
        await _context.SaveChangesAsync();
             
        return Ok(existingMovie);
    }
    
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove([FromRoute] int id)
    {
        var existingMovie = await _context.Movies.FindAsync(id);
        
        if (existingMovie is null)
            return NotFound();
        
        // _context.Movies.Remove(existingMovie); -> no need to do it on the dbset, can do it on context level
        // _context.Movies.Remove(new Movie { Id = id });
        _context.Remove(existingMovie);
        
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    private static readonly Func<MoviesContext, AgeRating, IEnumerable<MovieTitle>> CompiledQuery = 
        EF.CompileQuery(
            (MoviesContext context, AgeRating ageRating) 
            => context.Movies
                    .Where(movie => movie.AgeRating <= ageRating)
                    .Select(movie => new MovieTitle { Id = movie.Identifier, Title = movie.Title })
            );
    
    [HttpGet("until-age/{ageRating}")]
    [ProducesResponseType(typeof(List<MovieTitle>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUntilAge([FromRoute] AgeRating ageRating)
    {
        var filteredTitles = CompiledQuery(_context, ageRating).ToList();

        return Ok(filteredTitles);
    }
}