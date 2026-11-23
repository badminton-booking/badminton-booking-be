# DATABASE — CỤM CAMPAIGN

> Campaign là chương trình giảm giá dùng chung cho Admin và Owner. Mỗi Campaign có nhiều mã public/private; mỗi Booking dùng tối đa một CampaignCode, snapshot kết quả giảm giá tại Booking và phân bổ số tiền giảm xuống BookingDetail. Lượt dùng mã **đếm trực tiếp từ `bookings`** (booking giữ `campaign_code_id`), không có bảng redemption riêng.
>
> Campaign của Admin (`owner_profile_id IS NULL`): sàn chịu tiền giảm. Campaign của Owner: Owner chịu tiền giảm.

## Enum definitions

### `CampaignStatus`

| Value |
|---|
| `DRAFT` |
| `ACTIVE` |
| `PAUSED` |
| `ENDED` |

### `CampaignDiscountType`

| Value |
|---|
| `PERCENTAGE` |
| `FIXED_AMOUNT` |

### `CampaignEligibility`

| Value | Ý nghĩa |
|---|---|
| `ALL` | Mọi Customer |
| `FIRST_BOOKING` | Chỉ Customer chưa có Booking nào được thanh toán thành công |

### `CampaignTargetType`

| Value | `target_id` tham chiếu logic |
|---|---|
| `GLOBAL` | NULL |
| `OWNER` | `owner_profiles.id` |
| `FACILITY` | `facilities.id` |
| `COURT` | `courts.id` |
| `TEMPLATE_SLOT` | `template_slots.id` |

### `CampaignCodeVisibility`

| Value |
|---|
| `PUBLIC` |
| `PRIVATE` |

### `CampaignCodeStatus`

| Value |
|---|
| `ACTIVE` |
| `DISABLED` |

---

## 1. `campaigns`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `created_by_user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `owner_profile_id` | `uuid` | Yes | FK → `owner_profiles.id`, ON DELETE RESTRICT; NULL với Campaign của Admin |
| `name` | `varchar(150)` | No |  |
| `description` | `varchar(1000)` | Yes |  |
| `discount_type` | `varchar(20)` | No | CHECK `CampaignDiscountType` |
| `discount_percentage` | `numeric(5,2)` | Yes | Chỉ dùng với `PERCENTAGE` |
| `discount_amount` | `bigint` | Yes | Chỉ dùng với `FIXED_AMOUNT`, đơn vị VND |
| `min_order_amount` | `bigint` | No | Default `0`, đơn vị VND |
| `max_order_amount` | `bigint` | Yes | Đơn vị VND; NULL là không giới hạn |
| `min_discount_amount` | `bigint` | Yes | Đơn vị VND; NULL là không đặt mức tối thiểu |
| `max_discount_amount` | `bigint` | Yes | Đơn vị VND; NULL là không đặt mức tối đa |
| `eligibility` | `varchar(20)` | No | Default `ALL`, CHECK `CampaignEligibility` |
| `starts_at` | `timestamptz` | No |  |
| `ends_at` | `timestamptz` | No |  |
| `status` | `varchar(20)` | No | Default `DRAFT`, CHECK `CampaignStatus` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_campaigns_status` | `status IN ('DRAFT', 'ACTIVE', 'PAUSED', 'ENDED')` |
| `ck_campaigns_discount_type` | `discount_type IN ('PERCENTAGE', 'FIXED_AMOUNT')` |
| `ck_campaigns_name` | `length(btrim(name)) > 0` |
| `ck_campaigns_discount_value` | `PERCENTAGE`: `discount_percentage > 0 AND discount_percentage <= 100` và `discount_amount IS NULL`; `FIXED_AMOUNT`: `discount_amount > 0` và `discount_percentage IS NULL` |
| `ck_campaigns_order_amount` | `min_order_amount >= 0 AND (max_order_amount IS NULL OR max_order_amount >= min_order_amount)` |
| `ck_campaigns_discount_amount_range` | `min_discount_amount IS NULL OR min_discount_amount >= 0`; `max_discount_amount IS NULL OR max_discount_amount > 0`; nếu cả hai có giá trị thì `max_discount_amount >= min_discount_amount` |
| `ck_campaigns_period` | `ends_at > starts_at` |
| `ck_campaigns_eligibility` | `eligibility IN ('ALL', 'FIRST_BOOKING')` |

### Indexes

| Name | Definition |
|---|---|
| `ix_campaigns_owner_status_period` | (`owner_profile_id`, `status`, `starts_at`, `ends_at`) WHERE `owner_profile_id IS NOT NULL AND is_deleted = false` |
| `ix_campaigns_admin_status_period` | (`status`, `starts_at`, `ends_at`) WHERE `owner_profile_id IS NULL AND is_deleted = false` |
| `ix_campaigns_creator_created_at` | (`created_by_user_id`, `created_at` DESC) WHERE `is_deleted = false` |

---

## 2. `campaign_codes`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `campaign_id` | `uuid` | No | FK → `campaigns.id`, ON DELETE RESTRICT |
| `code` | `varchar(50)` | No | UNIQUE |
| `visibility` | `varchar(20)` | No | CHECK `CampaignCodeVisibility` |
| `status` | `varchar(20)` | No | Default `ACTIVE`, CHECK `CampaignCodeStatus` |
| `usage_limit` | `integer` | Yes | Tổng số lượt tối đa; NULL là không giới hạn |
| `per_user_limit` | `integer` | Yes | Số lượt tối đa của mỗi Customer; NULL là không giới hạn |
| `used_count` | `integer` | No | Default `0`; cache hiển thị, nguồn thật là đếm từ `bookings`; refresh khi áp/hủy mã |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_campaign_codes_code` | `code = upper(btrim(code)) AND code ~ '^[A-Z0-9_-]{3,50}$'` |
| `ck_campaign_codes_visibility` | `visibility IN ('PUBLIC', 'PRIVATE')` |
| `ck_campaign_codes_status` | `status IN ('ACTIVE', 'DISABLED')` |
| `ck_campaign_codes_usage_limit` | `usage_limit IS NULL OR usage_limit > 0` |
| `ck_campaign_codes_per_user_limit` | `per_user_limit IS NULL OR per_user_limit > 0` |
| `ck_campaign_codes_per_user_within_usage` | `per_user_limit IS NULL OR usage_limit IS NULL OR per_user_limit <= usage_limit` |
| `ck_campaign_codes_used_count` | `used_count >= 0 AND (usage_limit IS NULL OR used_count <= usage_limit)` |

### Indexes

| Name | Definition |
|---|---|
| `ux_campaign_codes_code` | UNIQUE (`code`) |
| `ix_campaign_codes_public_listing` | (`visibility`, `status`, `campaign_id`, `created_at` DESC) WHERE `visibility = 'PUBLIC' AND status = 'ACTIVE' AND is_deleted = false` |
| `ix_campaign_codes_campaign_status` | (`campaign_id`, `status`, `created_at` DESC) WHERE `is_deleted = false` |

---

## 3. `campaign_targets`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `campaign_id` | `uuid` | No | FK → `campaigns.id`, ON DELETE RESTRICT |
| `target_type` | `varchar(30)` | No | CHECK `CampaignTargetType` |
| `target_id` | `uuid` | Yes | Polymorphic target; không tạo FK vật lý |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_campaign_targets_type` | `target_type IN ('GLOBAL', 'OWNER', 'FACILITY', 'COURT', 'TEMPLATE_SLOT')` |
| `ck_campaign_targets_target` | `GLOBAL`: `target_id IS NULL`; loại khác: `target_id IS NOT NULL` |

### Indexes

| Name | Definition |
|---|---|
| `ux_campaign_targets_global` | UNIQUE (`campaign_id`, `target_type`) WHERE `target_type = 'GLOBAL' AND is_deleted = false` |
| `ux_campaign_targets_specific` | UNIQUE (`campaign_id`, `target_type`, `target_id`) WHERE `target_id IS NOT NULL AND is_deleted = false` |
| `ix_campaign_targets_lookup` | (`target_type`, `target_id`, `campaign_id`) WHERE `target_id IS NOT NULL AND is_deleted = false` |
| `ix_campaign_targets_campaign` | (`campaign_id`, `target_type`) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `campaign_creator_role` | `owner_profile_id IS NULL`: `created_by_user_id` phải có role `ADMIN`; `owner_profile_id IS NOT NULL`: User tạo phải sở hữu OwnerProfile đó và có role `OWNER` |
| `admin_campaign_scope` | Campaign của Admin phải có đúng một target `GLOBAL`, áp dụng cho toàn bộ Facility hiện tại và tương lai |
| `owner_campaign_scope` | Campaign của Owner không được dùng `GLOBAL`; chỉ được target `OWNER`, `FACILITY`, `COURT`, `TEMPLATE_SLOT` thuộc chính Owner đó |
| `owner_dynamic_target` | Target `OWNER` áp dụng cho toàn bộ Facility hiện tại và tương lai của Owner; `target_id` phải bằng `campaigns.owner_profile_id` |
| `owner_target_exclusivity` | Nếu Campaign có target `OWNER` thì không tạo thêm target khác vì đã bao phủ toàn bộ tài nguyên của Owner |
| `campaign_target_exists` | Application phải kiểm tra `target_id` tồn tại trong bảng tương ứng; polymorphic association không có FK vật lý |
| `campaign_target_union` | Khi Campaign có nhiều target cụ thể, BookingDetail hợp lệ nếu khớp ít nhất một target |
| `public_campaign_code` | Mã `PUBLIC` được hiển thị cho Customer đủ điều kiện để chọn; vẫn phải qua bước xác nhận áp mã |
| `private_campaign_code` | Mã `PRIVATE` không xuất hiện trong danh sách công khai; Customer phải nhập chính xác `code` được Owner/Admin cung cấp |
| `campaign_code_eligibility` | Campaign và CampaignCode phải `ACTIVE`, chưa soft-delete, thỏa `starts_at <= now() AND now() < ends_at` |
| `campaign_code_usage_atomic` | Khi áp mã: khóa dòng `campaign_codes` bằng `SELECT ... FOR UPDATE`, đếm số `bookings` dùng mã ở trạng thái `PENDING_PAYMENT`/`CONFIRMED`/`COMPLETED`; chỉ cho dùng nếu số lượt `< usage_limit`, rồi cập nhật `used_count` (cache) — tất cả trong cùng database transaction với việc tạo Booking |
| `campaign_per_user_limit` | Trong cùng khóa trên, đếm số `bookings` dùng mã của đúng `user_id` ở các trạng thái còn hiệu lực; chỉ cho dùng nếu `< per_user_limit` |
| `campaign_first_booking` | `eligibility = FIRST_BOOKING`: Customer chưa có Booking nào từng `CONFIRMED` hoặc `COMPLETED` |
| `campaign_usage_release` | Lượt dùng mã đếm từ `bookings` ở trạng thái `PENDING_PAYMENT`/`CONFIRMED`/`COMPLETED`; booking `EXPIRED`/`CANCELLED` tự rớt khỏi phép đếm nên lượt được hoàn tự nhiên, không cần trạng thái redemption riêng |
| `campaign_funding` | Campaign của Admin (`funded_by = PLATFORM`): sàn chịu tiền giảm, bù lại Owner qua `commissions.platform_subsidy` (xem cụm Giao dịch). Campaign của Owner (`funded_by = OWNER`): Owner tự chịu tiền giảm. `funded_by` lưu trong `bookings.campaign_snapshot` |
| `campaign_discount_locked` | Campaign đã có `bookings` dùng mã thì không được sửa mức giảm và điều kiện; chỉ được chuyển `PAUSED` hoặc `ENDED` |
| `campaign_order_eligibility` | Giá trị đủ điều kiện phải `>= min_order_amount` và `<= max_order_amount` nếu Campaign có giới hạn tối đa |
| `campaign_discount_calculation` | Tính discount gốc theo `discount_type`, sau đó áp `min_discount_amount`, `max_discount_amount`; discount cuối cùng không vượt giá trị đủ điều kiện và tổng tiền sau giảm không âm |
| `campaign_group_slot_discount` | Với group slot, target `TEMPLATE_SLOT` chỉ giảm trên phần giá của các TemplateSlot thành phần khớp target |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `campaigns` | `campaigns.created_by_user_id` |
| `owner_profiles` | 1 — N | `campaigns` | `campaigns.owner_profile_id` |
| `campaigns` | 1 — N | `campaign_codes` | `campaign_codes.campaign_id` |
| `campaigns` | 1 — N | `campaign_targets` | `campaign_targets.campaign_id` |
| `campaign_codes` | 1 — N | `bookings` | `bookings.campaign_code_id` |

> `campaign_targets.target_type + target_id` là Polymorphic Association nên không có FK vật lý đến các bảng target.
