public class RegisterModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } = "Customer"; // Default role
}

public class LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}