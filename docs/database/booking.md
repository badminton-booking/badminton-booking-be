# DATABASE — CỤM BOOKING

> Một Booking là một lần checkout của User và có nhiều BookingDetail. Mỗi BookingDetail đại diện cho một khoảng thời gian thực tế trên một Court, có thể bắt nguồn từ slot độc lập hoặc group slot. CampaignCode và thao tác hủy áp dụng cho toàn bộ Booking; không áp dụng hoặc hủy riêng từng BookingDetail.
>
> Tiền sân do Customer chuyển **thẳng vào tài khoản của Owner** (SePay OAuth của từng Owner). Sàn không giữ tiền; sàn chỉ ghi nhận hoa hồng Owner nợ sàn (xem cụm Giao dịch). Vì vậy một Booking chỉ thuộc **một cơ sở** của một Owner.
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
| `facility_id` | `uuid` | No | FK → `facilities.id`, ON DELETE RESTRICT; mọi BookingDetail thuộc cơ sở này, dùng khớp webhook đúng Owner |
| `booking_code` | `varchar(30)` | No | UNIQUE; chỉ gồm `A-Z0-9` vì ngân hàng có thể bỏ ký tự đặc biệt; đồng thời dùng làm nội dung chuyển khoản |
| `campaign_code_id` | `uuid` | Yes | FK → `campaign_codes.id`, ON DELETE RESTRICT; tối đa một mã cho toàn bộ Booking |
| `status` | `varchar(30)` | No | Default `PENDING_PAYMENT`, CHECK `BookingStatus` |
| `subtotal_amount` | `bigint` | No | Tổng giá gốc của các BookingDetail |
| `discount_amount` | `bigint` | No | Default `0`; tiền giảm từ CampaignCode |
| `total_amount` | `bigint` | No | Số tiền Customer phải chuyển cho Owner = `subtotal_amount - discount_amount` |
| `campaign_snapshot` | `jsonb` | Yes | Snapshot mã và cấu hình giảm giá tại thời điểm checkout; cấu trúc được định nghĩa bên dưới |
| `contact_name` | `varchar(150)` | No | Tên người liên hệ; cho phép đặt hộ |
| `contact_phone` | `varchar(20)` | No | Số điện thoại người liên hệ; cho phép đặt hộ |
| `payment_qr_url` | `varchar(2048)` | Yes | QR chuyển khoản vào tài khoản Owner; không tạo bảng Payment riêng |
| `payment_expires_at` | `timestamptz` | Yes | Đồng thời là hạn giữ slot khi chờ thanh toán |
| `note` | `varchar(1000)` | Yes | Ghi chú của Customer |
| `cancelled_at` | `timestamptz` | Yes | Thời điểm hủy toàn bộ Booking |
| `cancelled_by_user_id` | `uuid` | Yes | FK → `users.id`, ON DELETE RESTRICT; Customer, Owner hoặc Admin thực hiện hủy; NULL khi hệ thống hủy |
| `cancellation_reason` | `varchar(1000)` | Yes | Lý do hủy toàn bộ Booking |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_bookings_status` | `status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'EXPIRED')` |
| `ck_bookings_code` | `booking_code = upper(btrim(booking_code)) AND booking_code ~ '^[A-Z0-9]{6,30}$'` |
| `ck_bookings_amounts` | `subtotal_amount >= 0 AND discount_amount >= 0 AND discount_amount <= subtotal_amount AND total_amount = subtotal_amount - discount_amount` |
| `ck_bookings_contact` | `length(btrim(contact_name)) > 0 AND length(btrim(contact_phone)) > 0` |
| `ck_bookings_campaign` | Không dùng mã: `campaign_code_id IS NULL`, `campaign_snapshot IS NULL`, `discount_amount = 0`; có mã: `campaign_code_id IS NOT NULL`, `campaign_snapshot IS NOT NULL`, `jsonb_typeof(campaign_snapshot) = 'object'` và `discount_amount > 0` |
| `ck_bookings_campaign_snapshot_keys` | Khi có snapshot, JSON phải chứa đủ `code`, `discount_type`, `discount_percentage`, `discount_fixed_amount`, `funded_by` |
| `ck_bookings_campaign_funded_by` | Khi có snapshot: `campaign_snapshot->>'funded_by' IN ('PLATFORM', 'OWNER')` |
| `ck_bookings_discount_snapshot` | `PERCENTAGE`: `discount_percentage > 0 AND discount_percentage <= 100`, `discount_fixed_amount = null`; `FIXED_AMOUNT`: `discount_fixed_amount > 0`, `discount_percentage = null` |
| `ck_bookings_payment_data` | Nếu `total_amount > 0` thì `payment_qr_url` và `payment_expires_at` NOT NULL; nếu `total_amount = 0` thì cả hai NULL |
| `ck_bookings_payment_expiration` | `payment_expires_at IS NULL OR payment_expires_at > created_at` |
| `ck_bookings_note` | `note IS NULL OR length(btrim(note)) > 0` |
| `ck_bookings_cancellation` | `CANCELLED`: `cancelled_at` và `cancellation_reason` NOT NULL; trạng thái khác: `cancelled_at`, `cancelled_by_user_id`, `cancellation_reason` đều NULL |
| `ck_bookings_cancelled_time` | `cancelled_at IS NULL OR cancelled_at >= created_at` |

### Cấu trúc `campaign_snapshot`

| JSON field | JSON type | Ý nghĩa |
|---|---|---|
| `code` | `string` | Mã CampaignCode đã sử dụng |
| `discount_type` | `string` | `PERCENTAGE` hoặc `FIXED_AMOUNT` |
| `discount_percentage` | `number \| null` | Phần trăm giảm tại thời điểm checkout |
| `discount_fixed_amount` | `number \| null` | Số tiền giảm cố định tại thời điểm checkout, đơn vị VND |
| `funded_by` | `string` | `OWNER` (Campaign của Owner, Owner chịu tiền giảm) hoặc `PLATFORM` (Campaign của Admin, sàn bù bằng cách trừ vào hoa hồng Owner) |

```json
{
  "code": "SUMMER20",
  "discount_type": "PERCENTAGE",
  "discount_percentage": 20.00,
  "discount_fixed_amount": null,
  "funded_by": "OWNER"
}
```

### Indexes

| Name | Definition |
|---|---|
| `ux_bookings_booking_code` | UNIQUE (`booking_code`) |
| `ix_bookings_user_status_created_at` | (`user_id`, `status`, `created_at` DESC) WHERE `is_deleted = false` |
| `ix_bookings_facility_status_created_at` | (`facility_id`, `status`, `created_at` DESC) WHERE `is_deleted = false` |
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
| `ix_booking_details_to_complete` | (`end_at`, `id`) WHERE `status = 'CONFIRMED' AND is_deleted = false` — job chuyển `COMPLETED` |

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
| `booking_single_facility` | Mọi BookingDetail trong một Booking phải thuộc các Court của cùng `bookings.facility_id`; một Booking chỉ thanh toán vào một tài khoản SePay của một Owner |
| `booking_detail_availability` | Trước khi insert phải kiểm tra Facility/Court/template đang hoạt động, block, group và booking hiện có; exclusion constraint là lớp bảo vệ cuối chống double-book |
| `booking_detail_snapshot` | `court_id`, `start_at`, `end_at` và các amount của BookingDetail không được thay đổi sau khi Booking `CONFIRMED`; campaign snapshot được giữ tại Booking |
| `booking_amount_aggregate` | Tại checkout: `bookings.subtotal_amount = SUM(booking_details.original_amount)`, `discount_amount = SUM(booking_details.discount_amount)`, `total_amount = SUM(booking_details.final_amount)` của toàn bộ detail trong đơn |
| `booking_discount_allocation` | Nếu Booking có CampaignCode, Backend xác định các BookingDetail đủ điều kiện theo target rồi phân bổ tổng giảm giá vào `booking_details.discount_amount`; tổng phần phân bổ phải bằng `bookings.discount_amount` |
| `booking_amount_immutable` | Sau khi Booking `CONFIRMED`, không thay đổi các amount đã thanh toán; hủy được phản ánh bằng trạng thái Booking |
| `booking_status_sync` | Booking và toàn bộ BookingDetail chuyển đồng bộ cho `PENDING_PAYMENT`, `CONFIRMED`, `EXPIRED`. Khi mọi BookingDetail đã về trạng thái kết thúc (`COMPLETED` hoặc `CANCELLED`): có ít nhất một `COMPLETED` → Booking `COMPLETED`; tất cả `CANCELLED` → Booking `CANCELLED` |
| `booking_cancel_scope` | Customer chỉ được hủy **toàn bộ** Booking và chỉ **trước khi BookingDetail đầu tiên bắt đầu** (`start_at`); khi đó chuyển Booking và tất cả BookingDetail sang `CANCELLED`. Owner/Admin được hủy **từng BookingDetail chưa chơi** (`start_at` ở tương lai); BookingDetail đã `COMPLETED` giữ nguyên. Mỗi lần hủy giải phóng slot tương ứng trong cùng database transaction |
| `booking_payment_hold` | Booking `PENDING_PAYMENT` giữ slot đến `payment_expires_at`; khi hết hạn phải chuyển Booking và các detail pending sang `EXPIRED` để giải phóng exclusion constraint |
| `booking_qr_scope` | QR lưu tại Booking vì một lần thanh toán bao phủ toàn bộ BookingDetail; không sao chép QR xuống từng detail |
| `booking_payment_success` | Webhook SePay **của Owner** ghi `bank_transactions`; worker khớp `booking_code`, chuyển Booking và các detail pending sang `CONFIRMED` trong cùng database transaction (slot tự khóa qua exclusion constraint) |
| `booking_free_confirmation` | Nếu `total_amount = 0` (voucher giảm hết), Booking được xác nhận ngay, không tạo QR |
| `booking_late_payment` | Tiền tới sau khi Booking `EXPIRED`/`CANCELLED`: không xác nhận lại Booking. Vì tiền nằm ở tài khoản Owner, Owner hoàn lại cho khách; sàn chỉ ghi nhận giao dịch để đối soát |
| `booking_cancel_refund` | Tiền sân nằm ở Owner nên **Owner thực hiện hoàn**, theo `final_amount` của các BookingDetail bị hủy. Customer hủy: hoàn 100% nếu hủy trước `refund_before_hours` của cơ sở, sau mốc đó hoàn 0%. Owner/Admin hủy: hoàn 100% các detail bị hủy |
| `booking_commission_on_complete` | Khi Booking đạt trạng thái kết thúc có ít nhất một BookingDetail `COMPLETED`, ghi nhận hoa hồng Owner nợ sàn tính trên **các detail `COMPLETED`** (theo `platform_fee_percent` trong config, xem cụm Giao dịch); detail bị hủy không tính hoa hồng |
| `campaign_code_validation` | Booking dùng tối đa một CampaignCode. Mã phải hợp lệ theo Campaign, target, thời gian, visibility, giới hạn đơn hàng và số lượt sử dụng; target được kiểm tra trên các BookingDetail đủ điều kiện trong Booking |
| `campaign_code_reservation` | Khi tạo Booking pending có mã giới hạn lượt dùng, tăng `campaign_codes.used_count` nguyên tử; nếu Booking hết hạn hoặc hủy trước thanh toán thì giảm lại trong cùng transaction |
| `booking_completion` | BookingDetail chỉ chuyển `COMPLETED` sau `end_at`; Booking chuyển `COMPLETED` khi mọi BookingDetail đã về trạng thái kết thúc và có ít nhất một `COMPLETED` |
| `booking_feedback_unlock` | Chỉ BookingDetail `COMPLETED` thuộc Booking đã thanh toán thành công mới được tạo Feedback Court |
| `booking_no_physical_delete` | Không xóa vật lý Booking hoặc BookingDetail; dùng trạng thái nghiệp vụ và giữ dữ liệu để đối soát |

---

## Booking creation flow

1. Customer gửi Court (cùng một cơ sở), ngày, slot/group đã chọn và thông tin liên hệ; có thể chọn tối đa một CampaignCode cho toàn bộ Booking.
2. Backend kiểm tra `advance_booking_days`, `min_booking_minutes` (config cơ sở), dựng `start_at/end_at`, kiểm tra block/group, áp giá override, xác định các detail phù hợp target của CampaignCode và tính snapshot giá.
3. Trong một database transaction: tạo Booking (kèm `facility_id`), các BookingDetail và tăng `campaign_codes.used_count` nếu có mã.
4. Exclusion constraint từ chối transaction nếu có khoảng thời gian Court bị trùng → toàn bộ transaction rollback.
5. Nếu còn tiền phải trả, tạo QR vào tài khoản Owner và giữ slot đến `payment_expires_at`; nếu tổng tiền bằng 0 thì xác nhận ngay.
6. Webhook SePay của Owner ghi `bank_transactions`; worker khớp `booking_code`, chuyển Booking `CONFIRMED`.
7. Job hết hạn chuyển Booking chưa thanh toán sang `EXPIRED`, giải phóng slot và hoàn lại lượt CampaignCode đã giữ.
8. Job hoàn tất chuyển BookingDetail đã qua `end_at` sang `COMPLETED`; khi cả Booking `COMPLETED` thì ghi nhận hoa hồng Owner nợ sàn (cụm Giao dịch).

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `bookings` | `bookings.user_id` |
| `users` | 1 — N | `bookings` (người hủy) | `bookings.cancelled_by_user_id` |
| `facilities` | 1 — N | `bookings` | `bookings.facility_id` |
| `bookings` | 1 — N | `booking_details` | `booking_details.booking_id` |
| `courts` | 1 — N | `booking_details` | `booking_details.court_id` |
| `campaign_codes` | 1 — N | `bookings` | `bookings.campaign_code_id` |
| `bookings` | 1 — N | `bank_transactions` | `bank_transactions.booking_id` |
| `booking_details` | 1 — N | `feedbacks` | `feedbacks.booking_detail_id` |
