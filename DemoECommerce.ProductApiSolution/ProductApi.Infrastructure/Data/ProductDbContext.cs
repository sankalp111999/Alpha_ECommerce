using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Entities;


namespace ProductApi.Infrastructure.Data
{
    public class ProductDbContext(DbContextOptions<ProductDbContext> options):DbContext(options)    //Ye constructor injection hai (EF Core + DI)
    {
        public DbSet<Product> Products { get; set; }
    }
}
