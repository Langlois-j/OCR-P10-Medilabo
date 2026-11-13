using AuthService.Controllers;
using AuthService.DTO;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
namespace AuthService.UnitTests;

public class AuthControllerTests
{
    private sealed class FakeUserService : IUserService
    {
        public User? ValidateCredentials(string username, string password) =>
            username == "medilabo" && password == "secret"
                ? new User { Username = username, PasswordHash = "hash" }
                : null;
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateToken(User user) => "fake-token";
    }

    private static AuthController CreateController() =>
        new(new FakeUserService(), new FakeTokenService());

    [Fact]
    public void LoginWithValidCredentialsReturnsOkWithToken()
    {
        var result = CreateController().Login(new LoginCredentials("medilabo", "secret"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.Equal("fake-token", response.Token);
    }

    [Fact]
    public void LoginWithInvalidCredentialsReturnsUnauthorized()
    {
        var result = CreateController().Login(new LoginCredentials("medilabo", "wrong"));

        Assert.IsType<UnauthorizedResult>(result.Result);
    }
}
