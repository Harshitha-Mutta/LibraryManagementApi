using LibraryManagementApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Services;
using LibraryManagementApi.Dtos;
using Razorpay.Api;

namespace LibraryManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize()]
    public class FineController : ControllerBase
    {
        private readonly IFineRepository<Fine> _fineRepository;
        private readonly FineService _fineService;

        public FineController(IFineRepository<Fine> fineRepository, FineService fineService)
        {
            _fineRepository = fineRepository;
            _fineService = fineService;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllFines()
        {
            //log.Info("Fetching all fines...");

            // Fetch all fines, including user and book details
            var fines = await _fineRepository.GetAllAsync();

            // Create an anonymous type to shape the data
            var result = fines.Select(f => new
            {
                f.FineID,
                UserName = f.User != null ? f.User.Name : "N/A",   // Safely access UserName
                BookTitle = f.Book != null ? f.Book.Title : "N/A", // Safely access Book Title
                f.Amount,
                f.FineDate,
                PaidStatus = f.PaidStatus.ToString()               // Convert PaidStatus enum to string
            });

            if (!result.Any())
            {
                //log.Warn("No fines found.");
                return NotFound("No fines found.");
            }

            //log.Info("Fines fetched successfully");
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFineById(int id)
        {
            var fine = await _fineRepository.GetByIdAsync(id);
            if (fine == null) return NotFound();
            return Ok(fine);
        }

        [HttpPut("api/fines/{id}")]
        public async Task<IActionResult> UpdateFine(int id, [FromBody] FineUpdateDto fineUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the provided ID matches the DTO's ID
            if (id <= 0 || id != fineUpdateDto.FineID)
            {
                return BadRequest("Invalid Fine ID.");
            }

            // Retrieve the existing Fine entity from the repository
            var existingFine = await _fineRepository.GetByIdAsync(id);

            if (existingFine == null)
            {
                return NotFound("Fine not found.");
            }

            // Ensure the amount being paid matches the existing fine amount
            if (fineUpdateDto.Amount != existingFine.Amount)
            {
                return BadRequest("The payment amount must match the fine amount.");
            }

            // Update the fields of the existing Fine entity using the DTO
            existingFine.UserID = fineUpdateDto.UserID;
            existingFine.PaidStatus = FineStatus.Paid;

            // Update the fine in the repository
            await _fineRepository.UpdateAsync(existingFine);

            return NoContent();
        }
        [HttpGet("total-unpaid")]
        public async Task<IActionResult> GetTotalUnpaidFines()
        {
            try
            {
                // Calculate total unpaid fines
                var totalUnpaidAmount = await _fineRepository.GetTotalUnpaidFinesAsync();

                return Ok(new { TotalUnpaidAmount = totalUnpaidAmount });
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                // log.Error("Error fetching total unpaid fines", ex);
                return StatusCode(500, "Internal server error");
            }
        }
        //[HttpPost("pay-total/{userId}")]
        //public string PayTotalUnpaidFines(int userId)
        //{
        //    decimal totalUnpaidFines =  _fineRepository.GetTotalUnpaidFinesByUserIdAsync(userId);
        //    var totalUnpaidFines = Convert.ToInt32(fines);
        //    if(totalUnpaidFines == 0)
        //    {
        //        var error = "Error";
        //        return error;
        //    }
        //   // var totalUnpaidFines = Convert.ToDecimal(fines);
        //    var key = "rzp_test_WyjSlk9HXWyxhD";
        //    var secret = "os4liFzBVhm4PLmyvqh5NKne";
        //    Random _random = new Random();
        //    string TransactionId = _random.Next(0, 1000).ToString();
        //    // Create order for Razorpay
        //    var amount=totalUnpaidFines;
        //    Dictionary<string, object> options = new Dictionary<string, object>();
        //    options.Add("amount", Convert.ToDecimal(amount * 100)); // Convert to smallest currency unit (paise)
        //    options.Add("currency", "INR");
        //    options.Add("receipt", TransactionId);
        //    RazorpayClient _razorpayClient = new RazorpayClient(key, secret);
        //    Razorpay.Api.Order orderResponse = _razorpayClient.Order.Create(options);
        //    var orderId = orderResponse["id"].ToString();
        //    // Pass the order id and amount to the view for initiating Razorpay payment
        //    PaymentViewModel payment = new PaymentViewModel();
        //    payment.Id = 1;
        //    payment.OrderId = orderId;
        //    payment.Amount = Convert.ToDecimal(amount * 100);
        //    var amounttobesent= Convert.ToDecimal(amount * 100);
        //    var senditem = orderId +","+ Convert.ToString(amount);
        //    return senditem;

        //    //if (totalUnpaidFines == 0)
        //    //{
        //    //    return BadRequest("No unpaid fines found for this user.");
        //    //}

        //    //// Normally, you would integrate with a payment gateway here.
        //    //// For this example, we assume the user has successfully paid the total amount.

        //    //// Update all unpaid fines to "Paid"
        //    //await _fineRepository.UpdateAllUnpaidFinesToPaidByUserIdAsync(userId);

        //    //return Ok($"Successfully paid a total fine amount of {totalUnpaidFines}.");
        //}

        [HttpPost("pay-total/{userId}")]
        public async Task<string> PayTotalUnpaidFines(int userId)
        {
            // Await the asynchronous method to get the total unpaid fines
            decimal totalUnpaidFines = await _fineRepository.GetTotalUnpaidFinesByUserIdAsync(userId);

            // Check if there are any unpaid fines
            if (totalUnpaidFines <= 0)
            {
                return "Error";
            }

            var key = "rzp_test_WyjSlk9HXWyxhD";
            var secret = "os4liFzBVhm4PLmyvqh5NKne";
            Random random = new Random();
            string transactionId = random.Next(0, 1000).ToString();

            // Create order for Razorpay
            Dictionary<string, object> options = new Dictionary<string, object>
    {
        { "amount", totalUnpaidFines * 100 }, // Convert to smallest currency unit (paise)
        { "currency", "INR" },
        { "receipt", transactionId }
    };

            RazorpayClient razorpayClient = new RazorpayClient(key, secret);
            Razorpay.Api.Order orderResponse = razorpayClient.Order.Create(options);
            var orderId = orderResponse["id"].ToString();

            // Create the payment model
            PaymentViewModel payment = new PaymentViewModel
            {
                Id = 1,
                OrderId = orderId,
                Amount = totalUnpaidFines * 100 // Amount in paise
            };

            // Prepare the response to send back
            var responseToSend = orderId+ "," +Convert.ToString(totalUnpaidFines);
            return responseToSend;
        }

        [HttpPut("PaymentResponse/{Id}")]
        public async Task<IActionResult> PaymentResponse(int Id, [FromBody] string paymentId)
        {
            Console.WriteLine($"Received PaymentId: {paymentId}, UserId: {Id}");

            if (string.IsNullOrEmpty(paymentId) || Id <= 0)
            {
                return BadRequest("Invalid payment ID or user ID.");
            }

            // Process the payment response
            await _fineRepository.UpdateAllUnpaidFinesToPaidByUserIdAsync(Id);
            return Ok("Successfully paid a total fine amount.");
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFine(int id)
        {
            await _fineRepository.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetFinesByUserId(int userId)
        {
            var fines = await _fineRepository.GetByUserIdAsync(userId);

            var result = fines.Select(f => new
            {
                f.FineID,
                UserName = f.User != null ? f.User.Name : "N/A", // Access User Name safely
                BookTitle = f.Book != null ? f.Book.Title : "N/A", // Access Book Title safely
                f.Amount,
                f.FineDate,
                f.PaidStatus
            });

            return Ok(result);
        }
        [HttpGet("total-unpaid/{userId}")]
        public async Task<IActionResult> GetTotalUnpaidFines(int userId)
        {
            try
            {
                decimal totalUnpaidFines = await _fineRepository.GetTotalUnpaidFinesByUserIdAsync(userId);
                return Ok(new { TotalUnpaidAmount = totalUnpaidFines });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetFinesByStatus(FineStatus status)
        {
            try
            {
                var fines = await _fineRepository.GetFinesByStatusAsync(status);

                // Check if there are any fines for the specified status
                if (fines == null || !fines.Any())
                {
                    return NotFound($"No fines found with status: {status}");
                }
                var result = fines.Select(f => new
                {
                    f.FineID,
                    UserName = f.User != null ? f.User.Name : "N/A", // Access User Name safely
                    BookTitle = f.Book != null ? f.Book.Title : "N/A", // Access Book Title safely
                    f.Amount,
                    f.FineDate,
                    f.PaidStatus
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (optional, use your logging framework)
                // _logger.LogError(ex, "Error occurred while retrieving fines by status");

                // Return a 500 Internal Server Error with a generic message
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpGet("user/{userId}/status/{status}")]
        public async Task<IActionResult> GetFinesByUserIdAndStatus(int userId, FineStatus status)
        {
            try
            {
                var fines = await _fineRepository.GetFinesByUserIdAndStatusAsync(userId, status);

                // Check if there are any fines for the specified user ID and status
                if (fines == null || !fines.Any())
                {
                    return NotFound($"No fines found for user ID: {userId} with status: {status}");
                }

                var result = fines.Select(f => new
                {
                    f.FineID,
                    UserName = f.User != null ? f.User.Name : "N/A", // Access User Name safely
                    BookTitle = f.Book != null ? f.Book.Title : "N/A", // Access Book Title safely
                    f.Amount,
                    f.FineDate,
                    f.PaidStatus
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "Error occurred while retrieving fines for user ID: {userId} and status: {status}", userId, status);

                return StatusCode(500, "An error occurred while processing your request.");
            }
        }



    }
}
