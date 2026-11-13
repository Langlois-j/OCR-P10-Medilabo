using AuthService.Models;

namespace AuthService.Services;

public interface IUserService
{
    User? ValidateCredentials(string username, string password);
}
