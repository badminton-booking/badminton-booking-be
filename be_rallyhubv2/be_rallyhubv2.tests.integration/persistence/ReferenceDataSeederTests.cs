using be_rallyhubv2.domain.constants;
using be_rallyhubv2.domain.entities;
using be_rallyhubv2.domain.enums;
using be_rallyhubv2.persistence;
using be_rallyhubv2.persistence.seeding;
using be_rallyhubv2.tests.integration.support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace be_rallyhubv2.tests.integration.persistence;

public class ReferenceDataTests
{
    [Fact]
    public void Every_permission_constant_is_in_the_catalog_and_every_role_link_points_to_it()
    {
        var constants = typeof(PermissionCodes).GetFields()
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToHashSet();
        var catalog = ReferenceData.Permissions.Select(p => p.Code).ToHashSet();

        Assert.Equal(constants, catalog);
        Assert.All(ReferenceData.RolePermissions.Values.SelectMany(codes => codes), code => Assert.Contains(code, catalog));
    }
}

[Collection(PostgreSqlFixture.PostgreSqlCollection.Name)]
public class ReferenceDataSeederTests(PostgreSqlFixture fixture)
{
    private async Task SeedAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ReferenceDataSeeder>().SeedAsync(default);
    }

    private async Task<(int Roles, int Permissions, int Links, int PlatformConfigs)> CountAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (
            await db.Set<Role>().CountAsync(),
            await db.Set<Permission>().CountAsync(),
            await db.Set<RolePermission>().CountAsync(),
            await db.Set<Config>().CountAsync(c => c.Scope == ConfigScope.Platform
                                                   && ReferenceData.PlatformConfigs.Select(x => x.Key).Contains(c.Key)));
    }

    [Fact]
    public async Task Seeds_roles_permissions_links_and_platform_configs()
    {
        await SeedAsync();

        var counts = await CountAsync();
        Assert.Equal(4, counts.Roles);
        Assert.Equal(16, counts.Permissions);
        Assert.Equal(20, counts.Links);
        Assert.Equal(6, counts.PlatformConfigs);

        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var perRole = await db.Set<RolePermission>()
            .GroupBy(link => link.Role!.RoleKey)
            .Select(group => new { Role = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count);

        Assert.Equal(3, perRole[RoleKey.Customer]);
        Assert.Equal(7, perRole[RoleKey.Owner]);
        Assert.Equal(3, perRole[RoleKey.Staff]);
        Assert.Equal(7, perRole[RoleKey.Admin]);
    }

    [Fact]
    public async Task Running_twice_does_not_create_duplicates()
    {
        await SeedAsync();
        var first = await CountAsync();

        await SeedAsync();
        var second = await CountAsync();

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Existing_config_values_are_not_overwritten()
    {
        await SeedAsync();
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var config = await db.Set<Config>().SingleAsync(c => c.Scope == ConfigScope.Platform && c.Key == "platform_fee_percent");
            config.Value = "15";
            await db.SaveChangesAsync();
        }

        await SeedAsync();

        await using var checkScope = fixture.Services.CreateAsyncScope();
        var check = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var value = await check.Set<Config>()
            .Where(c => c.Scope == ConfigScope.Platform && c.Key == "platform_fee_percent")
            .Select(c => c.Value)
            .SingleAsync();
        Assert.Equal("15", value);
    }

    [Fact]
    public async Task Startup_service_runs_the_seeder()
    {
        var service = new DatabaseInitializationService(fixture.Services, new TestEnvironment("Development"));

        await service.StartAsync(default);

        Assert.Equal(16, (await CountAsync()).Permissions);
    }

    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}