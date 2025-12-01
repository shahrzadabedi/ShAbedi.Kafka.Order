using Microsoft.EntityFrameworkCore;

namespace ShAbedi.Order.Persistence
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
            modelBuilder.Entity<Domain.Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(l => l.Id);
            });

            modelBuilder.Entity<Domain.OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(l => l.Id);
            });
            
            modelBuilder.Entity<Domain.Outbox>(entity =>
            {
                entity.ToTable("Outbox");
                entity.HasKey(l => l.Id);
                entity.HasIndex(p => p.OccurredOn).IsClustered();
            });
        }

        public DbSet<Domain.Order> Orders { get; set; }
        public DbSet<Domain.Outbox> Outbox { get; set; }
    }
}
