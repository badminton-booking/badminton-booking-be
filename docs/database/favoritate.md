# DATABASE — CỤM YÊU THÍCH FACILITY

> User yêu thích Facility (cơ sở), không yêu thích từng Court.

## 1. `favorite_facilities`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT |
| `facility_id` | `uuid` | No | FK → `facilities.id`, ON DELETE RESTRICT |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ux_favorite_facilities_user_facility` | UNIQUE (`user_id`, `facility_id`) |

### Indexes

| Name | Definition |
|---|---|
| `ix_favorite_facilities_user_created_at` | (`user_id`, `created_at` DESC, `facility_id`) WHERE `is_deleted = false` |
| `ix_favorite_facilities_facility_user` | (`facility_id`, `user_id`) WHERE `is_deleted = false` |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `favorite_active_user` | Chỉ User đang hoạt động được thêm Facility vào danh sách yêu thích |
| `favorite_existing_facility` | Chỉ được thêm Facility tồn tại và chưa bị soft-delete; nếu Facility ngừng hoạt động thì record yêu thích vẫn được giữ nhưng UI không cho booking |
| `favorite_restore` | Khi User thích lại cùng Facility, restore record đã soft-delete thay vì tạo record mới |
| `favorite_remove` | Khi User bỏ thích, soft-delete record; không xóa vật lý |

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `users` | 1 — N | `favorite_facilities` | `favorite_facilities.user_id` |
| `facilities` | 1 — N | `favorite_facilities` | `favorite_facilities.facility_id` |

Suy ra `users` và `facilities` có quan hệ N — N thông qua `favorite_facilities`.
