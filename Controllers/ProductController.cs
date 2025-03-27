using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ProductsController : ControllerBase
{
    private readonly ECommerceContext _context;

    public ProductsController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        product.CreatedBy = userId;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.ProductId)
        {
            return BadRequest();
        }

        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return product;
    }

    [HttpGet]
[AllowAnonymous]
public IActionResult GetProducts()
{
    var products = _context.Products.ToList();
    return Ok(products);
}
}