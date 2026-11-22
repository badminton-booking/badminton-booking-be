# DATABASE — CỤM SETUP SÂN

> Quy ước `day_of_week`: ISO-8601, `1 = MONDAY` ... `7 = SUNDAY`.
>
> Đơn vị tiền: VND, lưu bằng `bigint`. Hệ thống sử dụng cố định múi giờ Việt Nam `Asia/Ho_Chi_Minh`.

## Enum definitions

### `FacilityStatus`

| Value | Ý nghĩa |
|---|---|
| `DRAFT` | Nháp, owner đang soạn |
| `PENDING_APPROVAL` | Đã gửi, chờ admin duyệt |
| `ACTIVE` | Đang hoạt động |
| `REJECTED` | Admin từ chối |
| `INACTIVE` | Ngừng hoạt động — owner tự tắt, hoặc admin tắt khi vi phạm |

### `CourtStatus`

| Value | Ý nghĩa |
|---|---|
| `ACTIVE` | Sân đang mở bán |
| `INACTIVE` | Đóng sân (bảo trì, hỏng, ngừng bán) — có thể mở lại |

### `ScheduleTemplateStatus`

| Value | Ý nghĩa |
|---|---|
| `DRAFT` | Nháp |
| `ACTIVE` | Đang dùng |
| `INACTIVE` | Không dùng nữa |

### `CourtSlotBlockType`

| Value |
|---|
| `WEEKLY` |
| `SPECIFIC_DATE` |

---

## 1. `facilities`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `owner_profile_id` | `uuid` | No | FK → `owner_profiles.id`, ON DELETE RESTRICT |
| `name` | `varchar(200)` | No |  |
| `description` | `varchar(2000)` | Yes |  |
| `phone_number` | `varchar(20)` | Yes |  |
| `address_line` | `varchar(500)` | No |  |
| `province` | `varchar(100)` | No | Tỉnh/thành phố, dùng tìm sân theo khu vực |
| `district` | `varchar(100)` | No | Quận/huyện, dùng tìm sân theo khu vực |
| `latitude` | `numeric(9,6)` | Yes |  |
| `longitude` | `numeric(9,6)` | Yes |  |
| `advance_booking_days` | `smallint` | No | Default `3`; Customer được đặt trước tối đa bao nhiêu ngày |
| `min_booking_minutes` | `smallint` | No | Default `60`; tổng thời lượng tối thiểu của một lần đặt trên một Court |
| `cancellation_policy` | `jsonb` | No | Chính sách hoàn tiền khi hủy; cấu trúc định nghĩa bên dưới |
| `status` | `varchar(30)` | No | Default `DRAFT`, CHECK `FacilityStatus` |
| `submitted_at` | `timestamptz` | Yes |  |
| `reviewed_at` | `timestamptz` | Yes |  |
| `rejection_reason` | `varchar(1000)` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_facilities_status` | `status IN ('DRAFT', 'PENDING_APPROVAL', 'ACTIVE', 'REJECTED', 'INACTIVE')` |
| `ck_facilities_name` | `length(btrim(name)) > 0` |
| `ck_facilities_address` | `length(btrim(address_line)) > 0` |
| `ck_facilities_area` | `length(btrim(province)) > 0 AND length(btrim(district)) > 0` |
| `ck_facilities_advance_booking_days` | `advance_booking_days BETWEEN 1 AND 60` |
| `ck_facilities_min_booking_minutes` | `min_booking_minutes BETWEEN 5 AND 720` |
| `ck_facilities_cancellation_policy` | `jsonb_typeof(cancellation_policy) = 'array'` |
| `ck_facilities_phone` | `phone_number IS NULL OR length(btrim(phone_number)) > 0` |
| `ck_facilities_location_pair` | `latitude` và `longitude` cùng NULL hoặc cùng NOT NULL |
| `ck_facilities_latitude` | `latitude IS NULL OR latitude BETWEEN -90 AND 90` |
| `ck_facilities_longitude` | `longitude IS NULL OR longitude BETWEEN -180 AND 180` |
| `ck_facilities_review_time` | `reviewed_at IS NULL OR (submitted_at IS NOT NULL AND reviewed_at >= submitted_at)` |
| `ck_facilities_rejection` | `status = 'REJECTED'`: `submitted_at`, `reviewed_at`, `rejection_reason` NOT NULL; trạng thái khác: `rejection_reason` NULL |
| `ck_facilities_submission` | `DRAFT`: `submitted_at`, `reviewed_at` NULL; `PENDING_APPROVAL`: `submitted_at` NOT NULL, `reviewed_at` NULL; `ACTIVE/REJECTED/INACTIVE`: `submitted_at`, `reviewed_at` NOT NULL |

### Indexes

| Name | Definition |
|---|---|
| `ix_facilities_owner_status_created_at` | (`owner_profile_id`, `status`, `created_at` DESC) WHERE `is_deleted = false` |
| `ix_facilities_status_created_at` | (`status`, `created_at` DESC) WHERE `is_deleted = false` |
| `ix_facilities_active_area` | (`province`, `district`) WHERE `status = 'ACTIVE' AND is_deleted = false` |
| `ix_facilities_active_location` | (`latitude`, `longitude`) WHERE `status = 'ACTIVE' AND latitude IS NOT NULL AND is_deleted = false` |

### Cấu trúc `cancellation_policy`

Mảng các mốc hoàn tiền, sắp xếp giảm dần theo `min_hours_before`. Khi hủy, lấy mốc đầu tiên thỏa `số giờ còn lại trước start_at sớm nhất >= min_hours_before`; không thỏa mốc nào thì hoàn `0%`.

| JSON field | JSON type | Ý nghĩa |
|---|---|---|
| `min_hours_before` | `number` | Hủy trước giờ chơi ít nhất bao nhiêu giờ |
| `refund_percent` | `number` | Phần trăm hoàn tiền, `0..100` |

```json
[
  { "min_hours_before": 24, "refund_percent": 100 },
  { "min_hours_before": 6, "refund_percent": 50 }
]
```

---

## 2. `facility_images`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `facility_id` | `uuid` | No | FK → `facilities.id`, ON DELETE RESTRICT |
| `image_url` | `varchar(2048)` | No |  |
| `storage_key` | `varchar(500)` | No | `public_id` trên Cloudinary, dùng để xóa file |
| `is_cover` | `boolean` | No | Default `false` |
| `display_order` | `smallint` | No | Default `0`, CHECK `display_order >= 0` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_facility_images_url` | `length(btrim(image_url)) > 0` |
| `ck_facility_images_storage_key` | `length(btrim(storage_key)) > 0` |
| `ck_facility_images_display_order` | `display_order >= 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_facility_images_one_cover` | UNIQUE (`facility_id`) WHERE `is_cover = true AND is_deleted = false` |
| `ux_facility_images_display_order` | UNIQUE (`facility_id`, `display_order`) WHERE `is_deleted = false` |
| `ix_facility_images_facility_order` | (`facility_id`, `is_cover` DESC, `display_order`) WHERE `is_deleted = false` |

---

## 3. `schedule_templates`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `facility_id` | `uuid` | No | FK → `facilities.id`, ON DELETE RESTRICT |
| `name` | `varchar(150)` | No |  |
| `description` | `varchar(1000)` | Yes |  |
| `version` | `integer` | No | Default `1`, CHECK `version > 0` |
| `slot_duration_minutes` | `smallint` | No | CHECK `slot_duration_minutes BETWEEN 5 AND 720` |
| `status` | `varchar(20)` | No | Default `DRAFT`, CHECK `ScheduleTemplateStatus` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_schedule_templates_status` | `status IN ('DRAFT', 'ACTIVE', 'INACTIVE')` |
| `ck_schedule_templates_name` | `length(btrim(name)) > 0` |
| `ck_schedule_templates_version` | `version > 0` |
| `ck_schedule_templates_duration` | `slot_duration_minutes BETWEEN 5 AND 720` |

### Indexes

| Name | Definition |
|---|---|
| `ux_schedule_templates_facility_name_version` | UNIQUE (`facility_id`, `name`, `version`) WHERE `is_deleted = false` |
| `ix_schedule_templates_facility_status` | (`facility_id`, `status`, `created_at` DESC) WHERE `is_deleted = false` |

---

## 4. `template_slots`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `schedule_template_id` | `uuid` | No | FK → `schedule_templates.id`, ON DELETE RESTRICT |
| `day_of_week` | `smallint` | No | CHECK `day_of_week BETWEEN 1 AND 7` |
| `start_time` | `time` | No |  |
| `end_time` | `time` | No |  |
| `price` | `bigint` | No | CHECK `price >= 0` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_template_slots_day` | `day_of_week BETWEEN 1 AND 7` |
| `ck_template_slots_time` | `end_time > start_time` |
| `ck_template_slots_price` | `price >= 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_template_slots_template_day_start` | UNIQUE (`schedule_template_id`, `day_of_week`, `start_time`) WHERE `is_deleted = false` |
| `ix_template_slots_template_day_time` | (`schedule_template_id`, `day_of_week`, `start_time`, `end_time`) INCLUDE (`price`) WHERE `is_deleted = false` |

---

## TemplateSlot generation flow

1. Owner tạo ScheduleTemplate ở trạng thái `DRAFT` và chọn `slot_duration_minutes`.
2. Owner chọn các ngày trong tuần, giờ mở cửa, giờ đóng cửa và một mức giá mặc định.
3. Backend kiểm tra `(close_time - open_time)` chia hết cho `slot_duration_minutes`.
4. Backend sinh hữu hạn các TemplateSlot cho từng `day_of_week`; tất cả nhận giá mặc định ban đầu.
5. Owner có thể chỉnh riêng `price` của từng TemplateSlot; các field cấu trúc thời gian chỉ được sửa khi template còn `DRAFT`.
6. Sau khi kiểm tra không chồng lấn và có ít nhất một TemplateSlot, ScheduleTemplate mới được chuyển sang `ACTIVE`.

Ví dụ `05:00–10:00`, thời lượng `60` phút, giá mặc định `100000` sẽ sinh 5 TemplateSlot cho mỗi ngày được chọn. Không sinh record theo từng ngày trong tương lai.

---

## 5. `courts`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `facility_id` | `uuid` | No | FK → `facilities.id`, ON DELETE RESTRICT |
| `schedule_template_id` | `uuid` | Yes | FK → `schedule_templates.id`, ON DELETE RESTRICT |
| `name` | `varchar(100)` | No |  |
| `description` | `varchar(1000)` | Yes |  |
| `status` | `varchar(20)` | No | Default `INACTIVE`, CHECK `CourtStatus` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_courts_status` | `status IN ('ACTIVE', 'INACTIVE')` |
| `ck_courts_name` | `length(btrim(name)) > 0` |
| `ck_courts_active_template` | `status <> 'ACTIVE' OR schedule_template_id IS NOT NULL` |

### Indexes

| Name | Definition |
|---|---|
| `ux_courts_facility_name` | UNIQUE (`facility_id`, `name`) WHERE `is_deleted = false` |
| `ix_courts_facility_status_name` | (`facility_id`, `status`, `name`) WHERE `is_deleted = false` |
| `ix_courts_schedule_template` | (`schedule_template_id`, `id`) WHERE `schedule_template_id IS NOT NULL AND is_deleted = false` |

---

## 6. `court_slot_blocks`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `court_id` | `uuid` | No | FK → `courts.id`, ON DELETE RESTRICT |
| `template_slot_id` | `uuid` | No | FK → `template_slots.id`, ON DELETE RESTRICT |
| `block_type` | `varchar(20)` | No | CHECK `CourtSlotBlockType` |
| `blocked_date` | `date` | Yes | Bắt buộc với `SPECIFIC_DATE`, NULL với `WEEKLY` |
| `reason` | `varchar(500)` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_court_slot_blocks_type` | `block_type IN ('WEEKLY', 'SPECIFIC_DATE')` |
| `ck_court_slot_blocks_scope` | `WEEKLY`: `blocked_date IS NULL`; `SPECIFIC_DATE`: `blocked_date IS NOT NULL` |
| `ck_court_slot_blocks_reason` | `reason IS NULL OR length(btrim(reason)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_court_slot_blocks_weekly` | UNIQUE (`court_id`, `template_slot_id`) WHERE `block_type = 'WEEKLY' AND is_deleted = false` |
| `ux_court_slot_blocks_specific_date` | UNIQUE (`court_id`, `template_slot_id`, `blocked_date`) WHERE `block_type = 'SPECIFIC_DATE' AND is_deleted = false` |
| `ix_court_slot_blocks_court_date` | (`court_id`, `blocked_date`, `template_slot_id`) WHERE `block_type = 'SPECIFIC_DATE' AND is_deleted = false` |
| `ix_court_slot_blocks_court_weekly` | (`court_id`, `template_slot_id`) WHERE `block_type = 'WEEKLY' AND is_deleted = false` |
| `ix_court_slot_blocks_slot_court` | (`template_slot_id`, `court_id`) WHERE `is_deleted = false` |

---

## 7. `court_slot_groups`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `court_id` | `uuid` | No | FK → `courts.id`, ON DELETE RESTRICT |
| `name` | `varchar(100)` | Yes |  |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_court_slot_groups_name` | `name IS NULL OR length(btrim(name)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ix_court_slot_groups_court_created_at` | (`court_id`, `created_at`) WHERE `is_deleted = false` |

---

## 8. `court_slot_group_items`

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `court_slot_group_id` | `uuid` | No | FK → `court_slot_groups.id`, ON DELETE RESTRICT |
| `template_slot_id` | `uuid` | No | FK → `template_slots.id`, ON DELETE RESTRICT |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Indexes

| Name | Definition |
|---|---|
| `ux_court_slot_group_items_group_slot` | UNIQUE (`court_slot_group_id`, `template_slot_id`) WHERE `is_deleted = false` |
| `ix_court_slot_group_items_slot_group` | (`template_slot_id`, `court_slot_group_id`) WHERE `is_deleted = false` |

---

## 9. `court_price_overrides`

> Giá riêng của một TemplateSlot trên một Court vào một ngày cụ thể (ngày lễ, ngày đặc biệt). Không thay đổi `template_slots.price`.

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `court_id` | `uuid` | No | FK → `courts.id`, ON DELETE RESTRICT |
| `template_slot_id` | `uuid` | No | FK → `template_slots.id`, ON DELETE RESTRICT |
| `override_date` | `date` | No | Ngày áp dụng giá riêng |
| `price` | `bigint` | No | CHECK `price >= 0` |
| `reason` | `varchar(200)` | Yes | Ví dụ: `Lễ 30/4` |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Constraints

| Name | Definition |
|---|---|
| `ck_court_price_overrides_price` | `price >= 0` |
| `ck_court_price_overrides_reason` | `reason IS NULL OR length(btrim(reason)) > 0` |

### Indexes

| Name | Definition |
|---|---|
| `ux_court_price_overrides_court_slot_date` | UNIQUE (`court_id`, `template_slot_id`, `override_date`) WHERE `is_deleted = false` |
| `ix_court_price_overrides_court_date` | (`court_id`, `override_date`) INCLUDE (`template_slot_id`, `price`) WHERE `is_deleted = false` |

---

## 10. `facility_staffs`

> Nhân viên nào được giao quản lý cơ sở nào. Dùng để kiểm tra quyền trên tài nguyên: Staff chỉ thao tác được trên Facility có mặt trong bảng này.

| Field | PostgreSQL type | Nullable | Key / Default / Constraint |
|---|---|---:|---|
| `id` | `uuid` | No | PK, default `gen_random_uuid()` |
| `facility_id` | `uuid` | No | FK → `facilities.id`, ON DELETE RESTRICT |
| `user_id` | `uuid` | No | FK → `users.id`, ON DELETE RESTRICT; nhân viên |
| `assigned_by` | `uuid` | Yes | FK → `users.id`, ON DELETE RESTRICT; owner đã gán (audit) |
| `is_deleted` | `boolean` | No | Default `false` |
| `created_at` | `timestamptz` | No | Default `now()` |
| `updated_at` | `timestamptz` | Yes |  |

### Indexes

| Name | Definition |
|---|---|
| `ux_facility_staffs_facility_user` | UNIQUE (`facility_id`, `user_id`) WHERE `is_deleted = false` |
| `ix_facility_staffs_user` | (`user_id`, `facility_id`) WHERE `is_deleted = false` — các cơ sở của một Staff |

---

## Cross-table invariants

| Name | Rule |
|---|---|
| `facility_owner_authorization` | Chỉ User sở hữu `owner_profile_id` tương ứng hoặc ADMIN được thay đổi dữ liệu setup của Facility |
| `facility_staff_scope` | Staff chỉ thao tác (block/gộp slot, đổi trạng thái Court...) trên Facility có dòng `facility_staffs` khớp `user_id`; `assigned_by` phải là Owner của chính Facility đó. Xem [[user-phan-quyen]] invariant `staff_role_assignment` |
| `facility_activation` | Chỉ ADMIN được chuyển Facility sang `ACTIVE` hoặc `REJECTED`; Facility chỉ `ACTIVE` khi có tọa độ, ít nhất một ảnh cover và ít nhất một Court hợp lệ. `INACTIVE`: owner hoặc admin đều đặt được |
| `court_template_same_facility` | `courts.schedule_template_id` phải thuộc cùng `facility_id` với Court |
| `court_uses_active_template` | Court `ACTIVE` chỉ được trỏ đến ScheduleTemplate `ACTIVE` |
| `template_slot_duration` | Mỗi TemplateSlot phải có thời lượng đúng bằng `schedule_templates.slot_duration_minutes` |
| `template_slots_no_overlap` | Các TemplateSlot đang hoạt động trong cùng ScheduleTemplate và cùng `day_of_week` không được chồng lấn thời gian |
| `active_template_structure_immutable` | Với ScheduleTemplate `ACTIVE`, chỉ được sửa `template_slots.price`; thay đổi `day_of_week`, `start_time`, `end_time`, `slot_duration_minutes` hoặc xóa slot phải tạo version mới |
| `template_archive_guard` | Không chuyển `INACTIVE` hoặc soft-delete ScheduleTemplate đang được Court sử dụng |
| `block_matches_court_template` | `court_slot_blocks.template_slot_id` phải thuộc ScheduleTemplate hiện tại của `court_id` |
| `specific_block_weekday` | Với `SPECIFIC_DATE`, thứ của `blocked_date` phải bằng `template_slots.day_of_week` |
| `group_item_matches_court_template` | Mỗi `court_slot_group_items.template_slot_id` phải thuộc ScheduleTemplate hiện tại của Court chứa group |
| `group_minimum_items` | Mỗi CourtSlotGroup hợp lệ phải có ít nhất 2 CourtSlotGroupItem |
| `group_items_contiguous` | Các TemplateSlot trong một group phải cùng `day_of_week`, không trùng nhau và liên tiếp: `end_time` slot trước bằng `start_time` slot sau |
| `one_group_per_court_slot` | Một TemplateSlot chỉ được nằm trong tối đa một CourtSlotGroup của cùng Court |
| `group_price` | Giá CourtSlotGroup được tính động bằng tổng `template_slots.price` của các item đang hoạt động trong group |
| `block_group_runtime_split` | Nếu một item bị block, group không được render trong phạm vi block; item bị block bị loại, các item còn lại render như slot độc lập. Không xóa cấu hình group |
| `court_template_reassignment` | Khi đổi template của Court, phải xóa/migrate toàn bộ block và group cũ trong cùng database transaction; không đổi nếu gây xung đột với booking tương lai |
| `slot_configuration_atomic_write` | Tạo/sửa/xóa group, group item và block phải nằm trong cùng database transaction |
| `price_override_matches_court_template` | `court_price_overrides.template_slot_id` phải thuộc ScheduleTemplate hiện tại của `court_id` |
| `price_override_weekday` | Thứ của `override_date` phải bằng `template_slots.day_of_week` |
| `price_override_future_only` | Chỉ tạo/sửa override cho ngày `>= hôm nay`; không ảnh hưởng BookingDetail đã tạo vì giá đã được snapshot |
| `group_price_with_override` | Giá group trong một ngày = tổng giá của từng item, mỗi item lấy giá override của ngày đó nếu có, ngược lại lấy `template_slots.price` |
| `cancellation_policy_floor` | Mỗi mốc `refund_percent` trong khoảng `0..100`; chính sách của Owner không được tệ hơn mức sàn hệ thống (hủy trước ≥ 24 giờ hoàn 100%) |
| `booking_window_rule` | Customer chỉ đặt ngày trong khoảng `hôm nay` đến `hôm nay + advance_booking_days`; tổng thời lượng trên mỗi Court trong một Booking `>= min_booking_minutes` |

---

## Availability query path

Khi lấy slot của một Court cho một ngày:

1. Lấy `courts.schedule_template_id`; ngày được xử lý theo múi giờ cố định `Asia/Ho_Chi_Minh`.
2. Lọc `template_slots` bằng `schedule_template_id + day_of_week`.
3. Áp giá `court_price_overrides` của đúng `court_id + override_date` lên các TemplateSlot tương ứng.
4. Lấy block `WEEKLY` và block `SPECIFIC_DATE` khớp đúng ngày đang truy vấn.
5. Với group không chứa item bị block: render group và ẩn các item độc lập.
6. Với group có item bị block: không render group, loại item bị block và render các item còn lại như slot độc lập.
7. Cuối cùng loại các slot/group đã có BookingDetail còn hiệu lực của ngày đó.
8. Kết quả có thể cache theo `court_id + ngày` trong thời gian ngắn; cache chỉ dùng để hiển thị, exclusion constraint của `booking_details` mới quyết định đặt được hay không.

---

## Relationships

| Parent | Relationship | Child | Foreign key |
|---|---|---|---|
| `owner_profiles` | 1 — N | `facilities` | `facilities.owner_profile_id` |
| `facilities` | 1 — N | `facility_images` | `facility_images.facility_id` |
| `facilities` | 1 — N | `schedule_templates` | `schedule_templates.facility_id` |
| `facilities` | 1 — N | `courts` | `courts.facility_id` |
| `schedule_templates` | 1 — N | `template_slots` | `template_slots.schedule_template_id` |
| `schedule_templates` | 1 — N | `courts` | `courts.schedule_template_id` |
| `courts` | 1 — N | `court_slot_blocks` | `court_slot_blocks.court_id` |
| `template_slots` | 1 — N | `court_slot_blocks` | `court_slot_blocks.template_slot_id` |
| `courts` | 1 — N | `court_slot_groups` | `court_slot_groups.court_id` |
| `court_slot_groups` | 1 — N | `court_slot_group_items` | `court_slot_group_items.court_slot_group_id` |
| `template_slots` | 1 — N | `court_slot_group_items` | `court_slot_group_items.template_slot_id` |
| `courts` | 1 — N | `court_price_overrides` | `court_price_overrides.court_id` |
| `template_slots` | 1 — N | `court_price_overrides` | `court_price_overrides.template_slot_id` |
| `facilities` | 1 — N | `facility_staffs` | `facility_staffs.facility_id` |
| `users` | 1 — N | `facility_staffs` | `facility_staffs.user_id` |
