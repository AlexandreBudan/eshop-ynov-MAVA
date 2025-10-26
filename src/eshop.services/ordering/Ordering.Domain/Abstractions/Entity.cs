namespace Ordering.Domain.Abstractions;

/// <summary>
/// Represents a base entity with common properties and behaviors shared across the domain.
/// </summary>
/// <typeparam name="T">The type of the unique identifier for the entity.</typeparam>
public class Entity<T> : IEntity<T>
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public T Id { get; set; } = default!;
}