namespace AuthService.Models;

public class User
{
    public required string Username { get; init; }

    public required string PasswordHash { get; init; }
}
