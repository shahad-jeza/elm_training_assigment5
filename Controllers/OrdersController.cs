using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ECommerceContext _context;

    public OrdersController(ECommerceContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    [Authorize(Policy = "CanViewOrders")]
    public IActionResult GetAllOrders()
    {
        var orders = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToList();
        
        return Ok(orders);
    }

    [HttpPost("{id}/refund")]
    [Authorize(Policy = "CanRefundOrders")]
    public IActionResult RefundOrder(int id)
    {
        var order = _context.Orders.Find(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = "Refunded";
        _context.SaveChanges();
        
        return Ok(new { Message = "Order refunded successfully." });
    }

[HttpPost("create")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> CreateOrder([FromBody] List<OrderItemModel> items)
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    
    // Calculate total amount and validate products
    decimal totalAmount = 0;
    var orderItems = new List<OrderItem>();
    
    foreach (var item in items)
    {
        var product = await _context.Products.FindAsync(item.ProductId);
        if (product == null)
        {
            return BadRequest($"Product with ID {item.ProductId} not found.");
        }
        
        if (product.Stock < item.Quantity)
        {
            return BadRequest($"Insufficient stock for product {product.Name}.");
        }
        
        totalAmount += product.Price * item.Quantity;
        
        // Update product stock
        product.Stock -= item.Quantity;
        
        orderItems.Add(new OrderItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = product.Price
        });
    }
    
    // Create the order
    var order = new Order
    {
        UserId = userId,
        OrderDate = DateTime.UtcNow,
        TotalAmount = totalAmount,
        Status = "Pending",
        OrderItems = orderItems
    };
    
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
}

[HttpGet("{id}")]
public async Task<IActionResult> GetOrder(int id)
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    var isAdmin = User.IsInRole("Admin");
    
    var order = await _context.Orders
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .FirstOrDefaultAsync(o => o.OrderId == id);
    
    if (order == null)
    {
        return NotFound();
    }
    
    // Customers can only view their own orders
    if (!isAdmin && order.UserId != userId)
    {
        return Forbid();
    }
    
    return Ok(order);
}
}