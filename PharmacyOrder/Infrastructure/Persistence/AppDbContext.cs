using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PharmacyOrder.Domain;

namespace PharmacyOrder.Infrastructure.Persistence
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
                    "Host=localhost;Database=PharmacyOrder;Username=postgres;Password=MyPass123");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure OrderStatus enum to be stored as string
            var statusConverter = new EnumToStringConverter<OrderStatus>();

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(l => l.Id);
                
                entity.Property(o => o.Status)
                    .HasConversion(statusConverter)
                    .HasMaxLength(50);
                
                entity.Property(o => o.CreateDateTime)
                    .HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(l => l.Id);
            });
        }

        public DbSet<Order> Orders { get; set; }
    }
}
