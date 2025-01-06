using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Repository
{
    public class ReviewRepository : IReviewRepository<Review>
    {
        private readonly LibraryContext _context;

        public ReviewRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review> UpdateReviewAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewID == reviewId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByTitleAsync(string title)
        {
            return await _context.Reviews
                .Where(r => r.Book.Title == title) // Search by book title
                .Include(r => r.Book)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByNameAsync(string name)
        {
            return await _context.Reviews
                .Where(r => r.User.Name == name) // Search by user name
                .Include(r => r.Book)
                .Include(r => r.User)
                .ToListAsync();
        }


        // New method to get all reviews
        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<Dictionary<int, double>> GetAverageRatingForEachBookAsync()
        {
            // Group reviews by BookID and calculate the average rating
            var averageRatings = await _context.Reviews
                .GroupBy(r => r.BookID)
                .Select(g => new
                {
                    BookID = g.Key,
                    AverageRating = g.Average(r => r.Rating)
                })
                .ToDictionaryAsync(x => x.BookID, x => x.AverageRating);

            return averageRatings;
        }
    }
}
