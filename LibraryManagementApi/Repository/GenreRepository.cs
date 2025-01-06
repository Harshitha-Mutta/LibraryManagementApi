using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementApi.Repository
{
    public class GenreRepository : IGenreRepository<Genre>
    {
        private readonly LibraryContext _context;

        public GenreRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await _context.Genres.ToListAsync();
        }

        public async Task<Genre> GetByIdAsync(int id)
        {
            return await _context.Genres.FindAsync(id);
        }
        public async Task<int> GetGenreCountAsync()
        {
            // Fetch the count of genres from the database
            return await _context.Genres.CountAsync();
        }

        public async Task AddAsync(Genre entity)
        {
            var existingGenre = await _context.Genres
                .AnyAsync(g => g.GenreName == entity.GenreName);

            if (existingGenre)
            {
                throw new InvalidOperationException("Genre with this name already exists.");
            }

            _context.Genres.Add(entity);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateAsync(Genre entity)
        {
            _context.Genres.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre != null)
            {
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();
            }
        }
    }
}
