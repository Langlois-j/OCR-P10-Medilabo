using AuthService.DTO;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginCredentials credentials)
    {
        User? user = _userService.ValidateCredentials(credentials.Username, credentials.Password);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse(_tokenService.GenerateToken(user)));
    }
}
