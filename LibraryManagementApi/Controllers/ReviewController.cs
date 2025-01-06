using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementApi.Models;
using LibraryManagementApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using log4net;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Dtos;

namespace LibraryManagementApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository<Review> _reviewRepository;
        private static readonly ILog log = LogManager.GetLogger(typeof(ReviewController));

        public ReviewController(IReviewRepository<Review> reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            log.Info($"Fetching review with ID: {id}");
            var review = await _reviewRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                log.Warn($"Review with ID {id} not found");
                return NotFound();
            }
            log.Info($"Review with ID {id} fetched successfully");
            return Ok(review);
        }

        [HttpGet("title/{bookTitle}")]
        public async Task<IActionResult> GetReviewsByTitle(string bookTitle)
        {
            log.Info($"Fetching reviews for book with title: {bookTitle}");
            var reviews = await _reviewRepository.GetReviewsByTitleAsync(bookTitle);
            if (!reviews.Any())
            {
                log.Warn($"No reviews found for book with title {bookTitle}");
                return NotFound();
            }
            log.Info($"Reviews for book titled {bookTitle} fetched successfully");
            return Ok(reviews);
        }

        [HttpGet("user/{userName}")]
        public async Task<IActionResult> GetReviewsByName(string userName)
        {
            log.Info($"Fetching reviews by user: {userName}");
            var reviews = await _reviewRepository.GetReviewsByNameAsync(userName);
            if (!reviews.Any())
            {
                log.Warn($"No reviews found by user {userName}");
                return NotFound();
            }
            log.Info($"Reviews by user {userName} fetched successfully");
            return Ok(reviews);
        }




        // New endpoint to get all reviews
        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            log.Info("Fetching all reviews");
            var reviews = await _reviewRepository.GetAllReviewsAsync();
            log.Info("All reviews fetched successfully");
            var result = reviews.Select(r => new
            {
                r.ReviewID, // Review ID
                BookTitle = r.Book.Title, // Book Title
                UserName = r.User.Name, // User Name
                r.Description, // Review Description
                r.Rating, // Rating
                r.ReviewDate // Review Date
            });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewCreateDto reviewDto)
        {
            log.Info("Creating a new review...");
            if (!ModelState.IsValid)
            {
                log.Warn("Invalid review data");
                return BadRequest(ModelState);
            }
            var review = new Review
            {
                UserID = reviewDto.UserID,
                BookID = reviewDto.BookID,
                Description = reviewDto.Description,
                Rating = reviewDto.Rating,
                ReviewDate = DateTime.Now
            };
            await _reviewRepository.AddReviewAsync(review);
            log.Info($"Review for book ID {review.BookID} created successfully");
            return CreatedAtAction(nameof(GetReviewById), new { id = review.ReviewID }, review);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewCreateDto reviewDto)
        {
            log.Info($"Updating review with ID: {id}");

            // Find the review by id first
            var existingReview = await _reviewRepository.GetReviewByIdAsync(id);
            if (existingReview == null)
            {
                log.Warn($"Review with ID {id} not found");
                return NotFound();
            }

            // Update the existing review's properties
            existingReview.Description = reviewDto.Description;
            existingReview.Rating = reviewDto.Rating;
            existingReview.ReviewDate = DateTime.Now; // Optional: Update the review date if required

            await _reviewRepository.UpdateReviewAsync(existingReview);
            log.Info($"Review with ID {id} updated successfully");
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            log.Info($"Deleting review with ID: {id}");
            var success = await _reviewRepository.DeleteReviewAsync(id);
            if (!success)
            {
                log.Warn($"Review with ID {id} not found");
                return NotFound();
            }
            log.Info($"Review with ID {id} deleted successfully");
            return NoContent();
        }
        [HttpGet("average-ratings")]
        public async Task<IActionResult> GetAverageRatings()
        {
            // Call the service method to get average ratings
            var averageRatings = await _reviewRepository.GetAverageRatingForEachBookAsync();

            // Return the average ratings as a JSON response
            return Ok(averageRatings);
        }
    }
}
