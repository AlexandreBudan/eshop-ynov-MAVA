using Catalog.API.Features.Products.Commands.BulkImportProducts;
using Catalog.API.Features.Products.Commands.CreateProduct;
using Catalog.API.Features.Products.Commands.DeleteProduct;
using Catalog.API.Features.Products.Commands.UpdateProduct;
using Catalog.API.Features.Products.Queries.GetProductById;
using Catalog.API.Features.Products.Queries.GetProductByCategory;
using Catalog.API.Features.Products.Queries.GetProducts;
using Catalog.API.Features.Products.Queries.ExportProducts;
using Catalog.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

/// <summary>
/// Manages operations related to products within the catalog, including retrieving product data
/// and creating new products.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ProductsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>The product matching the specified identifier, if found; otherwise, a not found response.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductById(Guid id)
    {
        var result = await sender.Send(new GetProductByIdQuery(id));
        return Ok(result.Product);

    }

    /// <summary>
    /// Retrieves a collection of products within a specified category.
    /// </summary>
    /// <param name="category">The category by which to filter the products.</param>
    /// <returns>A collection of products belonging to the specified category, if found; otherwise, a bad request response.</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("Category is required");
    
        var result = await sender.Send(new GetProductByCategoryQuery(category));
        return Ok(result.Products); 
    }

    /// <summary>
    /// Retrieves a paginated collection of products from the catalog.
    /// </summary>
    /// <param name="pageNumber">The current page number (starting from 1).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="name">Optional: Filter by product name.</param>
    /// <param name="minPrice">Optional: Filter by minimum price.</param>
    /// <param name="maxPrice">Optional: Filter by maximum price.</param>
    /// <param name="category">Optional: Filter by category.</param>
    /// <returns>A collection of products wrapped in an action result.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? category = null)
    {
        var result = await sender.Send(new GetProductsQuery(pageNumber, pageSize, name, minPrice, maxPrice, category));
        return Ok(result.Products);
    }

    /// <summary>
    /// Handles the creation of a new product.
    /// </summary>
    /// <param name="request">The command containing the details of the product to be created.</param>
    /// <returns>A result object containing the ID of the newly created product.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateProductCommandResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateProductCommandResult>> CreateProduct(CreateProductCommand request)
    {
        var result = await sender.Send(request);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates a product with the specified ID using the provided update details.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="request">The details to update the specified product.</param>
    /// <returns>A boolean indicating whether the update was successful or an appropriate error response if the product was not found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand request)
    {
        var result = await sender.Send(new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.ImageFile, request.Categories));
        return Ok(result.Product);
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>True if the product was successfully deleted; otherwise, a not found response.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> DeleteProduct(Guid id)
    {
        var result = await sender.Send(new DeleteProductCommand(id));
        return Ok(result.IsSuccessful);
    }

    /// <summary>
    /// Imports multiple products from an Excel file.
    /// </summary>
    /// <param name="file">The Excel file containing the products to import.</param>
    /// <returns>Import statistics including success count, failure count, and errors.</returns>
    /// <remarks>
    /// Expected Excel format:
    /// - Column 1: Name (required)
    /// - Column 2: Description
    /// - Column 3: Price (required, numeric)
    /// - Column 4: ImageFile
    /// - Column 5: Categories (comma-separated)
    ///
    /// The first row should contain headers and will be skipped.
    /// </remarks>
    [HttpPost("bulk-import")]
    [ProducesResponseType(typeof(BulkImportProductsCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkImportProductsCommandResult>> BulkImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");
        await using var fileStream = file.OpenReadStream();
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        var command = new BulkImportProductsCommand
        {
            FileStream = memoryStream,
            FileName = file.FileName
        };
        var result = await sender.Send(command);
        memoryStream.Dispose();
        if (result.TotalProcessed == 0)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Exports all products to an Excel or CSV file.
    /// </summary>
    /// <param name="name">Optional: Filter by product name.</param>
    /// <param name="minPrice">Optional: Filter by minimum price.</param>
    /// <param name="maxPrice">Optional: Filter by maximum price.</param>
    /// <param name="category">Optional: Filter by category.</param>
    /// <param name="format">The format of the export file (excel or csv).</param>
    /// <returns>An Excel or CSV file containing all products.</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportProducts([FromQuery] string? name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] string? category, [FromQuery] string format = "excel")
    {
        if (!string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase) && !string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Invalid format. Supported values are 'excel' and 'csv'.");
        }
        var result = await sender.Send(new ExportProductsQuery(name, minPrice, maxPrice, category, format));
        var contentType = format.Equals("csv", StringComparison.OrdinalIgnoreCase) ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(result.FileContents, contentType, result.FileName);
    }
}