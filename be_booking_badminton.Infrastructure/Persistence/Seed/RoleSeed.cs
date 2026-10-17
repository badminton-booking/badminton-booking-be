using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace be_booking_badminton.Infrastructure.Persistence.Seed;

internal static class RoleSeed
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        // Stable IDs and timestamps keep subsequent migrations deterministic.
        var createdAt = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        modelBuilder.Entity<Role>().HasData(
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), RoleKey = RoleKey.Customer, Name = "KhĂ¡ch hĂ ng", IsDeleted = false, CreatedAt = createdAt },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), RoleKey = RoleKey.Owner, Name = "Chá»§ sĂ¢n", IsDeleted = false, CreatedAt = createdAt },
            new { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), RoleKey = RoleKey.Admin, Name = "Quáº£n trá»‹ viĂªn", IsDeleted = false, CreatedAt = createdAt });
    }
}
