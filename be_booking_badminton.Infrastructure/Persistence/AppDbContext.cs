using System.Linq.Expressions;
using be_booking_badminton.Application.Abstractions.Persistence;
using be_booking_badminton.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace be_booking_badminton.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        var softDeletableTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(entityType =>
                typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType));

        foreach (var entityType in softDeletableTypes)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var isDeleted = Expression.Property(
                parameter,
                nameof(ISoftDeletable.IsDeleted));
            Expression body = Expression.Not(isDeleted);

            var existingFilter = entityType.GetQueryFilter();
            if (existingFilter is not null)
            {
                var existingBody = new ReplaceParameterVisitor(
                    existingFilter.Parameters[0],
                    parameter)
                    .Visit(existingFilter.Body)!;

                body = Expression.AndAlso(existingBody, body);
            }

            entityType.SetQueryFilter(Expression.Lambda(body, parameter));
        }
    }

    private sealed class ReplaceParameterVisitor(
        ParameterExpression source,
        ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == source ? target : base.VisitParameter(node);
    }
}
