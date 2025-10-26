using Ordering.API.Extensions;
using Ordering.Application.Extensions;
using Ordering.Infrastructure;
using Ordering.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services
    .AddApplicationServices(configuration)
    .AddInfraStructureServices(configuration)
    .AddApiServices(configuration);

var app = builder.Build();

app.UseApiServices();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.InitialiseDatabaseAsync();
}

app.Run();