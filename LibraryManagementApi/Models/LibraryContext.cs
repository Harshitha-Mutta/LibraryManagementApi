using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using LibraryManagementApi.Models;
using Microsoft.Identity.Client;
using System.Security.Principal;

namespace LibraryManagementApi.Data
{
    public class LibraryContext : DbContext
    {
        IConfiguration appconfig;
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {

        }

        // DbSets for each entity
        public DbSet<User> Users { get; set; }
        public DbSet<BorrowedBook> BorrowedBooks { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(appconfig.GetConnectionString("LibraryManage"));
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<BorrowedBook>()
            //.Property(b => b.DueDate)
            //.IsRequired(false);

            modelBuilder.Entity<User>()
               .Property(e => e.Role)
               .HasConversion<string>();

            modelBuilder.Entity<Fine>()
               .Property(e => e.PaidStatus)
               .HasConversion<string>();

            modelBuilder.Entity<Reservation>()
               .Property(e => e.ReservationStatus)
               .HasConversion<string>();

            modelBuilder.Entity<BorrowedBook>()
                .Property(e => e.BorrowStatus)
               .HasConversion<string>();

            // User: Email should be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // One User can have many BorrowedBooks
            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.User)
                .WithMany(u => u.BorrowedBooks)
                .HasForeignKey(bb => bb.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Prevent cascade delete

            // One User can have many Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // One User can have many Fines
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.User)
                .WithMany(u => u.Fines)
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // One Book can be borrowed in many BorrowedBooks
            modelBuilder.Entity<BorrowedBook>()
                .HasOne(bb => bb.Book)
                .WithMany(b => b.BorrowedBooks)
                .HasForeignKey(bb => bb.BookID)
                .OnDelete(DeleteBehavior.Cascade);

            // One Book can have many Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reservations)
                .HasForeignKey(r => r.BookID)
                .OnDelete(DeleteBehavior.Restrict);

            // One Book can have many Fines
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.Book)
                .WithMany(b => b.Fines)
                .HasForeignKey(f => f.BookID)
                .OnDelete(DeleteBehavior.Restrict);

            // One Genre can have many Books
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fine>()
                .Property(a => a.Amount)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Review>()
            .HasOne(r => r.Book)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BookID);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserID);


            base.OnModelCreating(modelBuilder);
        }
    }
}
