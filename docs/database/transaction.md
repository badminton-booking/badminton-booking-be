# DATABASE — CỤM GIAO DỊCH / HOA HỒNG

> Đơn vị tiền: VND, lưu bằng `bigint`. Thời điểm lưu bằng `timestamptz`, múi giờ `Asia/Ho_Chi_Minh`.
>
> **Mô hình tiền:** Customer chuyển tiền sân **thẳng vào tài khoản của Owner** (SePay OAuth của từng Owner). Sàn **không giữ tiền**. Sàn chỉ ghi nhận **hoa hồng Owner nợ sàn** mỗi khi Booking hoàn tất, gom theo kỳ thành bảng đối soát; Owner chuyển hoa hồng về tài khoản Admin. Không có escrow, payout, credit hay double-entry ledger.
>
> Có 2 dòng tiền, đều đi qua SePay và rơi vào `bank_transactions`:
> 1. **Khách → Owner** (tiền sân): khớp `booking_code` → xác nhận Booking.
> 2. **Owner → Admin** (hoa hồng): khớp `settlement_code` → tất toán đối soát.

## Enum definitions

### `BankTransactionDirection`

| Value |
|---|
| `IN` |
| `OUT` |

### `BankTransactionMatchState`

| Value | Ý nghĩa |
|---|---|
| `PENDING` | Chưa xử lý |
| `MATCHED_BOOKING` | Khớp `booking_code` — tiền khách trả Owner |
| `MATCHED_SETTLEMENT` | Khớp `settlement_code` — hoa hồng Owner trả Admin |
| `UNMATCHED` | Không khớp được (sai nội dung) — Admin/Owner xử lý thủ công |
| `IGNORED` | Giao dịch không liên quan, bỏ qua |

### `CommissionStatus`

| Value | Ý nghĩa |
|---|---|
| `PENDING` | Đã ghi nhận, chưa đưa vào kỳ đối soát |
| `SETTLED` | Đã thuộc một đối soát Owner đã thanh toán |

### `SettlementStatus`

| Value | Ý nghĩa |
|---|---|
| `PENDING` | Chờ Owner chuyển hoa hồng |
| `PAID` | Owner đã chuyển, khớp được giao dịch ngân hàng |
| `OVERDUE` | Quá hạn chưa thanh toán |

---

## 1. `owner_payment_accounts`

> Tài khoản nhận tiền của Owner, kết nối qua SePay OAuth. Dùng để tạo QR cho khách và để biết webhook tiền vào thuộc Owner nào.

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `owner_profile_id` | `uuid` | No | FK → `owner_profiles.id`, ON DELETE RESTRICT |
| `provider` | `varchar(50)` | No | Default `SEPAY` |
| `account_number` | `varchar(50)` | No | Số tài khoản nhận tiền |
| `account_holder` | `varchar(150)` | No | Tên chủ tài khoản |
| `bank_code` | `varchar(50)` | Yes | Mã ngân hàng |
| `account_ref` | `varchar(255)` | No | Định danh tài khoản bên cổng thanh toán (OAuth) |
| `is_active` | `boolean` | No | Default `true`; tài khoản đang dùng để nhận tiền |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_owner_payment_accounts_number` | `length(btrim(account_number)) > 0` |
| `ck_owner_payment_accounts_holder` | `length(btrim(account_holder)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_owner_payment_accounts_active` | UNIQUE (`owner_profile_id`) WHERE `is_active = true AND is_deleted = false` — mỗi Owner một tài khoản nhận đang hoạt động |
| `ux_owner_payment_accounts_number` | UNIQUE (`provider`, `account_number`) WHERE `is_deleted = false` |
| `ix_owner_payment_accounts_owner` | (`owner_profile_id`, `is_active`) WHERE `is_deleted = false` |

---

## 2. `bank_transactions`

> Ghi lại mọi khoản tiền SePay báo về (vào tài khoản Owner hoặc Admin). Webhook chỉ `INSERT` rồi để worker khớp sau.

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `provider` | `varchar(50)` | No | Ví dụ `SEPAY` |
| `provider_reference` | `varchar(255)` | No | Mã giao dịch duy nhất do SePay trả về |
| `direction` | `varchar(5)` | No | CHECK `BankTransactionDirection` |
| `amount` | `bigint` | No | CHECK `amount > 0` |
| `account_number` | `varchar(50)` | No | Tài khoản nhận (Owner hoặc Admin) |
| `transfer_content` | `varchar(500)` | Yes | Nội dung chuyển khoản gốc (chứa `booking_code` hoặc `settlement_code`) |
| `occurred_at` | `timestamptz` | No | Giờ ngân hàng ghi nhận; dùng xét thanh toán đúng hạn |
| `match_state` | `varchar(20)` | No | Default `PENDING`, CHECK `BankTransactionMatchState` |
| `booking_id` | `uuid` | Yes | FK → `bookings.id`, ON DELETE RESTRICT; gán khi khớp `booking_code` |
| `commission_settlement_id` | `uuid` | Yes | FK → `commission_settlements.id`, ON DELETE RESTRICT; gán khi khớp `settlement_code` |
| `processed_at` | `timestamptz` | Yes | NULL = chưa xử lý xong |
| `last_error` | `varchar(500)` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()`; thời điểm webhook tới |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_bank_transactions_direction` | `direction IN ('IN', 'OUT')` |
| `ck_bank_transactions_amount` | `amount > 0` |
| `ck_bank_transactions_match_state` | `match_state IN ('PENDING', 'MATCHED_BOOKING', 'MATCHED_SETTLEMENT', 'UNMATCHED', 'IGNORED')` |
| `ck_bank_transactions_match_target` | `MATCHED_BOOKING`: `booking_id NOT NULL`, `commission_settlement_id NULL`. `MATCHED_SETTLEMENT`: `commission_settlement_id NOT NULL`, `booking_id NULL`. Trạng thái khác: cả hai NULL |

### Indexes

| Name | Definition |
|---|---|
| `ux_bank_transactions_provider_reference` | UNIQUE (`provider`, `provider_reference`) — webhook gửi lại cũng chỉ ghi một lần |
| `ix_bank_transactions_unprocessed` | (`created_at`, `id`) WHERE `processed_at IS NULL AND is_deleted = false` — hàng đợi worker khớp |
| `ix_bank_transactions_booking` | (`booking_id`, `occurred_at`) WHERE `booking_id IS NOT NULL` |
| `ix_bank_transactions_settlement` | (`commission_settlement_id`) WHERE `commission_settlement_id IS NOT NULL` |
| `ix_bank_transactions_unmatched` | (`created_at` DESC) WHERE `match_state = 'UNMATCHED'` — Admin/Owner xử lý thủ công |

---

## 3. `commissions`

> Hoa hồng Owner nợ sàn, ghi nhận khi một Booking đạt trạng thái kết thúc có ít nhất một `booking_details` `COMPLETED`. Một Booking tối đa một dòng. Chỉ tính trên các detail đã `COMPLETED`; slot bị hủy không tính hoa hồng.

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `booking_id` | `uuid` | No | FK → `bookings.id`, ON DELETE RESTRICT; UNIQUE |
| `owner_profile_id` | `uuid` | No | FK → `owner_profiles.id`, ON DELETE RESTRICT |
| `facility_id` | `uuid` | No | FK → `facilities.id`, ON DELETE RESTRICT |
| `gross_amount` | `bigint` | No | Tổng `original_amount` của các `booking_details` `COMPLETED`; CHECK `>= 0` |
| `commission_rate` | `numeric(5,2)` | No | % phí sàn chốt tại thời điểm tính; CHECK `0..100` |
| `commission_amount` | `bigint` | No | Hoa hồng = `round(gross_amount * commission_rate / 100)`; CHECK `>= 0` |
| `platform_subsidy` | `bigint` | No | Default `0`; phần voucher `funded_by = PLATFORM` sàn bù cho Owner; CHECK `>= 0` |
| `net_payable` | `bigint` | No | Owner thực nợ sàn = `commission_amount - platform_subsidy` |
| `status` | `varchar(20)` | No | Default `PENDING`, CHECK `CommissionStatus` |
| `commission_settlement_id` | `uuid` | Yes | FK → `commission_settlements.id`, ON DELETE RESTRICT; kỳ đối soát chứa dòng này |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_commissions_status` | `status IN ('PENDING', 'SETTLED')` |
| `ck_commissions_rate` | `commission_rate >= 0 AND commission_rate <= 100` |
| `ck_commissions_amounts` | `gross_amount >= 0 AND commission_amount >= 0 AND platform_subsidy >= 0 AND net_payable = commission_amount - platform_subsidy` |
| `ck_commissions_settled_link` | `status = 'SETTLED'` thì `commission_settlement_id IS NOT NULL`; `PENDING` thì `commission_settlement_id IS NULL` |

### Indexes

| Name | Definition |
|---|---|
| `ux_commissions_booking` | UNIQUE (`booking_id`) WHERE `is_deleted = false` — mỗi Booking một hoa hồng |
| `ix_commissions_owner_status` | (`owner_profile_id`, `status`, `created_at`) WHERE `is_deleted = false` — gom hoa hồng chờ đối soát |
| `ix_commissions_settlement` | (`commission_settlement_id`) WHERE `commission_settlement_id IS NOT NULL AND is_deleted = false` |

---

## 4. `commission_settlements`

> Bảng đối soát theo kỳ: gom hoa hồng `PENDING` của một Owner thành một khoản để Owner chuyển về Admin.

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `owner_profile_id` | `uuid` | No | FK → `owner_profiles.id`, ON DELETE RESTRICT |
| `settlement_code` | `varchar(30)` | No | UNIQUE; nội dung Owner ghi khi chuyển khoản hoa hồng; chỉ `A-Z0-9` |
| `period_start` | `date` | No | Đầu kỳ đối soát |
| `period_end` | `date` | No | Cuối kỳ đối soát |
| `total_amount` | `bigint` | No | Tổng `net_payable` các hoa hồng trong kỳ; CHECK `>= 0` |
| `status` | `varchar(20)` | No | Default `PENDING`, CHECK `SettlementStatus` |
| `due_date` | `date` | Yes | Hạn chuyển hoa hồng |
| `paid_at` | `timestamptz` | Yes | Thời điểm khớp được tiền Owner chuyển |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_commission_settlements_code` | `settlement_code = upper(btrim(settlement_code)) AND settlement_code ~ '^[A-Z0-9]{6,30}$'` |
| `ck_commission_settlements_status` | `status IN ('PENDING', 'PAID', 'OVERDUE')` |
| `ck_commission_settlements_period` | `period_end >= period_start` |
| `ck_commission_settlements_total` | `total_amount >= 0` |
| `ck_commission_settlements_paid` | `status = 'PAID'` thì `paid_at IS NOT NULL`; trạng thái khác: `paid_at IS NULL` |

### Indexes

| Name | Definition |
|---|---|
| `ux_commission_settlements_code` | UNIQUE (`settlement_code`) |
| `ix_commission_settlements_owner_status` | (`owner_profile_id`, `status`, `period_end` DESC) WHERE `is_deleted = false` |
| `ix_commission_settlements_overdue` | (`due_date`) WHERE `status = 'PENDING' AND is_deleted = false` — job gắn `OVERDUE` |

---

## Ví dụ tính hoa hồng

Booking các slot đã `COMPLETED` tổng giá gốc `200.000đ`, phí sàn `5%`.

| Trường hợp | gross | rate | commission | subsidy | net_payable |
|---|---|---|---|---|---|
| Không voucher | 200.000 | 5% | 10.000 | 0 | **10.000** |
| Voucher của Owner (−40.000) | 200.000 | 5% | 10.000 | 0 | **10.000** |
| Voucher của sàn (−40.000, `funded_by = PLATFORM`) | 200.000 | 5% | 10.000 | 40.000 | **−30.000** (sàn bù lại Owner, trừ vào kỳ sau) |

> `gross_amount` chỉ gồm các detail `COMPLETED` — slot bị Owner/Admin hủy không nằm trong đây. Voucher Owner: Owner tự chịu, hoa hồng vẫn tính trên giá gốc phần đã chơi. Voucher sàn: sàn bù phần giảm cho Owner bằng cách trừ vào hoa hồng. `net_payable` âm nghĩa là kỳ đó sàn nợ Owner — bù trừ vào kỳ kế tiếp.

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `webhook_record_only` | Webhook SePay chỉ xác thực rồi `INSERT ... ON CONFLICT (provider, provider_reference) DO NOTHING` vào `bank_transactions`; không xử lý nghiệp vụ trong request webhook |
| `bank_transaction_matching` | Worker đọc `bank_transactions` chưa `processed_at`: chiều `IN` vào tài khoản Owner khớp `booking_code`; vào tài khoản Admin khớp `settlement_code`; không khớp → `UNMATCHED` |
| `customer_payment_confirms_booking` | Tiền khớp `booking_code` đủ `bookings.total_amount` và `occurred_at < payment_expires_at`: chuyển Booking `CONFIRMED`, `bank_transactions` `MATCHED_BOOKING`, đánh dấu `processed_at` trong cùng database transaction |
| `late_or_wrong_payment` | Tiền tới sau hạn, sai nội dung, hoặc Booking đã `EXPIRED`/`CANCELLED`: không xác nhận Booking. Vì tiền nằm ở tài khoản Owner, **Owner hoàn lại cho khách**; sàn chỉ giữ bản ghi `UNMATCHED` để đối soát |
| `commission_on_complete` | Khi Booking đạt trạng thái kết thúc có ít nhất một detail `COMPLETED`, tạo một `commissions` (`PENDING`); `gross_amount` = tổng `original_amount` các detail `COMPLETED`, `commission_rate` lấy từ config phí sàn tại thời điểm đó; Booking không có detail `COMPLETED` nào thì không tạo hoa hồng |
| `commission_once` | Mỗi Booking tối đa một dòng `commissions` |
| `settlement_grouping` | Job định kỳ gom các `commissions` `PENDING` của một Owner trong kỳ thành một `commission_settlements`; gán `commission_settlement_id` và chuyển các dòng sang `SETTLED` khi kỳ được tạo; `total_amount = SUM(net_payable)` |
| `settlement_payment_match` | Tiền Owner chuyển vào tài khoản Admin khớp `settlement_code`: chuyển `commission_settlements` `PAID`, set `paid_at`, `bank_transactions` `MATCHED_SETTLEMENT` trong cùng database transaction |
| `settlement_overdue_suspend` | `commission_settlements` quá `due_date` chưa `PAID` → `OVERDUE`; Owner nợ quá hạn có thể bị chuyển các cơ sở sang `INACTIVE` cho đến khi thanh toán |
| `immutable_commission_history` | Không sửa/xóa vật lý `commissions` đã `SETTLED` và `bank_transactions`; điều chỉnh sai sót bằng dòng hoa hồng bù ở kỳ sau |
| `daily_reconciliation` | Job hằng ngày gửi Owner bảng đối soát (hoa hồng phát sinh, kỳ chờ thanh toán) và đối chiếu `commission_settlements` `PAID` với giao dịch ngân hàng thực nhận của Admin |

---

## Luồng tiền tóm tắt

1. Khách đặt sân → quét QR chuyển khoản **vào tài khoản Owner** (`owner_payment_accounts`), nội dung là `booking_code`.
2. SePay của Owner báo webhook → `bank_transactions` (`IN`). Worker khớp `booking_code` → Booking `CONFIRMED`, slot khóa.
3. Tới giờ, qua `end_at` → Booking `COMPLETED` → tạo `commissions` (`PENDING`).
4. Định kỳ: job gom hoa hồng của Owner → `commission_settlements`, gửi bảng đối soát, các `commissions` thành `SETTLED`.
5. Owner chuyển hoa hồng **vào tài khoản Admin**, nội dung là `settlement_code`.
6. SePay của Admin báo webhook → worker khớp `settlement_code` → `commission_settlements` `PAID`.
7. Quá hạn chưa trả → `OVERDUE` → có thể `INACTIVE` cơ sở của Owner.

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `owner_profiles` | 1 — N | `owner_payment_accounts` | `owner_payment_accounts.owner_profile_id` |
| `bookings` | 1 — N | `bank_transactions` | `bank_transactions.booking_id` |
| `commission_settlements` | 1 — N | `bank_transactions` | `bank_transactions.commission_settlement_id` |
| `bookings` | 1 — 0..1 | `commissions` | `commissions.booking_id` |
| `owner_profiles` | 1 — N | `commissions` | `commissions.owner_profile_id` |
| `facilities` | 1 — N | `commissions` | `commissions.facility_id` |
| `commission_settlements` | 1 — N | `commissions` | `commissions.commission_settlement_id` |
| `owner_profiles` | 1 — N | `commission_settlements` | `commission_settlements.owner_profile_id` |
