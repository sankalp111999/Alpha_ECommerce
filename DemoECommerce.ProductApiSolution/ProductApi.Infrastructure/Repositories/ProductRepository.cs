using eCommerce.SharedLibrary.Logs;
using eCommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Infrastructure.Data;
using System.Linq.Expressions;


namespace ProductApi.Infrastructure.Repositories
{
    internal class ProductRepository(ProductDbContext context) : IProduct
    {
        public async Task<Response> CreateAsync(Product entity)
        {
            try
            {
                //check if the product already exists
                var getProduct = await GetByAsync(_ => _.Name!.Equals(entity.Name));
                // !-bolta hai: "Main guarantee deta hun null nahi hai"
                //  _  : Lambda parameter (har Product row ke liye)
                //_ => _.Name! .Equals( entity.Name) means har product ke Name ko check karo 
                //with entity.Name agar equals hua to getProduct me daal do.
                if (getProduct != null && !string.IsNullOrEmpty(getProduct.Name))    //measn product is not null and it's not null or empty
                    return new Response(false, $"{entity.Name} already exists.");

                var currentEntity = context.Products.Add(entity).Entity;
                await context.SaveChangesAsync();
                if (currentEntity is not null && currentEntity.Id > 0)
                    return new Response(true, $"{entity.Name} created successfully.");
                else
                    return new Response(false, $"Failed to create {entity.Name}.");
            }
            catch (Exception ex)
            {
                //Log the original exception
                LogException.LogExceptions(ex);

                //display scary-free message to the client
                return new Response(false, "An error occurred while creating the product.");      //return = "Pipeline STOP! Response bhej do!"
            }
        }

        public async Task<Response> DeleteAsync(Product entity)
        {
            try
            {
                var product = await FindByIdAsync(entity.Id);
                if (product == null)
                    return new Response(false, $"{entity.Name} not found.");
                else
                {
                    context.Products.Remove(product);
                    await context.SaveChangesAsync();
                    return new Response(true, $"{entity.Name} deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                //Log the original exception
                LogException.LogExceptions(ex);
                //display scary-free message to the client
                return new Response(false, "An error occurred while deleting the product.");      //return = "Pipeline STOP! Response bhej do!"
            }
        }

        public async Task<Product> FindByIdAsync(int id)
        {
            try
            {
                //check if the product exists
                var product = await context.Products.FindAsync(id);
                return product is not null ? product : null!;    //null! means "Main guarantee deta hun null hai"
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while fetching the product");
                // Exception ka matlab hai ki koi bhi general error jo specific nahi hai.
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                var product = await context.Products.AsNoTracking().ToListAsync();
                /* AsNoTracking() ka matlab hai ki hum sirf read-only operation kar rahe hain,
                 * isliye EF Core ko entities ko track karne ki zarurat nahi hai.
                 * Yeh performance improve karta hai, especially jab hum large datasets ke saath kaam kar rahe hote hain.
                 */
                return product is not null ? product : Enumerable.Empty<Product>();
                /* Enumerable.Empty<Product>() ka matlab hai ki agar product null hai,
                 * toh hum ek empty collection return kar rahe hain jisme koi bhi Product entity nahi hai.
                 * Isse hum null reference exceptions se bach sakte hain jab hum is result ko iterate karte hain.
                 */
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new InvalidOperationException("An error occurred while fetching products");
                // InvalidOperationException ka matlab hai ki operation jo hum karne ki koshish kar rahe hain,
            }
        }

        public async Task<Product> GetByAsync(Expression<Func<Product, bool>> predicate)
        // Expression<Func<Product, bool>> predicate ka matlab hai ki hum ek expression (condition) pass kar rahe hain
        //Expression<Func<>>** = "EF Core ko bol do: 'Yeh condition ko SQL bana do jisse hum database me query kar sakein."
        //Expression ka fayda yeh hai ki hum complex filtering logic define kar sakte hain jo database level par execute hota hai,
        {
            try
            {
                var product = await context.Products.Where(predicate).FirstOrDefaultAsync()!;
                /* 
                 * Where(predicate) ka matlab hai ki hum products ko filter kar rahe hain
                 * based on the condition defined in the predicate.
                 * FirstOrDefaultAsync() ka matlab hai ki hum filtered results me se pehla product
                 * le rahe hain, ya agar koi product nahi milta toh default value (null) return kar rahe hain.
                 */
                return product is not null ? product : null!;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new InvalidOperationException("An error occurred while fetching the product");
            }
        }

        public async Task<Response> UpdateAsync(Product entity)
        {
            try
            {
                var product = await FindByIdAsync(entity.Id);
                if (product == null)
                    return new Response(false, $"{entity.Name} not found.");
                context.Entry(product).State = EntityState.Detached;
                //context.Entry(p).State = EntityState.Detached;  // Memory free!
                // Ab 1000 products ka tracking OFF → 50MB memory save!

                context.Products.Update(entity);
                await context.SaveChangesAsync();

                /*BEFORE ANYTHING:
DB: {Id=1, Name="iPhone 14", Price=50000}

1️⃣ var product = await FindByIdAsync(1);
DB → product = {Id=1, Name="iPhone 14", Price=50000} [TRACKED]

2️⃣ context.Entry(product).State = Detached;
product = {Id=1, Name="iPhone 14", Price=50000} [UNTRACKED]

3️⃣ var entity = Request se = {Id=1, Name="iPhone 15", Price=60000} [DETACHED]

4️⃣ context.Products.Update(entity);
entity = {Id=1, Name="iPhone 15", Price=60000} [TRACKED + MODIFIED]

5️⃣ await context.SaveChangesAsync();
↓ SQL EXECUTE:
UPDATE Products SET Name='iPhone 15', Price=60000 WHERE Id=1 ✅

AFTER UPDATE:
DB: {Id=1, Name="iPhone 15", Price=60000}  ← NEW DATA!*/


                return new Response(true, $"{entity.Name} updated successfully.");
            }
            catch (Exception ex)
            {
                //Log the original exception
                LogException.LogExceptions(ex);
                //display scary-free message to the client
                return new Response(false, "An error occurred while updating the product.");      //return = "Pipeline STOP! Response bhej do!"
            }
        }
    }
}
