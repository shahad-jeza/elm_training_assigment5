using Moq;
using Xunit;
using Dapper;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using ECommerceSystem.Repositories;

namespace ECommerceSystem.Tests
{
    public class OrderRepositoryTests
    {
        private readonly Mock<ECommerceContext> _mockContext;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests()
        {
            _mockContext = new Mock<ECommerceContext>();
            _repository = new OrderRepository(_mockContext.Object);
        }

        [Fact]
        public async Task GetOrdersByUserId_ReturnsUserOrders()
        {
            // Arrange
            var testUserId = 1;
            var testData = new List<Order>
            {
                new Order { OrderId = 1, UserId = testUserId },
                new Order { OrderId = 2, UserId = testUserId },
                new Order { OrderId = 3, UserId = 2 } // Different user
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Order>>();
            mockSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(testData.Provider);
            mockSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());

            _mockContext.Setup(c => c.Orders).Returns(mockSet.Object);

            // Act
            var result = _repository.GetOrdersByUserId(testUserId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, o => Assert.Equal(testUserId, o.UserId));
        }

        [Fact]
        public async Task AddOrder_SavesToDatabase()
        {
            // Arrange
            var testOrder = new Order { OrderId = 1, UserId = 1 };
            var mockSet = new Mock<DbSet<Order>>();
            _mockContext.Setup(m => m.Orders).Returns(mockSet.Object);

            // Act
            _repository.AddOrder(testOrder);

            // Assert
            mockSet.Verify(m => m.Add(It.IsAny<Order>()), Times.Once());
            _mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }
    }
}