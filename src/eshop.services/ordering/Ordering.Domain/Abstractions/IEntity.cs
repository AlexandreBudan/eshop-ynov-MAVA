namespace Ordering.Domain.Abstractions;

/// <summary>
/// Defines a contract for all entities within the domain.
/// </summary>
public interface IEntity<T> : IEntity
{
    public T Id { get; set; }
}

/// <summary>
/// Represents the base interface for all entities in the domain, providing common metadata properties.
/// </summary>
public interface IEntity
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}