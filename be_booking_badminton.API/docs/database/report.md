# DATABASE — CỤM REPORT

> Report là kênh hỗ trợ riêng giữa User và Admin. Report không liên kết trực tiếp với FinancialTransaction; vấn đề thanh toán được report qua Booking hoặc GENERAL, sau đó Admin đối soát ở cụm transaction.

## Enum definitions

### `ReportStatus`

| Value |
|---|
| `OPEN` |
| `IN_PROGRESS` |
| `RESOLVED` |
| `REJECTED` |
| `CANCELLED` |

### `ReportTargetType`

| Value | `target_id` tham chiếu logic |
|---|---|
| `GENERAL` | NULL |
| `USER` | `users.id` |
| `FACILITY` | `facilities.id` |
| `COURT` | `courts.id` |
| `BOOKING` | `bookings.id` |
| `FEEDBACK` | `feedbacks.id` |

---

## 1. `reports`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `reporter_user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `handled_by_user_id` | `uuid` | Yes | FK → `users.id`, ON DELETE RESTRICT |
| `target_type` | `varchar(20)` | No | CHECK `ReportTargetType` |
| `target_id` | `uuid` | Yes | Polymorphic target; không tạo FK vật lý |
| `subject` | `varchar(200)` | No |  |
| `description` | `varchar(4000)` | No |  |
| `status` | `varchar(20)` | No | Default `OPEN`, CHECK `ReportStatus` |
| `last_message_at` | `timestamptz` | No | Default `now()` |
| `closed_at` | `timestamptz` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_reports_status` | `status IN ('OPEN', 'IN_PROGRESS', 'RESOLVED', 'REJECTED', 'CANCELLED')` |
| `ck_reports_target_type` | `target_type IN ('GENERAL', 'USER', 'FACILITY', 'COURT', 'BOOKING', 'FEEDBACK')` |
| `ck_reports_target` | `GENERAL`: `target_id IS NULL`; loại khác: `target_id IS NOT NULL` |
| `ck_reports_subject` | `length(btrim(subject)) > 0` |
| `ck_reports_description` | `length(btrim(description)) > 0` |
| `ck_reports_handler` | `OPEN/CANCELLED`: `handled_by_user_id IS NULL`; `IN_PROGRESS/RESOLVED/REJECTED`: `handled_by_user_id IS NOT NULL` |
| `ck_reports_closed_at` | `RESOLVED/REJECTED/CANCELLED`: `closed_at IS NOT NULL`; `OPEN/IN_PROGRESS`: `closed_at IS NULL` |
| `ck_reports_closed_time` | `closed_at IS NULL OR closed_at >= created_at` |
| `ck_reports_last_message_time` | `last_message_at >= created_at` |

### Indexes

| Name | Definition |
|---|---|
| `ix_reports_reporter_status_activity` | (`reporter_user_id`, `status`, `last_message_at` DESC) WHERE `is_deleted = false` |
| `ix_reports_admin_inbox` | (`status`, `last_message_at` DESC) WHERE `status IN ('OPEN', 'IN_PROGRESS') AND is_deleted = false` |
| `ix_reports_handler_status_activity` | (`handled_by_user_id`, `status`, `last_message_at` DESC) WHERE `handled_by_user_id IS NOT NULL AND is_deleted = false` |
| `ix_reports_target` | (`target_type`, `target_id`, `created_at` DESC) WHERE `target_id IS NOT NULL AND is_deleted = false` |

---

## 2. `report_messages`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `report_id` | `uuid` | No | FK → `reports.id`, ON DELETE RESTRICT |
| `sender_user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `content` | `varchar(4000)` | No |  |
| `read_at` | `timestamptz` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_report_messages_content` | `length(btrim(content)) > 0` |
| `ck_report_messages_read_time` | `read_at IS NULL OR read_at >= created_at` |

### Indexes

| Name | Definition |
|---|---|
| `ix_report_messages_report_created_at` | (`report_id`, `created_at`, `id`) WHERE `is_deleted = false` |
| `ix_report_messages_report_unread` | (`report_id`, `created_at`) WHERE `read_at IS NULL AND is_deleted = false` |
| `ix_report_messages_sender_created_at` | (`sender_user_id`, `created_at` DESC) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `report_target_exists` | Application phải kiểm tra `target_id` tồn tại trong bảng tương ứng với `target_type`; polymorphic association không có FK vật lý |
| `report_target_access` | Reporter chỉ được report target mà họ được phép xem; với `BOOKING`, reporter phải là Customer của Booking hoặc Owner sở hữu Court liên quan |
| `report_handler_is_admin` | `handled_by_user_id` bắt buộc là User có role `ADMIN` |
| `report_message_sender` | `sender_user_id` chỉ được là `reporter_user_id` hoặc Admin đang xử lý report |
| `report_first_admin_response` | Khi Admin gửi message đầu tiên cho report `OPEN`, phải đồng thời đặt `handled_by_user_id` và chuyển status sang `IN_PROGRESS` |
| `report_closed_guard` | Không tạo ReportMessage mới khi report ở trạng thái `RESOLVED`, `REJECTED` hoặc `CANCELLED` |
| `report_cancel_guard` | Chỉ reporter được chuyển report từ `OPEN` sang `CANCELLED` |
| `report_last_message_at` | Khi tạo ReportMessage, phải cập nhật `reports.last_message_at = report_messages.created_at` trong cùng database transaction |
| `report_no_transaction_target` | Không dùng FinancialTransaction làm target; report tiền bạc trỏ tới `BOOKING` hoặc dùng `GENERAL` |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `reports` (Reporter) | `reports.reporter_user_id` |
| `users` | 1 — N | `reports` (Handler/Admin) | `reports.handled_by_user_id` |
| `reports` | 1 — N | `report_messages` | `report_messages.report_id` |
| `users` | 1 — N | `report_messages` | `report_messages.sender_user_id` |

> `target_type + target_id` là Polymorphic Association nên các bảng target không có quan hệ FK vật lý tới `reports`.
