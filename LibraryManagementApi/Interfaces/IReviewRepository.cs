using LibraryManagementApi.Models;

namespace LibraryManagementApi.Interfaces
{
    public interface IReviewRepository<T> where T : class
    {
        Task<Review> AddReviewAsync(Review review); //user
        Task<Review> UpdateReviewAsync(Review review);//user
        Task<bool> DeleteReviewAsync(int reviewId);//user
        Task<Review> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<Review>> GetReviewsByTitleAsync(string title);
        Task<IEnumerable<Review>> GetReviewsByNameAsync(string name);
        // New method to get all reviews
        Task<IEnumerable<Review>> GetAllReviewsAsync();//admin user
        Task<Dictionary<int, double>> GetAverageRatingForEachBookAsync();
    }
}
