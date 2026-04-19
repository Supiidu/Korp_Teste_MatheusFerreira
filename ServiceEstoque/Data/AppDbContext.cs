
using Microsoft.EntityFrameworkCore;
using ServiceEstoque.Models;

namespace ServiceEstoque.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options){}

        public DbSet<Produto> Estoque {get; set;}
    }
}