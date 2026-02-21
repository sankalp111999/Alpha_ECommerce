using System.ComponentModel.DataAnnotations;

namespace OrderApi.Application.DTOs
{
    public record ProductDTO
    (
        int Id,
        [Required, MaxLength(100)] string Name,
        [Required,Range(1,int.MaxValue)] int Quantity,
        [Required,DataType(DataType.Currency)] decimal Price
        );
}
