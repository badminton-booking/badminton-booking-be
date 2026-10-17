# DATABASE — CỤM FEEDBACK

> Có hai loại feedback: đánh giá Court sau khi hoàn thành BookingDetail và đánh giá chung về nền tảng. Một BookingDetail được phép có nhiều Feedback theo rule hiện tại.

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

> Không tạo UNIQUE trên `booking_detail_id` vì một BookingDetail được phép có nhiều Feedback.

---

## 2. `feedback_replies`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `feedback_id` | `uuid` | No | FK → `feedbacks.id`, ON DELETE RESTRICT |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `parent_reply_id` | `uuid` | Yes | FK → `feedback_replies.id`, ON DELETE RESTRICT |
| `content` | `varchar(2000)` | No |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_feedback_replies_content` | `length(btrim(content)) > 0` |
| `ck_feedback_replies_no_self_parent` | `parent_reply_id IS NULL OR parent_reply_id <> id` |

### Indexes

| Name | Definition |
|---|---|
| `ix_feedback_replies_feedback_parent_created_at` | (`feedback_id`, `parent_reply_id`, `created_at`, `id`) WHERE `is_deleted = false` |
| `ix_feedback_replies_parent_created_at` | (`parent_reply_id`, `created_at`, `id`) WHERE `parent_reply_id IS NOT NULL AND is_deleted = false` |
| `ix_feedback_replies_user_created_at` | (`user_id`, `created_at` DESC) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `court_feedback_user_matches_booking` | Với Feedback `COURT`, `feedbacks.user_id` phải là Customer sở hữu Booking chứa `booking_detail_id` |
| `court_feedback_court_matches_booking` | `feedbacks.court_id` phải bằng Court được lưu trong `booking_details.court_id` |
| `court_feedback_booking_completed` | Chỉ được tạo Feedback `COURT` khi BookingDetail đã hoàn thành và Booking đã thanh toán thành công |
| `multiple_feedbacks_allowed` | Không giới hạn số Feedback của một BookingDetail theo rule hiện tại |
| `platform_feedback_no_booking` | Feedback `PLATFORM` không yêu cầu BookingDetail hoặc Court |
| `reply_parent_same_feedback` | Nếu có `parent_reply_id`, parent reply phải thuộc cùng `feedback_id` |
| `reply_authorization` | Mọi User đang hoạt động đều được reply Feedback chưa bị soft-delete |
| `reply_tree_delete` | Khi một reply cha bị soft-delete, reply con vẫn được giữ; UI hiển thị nội dung cha là đã bị xóa để không làm vỡ cây hội thoại |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `feedbacks` | `feedbacks.user_id` |
| `courts` | 1 — N | `feedbacks` | `feedbacks.court_id` |
| `booking_details` | 1 — N | `feedbacks` | `feedbacks.booking_detail_id` |
| `feedbacks` | 1 — N | `feedback_replies` | `feedback_replies.feedback_id` |
| `users` | 1 — N | `feedback_replies` | `feedback_replies.user_id` |
| `feedback_replies` | 1 — N | `feedback_replies` (Child reply) | `feedback_replies.parent_reply_id` |
