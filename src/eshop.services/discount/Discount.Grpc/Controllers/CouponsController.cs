using Discount.Grpc.Data;
using Discount.Grpc.DTOs;
using Discount.Grpc.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Controllers;

/// <summary>
/// REST API Controller for managing discount coupons lifecycle
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CouponsController : ControllerBase
{
    private readonly DiscountContext _context;
    private readonly ILogger<CouponsController> _logger;

    public CouponsController(DiscountContext context, ILogger<CouponsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new coupon
    /// </summary>
    /// <param name="createDto">Coupon creation data</param>
    /// <returns>The created coupon</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponResponseDto>> CreateCoupon([FromBody] CreateCouponDto createDto)
    {
        _logger.LogInformation("Creating new coupon for product: {ProductName}", createDto.ProductName);

        // Validate dates
        if (createDto.StartDate.HasValue && createDto.EndDate.HasValue && createDto.StartDate > createDto.EndDate)
        {
            return BadRequest("Start date must be before end date");
        }

        // Check if coupon code already exists
        if (!string.IsNullOrEmpty(createDto.CouponCode))
        {
            var exists = await _context.Coupons.AnyAsync(c => c.CouponCode == createDto.CouponCode);
            if (exists)
            {
                return BadRequest($"Coupon code '{createDto.CouponCode}' already exists");
            }
        }

        var coupon = createDto.Adapt<Coupon>();
        coupon.CreatedAt = DateTime.UtcNow;
        coupon.UpdatedAt = DateTime.UtcNow;
        coupon.UpdateStatus(); // Set initial status based on dates

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        var response = MapToResponseDto(coupon);

        _logger.LogInformation("Coupon created with ID: {Id}", coupon.Id);

        return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, response);
    }

    /// <summary>
    /// Gets a specific coupon by ID
    /// </summary>
    /// <param name="id">Coupon ID</param>
    /// <returns>The coupon details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponseDto>> GetCoupon(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound($"Coupon with ID {id} not found");
        }

        // Update status before returning
        coupon.UpdateStatus();
        await _context.SaveChangesAsync();

        return Ok(MapToResponseDto(coupon));
    }

    /// <summary>
    /// Gets a paginated and filtered list of coupons
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of coupons</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<CouponResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<CouponResponseDto>>> GetCoupons(
        [FromQuery] CouponFilterDto filter,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting coupons list - Page: {Page}, Size: {Size}", pageNumber, pageSize);

        var query = _context.Coupons.AsQueryable();

        // Apply filters
        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        if (!string.IsNullOrEmpty(filter.ProductName))
        {
            query = query.Where(c => c.ProductName.Contains(filter.ProductName));
        }

        if (!string.IsNullOrEmpty(filter.ProductId))
        {
            query = query.Where(c => c.ProductId == filter.ProductId);
        }

        if (!string.IsNullOrEmpty(filter.CouponCode))
        {
            query = query.Where(c => c.CouponCode == filter.CouponCode);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(c => c.Type == filter.Type.Value);
        }

        if (filter.IsActive.HasValue && filter.IsActive.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(c =>
                c.Status == CouponStatus.Active &&
                (!c.StartDate.HasValue || c.StartDate <= now) &&
                (!c.EndDate.HasValue || c.EndDate >= now));
        }

        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(c => !c.StartDate.HasValue || c.StartDate >= filter.StartDateFrom.Value);
        }

        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(c => !c.StartDate.HasValue || c.StartDate <= filter.StartDateTo.Value);
        }

        if (filter.EndDateFrom.HasValue)
        {
            query = query.Where(c => !c.EndDate.HasValue || c.EndDate >= filter.EndDateFrom.Value);
        }

        if (filter.EndDateTo.HasValue)
        {
            query = query.Where(c => !c.EndDate.HasValue || c.EndDate <= filter.EndDateTo.Value);
        }

        var totalCount = await query.CountAsync();

        var coupons = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Update statuses
        foreach (var coupon in coupons)
        {
            coupon.UpdateStatus();
        }
        await _context.SaveChangesAsync();

        var result = new PagedResultDto<CouponResponseDto>
        {
            Items = coupons.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing coupon
    /// </summary>
    /// <param name="id">Coupon ID</param>
    /// <param name="updateDto">Update data</param>
    /// <returns>The updated coupon</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponResponseDto>> UpdateCoupon(int id, [FromBody] UpdateCouponDto updateDto)
    {
        _logger.LogInformation("Updating coupon with ID: {Id}", id);

        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound($"Coupon with ID {id} not found");
        }

        // Validate dates if provided
        var startDate = updateDto.StartDate ?? coupon.StartDate;
        var endDate = updateDto.EndDate ?? coupon.EndDate;

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return BadRequest("Start date must be before end date");
        }

        // Check coupon code uniqueness if changed
        if (!string.IsNullOrEmpty(updateDto.CouponCode) && updateDto.CouponCode != coupon.CouponCode)
        {
            var exists = await _context.Coupons.AnyAsync(c => c.CouponCode == updateDto.CouponCode && c.Id != id);
            if (exists)
            {
                return BadRequest($"Coupon code '{updateDto.CouponCode}' already exists");
            }
        }

        // Apply updates (only update non-null fields)
        if (updateDto.ProductName != null) coupon.ProductName = updateDto.ProductName;
        if (updateDto.ProductId != null) coupon.ProductId = updateDto.ProductId;
        if (updateDto.Description != null) coupon.Description = updateDto.Description;
        if (updateDto.Amount.HasValue) coupon.Amount = updateDto.Amount.Value;
        if (updateDto.Type.HasValue) coupon.Type = updateDto.Type.Value;
        if (updateDto.PercentageDiscount.HasValue) coupon.PercentageDiscount = updateDto.PercentageDiscount.Value;
        if (updateDto.CouponCode != null) coupon.CouponCode = updateDto.CouponCode;
        if (updateDto.IsStackable.HasValue) coupon.IsStackable = updateDto.IsStackable.Value;
        if (updateDto.MaxStackPercentage.HasValue) coupon.MaxStackPercentage = updateDto.MaxStackPercentage.Value;
        if (updateDto.Priority.HasValue) coupon.Priority = updateDto.Priority.Value;
        if (updateDto.StartDate.HasValue) coupon.StartDate = updateDto.StartDate;
        if (updateDto.EndDate.HasValue) coupon.EndDate = updateDto.EndDate;
        if (updateDto.MinimumPurchaseAmount.HasValue) coupon.MinimumPurchaseAmount = updateDto.MinimumPurchaseAmount.Value;
        if (updateDto.MaxUsageCount.HasValue) coupon.MaxUsageCount = updateDto.MaxUsageCount.Value;
        if (updateDto.Status.HasValue) coupon.Status = updateDto.Status.Value;

        coupon.UpdatedAt = DateTime.UtcNow;
        coupon.UpdateStatus(); // Recalculate status based on new data

        await _context.SaveChangesAsync();

        _logger.LogInformation("Coupon {Id} updated successfully", id);

        return Ok(MapToResponseDto(coupon));
    }

    /// <summary>
    /// Deletes a coupon permanently
    /// </summary>
    /// <param name="id">Coupon ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        _logger.LogInformation("Deleting coupon with ID: {Id}", id);

        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound($"Coupon with ID {id} not found");
        }

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Coupon {Id} deleted successfully", id);

        return NoContent();
    }

    /// <summary>
    /// Disables a coupon (soft delete)
    /// </summary>
    /// <param name="id">Coupon ID</param>
    /// <returns>The updated coupon</returns>
    [HttpPost("{id}/disable")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponseDto>> DisableCoupon(int id)
    {
        _logger.LogInformation("Disabling coupon with ID: {Id}", id);

        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound($"Coupon with ID {id} not found");
        }

        coupon.Status = CouponStatus.Disabled;
        coupon.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Coupon {Id} disabled successfully", id);

        return Ok(MapToResponseDto(coupon));
    }

    /// <summary>
    /// Enables a disabled coupon
    /// </summary>
    /// <param name="id">Coupon ID</param>
    /// <returns>The updated coupon</returns>
    [HttpPost("{id}/enable")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponseDto>> EnableCoupon(int id)
    {
        _logger.LogInformation("Enabling coupon with ID: {Id}", id);

        var coupon = await _context.Coupons.FindAsync(id);

        if (coupon == null)
        {
            return NotFound($"Coupon with ID {id} not found");
        }

        coupon.UpdateStatus(); // Recalculate status based on dates
        coupon.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Coupon {Id} enabled successfully with status: {Status}", id, coupon.Status);

        return Ok(MapToResponseDto(coupon));
    }

    private static CouponResponseDto MapToResponseDto(Coupon coupon)
    {
        var dto = coupon.Adapt<CouponResponseDto>();
        dto.IsCurrentlyValid = coupon.IsValid();
        return dto;
    }
}
