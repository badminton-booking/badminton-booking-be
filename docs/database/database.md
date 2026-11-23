Project rallyhub {
  database_type: 'PostgreSQL'
  Note: 'RallyHub - dat san cau long. 9 cum, 37 bang. Moi bang co BaseEntity: id, is_deleted, created_at, updated_at. Tien: bigint VND. Gio: timestamptz, ngay nghiep vu theo Asia/Ho_Chi_Minh. Tien khach vao thang tai khoan Owner (SePay OAuth); san chi ghi nhan hoa hong Owner no san.'
}

// ========================= ENUMS =========================

Enum UserStatus {
  NOT_VERIFIED
  VERIFIED
  BANNED
}
Enum RoleKey {
  CUSTOMER
  OWNER
  STAFF
  ADMIN
}
Enum ExternalLoginProvider {
  GOOGLE
}
Enum OwnerRequestStatus {
  PENDING
  APPROVED
  REJECTED
  CANCELLED
}

Enum FacilityStatus {
  DRAFT
  PENDING_APPROVAL
  ACTIVE
  REJECTED
  INACTIVE
}
Enum CourtStatus {
  ACTIVE
  INACTIVE
}
Enum ScheduleTemplateStatus {
  DRAFT
  ACTIVE
  INACTIVE
}
Enum CourtSlotBlockType {
  WEEKLY
  SPECIFIC_DATE
}

Enum BookingStatus {
  PENDING_PAYMENT
  CONFIRMED
  CANCELLED
  COMPLETED
  EXPIRED
}
Enum BookingDetailStatus {
  PENDING_PAYMENT
  CONFIRMED
  CANCELLED
  COMPLETED
  EXPIRED
}

Enum CampaignStatus {
  DRAFT
  ACTIVE
  PAUSED
  ENDED
}
Enum CampaignDiscountType {
  PERCENTAGE
  FIXED_AMOUNT
}
Enum CampaignEligibility {
  ALL
  FIRST_BOOKING
}
Enum CampaignTargetType {
  GLOBAL
  OWNER
  FACILITY
  COURT
  TEMPLATE_SLOT
}
Enum CampaignCodeVisibility {
  PUBLIC
  PRIVATE
}
Enum CampaignCodeStatus {
  ACTIVE
  DISABLED
}

Enum BankTransactionDirection {
  IN
  OUT
}
Enum BankTransactionMatchState {
  PENDING
  MATCHED_BOOKING
  MATCHED_SETTLEMENT
  UNMATCHED
  IGNORED
}
Enum CommissionStatus {
  PENDING
  SETTLED
}
Enum SettlementStatus {
  PENDING
  PAID
  OVERDUE
}

Enum FeedbackType {
  COURT
  PLATFORM
}

Enum ReportStatus {
  OPEN
  IN_PROGRESS
  RESOLVED
  REJECTED
  CANCELLED
}
Enum ReportTargetType {
  GENERAL
  USER
  FACILITY
  COURT
  BOOKING
  FEEDBACK
}

Enum ConfigScope {
  PLATFORM
  FACILITY
}
Enum NotificationType {
  BOOKING_CONFIRMED
  BOOKING_CANCELLED
  BOOKING_EXPIRED
  BOOKING_COMPLETED
  SETTLEMENT_DUE
  SETTLEMENT_PAID
  SETTLEMENT_OVERDUE
  OWNER_REQUEST_REVIEWED
  FACILITY_REVIEWED
  REPORT_MESSAGE
  FEEDBACK_REPLY
}
// ===================== CUM 1: USER / PHAN QUYEN =====================

Table users {
  id uuid [pk]
  email varchar(320) [not null, unique]
  password_hash varchar(512)
  status UserStatus [not null, default: 'NOT_VERIFIED']
  email_verified_at timestamptz
  ban_reason varchar(1000)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table user_profiles {
  id uuid [pk]
  user_id uuid [not null, unique, ref: > users.id]
  username varchar(50)
  full_name varchar(150) [not null]
  phone_number varchar(20)
  avatar_url varchar(2048)
  bio varchar(500)
  preferred_locale varchar(10) [not null, default: 'vi']
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table roles {
  id uuid [pk]
  role_key RoleKey [not null, unique]
  name varchar(100) [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table permissions {
  id uuid [pk]
  code varchar(100) [not null, unique]
  description varchar(300) [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table role_permissions {
  id uuid [pk]
  role_id uuid [not null, ref: > roles.id]
  permission_id uuid [not null, ref: > permissions.id]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (role_id, permission_id) [unique]
  }
}

Table user_roles {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  role_id uuid [not null, ref: > roles.id]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (user_id, role_id) [unique]
  }
}

Table refresh_tokens {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  token_hash char(64) [not null, unique]
  family_id uuid [not null]
  expires_at timestamptz [not null]
  revoked_at timestamptz
  replaced_by_token_id uuid [ref: > refresh_tokens.id]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table user_external_logins {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  provider ExternalLoginProvider [not null]
  provider_subject varchar(255) [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (provider, provider_subject) [unique]
    (user_id, provider) [unique]
  }
}

Table owner_requests {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  status OwnerRequestStatus [not null, default: 'PENDING']
  business_name varchar(200) [not null]
  legal_representative_name varchar(150) [not null]
  tax_code varchar(30)
  business_license_number varchar(100)
  business_license_image_url varchar(2048)
  identity_front_image_url varchar(2048) [not null]
  identity_back_image_url varchar(2048) [not null]
  request_note varchar(1000)
  reviewed_at timestamptz
  rejection_reason varchar(1000)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table owner_profiles {
  id uuid [pk]
  user_id uuid [not null, unique, ref: > users.id]
  approved_owner_request_id uuid [not null, unique, ref: > owner_requests.id]
  business_name varchar(200) [not null]
  description varchar(2000)
  logo_url varchar(2048)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table user_bank_accounts {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  bank_code varchar(20) [not null]
  account_number_encrypted text [not null]
  account_number_hash char(64) [not null]
  account_holder_name varchar(150) [not null]
  is_default boolean [not null, default: false]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

// ===================== CUM 2: SETUP SAN =====================

Table facilities {
  id uuid [pk]
  owner_profile_id uuid [not null, ref: > owner_profiles.id]
  name varchar(200) [not null]
  description varchar(2000)
  phone_number varchar(20)
  address_line varchar(500) [not null]
  latitude "numeric(9,6)"
  longitude "numeric(9,6)"
  status FacilityStatus [not null, default: 'DRAFT']
  submitted_at timestamptz
  reviewed_at timestamptz
  rejection_reason varchar(1000)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table facility_images {
  id uuid [pk]
  facility_id uuid [not null, ref: > facilities.id]
  image_url varchar(2048) [not null]
  is_cover boolean [not null, default: false]
  display_order smallint [not null, default: 0]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table schedule_templates {
  id uuid [pk]
  facility_id uuid [not null, ref: > facilities.id]
  name varchar(150) [not null]
  description varchar(1000)
  version integer [not null, default: 1]
  slot_duration_minutes smallint [not null]
  status ScheduleTemplateStatus [not null, default: 'DRAFT']
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table template_slots {
  id uuid [pk]
  schedule_template_id uuid [not null, ref: > schedule_templates.id]
  day_of_week smallint [not null, note: '1=MON..7=SUN']
  start_time time [not null]
  end_time time [not null]
  price bigint [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table courts {
  id uuid [pk]
  facility_id uuid [not null, ref: > facilities.id]
  schedule_template_id uuid [ref: > schedule_templates.id]
  name varchar(100) [not null]
  description varchar(1000)
  status CourtStatus [not null, default: 'INACTIVE']
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table court_slot_blocks {
  id uuid [pk]
  template_slot_id uuid [not null, ref: > template_slots.id]
  court_id uuid [not null, ref: > courts.id]
  
  block_type CourtSlotBlockType [not null]
  blocked_date date
  reason varchar(500)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table court_slot_groups {
  id uuid [pk]
  court_id uuid [not null, ref: > courts.id]
  name varchar(100)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table court_slot_group_items {
  id uuid [pk]
  template_slot_id uuid [not null, ref: > template_slots.id]
  court_slot_group_id uuid [not null, ref: > court_slot_groups.id]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (court_slot_group_id, template_slot_id) [unique]
  }
}

Table court_price_overrides {
  id uuid [pk]
  court_id uuid [not null, ref: > courts.id]
  template_slot_id uuid [not null, ref: > template_slots.id]
  override_date date [not null]
  price bigint [not null]
  reason varchar(200)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (court_id, template_slot_id, override_date) [unique]
  }
}

Table facility_staffs {
  id uuid [pk]
  facility_id uuid [not null, ref: > facilities.id]
  user_id uuid [not null, ref: > users.id]
  assigned_by uuid [ref: > users.id]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (facility_id, user_id) [unique]
  }
}

// ===================== CUM 3: BOOKING =====================

Table bookings {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  facility_id uuid [not null, ref: > facilities.id]
  booking_code varchar(30) [not null, unique]
  campaign_code_id uuid [ref: > campaign_codes.id]
  status BookingStatus [not null, default: 'PENDING_PAYMENT']
  subtotal_amount bigint [not null]
  discount_amount bigint [not null, default: 0]
  total_amount bigint [not null]
  campaign_snapshot jsonb
  contact_name varchar(150) [not null]
  contact_phone varchar(20) [not null]
  payment_qr_url varchar(2048)
  payment_expires_at timestamptz
  note varchar(1000)
  cancelled_at timestamptz
  cancelled_by_user_id uuid [ref: > users.id]
  cancellation_reason varchar(1000)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table booking_details {
  id uuid [pk]
  booking_id uuid [not null, ref: > bookings.id]
  court_id uuid [not null, ref: > courts.id]
  status BookingDetailStatus [not null, default: 'PENDING_PAYMENT']
  start_at timestamptz [not null]
  end_at timestamptz [not null]
  original_amount bigint [not null]
  discount_amount bigint [not null, default: 0]
  final_amount bigint [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
  Note: 'EXCLUDE gist (court_id, tstzrange(start_at,end_at)) WHERE status IN (PENDING_PAYMENT,CONFIRMED) - chong trung lich'
}

// ===================== CUM 4: CAMPAIGN =====================

Table campaigns {
  id uuid [pk]
  owner_profile_id uuid [ref: > owner_profiles.id]
  created_by_user_id uuid [not null, ref: > users.id]
  
  name varchar(150) [not null]
  description varchar(1000)
  discount_type CampaignDiscountType [not null]
  discount_percentage "numeric(5,2)"
  discount_amount bigint
  min_order_amount bigint [not null, default: 0]
  max_order_amount bigint
  min_discount_amount bigint
  max_discount_amount bigint
  eligibility CampaignEligibility [not null, default: 'ALL']
  starts_at timestamptz [not null]
  ends_at timestamptz [not null]
  status CampaignStatus [not null, default: 'DRAFT']
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table campaign_codes {
  id uuid [pk]
  campaign_id uuid [not null, ref: > campaigns.id]
  code varchar(50) [not null, unique]
  visibility CampaignCodeVisibility [not null]
  status CampaignCodeStatus [not null, default: 'ACTIVE']
  usage_limit integer
  per_user_limit integer
  used_count integer [not null, default: 0]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table campaign_targets {
  id uuid [pk]
  campaign_id uuid [not null, ref: > campaigns.id]
  target_type CampaignTargetType [not null]
  target_id uuid [note: 'polymorphic, khong FK vat ly']
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

// ===================== CUM 5: GIAO DICH / HOA HONG =====================

Table owner_payment_accounts {
  id uuid [pk]
  owner_profile_id uuid [not null, ref: > owner_profiles.id]
  provider varchar(50) [not null, default: 'SEPAY']
  account_number varchar(50) [not null]
  account_holder varchar(150) [not null]
  bank_code varchar(50)
  account_ref varchar(255) [not null]
  is_active boolean [not null, default: true]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table bank_transactions {
  id uuid [pk]
  provider varchar(50) [not null]
  provider_reference varchar(255) [not null]
  direction BankTransactionDirection [not null]
  amount bigint [not null]
  account_number varchar(50) [not null]
  transfer_content varchar(500)
  occurred_at timestamptz [not null]
  match_state BankTransactionMatchState [not null, default: 'PENDING']
  booking_id uuid [ref: > bookings.id]
  commission_settlement_id uuid [ref: > commission_settlements.id]
  processed_at timestamptz
  last_error varchar(500)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (provider, provider_reference) [unique]
  }
}

Table commissions {
  id uuid [pk]
  booking_id uuid [not null, unique, ref: > bookings.id]
  owner_profile_id uuid [not null, ref: > owner_profiles.id]
  facility_id uuid [not null, ref: > facilities.id]
  gross_amount bigint [not null]
  commission_rate "numeric(5,2)" [not null]
  commission_amount bigint [not null]
  platform_subsidy bigint [not null, default: 0]
  net_payable bigint [not null]
  status CommissionStatus [not null, default: 'PENDING']
  commission_settlement_id uuid [ref: > commission_settlements.id]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table commission_settlements {
  id uuid [pk]
  owner_profile_id uuid [not null, ref: > owner_profiles.id]
  settlement_code varchar(30) [not null, unique]
  period_start date [not null]
  period_end date [not null]
  total_amount bigint [not null]
  status SettlementStatus [not null, default: 'PENDING']
  due_date date
  paid_at timestamptz
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

// ===================== CUM 6: FEEDBACK =====================

Table feedbacks {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  court_id uuid [ref: > courts.id]
  booking_detail_id uuid [ref: > booking_details.id]
  type FeedbackType [not null]
  rating smallint [not null, note: '1..5']
  content varchar(2000) [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (booking_detail_id) [unique, note: 'chi loai COURT']
  }
}

Table feedback_replies {
  id uuid [pk]
  feedback_id uuid [not null, ref: > feedbacks.id]
  user_id uuid [not null, ref: > users.id]
  content varchar(2000) [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

// ===================== CUM 7: REPORT =====================

Table reports {
  id uuid [pk]
  reporter_user_id uuid [not null, ref: > users.id]
  handled_by_user_id uuid [ref: > users.id]
  target_type ReportTargetType [not null]
  target_id uuid [note: 'polymorphic, khong FK vat ly']
  subject varchar(200) [not null]
  description varchar(4000) [not null]
  status ReportStatus [not null, default: 'OPEN']
  last_message_at timestamptz [not null]
  closed_at timestamptz
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table report_messages {
  id uuid [pk]
  report_id uuid [not null, ref: > reports.id]
  sender_user_id uuid [not null, ref: > users.id]
  content varchar(4000) [not null]
  read_at timestamptz
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

// ===================== CUM 8: YEU THICH =====================

Table favorite_facilities {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  facility_id uuid [not null, ref: > facilities.id]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Indexes {
    (user_id, facility_id) [unique]
  }
}

// ===================== CUM 9: HE THONG =====================

Table configs {
  id uuid [pk]
  scope ConfigScope [not null]
  facility_id uuid [ref: > facilities.id]
  key varchar(100) [not null]
  value jsonb [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}

Table notifications {
  id uuid [pk]
  user_id uuid [not null, ref: > users.id]
  type NotificationType [not null]
  title varchar(200) [not null]
  content varchar(1000) [not null]
  data jsonb
  read_at timestamptz
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz
}
