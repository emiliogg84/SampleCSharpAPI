using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleCSharpAPI.Data;
using SampleCSharpAPI.DTOs;
using SampleCSharpAPI.Models;
using System.Net;
using System.Net.Http.Json;

namespace SampleCSharpAPI.Features.Products.Tests
{
    [TestClass]
    public class ProductsEndpointV1Tests
    {
        private static WebApplicationFactory<SampleCSharpAPI.Program> _factory;
        private static HttpClient _client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _factory = new WebApplicationFactory<SampleCSharpAPI.Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ProductsDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ProductsDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task GetAllProducts_ReturnsOk()
        {
            // Arrange
            var dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ProductsDbContext>();
            dbContext.Products.Add(new Product { Name = "Test Product", Price = 10.99m });
            dbContext.SaveChanges();

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            Assert.IsNotNull(products);
            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public async Task GetProductById_ReturnsProduct_WhenExists()
        {
            // Arrange
            var dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ProductsDbContext>();
            var product = new Product { Name = "Test Product", Price = 10.99m };
            dbContext.Products.Add(product);
            dbContext.SaveChanges();

            // Act
            var response = await _client.GetAsync($"/api/products/{product.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var returnedProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
            Assert.IsNotNull(returnedProduct);
            Assert.AreEqual(product.Name, returnedProduct.Name);
        }

        [TestMethod]
        public async Task GetProductById_ReturnsNotFound_WhenNotExists()
        {
            // Act
            var response = await _client.GetAsync("/api/products/999");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProduct_ReturnsCreated()
        {
            // Arrange
            var productDto = new CreateProductDto("New Product", 15.99m);

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", productDto);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var createdProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
            Assert.IsNotNull(createdProduct);
            Assert.AreEqual(productDto.Name, createdProduct.Name);
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsNoContent_WhenExists()
        {
            // Arrange
            var dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ProductsDbContext>();
            var product = new Product { Name = "Old Product", Price = 20.99m };
            dbContext.Products.Add(product);
            dbContext.SaveChanges();

            var editDto = new EditProductDto(product.Id, "Updated Product", 25.99m);

            // Act
            var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", editDto);

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateProduct_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            var editDto = new EditProductDto(999, "Updated Product", 25.99m);

            // Act
            var response = await _client.PutAsJsonAsync("/api/products/999", editDto);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProduct_ReturnsNoContent_WhenExists()
        {
            // Arrange
            var dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ProductsDbContext>();
            var product = new Product { Name = "Test Product", Price = 10.99m };
            dbContext.Products.Add(product);
            dbContext.SaveChanges();

            // Act
            var response = await _client.DeleteAsync($"/api/products/{product.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProduct_ReturnsNotFound_WhenNotExists()
        {
            // Act
            var response = await _client.DeleteAsync("/api/products/999");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
