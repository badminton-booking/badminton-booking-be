using System.Linq.Expressions;
using be_booking_badminton.Application.Abstractions.Persistence;
using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace be_booking_badminton.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OwnerRequest> OwnerRequests => Set<OwnerRequest>();
    public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>();
    public DbSet<UserBankAccount> UserBankAccounts => Set<UserBankAccount>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<FacilityImage> FacilityImages => Set<FacilityImage>();
    public DbSet<ScheduleTemplate> ScheduleTemplates => Set<ScheduleTemplate>();
    public DbSet<TemplateSlot> TemplateSlots => Set<TemplateSlot>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<CourtSlotBlock> CourtSlotBlocks => Set<CourtSlotBlock>();
    public DbSet<CourtSlotGroup> CourtSlotGroups => Set<CourtSlotGroup>();
    public DbSet<CourtSlotGroupItem> CourtSlotGroupItems => Set<CourtSlotGroupItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignCode> CampaignCodes => Set<CampaignCode>();
    public DbSet<CampaignTarget> CampaignTargets => Set<CampaignTarget>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<LedgerAccount> LedgerAccounts => Set<LedgerAccount>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<FeedbackReply> FeedbackReplies => Set<FeedbackReply>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportMessage> ReportMessages => Set<ReportMessage>();
    public DbSet<FavoriteCourt> FavoriteCourts => Set<FavoriteCourt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("btree_gist");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        RoleSeed.Configure(modelBuilder);
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
