using EmployeeCrudApi.Controllers;
using EmployeeCrudApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Backend.Repositories;

namespace EmployeeCrudApi.Tests
{
    public class ProductControllerUnitTests
    {
        private class FakeProductRepository : IProductRepository
        {
            private readonly List<Product> _items = new();

            public FakeProductRepository(IEnumerable<Product> seed = null)
            {
                if (seed != null) _items.AddRange(seed);
            }

            public Task<Product> CreateAsync(Product product)
            {
                if (product.Id == 0)
                {
                    product.Id = _items.Any() ? _items.Max(x => x.Id) + 1 : 1;
                }
                _items.Add(product);
                return Task.FromResult(product);
            }

            public Task<bool> DeleteAsync(int id)
            {
                var idx = _items.FindIndex(x => x.Id == id);
                if (idx >= 0) { _items.RemoveAt(idx); return Task.FromResult(true); }
                return Task.FromResult(false);
            }

            public Task<List<Product>> GetAllAsync() => Task.FromResult(_items.ToList());

            public Task<Product> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

            public Task<Product> UpdateAsync(Product product)
            {
                var idx = _items.FindIndex(x => x.Id == product.Id);
                if (idx < 0) return Task.FromResult<Product>(null);
                _items[idx] = product;
                return Task.FromResult(product);
            }
        }

        [Fact]
        public async Task SetStock_Valid_UpdatesStock()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 1, Name = "P1", Stock = 0 } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);
            var result = await controller.SetStock(1, new ProductController.StockDto { Amount = 5 });
            var ok = Assert.IsType<OkObjectResult>(result);
            var prod = Assert.IsType<Product>(ok.Value);
            Assert.Equal(5, prod.Stock);
        }

        [Fact]
        public async Task SetStock_InvalidRange_ReturnsBadRequest()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 2, Name = "P2", Stock = 0 } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);
            var result = await controller.SetStock(2, new ProductController.StockDto { Amount = 500 });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task IncrementStock_Valid_Increments()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 3, Name = "P3", Stock = 2 } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);
            var result = await controller.IncrementStock(3, new ProductController.StockDto { Amount = 3 });
            var ok = Assert.IsType<OkObjectResult>(result);
            var prod = Assert.IsType<Product>(ok.Value);
            Assert.Equal(5, prod.Stock);
        }

        [Fact]
        public async Task DecrementStock_Valid_Decrements()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 4, Name = "P4", Stock = 10 } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);
            var result = await controller.DecrementStock(4, new ProductController.StockDto { Amount = 3 });
            var ok = Assert.IsType<OkObjectResult>(result);
            var prod = Assert.IsType<Product>(ok.Value);
            Assert.Equal(7, prod.Stock);
        }

        [Fact]
        public async Task IncrementStock_TooHigh_ReturnsBadRequest()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 5, Name = "P5", Stock = 99 } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);
            var result = await controller.IncrementStock(5, new ProductController.StockDto { Amount = 5 });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            var repo = new FakeProductRepository();
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);
            controller.ModelState.AddModelError("Name", "Required");

            var result = await controller.Create(new Product { Id = 10, Name = "" });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_DuplicateName_LogsWarning_WithMoq()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 1, Name = "Juan Perez" } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            var result = await controller.Create(new Product { Id = 2, Name = "Juan PEREZ" });
            Assert.IsType<BadRequestObjectResult>(result);

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Duplicate name attempted")),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
            ), Times.AtLeastOnce());
        }

        // The following EF-specific test is removed since controller no longer depends on EF directly.

        [Fact]
        public async Task SetStock_NullDto_ReturnsBadRequest()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 30, Name = "P30", Stock = 1 } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            var result = await controller.SetStock(30, null!);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            var repo = new FakeProductRepository();
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            var result = await controller.Update(new Product { Id = 999, Name = "X" });
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
