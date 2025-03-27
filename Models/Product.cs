public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CreatedBy { get; set; } // UserId of the admin who created it
    public ICollection<OrderItem> OrderItems { get; set; }
}