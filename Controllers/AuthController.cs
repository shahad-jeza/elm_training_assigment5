using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BCrypt.Net;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ECommerceContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ECommerceContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        // Check if user already exists
        if (_context.Users.Any(u => u.Email == model.Email))
        {
            return BadRequest("User already exists.");
        }

        // Create new user
        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = model.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginModel model)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = GenerateJwtToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpiryInMinutes"))
        });
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("CanViewOrders", "true") // Example claim
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpiryInMinutes")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("user/profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(new
        {
            user.UserId,
            user.Name,
            user.Email,
            user.Role
        });
    }
}