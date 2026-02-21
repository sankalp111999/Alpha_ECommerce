using System.ComponentModel.DataAnnotations;

namespace OrderApi.Application.DTOs
{
    public record AppUserDTO(
        int Id,
        [Required]string Name,
        [Required]string PhoneNumber,
        [Required]string Address,
        [Required]string Email,
        [Required,EmailAddress] string Password,
        [Required]string Role
        );
}
