using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementApi.Repository
{
    public class ReservationRepository : IReservationRepository<Reservation>
    {
        private readonly LibraryContext _context;

        public ReservationRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.User)  // Include User details
                .Include(r => r.Book)  // Include Book details
                .OrderByDescending(r => r.ReservationID)
                .ToListAsync();
        }


        public async Task<Reservation> GetByIdAsync(int id)
        {
            return await _context.Reservations.FindAsync(id);
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            return await _context.Reservations.Where(r => r.UserID == userId).ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByBookIdAsync(int bookId)
        {
            return await _context.Reservations.Where(r => r.BookID == bookId).ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetActiveReservationsAsync()
        {
            return await _context.Reservations.Where(r => r.ReservationStatus == Status.Active).ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetCancelledReservationsAsync()
        {
            return await _context.Reservations.Where(r => r.ReservationStatus == Status.Cancelled).ToListAsync();
        }

        public async Task AddAsync(Reservation entity)
        {
            // Retrieve the book associated with the reservation
            var book = await _context.Books.FindAsync(entity.BookID);

            // Check if the available copies are exactly zero
            if (book == null || book.AvailableCopies != 0)
            {
                throw new InvalidOperationException("Reservation can only be made when there are no available copies.");
            }

            // Check if the user already has an active reservation for this book
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.BookID == entity.BookID && r.UserID == entity.UserID && r.ReservationStatus == Status.Active);

            if (existingReservation != null)
            {
                throw new InvalidOperationException("User already has an active reservation for this book.");
            }

            // Get the current count of active reservations for the book
            var activeReservations = _context.Reservations
                .Where(r => r.BookID == entity.BookID && r.ReservationStatus == Status.Active)
                .ToList();

            // Assign the next QueuePosition
            entity.QueuePosition = activeReservations.Count + 1;

            // Proceed to add the reservation
            _context.Reservations.Add(entity);

            await _context.SaveChangesAsync();
        }


        public async Task<int> CountActiveReservationsAsync()
        {
            return await _context.Reservations.CountAsync(r => r.ReservationStatus == Status.Active);
        }
        public async Task<int> GetActiveReservationsCountByUserAsync(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserID == userId && r.ReservationStatus == Status.Active)
                .CountAsync();
        }

        public async Task UpdateAsync(Reservation entity)
        {
            _context.Reservations.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                // Mark the reservation as cancelled
                reservation.ReservationStatus = Status.Cancelled; // Assuming Status is your enum type or string
                await _context.SaveChangesAsync(); // Save changes to the database
            }
        }
        public async Task<IEnumerable<Reservation>> GetByStatusAsync(Status status)
        {
            return await _context.Reservations
                .Include(r => r.User) // Include user details
                .Include(r => r.Book) // Include book details
                .Where(r => r.ReservationStatus == status) // Filter by status
                .ToListAsync();
        }
        public async Task<IEnumerable<Reservation>> GetByStatusAndUserIdAsync(Status status, int userId)
        {
            return await _context.Reservations
                .Include(r => r.User) // Include user details
                .Include(r => r.Book) // Include book details
                .Where(r => r.ReservationStatus == status && r.UserID == userId) // Filter by status and user ID
                .ToListAsync();
        }
        public async Task CancelReservationAsync(string bookName, int userId)
        {
            // Find the book based on the book name
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Title == bookName);

            if (book == null)
            {
                throw new InvalidOperationException("Book not found.");
            }

            // Find the reservation for the specified user and book
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserID == userId && r.BookID == book.BookID && r.ReservationStatus == Status.Active);

            if (reservation == null)
            {
                throw new InvalidOperationException("No active reservation found for this book and user.");
            }

            // Update the reservation status to Cancelled
            reservation.ReservationStatus = Status.Cancelled;

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
        public async Task<List<Reservation>> GetReservationsByUserIdAsync(int userId)
        {
            // Fetch reservations where UserID matches the provided userId
            return await _context.Reservations
                .Where(r => r.UserID == userId)
                .Include(r => r.User) // Include related User entity
                .Include(r => r.Book) // Include related Book entity
                .ToListAsync();
        }
        public async Task<IEnumerable<Reservation>> GetActiveReservationsByUserIdAsync(int userId)
        {
            return await _context.Reservations
               .Where(r => r.UserID == userId && r.ReservationStatus == Status.Active)
               .Include(r => r.User)
               .Include(r => r.Book)
               .ToListAsync();

        }
        public async Task<IEnumerable<object>> GetUserEmailsForApprovedReservationsAsync()
        {
            // Query to get UserID and emails of users with reservations in the Approved status
            var userEmails = await _context.Reservations
                .Where(r => r.ReservationStatus == Status.Approved)
                .Include(r => r.User) // Load related User data
                .Select(r => new
                {
                    UserID = r.User.UserID, // Select UserID
                    Email = r.User.Email // Select the user's email
                })
                .Distinct() // Remove duplicate UserID and Email combinations if necessary
                .ToListAsync();

            return userEmails;
        }
        public async Task<Dictionary<string, int>> GetReservationStatusSummaryAsync()
        {
            var statusCounts = await _context.Reservations
                .GroupBy(r => r.ReservationStatus)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            return statusCounts;
        }


    }
}