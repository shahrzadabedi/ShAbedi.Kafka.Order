using Microsoft.EntityFrameworkCore;

namespace ShAbedi.OrderJobs.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=.;Initial Catalog=Order;User ID=sa;Password=123;Persist Security Info=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order.Domain.Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(l => l.Id);
            });

            modelBuilder.Entity<Order.Domain.OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(l => l.Id);
            });
        }

        public DbSet<Order.Domain.Order> Orders { get; set; }
        public DbSet<Order.Domain.Outbox> Outbox { get; set; }
    }
}
