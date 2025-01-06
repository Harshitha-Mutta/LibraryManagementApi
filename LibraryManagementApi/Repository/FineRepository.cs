using LibraryManagementApi.Data;
using LibraryManagementApi.Interfaces;
using LibraryManagementApi.Models;
using LibraryManagementApi.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementApi.Repository
{
    public class FineRepository : IFineRepository<Fine>
    {
        private readonly LibraryContext _context;

        public FineRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Fine>> GetAllAsync()
        {
            return await _context.Fines
                .Include(f => f.User)  // Include User details
                .Include(f => f.Book)
                .OrderByDescending(f => f.FineDate)
                .ToListAsync();
        }


        public async Task<Fine> GetByIdAsync(int id)
        {
            return await _context.Fines.FindAsync(id);
        }
        public async Task<List<Fine>> GetUnpaidFinesAsync()
        {
            return await _context.Fines
                                 .Where(f => f.PaidStatus == FineStatus.NotPaid)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<Fine>> GetByUserIdAsync(int userId)
        {
            return await _context.Fines
                .Include(f => f.User)  // Include User details
                .Include(f => f.Book)  // Include Book details
                .Where(f => f.UserID == userId)
                .OrderByDescending(f => f.FineDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<Fine>> GetFinesByStatusAsync(FineStatus status)
        {
            return await _context.Fines
                .Include(f => f.User)   // Include User details if needed
                .Include(f => f.Book)   // Include Book details if needed
                .Where(f => f.PaidStatus == status)
                .ToListAsync();
        }
        public async Task<IEnumerable<Fine>> GetFinesByUserIdAndStatusAsync(int userId, FineStatus status)
        {
            return await _context.Fines
                .Include(f => f.User)   // Include User details if needed
                .Include(f => f.Book)   // Include Book details if needed
                .Where(f => f.UserID == userId && f.PaidStatus == status)
                .ToListAsync();
        }

        public async Task AddAsync(Fine entity)
        {
            await _context.Fines.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Fine entity)
        {
            _context.Fines.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<decimal> GetTotalUnpaidFinesAsync()
        {
            return await _context.Fines
                .Where(f => f.PaidStatus == FineStatus.NotPaid)
                .SumAsync(f => f.Amount);
        }
        public async Task<decimal> GetTotalUnpaidFinesByUserIdAsync(int userId)
        {
            // This ensures that if the sum is null, it returns 0
            return (await _context.Fines
                .Where(f => f.UserID == userId && f.PaidStatus == FineStatus.NotPaid)
                .SumAsync(f => (decimal?)f.Amount)) ?? 0;
        }

        // Method to update all unpaid fines to Paid
        public async Task UpdateAllUnpaidFinesToPaidByUserIdAsync(int userId)
        {
            var unpaidFines = await _context.Fines
                .Where(f => f.UserID == userId && f.PaidStatus == FineStatus.NotPaid)
                .ToListAsync();

            foreach (var fine in unpaidFines)
            {
                fine.PaidStatus = FineStatus.Paid;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var fine = await GetByIdAsync(id);
            if (fine != null)
            {
                _context.Fines.Remove(fine);
                await _context.SaveChangesAsync();
            }
        }







    }
}
