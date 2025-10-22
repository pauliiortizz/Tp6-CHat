using EmployeeCrudApi.Controllers;
using EmployeeCrudApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Backend.Repositories;

namespace EmployeeCrudApi.Tests
{
    public class EmployeeControllerTests
    {
        private class FakeProductRepository : IProductRepository
        {
            private readonly List<Product> _items = new();

            public FakeProductRepository(IEnumerable<Product> seed = null)
            {
                if (seed != null) _items.AddRange(seed);
            }

            public Task<List<Product>> GetAllAsync() => Task.FromResult(_items.ToList());
            public Task<Product> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
            public Task<Product> CreateAsync(Product product)
            {
                if (product.Id == 0)
                {
                    product.Id = _items.Any() ? _items.Max(x => x.Id) + 1 : 1;
                }
                _items.Add(product);
                return Task.FromResult(product);
            }
            public Task<Product> UpdateAsync(Product product)
            {
                var idx = _items.FindIndex(x => x.Id == product.Id);
                if (idx < 0) return Task.FromResult<Product>(null);
                _items[idx] = product;
                return Task.FromResult(product);
            }
            public Task<bool> DeleteAsync(int id)
            {
                var idx = _items.FindIndex(x => x.Id == id);
                if (idx >= 0) { _items.RemoveAt(idx); return Task.FromResult(true); }
                return Task.FromResult(false);
            }
        }

        [Fact]
        public async Task GetAll_ReturnsListOfEmployees()
        {
            // Arrange
            var repo = new FakeProductRepository(new[] {
                new Product { Id = 1, Name = "John DOE" },
                new Product { Id = 2, Name = "Jane DOE" }
            });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("John DOE", result[0].Name);
            Assert.Equal("Jane DOE", result[1].Name);
        }

        [Fact]
        public async Task GetById_ReturnsEmployeeById()
        {
            // Arrange
            var repo = new FakeProductRepository(new[] { new Product { Id = 1, Name = "John DOE" } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            // Act
            var result = await controller.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John DOE", result.Name);
        }

        [Fact]
        public async Task Create_AddsEmployee()
        {
            // Arrange
            var repo = new FakeProductRepository();
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            var newProduct = new Product { Id = 3, Name = "New Product" };

            // Act
            var createResult = await controller.Create(newProduct);

            // Assert
            var list = await repo.GetAllAsync();
            var product = list.FirstOrDefault(x => x.Id == 3);
            Assert.NotNull(product);
            // The controller formats names; ensure it's set (case-insensitive check)
            Assert.Equal("New PRODUCT", product.Name);
        }

        [Fact]
        public async Task Update_UpdatesEmployee()
        {
            // Arrange
            var existingProduct = new Product { Id = 1, Name = "Old NAME" };
            var repo = new FakeProductRepository(new[] { existingProduct });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            var updatedProduct = new Product { Id = 1, Name = "Updated Name" };

            // Act
            await controller.Update(updatedProduct);

            // Assert
            var list = await repo.GetAllAsync();
            var product = list.FirstOrDefault(x => x.Id == 1);
            Assert.NotNull(product);
            Assert.Equal("Updated NAME", product!.Name);
        }

        [Fact]
        public async Task Delete_RemovesEmployee()
        {
            // Arrange
            var productToDelete = new Product { Id = 1, Name = "John Doe" };
            var repo = new FakeProductRepository(new[] { productToDelete });
            var logger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            // Act
            await controller.Delete(1);

            // Assert
            var list = await repo.GetAllAsync();
            Assert.DoesNotContain(list, x => x.Id == 1); // Verifica que el producto fue eliminado
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            // Arrange
            var repo = new FakeProductRepository();
            var logger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<ProductController>>();
            var controller = new ProductController(repo, logger.Object);

            // Act
            var result = await controller.Delete(12345);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Rejects_DuplicateName()
        {
            var repo = new FakeProductRepository(new[] { new Product { Id = 1, Name = "Existing User" } });
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new EmployeeCrudApi.Controllers.ProductController(repo, logger.Object);
            var newProduct = new Product { Id = 2, Name = "existing user" }; // different case

            var result = await controller.Create(newProduct);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_Formats_Name_As_GivenAndUppercaseSurname()
        {
            var repo = new FakeProductRepository();
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new EmployeeCrudApi.Controllers.ProductController(repo, logger.Object);

            var newProduct = new Product { Id = 5, Name = "juan carlos chamizo" };
            var result = await controller.Create(newProduct);

            Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            var list = await repo.GetAllAsync();
            var stored = list.FirstOrDefault(x => x.Id == 5);
            Assert.Equal("Juan Carlos CHAMIZO", stored!.Name);
        }

        [Fact]
        public async Task Create_Rejects_Names_With_Digits()
        {
            var repo = new FakeProductRepository();
            var logger = new Mock<ILogger<ProductController>>();
            var controller = new EmployeeCrudApi.Controllers.ProductController(repo, logger.Object);

            var newProduct = new Product { Id = 6, Name = "John D0e" };
            var result = await controller.Create(newProduct);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_Rejects_Excessive_Repeats()
        {
            var repo = new FakeProductRepository();
            var logger = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<ProductController>>();
            var controller = new EmployeeCrudApi.Controllers.ProductController(repo, logger.Object);

            var newProduct = new Product { Id = 7, Name = "Juuuuaannnn Perez" };
            var result = await controller.Create(newProduct);

            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        }
    }
}
