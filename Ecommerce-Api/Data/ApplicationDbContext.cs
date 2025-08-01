using Ecommerce_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Code = "ELEC",
                    Description = "Devices and gadgets",
                    CreatedDate = DateTime.Now,

                },
                new Category
                {
                    Id = 2,
                    Name = "Jewellery",
                    Code = "JEWEL",
                    Description = "Jewellery and accessories",
                    CreatedDate = DateTime.Now,

                },
                new Category
                {
                    Id = 3,
                    Name = "Clothing",
                    Code = "CLOTH",
                    Description = "Apparel and garments",
                    CreatedDate = DateTime.Now,

                },
                new Category
                {
                    Id = 4,
                    Name = "Women",
                    Code = "WOMEN",
                    Description = "Women's clothing and accessories",
                    CreatedDate = DateTime.Now,
                },
                new Category
                {
                    Id = 5,
                    Name = "Men",
                    Code = "MEN",
                    Description = "Men's clothing and accessories",
                    CreatedDate = DateTime.Now,
                }
            );
        }

    }
}
