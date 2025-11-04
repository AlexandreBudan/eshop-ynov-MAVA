namespace IntegrationTests
{
    /// <summary>
    /// Custom WebApplicationFactory for integration testing
    /// Configures Basket, Catalog, and Discount services with in-memory databases
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Basket.API.Program>
    {
        public IServiceProvider BasketServices { get; private set; } = null!;
        public IServiceProvider DiscountServices { get; private set; } = null!;
        public IServiceProvider CatalogServices { get; private set; } = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Configure Basket API with in-memory database
                services.RemoveAll(typeof(DbContextOptions<BasketContext>));
                services.AddDbContext<BasketContext>(options =>
                {
                    options.UseInMemoryDatabase("BasketTestDb_" + Guid.NewGuid());
                });

                // Configure Catalog API with in-memory database
                services.RemoveAll(typeof(DbContextOptions<CatalogContext>));
                services.AddDbContext<CatalogContext>(options =>
                {
                    options.UseInMemoryDatabase("CatalogTestDb_" + Guid.NewGuid());
                });

                // Configure Discount service with in-memory database
                services.RemoveAll(typeof(DbContextOptions<DiscountContext>));
                services.AddDbContext<DiscountContext>(options =>
                {
                    options.UseInMemoryDatabase("DiscountTestDb_" + Guid.NewGuid());
                });

                // Build temporary service provider to initialize databases
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;

                // Initialize Basket database
                var basketDb = scopedServices.GetRequiredService<BasketContext>();
                basketDb.Database.EnsureCreated();

                // Initialize Catalog database
                var catalogDb = scopedServices.GetRequiredService<CatalogContext>();
                catalogDb.Database.EnsureCreated();

                // Initialize Discount database
                var discountDb = scopedServices.GetRequiredService<DiscountContext>();
                discountDb.Database.EnsureCreated();

                // Store service providers for test access
                BasketServices = sp;
                CatalogServices = sp; // Catalog services also use this scope
            });

            // Configure separate Discount service instance
            var discountBuilder = new WebHostBuilder()
                .UseEnvironment("Testing")
                .ConfigureServices(services =>
                {
                    services.AddDbContext<DiscountContext>(options =>
                    {
                        options.UseInMemoryDatabase("DiscountTestDb_" + Guid.NewGuid());
                    });
                    services.AddScoped<Discount.Grpc.Services.DiscountCalculator>();
                });

            DiscountServices = discountBuilder.Build().Services;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (BasketServices as IDisposable)?.Dispose();
                (DiscountServices as IDisposable)?.Dispose();
                (CatalogServices as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
