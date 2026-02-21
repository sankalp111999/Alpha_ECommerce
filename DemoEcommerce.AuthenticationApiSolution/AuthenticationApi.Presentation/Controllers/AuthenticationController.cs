using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]       //all endpoints in this controller can be accessed without authentication
    public class AuthenticationController(IUser userInterface) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<Response>> Register(AppUserDTO appUserDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await userInterface.Register(appUserDTO);
            return result.Flag ? Ok(result) : BadRequest(Request);
        }

        [HttpPost("login")]
        public async Task<ActionResult<Response>> Login(LoginDTO loginDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await userInterface.Login(loginDTO);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}")]
        [Authorize]   //overrides the AllowAnonymous at the controller level, this endpoint requires authentication
        public async Task<ActionResult<GetUserDTO>> GetUser(int id)
        {
            if(id <= 0) return BadRequest("Invalid user ID");
            var result = await userInterface.GetUser(id);
            return result.Id > 0 ? Ok(result) : NotFound(Request);
        }
    }
}
