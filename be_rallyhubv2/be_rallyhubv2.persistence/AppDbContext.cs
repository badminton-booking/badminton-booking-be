using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;

namespace be_rallyhubv2.persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
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
    public DbSet<CourtPriceOverride> CourtPriceOverrides => Set<CourtPriceOverride>();
    public DbSet<FacilityStaff> FacilityStaffs => Set<FacilityStaff>();
    
    
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();
    
    
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignCode> CampaignCodes => Set<CampaignCode>();
    public DbSet<CampaignTarget> CampaignTargets => Set<CampaignTarget>();
    
    
    public DbSet<OwnerPaymentAccount> OwnerPaymentAccounts => Set<OwnerPaymentAccount>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<CommissionSettlement> CommissionSettlements => Set<CommissionSettlement>();
    
    
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<FeedbackReply> FeedbackReplies => Set<FeedbackReply>();
    
    
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportMessage> ReportMessages => Set<ReportMessage>();
    
    
    public DbSet<FavoriteFacility> FavoriteFacilities => Set<FavoriteFacility>();
    
    
    public DbSet<Config> Configs => Set<Config>();
    public DbSet<Notification> Notifications => Set<Notification>();


    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        var enumTypes = typeof(BaseEntity).Assembly
            .GetTypes()
            .Where(type => type.IsEnum && type.Namespace == "be_rallyhubv2.domain.enums");

        foreach (var enumType in enumTypes)
        {
            configurationBuilder
                .Properties(enumType)
                .HaveConversion(typeof(EnumStringConverter<>).MakeGenericType(enumType))
                .HaveMaxLength(30);
        }
        
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //trong postgres có 2 loại index
        /*
         * - b-tree: so sánh =, >, < trên giá trị đơn: số, chuỗi, guid
         * - GiST: cái bài toán cồng lấn, hình học, khoảng, overrlap trên khoảng gì đó
         */
        modelBuilder.HasPostgresExtension("btree_gist");
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}