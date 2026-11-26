using be_rallyhubv2.persistence;
using be_rallyhubv2.persistence.dependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace be_rallyhubv2.tests.integration.support;

public sealed class PostgreSqlFixture: IAsyncLifetime
{

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public ServiceProvider Services { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var services = new ServiceCollection();
        services.AddPersistence(_container.GetConnectionString());
        Services = services.BuildServiceProvider();
        
        await using var scope = Services.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();
        await _container.DisposeAsync();
    }

    [CollectionDefinition(Name)]
    public sealed class PostgreSqlCollection  : ICollectionFixture<PostgreSqlFixture>
    {
        public const string Name = "postgres";
    }
}