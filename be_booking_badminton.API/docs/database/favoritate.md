# DATABASE — CỤM YÊU THÍCH COURT

> User chỉ yêu thích Court, không yêu thích Facility.

## 1. `favorite_courts`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `court_id` | `uuid` | No | FK → `courts.id`, ON DELETE RESTRICT |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ux_favorite_courts_user_court` | UNIQUE (`user_id`, `court_id`) |

### Indexes

| Name | Definition |
|---|---|
| `ix_favorite_courts_user_created_at` | (`user_id`, `created_at` DESC, `court_id`) WHERE `is_deleted = false` |
| `ix_favorite_courts_court_user` | (`court_id`, `user_id`) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `favorite_active_user` | Chỉ User đang hoạt động được thêm Court vào danh sách yêu thích |
| `favorite_existing_court` | Chỉ được thêm Court tồn tại và chưa bị soft-delete; nếu Court ngừng hoạt động thì record yêu thích vẫn được giữ nhưng UI không cho booking |
| `favorite_restore` | Khi User thích lại cùng Court, restore record đã soft-delete thay vì tạo record mới |
| `favorite_remove` | Khi User bỏ thích, soft-delete record; không xóa vật lý |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `favorite_courts` | `favorite_courts.user_id` |
| `courts` | 1 — N | `favorite_courts` | `favorite_courts.court_id` |

Suy ra `users` và `courts` có quan hệ N — N thông qua `favorite_courts`.
