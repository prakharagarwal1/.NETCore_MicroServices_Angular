namespace LearningShop.Gateway.Models;

public class UserRecord
{
    public string Username { get; }
    public string Email { get; }
    public string Password { get; }
    public string Role { get; }

    public UserRecord(string username, string email, string password, string role)
    {
        Username = username;
        Email = email;
        Password = password;
        Role = role;
    }
}