using Microsoft.EntityFrameworkCore;
using SampleCSharpAPI.Data;
using SampleCSharpAPI.DTOs;
using SampleCSharpAPI.Models;

namespace SampleCSharpAPI.Features.Products;

public static class ProductsEndpointV1
{
    public const string ProductsTag = "Products";

    public static void MapProductsEndpointsV1(this WebApplication app)
    {
        app.MapGet("/api/products", async (ProductsDbContext db) =>
        {
            return await db.Products.Select(p => new ProductDto(p.Id, p.Name, p.Price)).ToListAsync();
        })
        .Produces<List<Product>>(StatusCodes.Status200OK)
        .WithTags(ProductsTag);

        app.MapGet("/api/products/{id}", async (int id, ProductsDbContext db) =>
        {
            return await db.Products.FindAsync(id)
                is Product product
                    ? Results.Ok(new ProductDto(product.Id, product.Name, product.Price))
                    : Results.NotFound("Product not found.");
        })
        .Produces<Product>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(ProductsTag);

        app.MapPost("/api/products", async (CreateProductDto productDto, ProductsDbContext db) =>
        {
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/products/{product.Id}", new ProductDto(product.Id, product.Name, product.Price));
        })
        .Produces<Product>(StatusCodes.Status201Created)
        .WithTags(ProductsTag);

        app.MapPut("/api/products/{id}", async (int id, EditProductDto productDto, ProductsDbContext db) =>
        {
            if (id != productDto.Id)
            {
                return Results.BadRequest("Product ID mismatch.");
            }

            var product = await db.Products.FindAsync(id);
            if (product is null)
            {
                return Results.NotFound("Product not found.");
            }
            product.Name = productDto.Name;
            product.Price = productDto.Price;
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(ProductsTag);

        app.MapDelete("/api/products/{id}", async (int id, ProductsDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
            {
                return Results.NotFound();
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(ProductsTag);
    }
}
