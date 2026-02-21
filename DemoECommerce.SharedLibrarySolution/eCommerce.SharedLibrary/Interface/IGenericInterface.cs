using eCommerce.SharedLibrary.Responses;
using System.Linq.Expressions;
namespace eCommerce.SharedLibrary.Interface
{
    public interface IGenericInterface<T> where T : class
    {

        //Single-entity CRUD returning Task<Response> for consistent error handling (e.g., validation fails → Flag=false).
        Task<Response> CreateAsync(T entity);
        Task<Response> UpdateAsync(T entity);
        Task<Response> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> FindByIdAsync(int id);
        Task<T> GetByAsync(Expression<Func<T, bool>> predicate);  //Predicate(Delegate) = "Smart filter" jo DB ko bolta hai exactly kya chahiye, poora table nahi laata!

    }
}
