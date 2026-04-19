
using Microsoft.EntityFrameworkCore;
using ServiceFaturamento.Models;

namespace ServiceFaturamento.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Nota> Notas { get; set; }
        public DbSet<ItemNota> ItensNota { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Nota>()
                .Property(n => n.Status)
                .HasConversion<string>();
        }
    }
}