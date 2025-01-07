using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkyStoreAPI.Models;

namespace SkyStoreAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {   
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
           
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Sandal", Description = "Sandal Items" },
                new Category { Id = 2, Name = "Clothes", Description = "Clothing Items" },
                new Category { Id = 3, Name = "Shoe", Description = "Shoe Items" });
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, CategoryId = 2, Name = "Giay Sneaker", Price = 100, Description = "", ImageUrl = "" },
                new Product { Id = 2, CategoryId = 2, Name = "Ao Hoodie", Price = 100, Description = "", ImageUrl = "" },
                new Product { Id = 3, CategoryId = 2, Name = "Giay Nice", Price = 100, Description = "", ImageUrl = "" });
        }
    }
    
}
