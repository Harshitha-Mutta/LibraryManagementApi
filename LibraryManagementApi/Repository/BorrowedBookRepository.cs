using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using LibraryManagementApi.Services;
using CloudinaryDotNet.Core;




namespace LibraryManagementApi.Repository
{
    public class BorrowedBookRepository : IBorrowedBookRepository<BorrowedBook>
    {
        private readonly LibraryContext _context;
        private readonly BorrowedBookEmailService _emailService;
        private readonly PdfService _pdfService;
        //private readonly ReservationEmailNotificationService _emailNotificationService;


        public BorrowedBookRepository(LibraryContext context, BorrowedBookEmailService emailService, PdfService pdfService)
        {
            _context = context;
            _emailService = emailService;
            _pdfService = pdfService;
            // _emailNotificationService = emailNotificationService;
        }

        public async Task<IEnumerable<BorrowedBook>> GetAllAsync()
        {
            return await _context.BorrowedBooks
                .Include(r => r.User)  // Include User details
                .Include(r => r.Book)  // Include Book details
                .OrderByDescending(r => r.BorrowID)  // Order by BorrowID in descending order
                .ToListAsync();
        }


        public async Task<BorrowedBook> GetByIdAsync(int id)
        {
            return await _context.BorrowedBooks.FindAsync(id);
        }

        public async Task<IEnumerable<BorrowedBook>> GetByUserIdAsync(int userId)
        {
            return await _context.BorrowedBooks.Where(b => b.UserID == userId)
                .Include(r => r.User)  // Include User details
                .Include(r => r.Book)  // Include Book details
                .OrderByDescending(r => r.BorrowID)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowedBook>> GetByBookIdAsync(int bookId)
        {
            return await _context.BorrowedBooks.Where(b => b.BookID == bookId)
                .Include(r => r.User)  // Include User details
                .Include(r => r.Book)  // Include Book details
                .OrderByDescending(r => r.BorrowID)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowedBook>> GetActiveBorrowedBooksAsync()
        {
            return await _context.BorrowedBooks.Where(b => b.ReturnDate == null).ToListAsync();
        }

        public async Task<IEnumerable<BorrowedBook>> GetReturnedBorrowedBooksAsync()
        {
            return await _context.BorrowedBooks.Where(b => b.ReturnDate != null).ToListAsync();
        }

        public async Task AddAsync(BorrowedBook entity)
        {
            // Fetch the book to check for available copies
            var book = await _context.Books.FindAsync(entity.BookID);
            if (book == null)
            {
                throw new InvalidOperationException("Book not found.");
            }

            // Check if there are available copies
            if (book.AvailableCopies <= 0)
            {
                throw new InvalidOperationException("No available copies of the book.");
            }

            // Fetch the user to update book count
            var user = await _context.Users.FindAsync(entity.UserID);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }
            if (user.BookCount >= 5)
            {
                throw new InvalidOperationException("User has reached the maximum limit of borrowed books.");
            }
            // Set DueDate to 10 days from BorrowDate and initialize ReturnDate to null
            entity.DueDate = entity.BorrowDate.AddDays(10);
            entity.ReturnDate = null;

            // Decrement AvailableCopies in Book
            book.AvailableCopies -= 1;
            _context.Books.Update(book);

            // Increment BookCount in User
            user.BookCount = (user.BookCount ?? 0) + 1;
            _context.Users.Update(user);

            // Add the new BorrowedBook record
            await _context.BorrowedBooks.AddAsync(entity);

            // Save all changes atomically
            await _context.SaveChangesAsync();
            var pdfPath = _pdfService.GeneratePdf(user.Name, user.PhoneNumber, user.Email, book.Title, entity.BorrowDate, entity.DueDate);
            if (!string.IsNullOrEmpty(pdfPath))
            {
                string subject = "Your Borrowed Book Details";
                string body = "Dear User,<br/><br/>Please find attached the details of your borrowed book.<br/><br/>Thank you!";
                _emailService.SendEmailWithPdf(user.Email, subject, body, pdfPath);
            }
        }





        public async Task UpdateAsync(BorrowedBook entity)
        {
            _context.BorrowedBooks.Update(entity);
            await _context.SaveChangesAsync();

            // If ReturnDate is set, decrement BookCount in User
            if (entity.ReturnDate.HasValue)
            {
                var userRepo = new UserRepository(_context);
                var bookRepo = new BookRepository(_context);

                // Decrease book count for the user
                await userRepo.UpdateBookCountAsync(entity.UserID, -1);

                // Check if there are active reservations for the book
                var activeReservations = _context.Reservations
                    .Where(r => r.BookID == entity.BookID && r.ReservationStatus == Status.Active)
                    .OrderBy(r => r.QueuePosition) // Order by queue position to process the first in line
                    .ToList();

                if (activeReservations.Any())
                {
                    // If there are active reservations, approve the next one in line
                    var nextReservation = activeReservations.First();
                    nextReservation.ReservationStatus = Status.Approved;

                    // Update reservation and notify the user
                    _context.Reservations.Update(nextReservation);
                    await _context.SaveChangesAsync();
                    var reservedUser = await userRepo.GetByIdAsync(nextReservation.UserID);
                    var reservedBook = await bookRepo.GetByIdAsync(nextReservation.BookID);
                    if (reservedUser != null)
                    {
                        string subject = "Your Book is Available!";
                        string body = $"Dear {reservedUser.Name},<br><br>" +
                                      $"Your reservation for the book '{reservedBook.Title}' has been approved. " +
                                      "Please visit the library to borrow it.";

                        // Call the static SendEmail method
                        ReservationEmailNotificationService.SendEmail(reservedUser.Email, subject, body);
                    }

                }
                else
                {
                    // If no reservations exist, increase available copies
                    await bookRepo.UpdateAvailableCopiesAsync(entity.BookID, +1);
                }
            }

        }



        public async Task DeleteAsync(int id)
        {
            var borrowedBook = await GetByIdAsync(id);
            if (borrowedBook != null)
            {
                _context.BorrowedBooks.Remove(borrowedBook);
                await _context.SaveChangesAsync();
            }
        }
        public async Task BorrowReservedBookAsync(int userId, int bookId)
        {
            // Fetch the reservation to verify it's approved
            var reservation = await _context.Reservations
                .Where(r => r.BookID == bookId && r.UserID == userId && r.ReservationStatus == Status.Approved)
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new InvalidOperationException("No approved reservation found for the user on this book.");
            }

            // Fetch the user to update book count
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Ensure user hasn't reached the max limit of borrowed books
            if (user.BookCount >= 5)
            {
                throw new InvalidOperationException("User has reached the maximum borrowing limit.");
            }

            // Fetch the book to ensure it exists
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                throw new InvalidOperationException("Book not found.");
            }

            // Create a BorrowedBook entry for the user
            var borrowedBook = new BorrowedBook
            {
                UserID = userId,
                BookID = bookId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(10), // Default 10-day borrow period
                ReturnDate = null,
                Book = book // Ensure the book is set
            };

            // Update the user's book count
            user.BookCount = (user.BookCount ?? 0) + 1;
            _context.Users.Update(user);

            // Update the reservation status to Completed
            reservation.ReservationStatus = Status.Completed;
            _context.Reservations.Update(reservation);

            // Add the new BorrowedBook record
            await _context.BorrowedBooks.AddAsync(borrowedBook);

            // Save all changes atomically
            await _context.SaveChangesAsync();

            // Generate PDF for the borrowed book
            var pdfPath = _pdfService.GeneratePdf(user.Name, user.PhoneNumber, user.Email, book.Title, borrowedBook.BorrowDate, borrowedBook.DueDate);

            if (!string.IsNullOrEmpty(pdfPath))
            {
                string subject = "Your Borrowed Book Details";
                string body = "Dear User,<br/><br/>Please find attached the details of your borrowed book.<br/><br/>Thank you!";
                _emailService.SendEmailWithPdf(user.Email, subject, body, pdfPath);
            }
        }



        public async Task<int> GetActiveBorrowedBookCountAsync()
        {
            // Count the borrowed books where ReturnDate is null (active books)
            return await _context.BorrowedBooks.CountAsync(b => b.ReturnDate == null);
        }

        public async Task<IEnumerable<BorrowedBook>> GetOverdueBooksAsync()
        {
            return await _context.BorrowedBooks
                .Where(bb => bb.DueDate < DateTime.Now && !bb.ReturnDate.HasValue && bb.BorrowStatus != BorrowStatus.Cancelled)
                .Include(r => r.User)
                .Include(r => r.Book)
                .ToListAsync();
        }


        public async Task<BorrowedBook> GetByUserAndBookAsync(int userId, int bookId)
        {
            return await _context.BorrowedBooks
                                 .FirstOrDefaultAsync(b => b.UserID == userId && b.BookID == bookId && b.ReturnDate == null);
        }
        public async Task<List<BorrowedBook>> GetBorrowedBooksByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure the end date is inclusive
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            return await _context.BorrowedBooks
                .Where(b => b.BorrowDate >= startDate && b.BorrowDate <= endDate)
                .Include(b => b.User)  // Include related User
                .Include(b => b.Book)  // Include related Book
                .ToListAsync();
        }
        public async Task PreBooking(int bookId, int userId)
        {
            // Step 1: Get the book and user from the database
            var book = await _context.Books.FindAsync(bookId);
            var user = await _context.Users.FindAsync(userId);

            if (book == null)
            {
                throw new ArgumentException("Book not found.");
            }

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            // Step 2: Check if there are available copies
            if (book.AvailableCopies <= 0)
            {
                throw new InvalidOperationException("No copies of the book are available.");
            }

            // Step 3: Check if user has reached borrowing limit (assuming limit is 5 books)
            if (user.BookCount >= 5)
            {
                throw new InvalidOperationException("User has reached the borrowing limit.");
            }

            // Step 4: Decrement the available copies and increment the user's book count
            book.AvailableCopies--;
            user.BookCount++;

            // Step 5: Create a new BorrowedBook entry
            var borrowedBook = new BorrowedBook
            {
                UserID = userId,
                BookID = bookId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now, // Assuming 2 weeks borrow time
                ReturnDate = null,
                BorrowStatus = BorrowStatus.BookingSuccess

            };

            // Step 6: Add the BorrowedBook to the database
            _context.BorrowedBooks.Add(borrowedBook);
            if (user != null)
            {
                string subject = "Booking Successfull !";
                string body = $"Dear {user.Name},<br><br>" +
                              $"Your Booking for the book '{book.Title}' is Success. " +
                              $"Please visit the library to take it before {DateTime.Now.AddDays(2).ToString("MMMM dd, yyyy")}. In any other cases your booking will be cancelled";

                // Call the static SendEmail method
                ReservationEmailNotificationService.SendEmail(user.Email, subject, body);
            }
            // Step 7: Save changes to the database
            await _context.SaveChangesAsync();
        }
        public async Task CancelPreBooking(int bookId, int userId)
        {
            // Step 1: Find the BorrowedBook record
            var borrowedBook = await _context.BorrowedBooks
                .FirstOrDefaultAsync(b => b.BookID == bookId && b.UserID == userId && b.BorrowStatus == BorrowStatus.BookingSuccess);

            if (borrowedBook == null)
            {
                throw new ArgumentException("Borrowed book record not found.");
            }

            // Step 2: Find the user and book
            var book = await _context.Books.FindAsync(bookId);
            var user = await _context.Users.FindAsync(userId);

            if (book == null)
            {
                throw new ArgumentException("Book not found.");
            }

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            // Step 3: Update the status of the BorrowedBook to 'Cancelled'
            borrowedBook.BorrowStatus = BorrowStatus.Cancelled;

            // Step 4: Increment the available copies and decrement the user's book count
            book.AvailableCopies++;
            user.BookCount--;

            // Step 5: Update the BorrowedBook record in the database
            _context.BorrowedBooks.Update(borrowedBook);

            // Step 6: Save changes to the database
            await _context.SaveChangesAsync();

            // Optional: Send email notification to the user about the cancellation
            if (user != null)
            {
                string subject = "Booking Cancelled";
                string body = $"Dear {user.Name},<br><br>" +
                              $"Your booking for the book '{book.Title}' has been cancelled successfully. " +
                              $"You can book it again if you are interested.";

                // Call the static SendEmail method to send an email notification
                ReservationEmailNotificationService.SendEmail(user.Email, subject, body);
            }
        }

        public async Task ApprovePreBooking(int bookId, int userId)
        {
            // Step 1: Find the BorrowedBook record
            var borrowedBook = await _context.BorrowedBooks
                .FirstOrDefaultAsync(b => b.BookID == bookId && b.UserID == userId && b.BorrowStatus == BorrowStatus.BookingSuccess);

            if (borrowedBook == null)
            {
                throw new ArgumentException("Borrowed book record not found.");
            }

            // Step 2: Update the borrow date and due date
            borrowedBook.BorrowDate = DateTime.Now;
            borrowedBook.DueDate = borrowedBook.BorrowDate.AddDays(10); // Borrow period extended by 10 days
            borrowedBook.BorrowStatus = BorrowStatus.BookingAllocated;

            // Step 3: Save changes to the database
            await _context.SaveChangesAsync();

            // Step 4: Generate PDF for the approved booking
            var user = await _context.Users.FindAsync(userId);
            var book = await _context.Books.FindAsync(bookId);

            if (user != null && book != null)
            {
                var pdfPath = _pdfService.GeneratePdf(user.Name, user.PhoneNumber, user.Email, book.Title, borrowedBook.BorrowDate, borrowedBook.DueDate);

                if (!string.IsNullOrEmpty(pdfPath))
                {
                    string subject = "Your Pre-Booking has been Approved!";
                    string body = $"Dear {user.Name},<br/><br/>" +
                                  $"Your booking for the book '{book.Title}' has been approved. " +
                                  $"Please find the details attached in the PDF.<br/><br/>" +
                                  $"You have until {borrowedBook.DueDate.ToString("MMMM dd, yyyy")} to pick it up.<br/><br/>" +
                                  "Thank you!";

                    // Send the email with the PDF attachment
                    _emailService.SendEmailWithPdf(user.Email, subject, body, pdfPath);
                }
            }
        }

        public async Task<IEnumerable<BorrowedBooksMonthlyCount>> GetBorrowedBooksCountAsync(int userId)
        {
            var borrowedBooks = await _context.BorrowedBooks
                .Where(b => b.UserID == userId && b.BorrowDate != null)
                .GroupBy(b => new
                {
                    Year = b.BorrowDate.Year,
                    Month = b.BorrowDate.Month
                })
                .Select(g => new BorrowedBooksMonthlyCount
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            return borrowedBooks;
        }
        public async Task<IEnumerable<BorrowedBooksMonthlyCount>> GetBorrowedBooksCountForAllYearsAsync()
        {
            var borrowedBooksByMonth = await _context.BorrowedBooks
                .GroupBy(b => new { b.BorrowDate.Year, b.BorrowDate.Month }) // Group by year and month
                .Select(g => new BorrowedBooksMonthlyCount
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count() // Count how many books were borrowed in each month
                })
                .OrderBy(b => b.Year)
                .ThenBy(b => b.Month) // Ensure results are ordered by year and month
                .ToListAsync();

            return borrowedBooksByMonth;
        }
        public async Task<BorrowedBook> GetApprovedOrNullBorrowedBookByUserAndBookAsync(int userId, int bookId)
        {
            return await _context.BorrowedBooks
                .Where(bb => bb.UserID == userId && bb.BookID == bookId && bb.ReturnDate == null &&
                             (bb.BorrowStatus == BorrowStatus.BookingAllocated || bb.BorrowStatus == null))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetEmailsOfUsersWithUnreturnedBooksAsync()
        {
            // Retrieve the UserID and Email of users with unreturned books (ReturnDate is null)
            var userEmails = await _context.BorrowedBooks
                .Include(b => b.User) // Include the User navigation property
                .Where(b => b.ReturnDate == null && b.BorrowStatus != BorrowStatus.Cancelled) // Filter for books that have not been returned
                .Select(b => new
                {
                    UserID = b.User.UserID, // Select the UserID property from the User entity
                    Email = b.User.Email // Select the Email property from the User entity
                })
                .Distinct() // Remove duplicate UserID and Email combinations
                .ToListAsync(); // Execute the query and return the results as a list

            return userEmails;
        }
        public async Task<IEnumerable<object>> GetUsersWithSameBorrowAndDueDateAsync()
        {
            // Query to get UserID and emails of users where BorrowDate and DueDate are the same
            var usersWithSameDate = await _context.BorrowedBooks
                .Include(b => b.User) // Load related User data
                .Where(b => b.BorrowDate.Date == b.DueDate.Date && b.BorrowStatus == BorrowStatus.BookingSuccess) // Filter where BorrowDate equals DueDate
                .Select(b => new
                {
                    UserID = b.User.UserID, // Select UserID
                    Email = b.User.Email // Select the user's email
                })
                .Distinct() // Remove duplicates if necessary
                .ToListAsync();

            return usersWithSameDate;
        }
        public async Task<IEnumerable<object>> GetAllUserEmailsAsync()
        {
            // Query to get UserID and emails of all users
            var allUsers = await _context.Users
                .Select(u => new
                {
                    UserID = u.UserID, // Select UserID
                    Email = u.Email // Select the user's email
                })
                .Distinct() // Remove duplicates if necessary
                .ToListAsync();

            return allUsers;
        }


    }
}