using System.ComponentModel.DataAnnotations;

namespace ProductApi.Application.DTOs
{
    public record ProductDTO(               //record C# ka special reference type hai jo immutable data represent karta hai.
        int Id ,
        [Required] string Name,
        [Required, Range(1,int.MaxValue)] int Quantity,
        [Required, DataType(DataType.Currency)] decimal Price
        );      
}
