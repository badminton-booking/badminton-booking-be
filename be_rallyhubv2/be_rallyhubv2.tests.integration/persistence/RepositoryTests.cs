using be_rallyhubv2.domain.abstractions.repositories;
using be_rallyhubv2.domain.entities;
using be_rallyhubv2.domain.enums;
using be_rallyhubv2.persistence;
using be_rallyhubv2.persistence.dependencyInjection;
using be_rallyhubv2.persistence.repositories;
using be_rallyhubv2.tests.integration.support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace be_rallyhubv2.tests.integration.persistence;

public interface IConfigTestRepository : IRepositoryBase<Config>;

public sealed class ConfigTestRepository(AppDbContext dbContext)
    : RepositoryBase<Config>(dbContext), IConfigTestRepository;

public class RepositoryRegistrationTests
{
    [Fact]
    public void Repositories_are_registered_by_naming_convention()
    {
        var services = new ServiceCollection();

        services.AddRepositoriesFromAssembly(typeof(ConfigTestRepository).Assembly);

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IConfigTestRepository));
        Assert.Equal(typeof(ConfigTestRepository), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void Persistence_assembly_registers_the_unit_of_work()
    {
        var services = new ServiceCollection();

        services.AddPersistence("Host=localhost;Database=unused");

        Assert.Contains(services, d => d.ServiceType == typeof(IUnitOfWork) && d.ImplementationType == typeof(EfUnitOfWork));
    }
}

[Collection(PostgreSqlFixture.PostgreSqlCollection.Name)]
public class RepositoryBaseTests(PostgreSqlFixture fixture)
{
    private static Config NewConfig() => new()
    {
        Scope = ConfigScope.Platform,
        Key = "test_" + Guid.NewGuid().ToString("N")[..12],
        Value = "1"
    };

    [Fact]
    public async Task Added_entity_can_be_found_by_id()
    {
        var config = NewConfig();
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var repository = new ConfigTestRepository(scope.ServiceProvider.GetRequiredService<AppDbContext>());
            repository.Add(config);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(default);
        }

        await using var checkScope = fixture.Services.CreateAsyncScope();
        var found = await new ConfigTestRepository(checkScope.ServiceProvider.GetRequiredService<AppDbContext>())
            .GetByIdAsync(config.Id, default);

        Assert.NotNull(found);
        Assert.Equal(config.Key, found.Key);
    }

    [Fact]
    public async Task Unknown_id_returns_null()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var repository = new ConfigTestRepository(scope.ServiceProvider.GetRequiredService<AppDbContext>());

        Assert.Null(await repository.GetByIdAsync(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task Remove_is_a_soft_delete_that_hides_the_row()
    {
        var config = NewConfig();
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var repository = new ConfigTestRepository(scope.ServiceProvider.GetRequiredService<AppDbContext>());
            repository.Add(config);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(default);
        }

        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var repository = new ConfigTestRepository(scope.ServiceProvider.GetRequiredService<AppDbContext>());
            var tracked = await repository.GetByIdAsync(config.Id, default);
            repository.Remove(tracked!);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(default);
        }

        await using var checkScope = fixture.Services.CreateAsyncScope();
        var db = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Null(await new ConfigTestRepository(db).GetByIdAsync(config.Id, default));
        var row = await db.Set<Config>().IgnoreQueryFilters().SingleAsync(c => c.Id == config.Id);
        Assert.True(row.IsDeleted);
    }
}