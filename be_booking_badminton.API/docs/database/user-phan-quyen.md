# DATABASE — CỤM USER / PHÂN QUYỀN

## Enum definitions

### `UserStatus`

| Value |
|---|
| `PENDING_VERIFICATION` |
| `ACTIVE` |
| `SUSPENDED` |
| `DISABLED` |

### `RoleKey`

| Value |
|---|
| `CUSTOMER` |
| `OWNER` |
| `ADMIN` |

### `OwnerRequestStatus`

| Value |
|---|
| `PENDING` |
| `APPROVED` |
| `REJECTED` |
| `CANCELLED` |

### `OwnerProfileStatus`

| Value |
|---|
| `ACTIVE` |
| `SUSPENDED` |
| `DISABLED` |

---

## 1. `users`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `email` | `varchar(320)` | No | UNIQUE |
| `password_hash` | `varchar(512)` | No |  |
| `status` | `varchar(30)` | No | Default `PENDING_VERIFICATION`, CHECK `UserStatus` |
| `email_verified_at` | `timestamptz` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_users_status` | `status IN ('PENDING_VERIFICATION', 'ACTIVE', 'SUSPENDED', 'DISABLED')` |
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
| `ck_roles_role_key` | `role_key IN ('CUSTOMER', 'OWNER', 'ADMIN')` |
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
| `ADMIN` | `Quản trị viên` |

---

## 4. `user_roles`

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

## 5. `refresh_tokens`

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

## 6. `owner_requests`

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

## 7. `owner_profiles`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, UNIQUE, ON DELETE RESTRICT |
| `approved_owner_request_id` | `uuid` | No | FK → `owner_requests.id`, UNIQUE, ON DELETE RESTRICT |
| `business_name` | `varchar(200)` | No |  |
| `description` | `varchar(2000)` | Yes |  |
| `logo_url` | `varchar(2048)` | Yes |  |
| `status` | `varchar(20)` | No | Default `ACTIVE`, CHECK `OwnerProfileStatus` |
| `suspended_at` | `timestamptz` | Yes |  |
| `suspension_reason` | `varchar(1000)` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_owner_profiles_status` | `status IN ('ACTIVE', 'SUSPENDED', 'DISABLED')` |
| `ck_owner_profiles_business_name` | `length(btrim(business_name)) > 0` |
| `ck_owner_profiles_suspension` | `SUSPENDED`: `suspended_at` and `suspension_reason` are NOT NULL; `ACTIVE/DISABLED`: both are NULL |

### Indexes

| Name | Definition |
|---|---|
| `ux_owner_profiles_user_id` | UNIQUE (`user_id`) |
| `ux_owner_profiles_approved_request_id` | UNIQUE (`approved_owner_request_id`) |
| `ix_owner_profiles_status_active` | (`status`) WHERE `is_deleted = false` |

---

## 8. `user_bank_accounts`

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
| `refresh_token_family_revocation` | Khi phát hiện reuse, revoke toàn bộ `refresh_tokens` có cùng `family_id` |
| `user_bank_account_default` | Khi đặt một tài khoản làm mặc định, phải bỏ mặc định tài khoản cũ của cùng `user_id` trong cùng database transaction |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — 0..1 | `user_profiles` | `user_profiles.user_id` |
| `users` | 1 — N | `user_roles` | `user_roles.user_id` |
| `roles` | 1 — N | `user_roles` | `user_roles.role_id` |
| `users` | 1 — N | `refresh_tokens` | `refresh_tokens.user_id` |
| `refresh_tokens` | 1 — 0..1 | `refresh_tokens` | `refresh_tokens.replaced_by_token_id` |
| `users` | 1 — N | `owner_requests` | `owner_requests.user_id` |
| `users` | 1 — 0..1 | `owner_profiles` | `owner_profiles.user_id` |
| `owner_requests` | 1 — 0..1 | `owner_profiles` | `owner_profiles.approved_owner_request_id` |
| `users` | 1 — N | `user_bank_accounts` | `user_bank_accounts.user_id` |
