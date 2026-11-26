using be_rallyhubv2.domain.constants;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.persistence.seeding;

public static class ReferenceData
{
    public static readonly IReadOnlyList<(RoleKey Key, string Name)> Roles = new[]
    {
        (RoleKey.Customer, "Khách hàng"),
        (RoleKey.Owner, "Chủ sân"),
        (RoleKey.Staff, "Nhân viên sân"),
        (RoleKey.Admin, "Quản trị viên"),
    };

    public static readonly IReadOnlyList<(string Code, string Description)> Permissions = new[]
    {
        (PermissionCodes.BookingCreate, "Đặt sân"),
        (PermissionCodes.FeedbackCreate, "Đánh giá sân đã chơi"),
        (PermissionCodes.OwnerRequestCreate, "Gửi đơn đăng ký làm chủ sân"),
        (PermissionCodes.FacilityManage, "Tạo/sửa/xóa cơ sở, sân, bộ lịch, giá"),
        (PermissionCodes.FacilityStaffManage, "Thêm/gỡ nhân viên cho cơ sở"),
        (PermissionCodes.FacilityBookingView, "Xem booking và thông tin khách của cơ sở"),
        (PermissionCodes.FacilitySlotManage, "Vận hành slot: block/mở block, gộp/tách, chỉnh slot thường"),
        (PermissionCodes.FacilityCourtStatus, "Đổi trạng thái sân con: đóng/mở tạm (bảo trì, hỏng sân)"),
        (PermissionCodes.FacilityRevenueView, "Xem doanh thu cơ sở"),
        (PermissionCodes.CampaignManage, "Tạo/sửa chương trình giảm giá"),
        (PermissionCodes.OwnerRequestReview, "Duyệt/từ chối đơn đăng ký chủ sân"),
        (PermissionCodes.FacilityReview, "Duyệt/từ chối/đình chỉ cơ sở"),
        (PermissionCodes.UserManage, "Xem, khóa, mở khóa tài khoản"),
        (PermissionCodes.ReportHandle, "Tiếp nhận và trả lời report"),
        (PermissionCodes.FinanceManage, "Hoàn tiền, payout, xem giao dịch toàn hệ thống"),
        (PermissionCodes.SystemConfigManage, "Sửa cấu hình hệ thống"),
    };

    public static readonly IReadOnlyDictionary<RoleKey, string[]> RolePermissions =
        new Dictionary<RoleKey, string[]>
        {
            [RoleKey.Customer] =
            [
                PermissionCodes.BookingCreate,
                PermissionCodes.FeedbackCreate,
                PermissionCodes.OwnerRequestCreate,
            ],
            [RoleKey.Owner] =
            [
                PermissionCodes.FacilityManage,
                PermissionCodes.FacilityStaffManage,
                PermissionCodes.FacilityBookingView,
                PermissionCodes.FacilitySlotManage,
                PermissionCodes.FacilityCourtStatus,
                PermissionCodes.FacilityRevenueView,
                PermissionCodes.CampaignManage,
            ],
            [RoleKey.Staff] =
            [
                PermissionCodes.FacilityBookingView,
                PermissionCodes.FacilitySlotManage,
                PermissionCodes.FacilityCourtStatus,
            ],
            [RoleKey.Admin] =
            [
                PermissionCodes.OwnerRequestReview,
                PermissionCodes.FacilityReview,
                PermissionCodes.UserManage,
                PermissionCodes.ReportHandle,
                PermissionCodes.FinanceManage,
                PermissionCodes.CampaignManage,
                PermissionCodes.SystemConfigManage,
            ],
        };

    public static readonly IReadOnlyList<(string Key, string Value)> PlatformConfigs = new[]
    {
        ("platform_fee_percent", "10"),
        ("payment_hold_minutes", "10"),
        ("settlement_period_days", "7"),
        ("advance_booking_days", "3"),
        ("min_booking_minutes", "60"),
        ("refund_before_hours", "24"),
    };
}