using Moq;
using Xunit;
using Dapper;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace ECommerceSystem.Repositories
{
    public class OrderRepositoryTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly OrderRepository _repository;
        private readonly string _testConnectionString = "DataSource=:memory:";

        public OrderRepositoryTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c.GetConnectionString("DefaultConnection"))
                      .Returns(_testConnectionString);
            
            _repository = new OrderRepository(_mockConfig.Object);
            
            // Initialize in-memory database
            InitializeTestDatabase();
        }

        private void InitializeTestDatabase()
        {
            using var connection = new SqliteConnection(_testConnectionString);
            connection.Open();

            // Create tables
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Orders (
                    OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    OrderDate TEXT NOT NULL,
                    TotalAmount REAL NOT NULL,
                    Status TEXT NOT NULL
                )");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Products (
                    ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Price REAL NOT NULL,
                    Stock INTEGER NOT NULL
                )");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS OrderItems (
                    OrderItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL,
                    Price REAL NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
                    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
                )");

            // Seed test data
            connection.Execute(@"
                INSERT INTO Products (ProductId, Name, Price, Stock)
                VALUES (1, 'Test Product', 10.99, 100)");

            connection.Execute(@"
                INSERT INTO Orders (OrderId, UserId, OrderDate, TotalAmount, Status)
                VALUES (1, 1, '2023-01-01', 21.98, 'Pending')");

            connection.Execute(@"
                INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
                VALUES (1, 1, 2, 10.99)");
        }

        [Fact]
        public void GetCustomerOrdersDapper_ReturnsOrdersWithItems()
        {
            // Arrange
            var testUserId = 1;

            // Act
            var result = _repository.GetCustomerOrdersDapper(testUserId).ToList();

            // Assert
            Assert.Single(result);
            var order = result.First();
            Assert.Equal(1, order.OrderId);
            Assert.Single(order.OrderItems);
            Assert.Equal("Test Product", order.OrderItems.First().Product.Name);
        }

        [Fact]
        public void GetProductByIdDapper_ReturnsCorrectProduct()
        {
            // Arrange
            var testProductId = 1;

            // Act
            var result = _repository.GetProductByIdDapper(testProductId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal(10.99m, result.Price);
        }

        [Fact]
        public void GetCustomerOrdersDapper_ReturnsEmptyForInvalidUser()
        {
            // Arrange
            var invalidUserId = 999;

            // Act
            var result = _repository.GetCustomerOrdersDapper(invalidUserId);

            // Assert
            Assert.Empty(result);
        }
    }
}