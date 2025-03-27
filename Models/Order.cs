public class Order
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } // "Pending", "Completed", "Cancelled"
    public ICollection<OrderItem> OrderItems { get; set; }
}