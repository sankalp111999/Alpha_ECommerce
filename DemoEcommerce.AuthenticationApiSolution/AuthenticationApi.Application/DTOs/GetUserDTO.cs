
using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Application.DTOs
{
    public record GetUserDTO
    (
        int Id,
        [Required] string Name,
        [Required] string PhoneNumber,
        [Required] string Address,
        [Required, EmailAddress] string Email,
        [Required] string Role
    );
}
