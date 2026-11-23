# DATABASE — CỤM HỆ THỐNG

> Các bảng hạ tầng dùng chung: cấu hình theo phạm vi (`configs`) và thông báo trong ứng dụng (`notifications`).

## Enum definitions

### `ConfigScope`

| Value | Ý nghĩa |
|---|---|
| `PLATFORM` | Cấu hình toàn sàn, do Admin đặt |
| `FACILITY` | Cấu hình riêng một cơ sở, do Owner đặt |

### `NotificationType`

| Value | Ý nghĩa |
|---|---|
| `BOOKING_CONFIRMED` | Booking đã thanh toán thành công (gửi Customer và Owner) |
| `BOOKING_CANCELLED` | Booking hoặc một phần slot bị hủy |
| `BOOKING_EXPIRED` | Booking hết hạn thanh toán |
| `BOOKING_COMPLETED` | Booking hoàn tất — mời Customer đánh giá |
| `SETTLEMENT_DUE` | Có bảng đối soát hoa hồng chờ Owner thanh toán |
| `SETTLEMENT_PAID` | Đã nhận được hoa hồng Owner chuyển |
| `SETTLEMENT_OVERDUE` | Hoa hồng quá hạn, cơ sở có thể bị tạm ngừng |
| `OWNER_REQUEST_REVIEWED` | Đơn đăng ký Owner được duyệt / từ chối |
| `FACILITY_REVIEWED` | Cơ sở được duyệt / từ chối |
| `REPORT_MESSAGE` | Có tin nhắn mới trong Report |
| `FEEDBACK_REPLY` | Có người trả lời Feedback |

---

## 1. `configs`

> Mỗi dòng là một cấu hình. `PLATFORM`: Admin đặt cho toàn sàn (`facility_id` NULL). `FACILITY`: Owner đặt riêng cho cơ sở. Giá trị lưu `jsonb` để linh hoạt kiểu dữ liệu.

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `scope` | `varchar(20)` | No | CHECK `ConfigScope` |
| `facility_id` | `uuid` | Yes | FK → `facilities.id`, ON DELETE RESTRICT; NOT NULL với `FACILITY`, NULL với `PLATFORM` |
| `key` | `varchar(100)` | No | Khóa cấu hình, ví dụ `advance_booking_days` |
| `value` | `jsonb` | No | Giá trị cấu hình |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_configs_scope` | `scope IN ('PLATFORM', 'FACILITY')` |
| `ck_configs_facility_scope` | `PLATFORM`: `facility_id IS NULL`; `FACILITY`: `facility_id IS NOT NULL` |
| `ck_configs_key` | `length(btrim(key)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_configs_platform_key` | UNIQUE (`key`) WHERE `scope = 'PLATFORM' AND is_deleted = false` |
| `ux_configs_facility_key` | UNIQUE (`facility_id`, `key`) WHERE `scope = 'FACILITY' AND is_deleted = false` |

### Các khóa cấu hình đang dùng

| Scope | `key` | Kiểu value | Ý nghĩa |
|---|---|---|---|
| `PLATFORM` | `platform_fee_percent` | number | % hoa hồng sàn thu trên mỗi Booking hoàn tất |
| `PLATFORM` | `payment_hold_minutes` | number | Thời gian giữ slot chờ thanh toán |
| `PLATFORM` | `settlement_period_days` | number | Chu kỳ gom đối soát hoa hồng |
| `FACILITY` | `advance_booking_days` | number | Cho đặt trước tối đa bao nhiêu ngày |
| `FACILITY` | `min_booking_minutes` | number | Thời lượng đặt tối thiểu mỗi Court |
| `FACILITY` | `refund_before_hours` | number | Hủy trước bao nhiêu giờ thì hoàn 100% (sau đó 0%) |

> Khi tạo cơ sở, sinh sẵn các khóa `FACILITY` với giá trị mặc định kế thừa từ `PLATFORM`; Owner chỉnh sau. Đọc cấu hình: lấy `FACILITY` nếu có, không thì fallback giá trị `PLATFORM`.

---

## 2. `notifications`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT; người nhận |
| `type` | `varchar(50)` | No | CHECK `NotificationType` |
| `title` | `varchar(200)` | No |  |
| `content` | `varchar(1000)` | No |  |
| `data` | `jsonb` | Yes | Dữ liệu điều hướng cho FE, ví dụ `{ "bookingId": "..." }` |
| `read_at` | `timestamptz` | Yes | NULL = chưa đọc |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_notifications_type` | `type IN ('BOOKING_CONFIRMED', 'BOOKING_CANCELLED', 'BOOKING_EXPIRED', 'BOOKING_COMPLETED', 'SETTLEMENT_DUE', 'SETTLEMENT_PAID', 'SETTLEMENT_OVERDUE', 'OWNER_REQUEST_REVIEWED', 'FACILITY_REVIEWED', 'REPORT_MESSAGE', 'FEEDBACK_REPLY')` |
| `ck_notifications_title` | `length(btrim(title)) > 0` |
| `ck_notifications_content` | `length(btrim(content)) > 0` |
| `ck_notifications_data` | `data IS NULL OR jsonb_typeof(data) = 'object'` |
| `ck_notifications_read_time` | `read_at IS NULL OR read_at >= created_at` |

### Indexes

| Name | Definition |
|---|---|
| `ix_notifications_user_created_at` | (`user_id`, `created_at` DESC, `id`) WHERE `is_deleted = false` |
| `ix_notifications_user_unread` | (`user_id`, `created_at` DESC) WHERE `read_at IS NULL AND is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `config_read_fallback` | Đọc cấu hình cơ sở: lấy dòng `FACILITY` của cơ sở nếu có, ngược lại dùng dòng `PLATFORM` cùng `key` |
| `config_change_not_retroactive` | Đổi cấu hình chỉ áp dụng cho Booking/đối soát mới; dữ liệu đã snapshot (giá, hoa hồng) không đổi theo |
| `notification_owner_only` | User chỉ được đọc và đánh dấu đã đọc `notifications` của chính mình |
| `notification_after_commit` | Thông báo được tạo sau khi nghiệp vụ gốc commit thành công; tạo trùng cùng một sự kiện phải chịu được (không nhân đôi) |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `facilities` | 1 — N | `configs` | `configs.facility_id` |
| `users` | 1 — N | `notifications` | `notifications.user_id` |

> Dòng `configs` scope `PLATFORM` không gắn `facility_id`.
