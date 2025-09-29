using Discount.Grpc.Data;
using Discount.Grpc.Data.Extensions;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddDbContext<DiscountContext>(options => options.UseSqlite(configuration.GetConnectionString("DiscountConnection")));

var app = builder.Build();

app.UseCustomMigration();

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountServiceServer>();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();