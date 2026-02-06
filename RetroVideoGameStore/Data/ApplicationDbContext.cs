using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RetroVideoGameStore.Models;

namespace RetroVideoGameStore.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public required DbSet<Category> Categories { get; set; }
        public required DbSet<Product> Products { get; set; }
        public DbSet<RetroVideoGameStore.Models.Category> Category { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
