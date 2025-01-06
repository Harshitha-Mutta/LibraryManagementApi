using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LibraryManagementApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreRepository<Genre> _genreRepository;

        public GenreController(IGenreRepository<Genre> genreRepository)
        {
            _genreRepository = genreRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllGenres()
        {
            var genres = await _genreRepository.GetAllAsync();
            return Ok(genres);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetGenreById(int id)
        {
            var genre = await _genreRepository.GetByIdAsync(id);
            if (genre == null) return NotFound();
            return Ok(genre);
        }
        [HttpGet("count")]
        [Authorize]
        public async Task<IActionResult> GetGenreCount()
        {
            var genreCount = await _genreRepository.GetGenreCountAsync();
            return Ok(genreCount);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddGenre([FromBody] Genre genre)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _genreRepository.AddAsync(genre);
                return CreatedAtAction(nameof(GetGenreById), new { id = genre.GenreID }, genre);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] Genre genre)
        {
            if (id != genre.GenreID)
                return BadRequest();

            await _genreRepository.UpdateAsync(genre);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            // Check if the genre exists
            var genre = await _genreRepository.GetByIdAsync(id);
            if (genre == null)
            {
                return NotFound(); // Returns 404 if the genre is not found
            }

            // Proceed to delete the genre
            await _genreRepository.DeleteAsync(id); // Call your repository method to delete the genre
            return NoContent(); // Returns 204 on successful deletion
        }

    }
}
