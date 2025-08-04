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
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring self-referencing relationship
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.Name, c.ParentCategoryId })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Code = "ELEC",
                    Description = "Devices and gadgets",
                    CreatedDate = DateTime.Now,
                    ParentCategoryId = null // Root category
                },
                new Category
                {
                    Id = 2,
                    Name = "Jewellery",
                    Code = "JEWEL",
                    Description = "Jewellery and accessories",
                    CreatedDate = DateTime.Now,
                    ParentCategoryId = null // Root category
                },
                new Category
                {
                    Id = 3,
                    Name = "Clothing",
                    Code = "CLOTH",
                    Description = "Apparel and garments",
                    CreatedDate = DateTime.Now,
                    ParentCategoryId = null // Root category
                },
                new Category
                {
                    Id = 4,
                    Name = "Women",
                    Code = "WOMEN",
                    Description = "Women's clothing and accessories",
                    CreatedDate = DateTime.Now,
                    ParentCategoryId = 3 // Subcategory of Clothing
                },
                new Category
                {
                    Id = 5,
                    Name = "Men",
                    Code = "MEN",
                    Description = "Men's clothing and accessories",
                    CreatedDate = DateTime.Now,
                    ParentCategoryId = 3 // Subcategory of Clothing
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id= 1,
                    Title= "Fjallraven - Foldsack No. 1 Backpack, Fits 15 Laptops",
                    Price= 109.95,
                    Description= "Your perfect pack for everyday use and walks in the forest. Stash your laptop (up to 15 inches) in the padded sleeve, your everyday",
                    Image = "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
                    CategoryId = 5,
                },
                new Product
                {
                    Id = 2,
                    Title = "Mens Casual Premium Slim Fit T-Shirts",
                    Price = 122.3,
                    Description = "Slim-fitting style, contrast raglan long sleeve, three-button henley placket, light weight & soft fabric for breathable and comfortable wearing. And Solid stitched shirts with round neck made for durability and a great fit for casual fashion wear and diehard baseball fans. The Henley style round neckline includes a three-button placket.",
                    Image = "https://fakestoreapi.com/img/71-3HjGNDUL._AC_SY879._SX._UX._SY._UY_.jpg",
                    CategoryId = 5,
                },
                new Product
                {
                    Id = 3,
                    Title = "John Hardy Women's Legends Naga Gold & Silver Dragon Station Chain Bracelet",
                    Price = 695,
                    Description = "From our Legends Collection, the Naga was inspired by the mythical water dragon that protects the ocean's pearl. Wear facing inward to be bestowed with love and abundance, or outward for protection.",
                    Image = "https://fakestoreapi.com/img/71pWzhdJNwL._AC_UL640_QL65_ML3_.jpg",
                    CategoryId = 2,
                },
                new Product
                {
                    Id = 4,
                    Title = "WD 2TB Elements Portable External Hard Drive - USB 3.0",
                    Price = 695,
                    Description = "USB 3.0 and USB 2.0 Compatibility Fast data transfers Improve PC Performance High Capacity; Compatibility Formatted NTFS for Windows 10, Windows 8.1, Windows 7; Reformatting may be required for other operating systems; Compatibility may vary depending on user’s hardware configuration and operating system",
                    Image = "https://fakestoreapi.com/img/61IBBVJvSDL._AC_SY879_.jpg",
                    CategoryId = 1,
                },
                new Product
                {
                    Id = 5,
                    Title = "BIYLACLESEN Women's 3-in-1 Snowboard Jacket Winter Coats",
                    Price = 695,
                    Description = "The Jackets is US standard size, Please choose size as your usual wear Material: 100% Polyester; Detachable Liner Fabric: Warm Fleece. Detachable Functional Liner: Skin Friendly, Lightweigt and Warm.Stand Collar Liner jacket, keep you warm in cold weather. Zippered Pockets: 2 Zippered Hand Pockets, 2 Zippered Pockets on Chest (enough to keep cards or keys)and 1 Hidden Pocket Inside.Zippered Hand Pockets and Hidden Pocket keep your things secure. Humanized Design: Adjustable and Detachable Hood and Adjustable cuff to prevent the wind and water,for a comfortable fit. 3 in 1 Detachable Design provide more convenience, you can separate the coat and inner as needed, or wear it together. It is suitable for different season and help you adapt to different climates",
                    Image = "https://fakestoreapi.com/img/51Y5NI-I5jL._AC_UX679_.jpg",
                    CategoryId = 4,
                }
            );


        }

    }
}
