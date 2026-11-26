using be_rallyhubv2.domain.abstractions.repositories;
using be_rallyhubv2.domain.entities;
using be_rallyhubv2.domain.exceptions;
using be_rallyhubv2.persistence;
using be_rallyhubv2.tests.integration.support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace be_rallyhubv2.tests.integration.persistence;

[Collection(PostgreSqlFixture.PostgreSqlCollection.Name)]
public class UnitOfWorkTests(PostgreSqlFixture fixture)
{
    private static string NewEmail() => $"{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Duplicate_email_is_translated_to_a_unique_violation()
    {
        var email = NewEmail();
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        db.Add(new User { Email = email });
        await unitOfWork.SaveChangesAsync(default);

        db.Add(new User { Email = email });
        var ex = await Assert.ThrowsAsync<ConstraintViolationException>(() => unitOfWork.SaveChangesAsync(default));

        Assert.Equal(ConstraintViolationKind.Unique, ex.Kind);
        Assert.Equal("ux_users_email", ex.ConstraintName);
    }

    [Fact]
    public async Task Uppercase_email_is_translated_to_a_check_violation()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        db.Add(new User { Email = "UPPER-" + NewEmail() });
        var ex = await Assert.ThrowsAsync<ConstraintViolationException>(() => unitOfWork.SaveChangesAsync(default));

        Assert.Equal(ConstraintViolationKind.Check, ex.Kind);
        Assert.Equal("ck_users_email_normalized", ex.ConstraintName);
    }

    [Fact]
    public async Task Unknown_foreign_key_is_translated_to_a_foreign_key_violation()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        db.Add(new UserRole { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() });
        var ex = await Assert.ThrowsAsync<ConstraintViolationException>(() => unitOfWork.SaveChangesAsync(default));

        Assert.Equal(ConstraintViolationKind.ForeignKey, ex.Kind);
        Assert.StartsWith("fk_", ex.ConstraintName);
    }

    [Fact]
    public async Task Commit_keeps_the_changes()
    {
        var email = NewEmail();
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginTransactionAsync(default);
            db.Add(new User { Email = email });
                                                    await unitOfWork.SaveChangesAsync(default);
            await unitOfWork.CommitAsync(default);
        }

        await using var checkScope = fixture.Services.CreateAsyncScope();
        var check = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await check.Set<User>().AnyAsync(user => user.Email == email));
    }

    [Fact]
    public async Task Rollback_discards_the_changes()
    {
        var email = NewEmail();
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginTransactionAsync(default);
            db.Add(new User { Email = email });
            await unitOfWork.SaveChangesAsync(default);
            await unitOfWork.RollbackAsync(default);
        }

        await using var checkScope = fixture.Services.CreateAsyncScope();
        var check = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await check.Set<User>().AnyAsync(user => user.Email == email));
    }

    [Fact]
    public async Task Beginning_a_second_transaction_throws()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.BeginTransactionAsync(default);

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.BeginTransactionAsync(default));
    }
}