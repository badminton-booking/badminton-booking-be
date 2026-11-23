# DATABASE — CỤM USER / PHÂN QUYỀN

> Phân quyền theo RBAC: `users` → `user_roles` → `roles` → `role_permissions` → `permissions`. Backend kiểm tra theo permission (ví dụ `facility.manage`), không kiểm tra trực tiếp theo tên role.
>
> Permission chỉ trả lời "được làm hành động gì". Quyền trên từng tài nguyên vẫn phải kiểm tra riêng: Owner chỉ thao tác Facility của mình, Staff chỉ thao tác Facility được giao (`facility_staffs` ở cụm Setup sân).

## Enum definitions

### `UserStatus`

| Value | Ý nghĩa |
|---|---|
| `NOT_VERIFIED` | Đã đăng ký, chưa xác thực email |
| `VERIFIED` | Đã xác thực, sử dụng bình thường |
| `BANNED` | Bị Admin khóa tài khoản |

### `RoleKey`

| Value | Ý nghĩa |
|---|---|
| `CUSTOMER` | Khách đặt sân; mọi User mới đều có |
| `OWNER` | Chủ sân, sau khi OwnerRequest được duyệt |
| `STAFF` | Nhân viên của Owner, làm việc tại Facility được giao |
| `ADMIN` | Quản trị nền tảng |

### `ExternalLoginProvider`

| Value |
|---|
| `GOOGLE` |

### `OwnerRequestStatus`

| Value |
|---|
| `PENDING` |
| `APPROVED` |
| `REJECTED` |
| `CANCELLED` |

---

## 1. `users`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `email` | `varchar(320)` | No | UNIQUE |
| `password_hash` | `varchar(512)` | Yes | NULL nếu User chỉ đăng nhập bằng Google |
| `status` | `varchar(20)` | No | Default `NOT_VERIFIED`, CHECK `UserStatus` |
| `email_verified_at` | `timestamptz` | Yes |  |
| `ban_reason` | `varchar(1000)` | Yes | Lý do khóa, hiển thị cho User khi đăng nhập |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_users_status` | `status IN ('NOT_VERIFIED', 'VERIFIED', 'BANNED')` |
| `ck_users_verified` | `status <> 'VERIFIED' OR email_verified_at IS NOT NULL` |
| `ck_users_ban_reason` | `status = 'BANNED'` thì `ban_reason IS NOT NULL`; trạng thái khác thì `ban_reason IS NULL` |
| `ck_users_email_normalized` | `email = lower(btrim(email))` |
| `ck_users_email_format` | `position('@' IN email) > 1` |

### Indexes

| Name | Definition |
|---|---|
| `ux_users_email` | UNIQUE (`email`) |
| `ix_users_status_active` | (`status`) WHERE `is_deleted = false` |

---

## 2. `user_profiles`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, UNIQUE, ON DELETE RESTRICT |
| `username` | `varchar(50)` | Yes |  |
| `full_name` | `varchar(150)` | No |  |
| `phone_number` | `varchar(20)` | Yes |  |
| `avatar_url` | `varchar(2048)` | Yes |  |
| `bio` | `varchar(500)` | Yes |  |
| `preferred_locale` | `varchar(10)` | No | Default `vi`, CHECK `preferred_locale IN ('vi', 'en')`; nhớ ngôn ngữ xuyên thiết bị |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_user_profiles_username_normalized` | `username IS NULL OR username = lower(btrim(username))` |
| `ck_user_profiles_username_not_blank` | `username IS NULL OR length(username) >= 3` |
| `ck_user_profiles_full_name_not_blank` | `length(btrim(full_name)) > 0` |
| `ck_user_profiles_phone_not_blank` | `phone_number IS NULL OR length(btrim(phone_number)) > 0` |
| `ck_user_profiles_preferred_locale` | `preferred_locale IN ('vi', 'en')` |

### Indexes

| Name | Definition |
|---|---|
| `ux_user_profiles_user_id` | UNIQUE (`user_id`) |
| `ux_user_profiles_username_active` | UNIQUE (`username`) WHERE `username IS NOT NULL AND is_deleted = false` |
| `ix_user_profiles_phone_number_active` | (`phone_number`) WHERE `phone_number IS NOT NULL AND is_deleted = false` |

---

## 3. `roles`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `role_key` | `varchar(30)` | No | UNIQUE, CHECK `RoleKey` |
| `name` | `varchar(100)` | No |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_roles_role_key` | `role_key IN ('CUSTOMER', 'OWNER', 'STAFF', 'ADMIN')` |
| `ck_roles_name_not_blank` | `length(btrim(name)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_roles_role_key` | UNIQUE (`role_key`) |

### Seed data

| `role_key` | `name` |
|---|---|
| `CUSTOMER` | `Khách hàng` |
| `OWNER` | `Chủ sân` |
| `STAFF` | `Nhân viên sân` |
| `ADMIN` | `Quản trị viên` |

---

## 4. `permissions`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `code` | `varchar(100)` | No | UNIQUE; mã dùng trong code để kiểm tra quyền |
| `description` | `varchar(300)` | No | Mô tả hiển thị cho Admin |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_permissions_code` | `code ~ '^[a-z_]+(\.[a-z_]+)+$'` |
| `ck_permissions_description` | `length(btrim(description)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_permissions_code` | UNIQUE (`code`) |

### Seed data

| `code` | `description` |
|---|---|
| `booking.create` | Đặt sân |
| `feedback.create` | Đánh giá sân đã chơi |
| `owner_request.create` | Gửi đơn đăng ký làm chủ sân |
| `facility.manage` | Tạo/sửa/xóa cơ sở, sân, bộ lịch, giá |
| `facility_staff.manage` | Thêm/gỡ nhân viên cho cơ sở |
| `facility_booking.view` | Xem booking và thông tin khách của cơ sở |
| `facility_slot.manage` | Vận hành slot: block/mở block, gộp/tách, chỉnh slot thường |
| `facility_court.status` | Đổi trạng thái sân con: đóng/mở tạm (bảo trì, hỏng sân) |
| `facility_revenue.view` | Xem doanh thu cơ sở |
| `campaign.manage` | Tạo/sửa chương trình giảm giá |
| `owner_request.review` | Duyệt/từ chối đơn đăng ký chủ sân |
| `facility.review` | Duyệt/từ chối/đình chỉ cơ sở |
| `user.manage` | Xem, khóa, mở khóa tài khoản |
| `report.handle` | Tiếp nhận và trả lời report |
| `finance.manage` | Hoàn tiền, payout, xem giao dịch toàn hệ thống |
| `system_config.manage` | Sửa cấu hình hệ thống |

---

## 5. `role_permissions`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `role_id` | `uuid` | No | FK → `roles.id`, ON DELETE RESTRICT |
| `permission_id` | `uuid` | No | FK → `permissions.id`, ON DELETE RESTRICT |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Indexes

| Name | Definition |
|---|---|
| `ux_role_permissions_role_permission_active` | UNIQUE (`role_id`, `permission_id`) WHERE `is_deleted = false` |
| `ix_role_permissions_permission_active` | (`permission_id`) WHERE `is_deleted = false` |

### Seed data

| Role | Permissions |
|---|---|
| `CUSTOMER` | `booking.create`, `feedback.create`, `owner_request.create` |
| `OWNER` | `facility.manage`, `facility_staff.manage`, `facility_booking.view`, `facility_slot.manage`, `facility_court.status`, `facility_revenue.view`, `campaign.manage` |
| `STAFF` | `facility_booking.view`, `facility_slot.manage`, `facility_court.status` |
| `ADMIN` | `owner_request.review`, `facility.review`, `user.manage`, `report.handle`, `finance.manage`, `campaign.manage`, `system_config.manage` |

---

## 6. `user_roles`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `role_id` | `uuid` | No | FK → `roles.id`, ON DELETE RESTRICT |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Indexes

| Name | Definition |
|---|---|
| `ux_user_roles_user_role_active` | UNIQUE (`user_id`, `role_id`) WHERE `is_deleted = false` |
| `ix_user_roles_role_user_active` | (`role_id`, `user_id`) WHERE `is_deleted = false` |

---

## 7. `refresh_tokens`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `token_hash` | `char(64)` | No | UNIQUE |
| `family_id` | `uuid` | No |  |
| `expires_at` | `timestamptz` | No |  |
| `revoked_at` | `timestamptz` | Yes |  |
| `replaced_by_token_id` | `uuid` | Yes | FK → `refresh_tokens.id`, ON DELETE SET NULL |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_refresh_tokens_expiration` | `expires_at > created_at` |
| `ck_refresh_tokens_revoked_at` | `revoked_at IS NULL OR revoked_at >= created_at` |
| `ck_refresh_tokens_replacement` | `replaced_by_token_id IS NULL OR revoked_at IS NOT NULL` |
| `ck_refresh_tokens_no_self_reference` | `replaced_by_token_id IS NULL OR replaced_by_token_id <> id` |

### Indexes

| Name | Definition |
|---|---|
| `ux_refresh_tokens_token_hash` | UNIQUE (`token_hash`) |
| `ux_refresh_tokens_replaced_by_token_id` | UNIQUE (`replaced_by_token_id`) WHERE `replaced_by_token_id IS NOT NULL` |
| `ix_refresh_tokens_user_active` | (`user_id`, `expires_at`) WHERE `revoked_at IS NULL AND is_deleted = false` |
| `ix_refresh_tokens_family_active` | (`family_id`) WHERE `is_deleted = false` |

---

## 8. `user_external_logins`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `provider` | `varchar(20)` | No | CHECK `ExternalLoginProvider` |
| `provider_subject` | `varchar(255)` | No | ID tài khoản do provider trả về (`sub` của Google) |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_user_external_logins_provider` | `provider IN ('GOOGLE')` |
| `ck_user_external_logins_subject` | `length(btrim(provider_subject)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_user_external_logins_provider_subject` | UNIQUE (`provider`, `provider_subject`) WHERE `is_deleted = false` |
| `ux_user_external_logins_user_provider` | UNIQUE (`user_id`, `provider`) WHERE `is_deleted = false` |

---

## 9. `owner_requests`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `status` | `varchar(20)` | No | Default `PENDING`, CHECK `OwnerRequestStatus` |
| `business_name` | `varchar(200)` | No |  |
| `legal_representative_name` | `varchar(150)` | No |  |
| `tax_code` | `varchar(30)` | Yes |  |
| `business_license_number` | `varchar(100)` | Yes |  |
| `business_license_image_url` | `varchar(2048)` | Yes |  |
| `identity_front_image_url` | `varchar(2048)` | No |  |
| `identity_back_image_url` | `varchar(2048)` | No |  |
| `request_note` | `varchar(1000)` | Yes |  |
| `reviewed_at` | `timestamptz` | Yes |  |
| `rejection_reason` | `varchar(1000)` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_owner_requests_status` | `status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED')` |
| `ck_owner_requests_business_name` | `length(btrim(business_name)) > 0` |
| `ck_owner_requests_legal_name` | `length(btrim(legal_representative_name)) > 0` |
| `ck_owner_requests_review_state` | `PENDING/CANCELLED`: `reviewed_at` and `rejection_reason` are NULL; `APPROVED`: `reviewed_at` is NOT NULL and `rejection_reason` is NULL; `REJECTED`: both are NOT NULL |
| `ck_owner_requests_business_license` | `business_license_number` and `business_license_image_url` are either both NULL or both NOT NULL |

### Indexes

| Name | Definition |
|---|---|
| `ux_owner_requests_one_pending_per_user` | UNIQUE (`user_id`) WHERE `status = 'PENDING' AND is_deleted = false` |
| `ux_owner_requests_one_approved_per_user` | UNIQUE (`user_id`) WHERE `status = 'APPROVED' AND is_deleted = false` |
| `ux_owner_requests_tax_code_approved` | UNIQUE (`tax_code`) WHERE `tax_code IS NOT NULL AND status = 'APPROVED' AND is_deleted = false` |
| `ux_owner_requests_business_license_approved` | UNIQUE (`business_license_number`) WHERE `business_license_number IS NOT NULL AND status = 'APPROVED' AND is_deleted = false` |
| `ix_owner_requests_status_created_at` | (`status`, `created_at`) WHERE `is_deleted = false` |
| `ix_owner_requests_user_created_at` | (`user_id`, `created_at` DESC) WHERE `is_deleted = false` |

---

## 10. `owner_profiles`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, UNIQUE, ON DELETE RESTRICT |
| `approved_owner_request_id` | `uuid` | No | FK → `owner_requests.id`, UNIQUE, ON DELETE RESTRICT |
| `business_name` | `varchar(200)` | No |  |
| `description` | `varchar(2000)` | Yes |  |
| `logo_url` | `varchar(2048)` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_owner_profiles_business_name` | `length(btrim(business_name)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_owner_profiles_user_id` | UNIQUE (`user_id`) |
| `ux_owner_profiles_approved_request_id` | UNIQUE (`approved_owner_request_id`) |

---

## 11. `user_bank_accounts`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `bank_code` | `varchar(20)` | No |  |
| `account_number_encrypted` | `text` | No |  |
| `account_number_hash` | `char(64)` | No |  |
| `account_holder_name` | `varchar(150)` | No |  |
| `is_default` | `boolean` | No | Default `false` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_user_bank_accounts_bank_code` | `bank_code = upper(btrim(bank_code)) AND length(bank_code) > 0` |
| `ck_user_bank_accounts_number_encrypted` | `length(btrim(account_number_encrypted)) > 0` |
| `ck_user_bank_accounts_number_hash` | `account_number_hash ~ '^[0-9a-f]{64}$'` |
| `ck_user_bank_accounts_holder_name` | `length(btrim(account_holder_name)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_user_bank_accounts_user_bank_number_active` | UNIQUE (`user_id`, `bank_code`, `account_number_hash`) WHERE `is_deleted = false` |
| `ux_user_bank_accounts_one_default_per_user` | UNIQUE (`user_id`) WHERE `is_default = true AND is_deleted = false` |
| `ix_user_bank_accounts_user_active` | (`user_id`, `is_default` DESC, `created_at` DESC) WHERE `is_deleted = false` |
| `ix_user_bank_accounts_bank_number_hash` | (`bank_code`, `account_number_hash`) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `owner_profile_matches_request_user` | `owner_profiles.user_id = owner_requests.user_id` của `approved_owner_request_id` |
| `owner_profile_requires_approved_request` | `owner_requests.status = 'APPROVED'` trước khi tạo `owner_profiles` |
| `owner_role_requires_owner_profile` | Role `OWNER` được thêm vào `user_roles` cùng transaction khi tạo `owner_profiles` |
| `default_customer_role` | User mới được thêm role `CUSTOMER` cùng transaction khi tạo `users` |
| `staff_role_assignment` | Role `STAFF` được thêm khi User được Owner gán vào `facility_staffs` lần đầu; gỡ role khi User không còn thuộc Facility nào |
| `permission_check` | API kiểm tra permission của User (hợp từ mọi role đang có), sau đó kiểm tra quyền trên tài nguyên: Owner sở hữu Facility, hoặc Staff thuộc `facility_staffs` của Facility đó |
| `permission_catalog_sync` | Danh sách `permissions` được khai báo bằng hằng số trong code và seed bằng migration; API kiểm tra khớp với database khi khởi động |
| `login_method_required` | Mỗi User phải có `password_hash` hoặc ít nhất một `user_external_logins` |
| `google_login_link` | Đăng nhập Google lần đầu: email đã tồn tại thì liên kết vào User đó; chưa có thì tạo User (`status = VERIFIED`, `email_verified_at` = thời điểm đăng nhập), `user_profiles` và role `CUSTOMER` trong cùng database transaction |
| `banned_user_blocked` | User `BANNED` không đăng nhập được; khi ban phải revoke toàn bộ `refresh_tokens` của User trong cùng transaction |
| `owner_business_suspend` | Muốn ngừng kinh doanh một cơ sở: dùng `facilities.status = INACTIVE` (cụm Setup sân); Owner vi phạm nặng: `users.status = BANNED` |
| `refresh_token_family_revocation` | Khi phát hiện reuse, revoke toàn bộ `refresh_tokens` có cùng `family_id` |
| `user_bank_account_default` | Khi đặt một tài khoản làm mặc định, phải bỏ mặc định tài khoản cũ của cùng `user_id` trong cùng database transaction |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — 0..1 | `user_profiles` | `user_profiles.user_id` |
| `users` | 1 — N | `user_roles` | `user_roles.user_id` |
| `roles` | 1 — N | `user_roles` | `user_roles.role_id` |
| `roles` | 1 — N | `role_permissions` | `role_permissions.role_id` |
| `permissions` | 1 — N | `role_permissions` | `role_permissions.permission_id` |
| `users` | 1 — N | `refresh_tokens` | `refresh_tokens.user_id` |
| `refresh_tokens` | 1 — 0..1 | `refresh_tokens` | `refresh_tokens.replaced_by_token_id` |
| `users` | 1 — N | `user_external_logins` | `user_external_logins.user_id` |
| `users` | 1 — N | `owner_requests` | `owner_requests.user_id` |
| `users` | 1 — 0..1 | `owner_profiles` | `owner_profiles.user_id` |
| `owner_requests` | 1 — 0..1 | `owner_profiles` | `owner_profiles.approved_owner_request_id` |
| `users` | 1 — N | `user_bank_accounts` | `user_bank_accounts.user_id` |

---

## Phân quyền theo vai trò

### Owner — toàn quyền trên các cơ sở của mình

| Nhóm | Tính năng | Permission |
|---|---|---|
| Cơ sở | Tạo / sửa / xóa Facility, ảnh, gửi duyệt | `facility.manage` |
| Sân con | Tạo / sửa / xóa Court | `facility.manage` |
| Lịch & giá | Tạo bộ lịch, sinh slot, sửa giá | `facility.manage` |
| Vận hành slot | Block/mở, gộp/tách, chỉnh slot thường | `facility_slot.manage` |
| Trạng thái sân | Đóng / mở sân con tạm (bảo trì, hỏng sân) | `facility_court.status` |
| Booking | Xem danh sách booking, thông tin khách đặt | `facility_booking.view` |
| Doanh thu | Xem dashboard, doanh thu, số tiền chờ thanh toán | `facility_revenue.view` |
| Voucher | Tạo chương trình, mã giảm giá cho sân của mình | `campaign.manage` |
| Nhân viên | Thêm / gỡ Staff cho từng cơ sở | `facility_staff.manage` |
| Feedback | Trả lời đánh giá của khách | (mọi User đã đăng nhập) |

### Staff — chỉ trên cơ sở được Owner giao

| Được làm | Không được làm |
|---|---|
| Xem lịch sân, danh sách booking, thông tin khách đặt | Tạo / sửa / xóa cơ sở, Court, bộ lịch |
| Block / mở block, gộp / tách, chỉnh slot thường | Sửa giá |
| Đóng / mở sân con tạm (bảo trì, hỏng sân) | Xem doanh thu, dashboard tài chính |
| Trả lời feedback của khách | Tạo voucher, quản lý nhân viên khác |

### Customer

Đặt sân, thanh toán, hủy đơn, đánh giá sân đã chơi, yêu thích sân, gửi report, gửi đơn làm chủ sân.

### Admin

Toàn quyền hệ thống: duyệt chủ sân / cơ sở, khóa tài khoản, xử lý report, hoàn tiền, payout, cấu hình hệ thống.
