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
                optionsBuilder.UseNpgsql(
                    "Host=localhost;Database=Order;Username=postgres;Password=MyPass123");
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

            modelBuilder.Entity<Order.Domain.Outbox>(entity =>
            {
                entity.ToTable("Outbox");
                entity.HasKey(l => l.Id);
                entity.HasIndex(p => p.OccurredOn);
            });
        }

        public DbSet<Order.Domain.Order> Orders { get; set; }
        public DbSet<Order.Domain.Outbox> Outbox { get; set; }
    }
}
