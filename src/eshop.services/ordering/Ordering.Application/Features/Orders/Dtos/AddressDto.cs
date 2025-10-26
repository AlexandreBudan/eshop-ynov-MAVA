namespace Ordering.Application.Features.Orders.Dtos;

public record AddressDto(string FirstName, string LastName, string EmailAddress, string AddressLine, string State, string Country, string ZipCode);