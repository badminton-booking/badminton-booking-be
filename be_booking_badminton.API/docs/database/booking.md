# DATABASE — CỤM BOOKING

> Một Booking là một lần checkout của User và có nhiều BookingDetail. Mỗi BookingDetail đại diện cho một khoảng thời gian thực tế trên một Court, có thể bắt nguồn từ slot độc lập hoặc group slot. CampaignCode và thao tác hủy áp dụng cho toàn bộ Booking; không áp dụng hoặc hủy riêng từng BookingDetail.
>
> Đơn vị tiền: VND, lưu bằng `bigint`. Thời điểm thực tế lưu bằng `timestamptz` theo múi giờ hệ thống `Asia/Ho_Chi_Minh`.

## PostgreSQL prerequisite

```sql
CREATE EXTENSION IF NOT EXISTS btree_gist;
```

Extension này dùng cho exclusion constraint chống trùng lịch theo `court_id + khoảng thời gian`.

## Enum definitions

### `BookingStatus`

| Value |
|---|
| `PENDING_PAYMENT` |
| `CONFIRMED` |
| `CANCELLED` |
| `COMPLETED` |
| `EXPIRED` |

### `BookingDetailStatus`

| Value |
|---|
| `PENDING_PAYMENT` |
| `CONFIRMED` |
| `CANCELLED` |
| `COMPLETED` |
| `EXPIRED` |

---

## 1. `bookings`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `booking_code` | `varchar(30)` | No | UNIQUE; đồng thời dùng làm nội dung nhận diện thanh toán |
| `campaign_code_id` | `uuid` | Yes | FK → `campaign_codes.id`, ON DELETE RESTRICT; tối đa một mã cho toàn bộ Booking |
| `status` | `varchar(30)` | No | Default `PENDING_PAYMENT`, CHECK `BookingStatus` |
| `subtotal_amount` | `bigint` | No | Tổng giá gốc của các BookingDetail |
| `discount_amount` | `bigint` | No | Default `0` |
| `total_amount` | `bigint` | No | Số tiền Customer phải thanh toán |
| `campaign_snapshot` | `jsonb` | Yes | Snapshot mã và cấu hình giảm giá tại thời điểm checkout; cấu trúc được định nghĩa bên dưới |
| `payment_qr_url` | `varchar(2048)` | Yes | QR chờ thanh toán; không tạo bảng Payment riêng |
| `payment_expires_at` | `timestamptz` | Yes | Đồng thời là hạn giữ slot khi chờ thanh toán |
| `note` | `varchar(1000)` | Yes | Ghi chú của Customer |
| `cancelled_at` | `timestamptz` | Yes | Thời điểm hủy toàn bộ Booking |
| `cancellation_reason` | `varchar(1000)` | Yes | Lý do hủy toàn bộ Booking |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_bookings_status` | `status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'EXPIRED')` |
| `ck_bookings_code` | `booking_code = upper(btrim(booking_code)) AND booking_code ~ '^[A-Z0-9_-]{6,30}$'` |
| `ck_bookings_amounts` | `subtotal_amount >= 0 AND discount_amount >= 0 AND discount_amount <= subtotal_amount AND total_amount = subtotal_amount - discount_amount` |
| `ck_bookings_campaign` | Không dùng mã: `campaign_code_id IS NULL`, `campaign_snapshot IS NULL`, `discount_amount = 0`; có mã: `campaign_code_id IS NOT NULL`, `campaign_snapshot IS NOT NULL`, `jsonb_typeof(campaign_snapshot) = 'object'` và `discount_amount > 0` |
| `ck_bookings_campaign_snapshot_keys` | Khi có snapshot, JSON phải chứa đủ `code`, `discount_type`, `discount_percentage`, `discount_fixed_amount` |
| `ck_bookings_discount_snapshot` | `PERCENTAGE`: `discount_percentage > 0 AND discount_percentage <= 100`, `discount_fixed_amount = null`; `FIXED_AMOUNT`: `discount_fixed_amount > 0`, `discount_percentage = null` |
| `ck_bookings_payment_data` | Nếu `total_amount > 0` thì `payment_qr_url` và `payment_expires_at` NOT NULL; nếu `total_amount = 0` thì cả hai NULL |
| `ck_bookings_payment_expiration` | `payment_expires_at IS NULL OR payment_expires_at > created_at` |
| `ck_bookings_note` | `note IS NULL OR length(btrim(note)) > 0` |
| `ck_bookings_cancellation` | `CANCELLED`: `cancelled_at` và `cancellation_reason` NOT NULL; trạng thái khác: cả hai NULL |
| `ck_bookings_cancelled_time` | `cancelled_at IS NULL OR cancelled_at >= created_at` |

### Cấu trúc `campaign_snapshot`

| JSON field | JSON type | Ý nghĩa |
|---|---|---|
| `code` | `string` | Mã CampaignCode đã sử dụng |
| `discount_type` | `string` | `PERCENTAGE` hoặc `FIXED_AMOUNT` |
| `discount_percentage` | `number \| null` | Phần trăm giảm tại thời điểm checkout |
| `discount_fixed_amount` | `number \| null` | Số tiền giảm cố định tại thời điểm checkout, đơn vị VND |

```json
{
  "code": "SUMMER20",
  "discount_type": "PERCENTAGE",
  "discount_percentage": 20.00,
  "discount_fixed_amount": null
}
```

### Indexes

| Name | Definition |
|---|---|
| `ux_bookings_booking_code` | UNIQUE (`booking_code`) |
| `ix_bookings_user_status_created_at` | (`user_id`, `status`, `created_at` DESC) WHERE `is_deleted = false` |
| `ix_bookings_pending_expiration` | (`payment_expires_at`, `id`) WHERE `status = 'PENDING_PAYMENT' AND is_deleted = false` |
| `ix_bookings_status_created_at` | (`status`, `created_at` DESC) WHERE `is_deleted = false` |
| `ix_bookings_campaign_code` | (`campaign_code_id`, `created_at` DESC) WHERE `campaign_code_id IS NOT NULL AND is_deleted = false` |

---

## 2. `booking_details`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `booking_id` | `uuid` | No | FK → `bookings.id`, ON DELETE RESTRICT |
| `court_id` | `uuid` | No | FK → `courts.id`, ON DELETE RESTRICT |
| `status` | `varchar(30)` | No | Default `PENDING_PAYMENT`, CHECK `BookingDetailStatus` |
| `start_at` | `timestamptz` | No | Snapshot thời điểm bắt đầu thực tế |
| `end_at` | `timestamptz` | No | Snapshot thời điểm kết thúc thực tế |
| `original_amount` | `bigint` | No | Snapshot tổng giá slot/group trước giảm |
| `discount_amount` | `bigint` | No | Default `0`, số tiền giảm thực tế |
| `final_amount` | `bigint` | No | Số tiền sau giảm |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_booking_details_status` | `status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'EXPIRED')` |
| `ck_booking_details_time` | `end_at > start_at` |
| `ck_booking_details_amounts` | `original_amount >= 0 AND discount_amount >= 0 AND discount_amount <= original_amount AND final_amount = original_amount - discount_amount` |

### Indexes

| Name | Definition |
|---|---|
| `ix_booking_details_booking_status_start` | (`booking_id`, `status`, `start_at`, `id`) WHERE `is_deleted = false` |
| `ix_booking_details_court_time_active` | (`court_id`, `start_at`, `end_at`) WHERE `status IN ('PENDING_PAYMENT', 'CONFIRMED') AND is_deleted = false` |
| `ix_booking_details_court_history` | (`court_id`, `start_at` DESC, `id`) WHERE `is_deleted = false` |

### Exclusion constraint

| Name | Definition |
|---|---|
| `ex_booking_details_court_time_overlap` | `EXCLUDE USING gist (court_id WITH =, tstzrange(start_at, end_at, '[)') WITH &&) WHERE (status IN ('PENDING_PAYMENT', 'CONFIRMED') AND is_deleted = false)` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `booking_minimum_detail` | Mỗi Booking phải có ít nhất một BookingDetail |
| `booking_owner_user` | `bookings.user_id` phải là User đang hoạt động và là Customer tạo Booking |
| `booking_detail_availability` | Trước khi insert phải kiểm tra Facility/Court/template đang hoạt động, block, group và booking hiện có; exclusion constraint là lớp bảo vệ cuối chống double-book |
| `booking_detail_snapshot` | `court_id`, `start_at`, `end_at` và các amount của BookingDetail không được thay đổi sau khi Booking `CONFIRMED`; campaign snapshot được giữ tại Booking |
| `booking_amount_aggregate` | Tại checkout: `bookings.subtotal_amount = SUM(booking_details.original_amount)`, `discount_amount = SUM(booking_details.discount_amount)`, `total_amount = SUM(booking_details.final_amount)` của toàn bộ detail trong đơn |
| `booking_discount_allocation` | Nếu Booking có CampaignCode, Backend xác định các BookingDetail đủ điều kiện theo target rồi phân bổ tổng giảm giá vào `booking_details.discount_amount`; tổng phần phân bổ phải bằng `bookings.discount_amount` |
| `booking_amount_immutable` | Sau khi Booking `CONFIRMED`, không thay đổi các amount đã thanh toán; hủy/hoàn tiền được phản ánh bằng trạng thái Booking và FinancialTransaction `REFUND` |
| `booking_status_sync` | Booking và toàn bộ BookingDetail phải chuyển trạng thái đồng bộ cho `PENDING_PAYMENT`, `CONFIRMED`, `CANCELLED`, `EXPIRED`; Booking chỉ `COMPLETED` khi tất cả BookingDetail đều `COMPLETED` |
| `booking_cancel_scope` | Chỉ cho phép hủy toàn bộ Booking, không cung cấp nghiệp vụ hủy riêng BookingDetail; khi hủy phải chuyển Booking và tất cả BookingDetail sang `CANCELLED`, giải phóng toàn bộ slot trong cùng database transaction |
| `booking_payment_hold` | Booking `PENDING_PAYMENT` giữ slot đến `payment_expires_at`; khi hết hạn phải chuyển Booking và các detail pending sang `EXPIRED` để giải phóng exclusion constraint |
| `booking_qr_scope` | QR lưu tại Booking vì một lần thanh toán bao phủ toàn bộ BookingDetail; không sao chép QR xuống từng detail |
| `booking_payment_success` | Khi nhận thanh toán thành công: tạo FinancialTransaction `PAYMENT`, chuyển Booking và các detail pending sang `CONFIRMED` trong cùng database transaction |
| `booking_free_confirmation` | Nếu `total_amount = 0`, Booking được xác nhận ngay, không tạo QR và không tạo FinancialTransaction giá trị 0 |
| `campaign_code_validation` | Booking dùng tối đa một CampaignCode. Mã phải hợp lệ theo Campaign, target, thời gian, visibility, giới hạn đơn hàng và số lượt sử dụng; target được kiểm tra trên các BookingDetail đủ điều kiện trong Booking |
| `campaign_code_reservation` | Khi tạo Booking pending có mã giới hạn lượt dùng, tăng `campaign_codes.used_count` nguyên tử; nếu Booking hết hạn hoặc hủy trước thanh toán thì giảm lại trong cùng transaction |
| `booking_completion` | BookingDetail chỉ chuyển `COMPLETED` sau `end_at`; Booking chuyển `COMPLETED` khi toàn bộ BookingDetail đã hoàn thành |
| `booking_feedback_unlock` | Chỉ BookingDetail `COMPLETED` thuộc Booking đã thanh toán thành công mới được tạo Feedback Court |
| `booking_no_physical_delete` | Không xóa vật lý Booking hoặc BookingDetail; dùng trạng thái nghiệp vụ và giữ dữ liệu để đối soát |

---

## Booking creation flow

1. Customer gửi Court, ngày và slot/group đã chọn; có thể chọn tối đa một CampaignCode cho toàn bộ Booking.
2. Backend dựng `start_at/end_at`, kiểm tra block/group, xác định các detail phù hợp target của CampaignCode và tính snapshot giá.
3. Trong một database transaction: tạo Booking, các BookingDetail và giữ lượt CampaignCode nếu có.
4. Exclusion constraint từ chối transaction nếu có khoảng thời gian Court bị trùng.
5. Nếu còn tiền phải trả, tạo QR và giữ slot đến `payment_expires_at`; nếu tổng tiền bằng 0 thì xác nhận ngay.
6. Webhook thanh toán thành công tạo FinancialTransaction và xác nhận Booking.
7. Job hết hạn chuyển Booking chưa thanh toán sang `EXPIRED`, giải phóng slot và hoàn lại lượt CampaignCode đã giữ.

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `bookings` | `bookings.user_id` |
| `bookings` | 1 — N | `booking_details` | `booking_details.booking_id` |
| `courts` | 1 — N | `booking_details` | `booking_details.court_id` |
| `campaign_codes` | 1 — N | `bookings` | `bookings.campaign_code_id` |
| `bookings` | 1 — N | `financial_transactions` | `financial_transactions.booking_id` |
| `booking_details` | 1 — N | `feedbacks` | `feedbacks.booking_detail_id` |
