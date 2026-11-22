# DATABASE — CỤM FEEDBACK

> Có hai loại feedback: đánh giá Court sau khi hoàn thành BookingDetail và đánh giá chung về nền tảng. Mỗi BookingDetail chỉ được **một** Feedback `COURT` (được sửa lại, không tạo thêm).

## Enum definitions

### `FeedbackType`

| Value |
|---|
| `COURT` |
| `PLATFORM` |

---

## 1. `feedbacks`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `court_id` | `uuid` | Yes | FK → `courts.id`, ON DELETE RESTRICT |
| `booking_detail_id` | `uuid` | Yes | FK → `booking_details.id`, ON DELETE RESTRICT |
| `type` | `varchar(20)` | No | CHECK `FeedbackType` |
| `rating` | `smallint` | No | CHECK `rating BETWEEN 1 AND 5` |
| `content` | `varchar(2000)` | No |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_feedbacks_type` | `type IN ('COURT', 'PLATFORM')` |
| `ck_feedbacks_rating` | `rating BETWEEN 1 AND 5` |
| `ck_feedbacks_content` | `length(btrim(content)) > 0` |
| `ck_feedbacks_target` | `COURT`: `court_id` và `booking_detail_id` NOT NULL; `PLATFORM`: cả hai NULL |

### Indexes

| Name | Definition |
|---|---|
| `ix_feedbacks_court_created_at` | (`court_id`, `created_at` DESC, `id`) INCLUDE (`rating`) WHERE `type = 'COURT' AND is_deleted = false` |
| `ix_feedbacks_booking_detail` | (`booking_detail_id`, `created_at` DESC) WHERE `booking_detail_id IS NOT NULL AND is_deleted = false` |
| `ix_feedbacks_user_type_created_at` | (`user_id`, `type`, `created_at` DESC) WHERE `is_deleted = false` |
| `ix_feedbacks_platform_created_at` | (`created_at` DESC, `id`) WHERE `type = 'PLATFORM' AND is_deleted = false` |
| `ux_feedbacks_booking_detail` | UNIQUE (`booking_detail_id`) WHERE `type = 'COURT' AND is_deleted = false` — mỗi BookingDetail một Feedback |

---

## 2. `feedback_replies`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `feedback_id` | `uuid` | No | FK → `feedbacks.id`, ON DELETE RESTRICT |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `content` | `varchar(2000)` | No |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_feedback_replies_content` | `length(btrim(content)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ix_feedback_replies_feedback_created_at` | (`feedback_id`, `created_at`, `id`) WHERE `is_deleted = false` |
| `ix_feedback_replies_user_created_at` | (`user_id`, `created_at` DESC) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `court_feedback_user_matches_booking` | Với Feedback `COURT`, `feedbacks.user_id` phải là Customer sở hữu Booking chứa `booking_detail_id` |
| `court_feedback_court_matches_booking` | `feedbacks.court_id` phải bằng Court được lưu trong `booking_details.court_id` |
| `court_feedback_booking_completed` | Chỉ được tạo Feedback `COURT` khi BookingDetail đã hoàn thành và Booking đã thanh toán thành công |
| `one_feedback_per_booking_detail` | Mỗi BookingDetail chỉ có một Feedback `COURT`; muốn đổi thì sửa feedback cũ, không tạo thêm |
| `platform_feedback_no_booking` | Feedback `PLATFORM` không yêu cầu BookingDetail hoặc Court |
| `reply_single_level` | Reply chỉ một tầng, trực tiếp dưới Feedback; không có reply của reply |
| `reply_authorization` | Mọi User đang hoạt động đều được reply Feedback chưa bị soft-delete |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `feedbacks` | `feedbacks.user_id` |
| `courts` | 1 — N | `feedbacks` | `feedbacks.court_id` |
| `booking_details` | 1 — N | `feedbacks` | `feedbacks.booking_detail_id` |
| `feedbacks` | 1 — N | `feedback_replies` | `feedback_replies.feedback_id` |
| `users` | 1 — N | `feedback_replies` | `feedback_replies.user_id` |
