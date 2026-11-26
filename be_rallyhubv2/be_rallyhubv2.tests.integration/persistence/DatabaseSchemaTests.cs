using be_rallyhubv2.application.behaviors;
using be_rallyhubv2.persistence;
using be_rallyhubv2.tests.integration.support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace be_rallyhubv2.tests.integration.persistence;

[Collection(PostgreSqlFixture.PostgreSqlCollection.Name)]
public class DatabaseSchemaTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task Migrations_apply_cleanly_on_an_empty_database()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task Every_constraint_in_the_error_map_exists_in_the_database()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var name in ConstraintErrorMap.Errors.Keys)
        {
            var count = await db.Database
                .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM pg_class WHERE relname = {name}")
                .SingleAsync();

            Assert.True(count == 1, $"Constraint/index '{name}' in ConstraintErrorMap does not exist in the database.");
        }
    }
}