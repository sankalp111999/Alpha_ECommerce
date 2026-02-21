
using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Infrastructure.Data;
using eCommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationApi.Infrastructure.Repositories
{
    public class UserRepository(AuthenticationDbContext context, IConfiguration config) : IUser
    {
        public async Task<AppUser> GetUserByEmail(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user is null ? null! : user!;
            
        }
        public async Task<GetUserDTO> GetUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user is not null? new GetUserDTO
            (
                user.Id,
                user.Name!,
                user.PhoneNumber!,
                user.Address!,
                user.Email!,
                user.Role!
            ) : null!;
        }

        public async Task<Response> Login(LoginDTO loginDTO)
        {
            var getUser = await GetUserByEmail(loginDTO.email);
            if (getUser is null)
                return new Response(false,"Invalid email or password");

            bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDTO.password, getUser.Password!);
            if (!verifyPassword)
                return new Response(false, "Invalid email or password");

            string token = GenerateToken(getUser);
            return new Response(true, token);
        }

        private string GenerateToken(AppUser user)
        {
            // Token generation logic here (e.g., using JWT)
            var key = Encoding.UTF8.GetBytes(config["Authentication:Key"]!);
            var secuityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(secuityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, user.Name!),
                new (ClaimTypes.Email, user.Email!)
            };
            if(!string.IsNullOrEmpty(user.Role) || !Equals("string", user.Role))
            {
                claims.Add(new (ClaimTypes.Role, user.Role!));
            }

            var token = new JwtSecurityToken(
                issuer: config["Authentication:Issuer"],
                audience: config["Authentication:Audience"],
                claims: claims,
                expires: null,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Response> Register(AppUserDTO appUserDTO)
        {
            var getUser =await GetUserByEmail(appUserDTO.Email);
            if(getUser is not null) 
                return new Response(false, "You cannot use this email for registration");

            var result = context.Users.Add(new AppUser
            {
                Name = appUserDTO.Name,
                PhoneNumber = appUserDTO.PhoneNumber,
                Address = appUserDTO.Address,
                Email = appUserDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(appUserDTO.Password),
                Role = appUserDTO.Role
            });
            await context.SaveChangesAsync();
            return result.Entity.Id > 0 ? new Response(true, "Registration successful") : new Response(false, "Registration failed: Invalid Data");
        }

    }
}
