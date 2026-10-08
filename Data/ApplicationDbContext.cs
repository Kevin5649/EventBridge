using EventBridge.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventBridge.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<EventPlanner> EventPlanners { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PlannerEventType> PlannerEventTypes { get; set; }
        public DbSet<PlannerPortfolio> PlannerPortfolios { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Customer 1:1 with User
            builder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure EventPlanner 1:1 with User
            builder.Entity<EventPlanner>()
                .HasOne(p => p.User)
                .WithOne(u => u.EventPlanner)
                .HasForeignKey<EventPlanner>(p => p.PlannerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PlannerEventType N:1 with EventPlanner
            builder.Entity<PlannerEventType>()
                .HasOne(p => p.Planner)
                .WithMany(ep => ep.PlannerEventTypes)
                .HasForeignKey(p => p.PlannerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PlannerEventType N:1 with Category
            builder.Entity<PlannerEventType>()
                .HasOne(p => p.Category)
                .WithMany(c => c.PlannerEventTypes)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite primary key
            builder.Entity<PlannerEventType>()
                .HasKey(p => new { p.PlannerId, p.CategoryId });

            // Configure PlannerPortfolio 1:N with EventPlanner
            builder.Entity<PlannerPortfolio>()
                .HasOne(p => p.EventPlanner)
                .WithMany(ep => ep.Portfolios)
                .HasForeignKey(p => p.PlannerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent cascade delete cycles for Enquiry
            builder.Entity<Enquiry>()
                .HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enquiry>()
                .HasOne(e => e.EventPlanner)
                .WithMany()
                .HasForeignKey(e => e.PlannerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enquiry>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascade delete cycles for Booking
            builder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.EventPlanner)
                .WithMany()
                .HasForeignKey(b => b.PlannerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascade delete cycles for Review
            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.EventPlanner)
                .WithMany()
                .HasForeignKey(r => r.PlannerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking to Payment 1:N
            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enquiry to Quotation 1:1
            builder.Entity<Quotation>()
                .HasOne(q => q.Enquiry)
                .WithOne(e => e.Quotation)
                .HasForeignKey<Quotation>(q => q.EnquiryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
