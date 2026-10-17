# DATABASE — CỤM TRANSACTION / LEDGER

> Đơn vị tiền: VND, lưu bằng `bigint`.

## Enum definitions

### `FinancialTransactionType`

| Value |
|---|
| `PAYMENT` |
| `REFUND` |
| `PAYOUT` |

### `LedgerAccountType`

| Value |
|---|
| `PLATFORM_CASH` |
| `PLATFORM_REVENUE` |
| `OWNER_PAYABLE` |

### `LedgerEntryType`

| Value |
|---|
| `DEBIT` |
| `CREDIT` |

---

## 1. `financial_transactions`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint                                                      |
|---|---|---:|---------------------------------------------------------------------------------|
| `id` | `uuid` | No | PK, default `gen_random_uuid()`                                                 |
| `booking_id` | `uuid` | Yes | FK → `bookings.id`, ON DELETE RESTRICT                                          |
| `owner_profile_id` | `uuid` | Yes | FK → `owner_profiles.id`, ON DELETE RESTRICT                                    |
| `destination_bank_account_id` | `uuid` | Yes | FK → `user_bank_accounts.id`, ON DELETE RESTRICT                                |
| `parent_transaction_id` | `uuid` | Yes | FK → `financial_transactions.id`, ON DELETE RESTRICT (giao dịch PAYMENT gốc của REFUND) |
| `type` | `varchar(20)` | No | CHECK `FinancialTransactionType`                                                |
| `amount` | `bigint` | No | CHECK `amount > 0`                                                              |
| `provider` | `varchar(50)` | Yes | (tên cổng thanh toán)                                                           |
| `provider_reference` | `varchar(255)` | Yes | (chứa mã giao dịch duy nhất do sepay trả về)                                    |
| `is_deleted` | `boolean` | No | Default `false`                                                                 |
| `created_at` | `timestamptz` | No | Default `now()`                                                                 |
| `updated_at` | `timestamptz` | Yes |                                                                                 |

### Constraints

| Name | Definition |
|---|---|
| `ck_financial_transactions_type` | `type IN ('PAYMENT', 'REFUND', 'PAYOUT')` |
| `ck_financial_transactions_amount` | `amount > 0` |
| `ck_financial_transactions_provider` | `provider` and `provider_reference` are either both NULL or both NOT NULL |
| `ck_financial_transactions_no_self_parent` | `parent_transaction_id IS NULL OR parent_transaction_id <> id` |
| `ck_financial_transactions_target` | `PAYMENT`: chỉ `booking_id` là NOT NULL; `REFUND`: `booking_id`, `parent_transaction_id`, `destination_bank_account_id` là NOT NULL và `owner_profile_id` là NULL; `PAYOUT`: `owner_profile_id`, `destination_bank_account_id` là NOT NULL và `booking_id`, `parent_transaction_id` là NULL |

### Indexes

| Name | Definition |
|---|---|
| `ux_financial_transactions_provider_reference` | UNIQUE (`provider`, `provider_reference`) WHERE `provider_reference IS NOT NULL` |
| `ix_financial_transactions_booking_type_created_at` | (`booking_id`, `type`, `created_at` DESC) WHERE `booking_id IS NOT NULL AND is_deleted = false` |
| `ix_financial_transactions_owner_type_created_at` | (`owner_profile_id`, `type`, `created_at` DESC) WHERE `owner_profile_id IS NOT NULL AND is_deleted = false` |
| `ix_financial_transactions_destination_bank_account` | (`destination_bank_account_id`, `created_at` DESC) WHERE `destination_bank_account_id IS NOT NULL` |
| `ix_financial_transactions_parent` | (`parent_transaction_id`) WHERE `parent_transaction_id IS NOT NULL` |
| `ix_financial_transactions_type_created_at` | (`type`, `created_at` DESC) WHERE `is_deleted = false` |

---

## 2. `ledger_accounts`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `owner_profile_id` | `uuid` | Yes | FK → `owner_profiles.id`, ON DELETE RESTRICT |
| `account_type` | `varchar(30)` | No | CHECK `LedgerAccountType` |
| `balance` | `bigint` | No | Default `0` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_ledger_accounts_type` | `account_type IN ('PLATFORM_CASH', 'PLATFORM_REVENUE', 'OWNER_PAYABLE')` |
| `ck_ledger_accounts_owner` | `OWNER_PAYABLE` requires `owner_profile_id IS NOT NULL`; platform account types require `owner_profile_id IS NULL` |

### Indexes

| Name | Definition |
|---|---|
| `ux_ledger_accounts_platform_type` | UNIQUE (`account_type`) WHERE `owner_profile_id IS NULL AND is_deleted = false` |
| `ux_ledger_accounts_owner_type` | UNIQUE (`owner_profile_id`, `account_type`) WHERE `owner_profile_id IS NOT NULL AND is_deleted = false` |
| `ix_ledger_accounts_owner_profile_id` | (`owner_profile_id`) WHERE `owner_profile_id IS NOT NULL AND is_deleted = false` |

---

## 3. `ledger_entries`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `financial_transaction_id` | `uuid` | No | FK → `financial_transactions.id`, ON DELETE RESTRICT |
| `ledger_account_id` | `uuid` | No | FK → `ledger_accounts.id`, ON DELETE RESTRICT |
| `entry_type` | `varchar(10)` | No | CHECK `LedgerEntryType` |
| `amount` | `bigint` | No | CHECK `amount > 0` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_ledger_entries_type` | `entry_type IN ('DEBIT', 'CREDIT')` |
| `ck_ledger_entries_amount` | `amount > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_ledger_entries_transaction_account_type` | UNIQUE (`financial_transaction_id`, `ledger_account_id`, `entry_type`) WHERE `is_deleted = false` |
| `ix_ledger_entries_transaction_id` | (`financial_transaction_id`) WHERE `is_deleted = false` |
| `ix_ledger_entries_account_created_at` | (`ledger_account_id`, `created_at` DESC) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `successful_transactions_only` | Chỉ tạo `financial_transactions` sau khi PAYMENT, REFUND hoặc PAYOUT thành công |
| `refund_parent_type` | `REFUND.parent_transaction_id` phải trỏ đến transaction loại `PAYMENT` |
| `refund_same_booking` | `REFUND.booking_id` phải bằng `PAYMENT.booking_id` của transaction gốc |
| `refund_amount_limit` | Tổng `REFUND.amount` của một PAYMENT không vượt quá `PAYMENT.amount` |
| `refund_destination_owner` | `REFUND.destination_bank_account_id` phải thuộc `users.id` đã tạo Booking được hoàn tiền |
| `payout_owner_account` | PAYOUT phải ghi giảm đúng LedgerAccount `OWNER_PAYABLE` thuộc `owner_profile_id` tương ứng |
| `payout_destination_owner` | `PAYOUT.destination_bank_account_id` phải thuộc `users.id` của `owner_profile_id` nhận tiền |
| `balanced_ledger_entries` | Trong mỗi transaction: tổng DEBIT = tổng CREDIT = `financial_transactions.amount` |
| `atomic_ledger_write` | `financial_transactions`, toàn bộ `ledger_entries` và cập nhật `ledger_accounts.balance` phải nằm trong cùng database transaction |
| `immutable_financial_history` | Không update/soft-delete `financial_transactions` và `ledger_entries`; sai sót phải tạo transaction đảo ngược |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `bookings` | 1 — N | `financial_transactions` | `financial_transactions.booking_id` |
| `owner_profiles` | 1 — N | `financial_transactions` | `financial_transactions.owner_profile_id` |
| `user_bank_accounts` | 1 — N | `financial_transactions` | `financial_transactions.destination_bank_account_id` |
| `financial_transactions` | 1 — N | `financial_transactions` (Refund) | `financial_transactions.parent_transaction_id` |
| `financial_transactions` | 1 — N | `ledger_entries` | `ledger_entries.financial_transaction_id` |
| `ledger_accounts` | 1 — N | `ledger_entries` | `ledger_entries.ledger_account_id` |
| `owner_profiles` | 1 — N | `ledger_accounts` | `ledger_accounts.owner_profile_id` |
