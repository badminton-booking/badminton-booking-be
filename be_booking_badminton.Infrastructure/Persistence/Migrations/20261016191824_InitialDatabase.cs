using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace be_booking_badminton.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role_key = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                    table.CheckConstraint("ck_roles_name_not_blank", "length(btrim(name)) > 0");
                    table.CheckConstraint("ck_roles_role_key", "role_key IN ('CUSTOMER', 'OWNER', 'ADMIN')");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDING_VERIFICATION"),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_email_format", "position('@' IN email) > 1");
                    table.CheckConstraint("ck_users_email_normalized", "email = lower(btrim(email))");
                    table.CheckConstraint("ck_users_status", "status IN ('PENDING_VERIFICATION', 'ACTIVE', 'SUSPENDED', 'DISABLED')");
                });

            migrationBuilder.CreateTable(
                name: "owner_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    business_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    legal_representative_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    tax_code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    business_license_number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    business_license_image_url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    identity_front_image_url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false),
                    identity_back_image_url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false),
                    request_note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    rejection_reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_owner_requests", x => x.id);
                    table.CheckConstraint("ck_owner_requests_business_license", "(business_license_number IS NULL) = (business_license_image_url IS NULL)");
                    table.CheckConstraint("ck_owner_requests_business_name", "length(btrim(business_name)) > 0");
                    table.CheckConstraint("ck_owner_requests_legal_name", "length(btrim(legal_representative_name)) > 0");
                    table.CheckConstraint("ck_owner_requests_review_state", "(status IN ('PENDING', 'CANCELLED') AND reviewed_at IS NULL AND rejection_reason IS NULL) OR (status = 'APPROVED' AND reviewed_at IS NOT NULL AND rejection_reason IS NULL) OR (status = 'REJECTED' AND reviewed_at IS NOT NULL AND rejection_reason IS NOT NULL)");
                    table.CheckConstraint("ck_owner_requests_status", "status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED')");
                    table.ForeignKey(
                        name: "fk_owner_requests_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "char(64)", maxLength: 64, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.CheckConstraint("ck_refresh_tokens_expiration", "expires_at > created_at");
                    table.CheckConstraint("ck_refresh_tokens_no_self_reference", "replaced_by_token_id IS NULL OR replaced_by_token_id <> id");
                    table.CheckConstraint("ck_refresh_tokens_replacement", "replaced_by_token_id IS NULL OR revoked_at IS NOT NULL");
                    table.CheckConstraint("ck_refresh_tokens_revoked_at", "revoked_at IS NULL OR revoked_at >= created_at");
                    table.ForeignKey(
                        name: "fk_refresh_tokens_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalTable: "refresh_tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reporter_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "OPEN"),
                    last_message_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.CheckConstraint("ck_reports_closed_at", "(status IN ('RESOLVED', 'REJECTED', 'CANCELLED') AND closed_at IS NOT NULL) OR (status IN ('OPEN', 'IN_PROGRESS') AND closed_at IS NULL)");
                    table.CheckConstraint("ck_reports_closed_time", "closed_at IS NULL OR closed_at >= created_at");
                    table.CheckConstraint("ck_reports_description", "length(btrim(description)) > 0");
                    table.CheckConstraint("ck_reports_last_message_time", "last_message_at >= created_at");
                    table.CheckConstraint("ck_reports_status", "status IN ('OPEN', 'IN_PROGRESS', 'RESOLVED', 'REJECTED', 'CANCELLED')");
                    table.CheckConstraint("ck_reports_subject", "length(btrim(subject)) > 0");
                    table.CheckConstraint("ck_reports_target", "(target_type = 'GENERAL' AND target_id IS NULL) OR (target_type <> 'GENERAL' AND target_id IS NOT NULL)");
                    table.CheckConstraint("ck_reports_target_type", "target_type IN ('GENERAL', 'USER', 'FACILITY', 'COURT', 'BOOKING', 'FEEDBACK')");
                    table.ForeignKey(
                        name: "fk_reports_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    account_number_encrypted = table.Column<string>(type: "text", nullable: false),
                    account_number_hash = table.Column<string>(type: "char(64)", maxLength: 64, nullable: false),
                    account_holder_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_bank_accounts", x => x.id);
                    table.CheckConstraint("ck_user_bank_accounts_bank_code", "bank_code = upper(btrim(bank_code)) AND length(bank_code) > 0");
                    table.CheckConstraint("ck_user_bank_accounts_holder_name", "length(btrim(account_holder_name)) > 0");
                    table.CheckConstraint("ck_user_bank_accounts_number_encrypted", "length(btrim(account_number_encrypted)) > 0");
                    table.CheckConstraint("ck_user_bank_accounts_number_hash", "account_number_hash ~ '^[0-9a-f]{64}$'");
                    table.ForeignKey(
                        name: "fk_user_bank_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    full_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    avatar_url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.id);
                    table.CheckConstraint("ck_user_profiles_full_name_not_blank", "length(btrim(full_name)) > 0");
                    table.CheckConstraint("ck_user_profiles_phone_not_blank", "phone_number IS NULL OR length(btrim(phone_number)) > 0");
                    table.CheckConstraint("ck_user_profiles_username_normalized", "username IS NULL OR username = lower(btrim(username))");
                    table.CheckConstraint("ck_user_profiles_username_not_blank", "username IS NULL OR length(username) >= 3");
                    table.ForeignKey(
                        name: "fk_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_roles_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "owner_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_owner_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    logo_url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    suspended_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    suspension_reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_owner_profiles", x => x.id);
                    table.CheckConstraint("ck_owner_profiles_business_name", "length(btrim(business_name)) > 0");
                    table.CheckConstraint("ck_owner_profiles_status", "status IN ('ACTIVE', 'SUSPENDED', 'DISABLED')");
                    table.CheckConstraint("ck_owner_profiles_suspension", "(status = 'SUSPENDED' AND suspended_at IS NOT NULL AND suspension_reason IS NOT NULL) OR (status IN ('ACTIVE', 'DISABLED') AND suspended_at IS NULL AND suspension_reason IS NULL)");
                    table.ForeignKey(
                        name: "fk_owner_profiles_approved_owner_request_id",
                        column: x => x.approved_owner_request_id,
                        principalTable: "owner_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_owner_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_messages", x => x.id);
                    table.CheckConstraint("ck_report_messages_content", "length(btrim(content)) > 0");
                    table.CheckConstraint("ck_report_messages_read_time", "read_at IS NULL OR read_at >= created_at");
                    table.ForeignKey(
                        name: "fk_report_messages_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_report_messages_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    discount_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    discount_amount = table.Column<long>(type: "bigint", nullable: true),
                    min_order_amount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    max_order_amount = table.Column<long>(type: "bigint", nullable: true),
                    min_discount_amount = table.Column<long>(type: "bigint", nullable: true),
                    max_discount_amount = table.Column<long>(type: "bigint", nullable: true),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaigns", x => x.id);
                    table.CheckConstraint("ck_campaigns_discount_amount_range", "(min_discount_amount IS NULL OR min_discount_amount >= 0) AND (max_discount_amount IS NULL OR max_discount_amount > 0) AND (min_discount_amount IS NULL OR max_discount_amount IS NULL OR max_discount_amount >= min_discount_amount)");
                    table.CheckConstraint("ck_campaigns_discount_type", "discount_type IN ('PERCENTAGE', 'FIXED_AMOUNT')");
                    table.CheckConstraint("ck_campaigns_discount_value", "(discount_type = 'PERCENTAGE' AND discount_percentage IS NOT NULL AND discount_percentage > 0 AND discount_percentage <= 100 AND discount_amount IS NULL) OR (discount_type = 'FIXED_AMOUNT' AND discount_amount IS NOT NULL AND discount_amount > 0 AND discount_percentage IS NULL)");
                    table.CheckConstraint("ck_campaigns_name", "length(btrim(name)) > 0");
                    table.CheckConstraint("ck_campaigns_order_amount", "min_order_amount >= 0 AND (max_order_amount IS NULL OR max_order_amount >= min_order_amount)");
                    table.CheckConstraint("ck_campaigns_period", "ends_at > starts_at");
                    table.CheckConstraint("ck_campaigns_status", "status IN ('DRAFT', 'ACTIVE', 'PAUSED', 'ENDED')");
                    table.ForeignKey(
                        name: "fk_campaigns_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_campaigns_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    address_line = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    rejection_reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    suspended_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    suspension_reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facilities", x => x.id);
                    table.CheckConstraint("ck_facilities_address", "length(btrim(address_line)) > 0");
                    table.CheckConstraint("ck_facilities_latitude", "latitude IS NULL OR latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_facilities_location_pair", "(latitude IS NULL) = (longitude IS NULL)");
                    table.CheckConstraint("ck_facilities_longitude", "longitude IS NULL OR longitude BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_facilities_name", "length(btrim(name)) > 0");
                    table.CheckConstraint("ck_facilities_phone", "phone_number IS NULL OR length(btrim(phone_number)) > 0");
                    table.CheckConstraint("ck_facilities_rejection", "(status = 'REJECTED' AND submitted_at IS NOT NULL AND reviewed_at IS NOT NULL AND rejection_reason IS NOT NULL) OR (status <> 'REJECTED' AND rejection_reason IS NULL)");
                    table.CheckConstraint("ck_facilities_review_time", "reviewed_at IS NULL OR (submitted_at IS NOT NULL AND reviewed_at >= submitted_at)");
                    table.CheckConstraint("ck_facilities_status", "status IN ('DRAFT', 'PENDING_APPROVAL', 'ACTIVE', 'REJECTED', 'SUSPENDED', 'INACTIVE')");
                    table.CheckConstraint("ck_facilities_submission", "(status = 'DRAFT' AND submitted_at IS NULL AND reviewed_at IS NULL) OR (status = 'PENDING_APPROVAL' AND submitted_at IS NOT NULL AND reviewed_at IS NULL) OR (status IN ('ACTIVE', 'REJECTED', 'SUSPENDED', 'INACTIVE') AND submitted_at IS NOT NULL AND reviewed_at IS NOT NULL)");
                    table.CheckConstraint("ck_facilities_suspension", "(status = 'SUSPENDED' AND suspended_at IS NOT NULL AND suspension_reason IS NOT NULL) OR (status <> 'SUSPENDED' AND suspended_at IS NULL AND suspension_reason IS NULL)");
                    table.ForeignKey(
                        name: "fk_facilities_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ledger_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    account_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    balance = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger_accounts", x => x.id);
                    table.CheckConstraint("ck_ledger_accounts_owner", "(account_type = 'OWNER_PAYABLE' AND owner_profile_id IS NOT NULL) OR (account_type IN ('PLATFORM_CASH', 'PLATFORM_REVENUE') AND owner_profile_id IS NULL)");
                    table.CheckConstraint("ck_ledger_accounts_type", "account_type IN ('PLATFORM_CASH', 'PLATFORM_REVENUE', 'OWNER_PAYABLE')");
                    table.ForeignKey(
                        name: "fk_ledger_accounts_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaign_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    visibility = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    usage_limit = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_codes", x => x.id);
                    table.CheckConstraint("ck_campaign_codes_code", "code = upper(btrim(code)) AND code ~ '^[A-Z0-9_-]{3,50}$'");
                    table.CheckConstraint("ck_campaign_codes_status", "status IN ('ACTIVE', 'DISABLED')");
                    table.CheckConstraint("ck_campaign_codes_usage_limit", "usage_limit IS NULL OR usage_limit > 0");
                    table.CheckConstraint("ck_campaign_codes_used_count", "used_count >= 0 AND (usage_limit IS NULL OR used_count <= usage_limit)");
                    table.CheckConstraint("ck_campaign_codes_visibility", "visibility IN ('PUBLIC', 'PRIVATE')");
                    table.ForeignKey(
                        name: "fk_campaign_codes_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaign_targets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_targets", x => x.id);
                    table.CheckConstraint("ck_campaign_targets_target", "(target_type = 'GLOBAL' AND target_id IS NULL) OR (target_type <> 'GLOBAL' AND target_id IS NOT NULL)");
                    table.CheckConstraint("ck_campaign_targets_type", "target_type IN ('GLOBAL', 'OWNER', 'FACILITY', 'COURT', 'TEMPLATE_SLOT')");
                    table.ForeignKey(
                        name: "fk_campaign_targets_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facility_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_order = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facility_images", x => x.id);
                    table.CheckConstraint("ck_facility_images_display_order", "display_order >= 0");
                    table.CheckConstraint("ck_facility_images_url", "length(btrim(image_url)) > 0");
                    table.ForeignKey(
                        name: "fk_facility_images_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "schedule_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    slot_duration_minutes = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_templates", x => x.id);
                    table.CheckConstraint("ck_schedule_templates_duration", "slot_duration_minutes BETWEEN 5 AND 720");
                    table.CheckConstraint("ck_schedule_templates_name", "length(btrim(name)) > 0");
                    table.CheckConstraint("ck_schedule_templates_status", "status IN ('DRAFT', 'ACTIVE', 'ARCHIVED')");
                    table.CheckConstraint("ck_schedule_templates_version", "version > 0");
                    table.ForeignKey(
                        name: "fk_schedule_templates_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    campaign_code_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDING_PAYMENT"),
                    subtotal_amount = table.Column<long>(type: "bigint", nullable: false),
                    discount_amount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    total_amount = table.Column<long>(type: "bigint", nullable: false),
                    campaign_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    payment_qr_url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    payment_expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    cancellation_reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.CheckConstraint("ck_bookings_amounts", "subtotal_amount >= 0 AND discount_amount >= 0 AND discount_amount <= subtotal_amount AND total_amount = subtotal_amount - discount_amount");
                    table.CheckConstraint("ck_bookings_campaign", "(campaign_code_id IS NULL AND campaign_snapshot IS NULL AND discount_amount = 0) OR (campaign_code_id IS NOT NULL AND campaign_snapshot IS NOT NULL AND jsonb_typeof(campaign_snapshot) = 'object' AND discount_amount > 0)");
                    table.CheckConstraint("ck_bookings_campaign_snapshot_keys", "campaign_snapshot IS NULL OR (campaign_snapshot ?& ARRAY['code', 'discount_type', 'discount_percentage', 'discount_fixed_amount'] AND jsonb_typeof(campaign_snapshot->'code') = 'string' AND jsonb_typeof(campaign_snapshot->'discount_type') = 'string')");
                    table.CheckConstraint("ck_bookings_cancellation", "(status = 'CANCELLED' AND cancelled_at IS NOT NULL AND cancellation_reason IS NOT NULL) OR (status <> 'CANCELLED' AND cancelled_at IS NULL AND cancellation_reason IS NULL)");
                    table.CheckConstraint("ck_bookings_cancelled_time", "cancelled_at IS NULL OR cancelled_at >= created_at");
                    table.CheckConstraint("ck_bookings_code", "booking_code = upper(btrim(booking_code)) AND booking_code ~ '^[A-Z0-9_-]{6,30}$'");
                    table.CheckConstraint("ck_bookings_discount_snapshot", "campaign_snapshot IS NULL OR (CASE WHEN campaign_snapshot->>'discount_type' = 'PERCENTAGE' AND jsonb_typeof(campaign_snapshot->'discount_percentage') = 'number' THEN (campaign_snapshot->>'discount_percentage')::numeric > 0 AND (campaign_snapshot->>'discount_percentage')::numeric <= 100 AND campaign_snapshot->'discount_fixed_amount' = 'null'::jsonb WHEN campaign_snapshot->>'discount_type' = 'FIXED_AMOUNT' AND jsonb_typeof(campaign_snapshot->'discount_fixed_amount') = 'number' THEN (campaign_snapshot->>'discount_fixed_amount')::numeric > 0 AND campaign_snapshot->'discount_percentage' = 'null'::jsonb ELSE false END) IS TRUE");
                    table.CheckConstraint("ck_bookings_note", "note IS NULL OR length(btrim(note)) > 0");
                    table.CheckConstraint("ck_bookings_payment_data", "(total_amount > 0 AND payment_qr_url IS NOT NULL AND payment_expires_at IS NOT NULL) OR (total_amount = 0 AND payment_qr_url IS NULL AND payment_expires_at IS NULL)");
                    table.CheckConstraint("ck_bookings_payment_expiration", "payment_expires_at IS NULL OR payment_expires_at > created_at");
                    table.CheckConstraint("ck_bookings_status", "status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'EXPIRED')");
                    table.ForeignKey(
                        name: "fk_bookings_campaign_code_id",
                        column: x => x.campaign_code_id,
                        principalTable: "campaign_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "courts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "INACTIVE"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courts", x => x.id);
                    table.CheckConstraint("ck_courts_active_template", "status <> 'ACTIVE' OR schedule_template_id IS NOT NULL");
                    table.CheckConstraint("ck_courts_name", "length(btrim(name)) > 0");
                    table.CheckConstraint("ck_courts_status", "status IN ('ACTIVE', 'INACTIVE', 'MAINTENANCE')");
                    table.ForeignKey(
                        name: "fk_courts_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_courts_schedule_template_id",
                        column: x => x.schedule_template_id,
                        principalTable: "schedule_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "template_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    schedule_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<short>(type: "smallint", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template_slots", x => x.id);
                    table.CheckConstraint("ck_template_slots_day", "day_of_week BETWEEN 1 AND 7");
                    table.CheckConstraint("ck_template_slots_price", "price >= 0");
                    table.CheckConstraint("ck_template_slots_time", "end_time > start_time");
                    table.ForeignKey(
                        name: "fk_template_slots_schedule_template_id",
                        column: x => x.schedule_template_id,
                        principalTable: "schedule_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financial_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_bank_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parent_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    provider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    provider_reference = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_financial_transactions", x => x.id);
                    table.CheckConstraint("ck_financial_transactions_amount", "amount > 0");
                    table.CheckConstraint("ck_financial_transactions_no_self_parent", "parent_transaction_id IS NULL OR parent_transaction_id <> id");
                    table.CheckConstraint("ck_financial_transactions_provider", "(provider IS NULL) = (provider_reference IS NULL)");
                    table.CheckConstraint("ck_financial_transactions_target", "(type = 'PAYMENT' AND booking_id IS NOT NULL AND owner_profile_id IS NULL AND destination_bank_account_id IS NULL AND parent_transaction_id IS NULL) OR (type = 'REFUND' AND booking_id IS NOT NULL AND parent_transaction_id IS NOT NULL AND destination_bank_account_id IS NOT NULL AND owner_profile_id IS NULL) OR (type = 'PAYOUT' AND owner_profile_id IS NOT NULL AND destination_bank_account_id IS NOT NULL AND booking_id IS NULL AND parent_transaction_id IS NULL)");
                    table.CheckConstraint("ck_financial_transactions_type", "type IN ('PAYMENT', 'REFUND', 'PAYOUT')");
                    table.ForeignKey(
                        name: "fk_financial_transactions_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_financial_transactions_destination_bank_account_id",
                        column: x => x.destination_bank_account_id,
                        principalTable: "user_bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_financial_transactions_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_financial_transactions_parent_transaction_id",
                        column: x => x.parent_transaction_id,
                        principalTable: "financial_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDING_PAYMENT"),
                    start_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    original_amount = table.Column<long>(type: "bigint", nullable: false),
                    discount_amount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    final_amount = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_details", x => x.id);
                    table.CheckConstraint("ck_booking_details_amounts", "original_amount >= 0 AND discount_amount >= 0 AND discount_amount <= original_amount AND final_amount = original_amount - discount_amount");
                    table.CheckConstraint("ck_booking_details_status", "status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'EXPIRED')");
                    table.CheckConstraint("ck_booking_details_time", "end_at > start_at");
                    table.ForeignKey(
                        name: "fk_booking_details_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_booking_details_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "court_slot_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_court_slot_groups", x => x.id);
                    table.CheckConstraint("ck_court_slot_groups_name", "name IS NULL OR length(btrim(name)) > 0");
                    table.ForeignKey(
                        name: "fk_court_slot_groups_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "favorite_courts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorite_courts", x => x.id);
                    table.ForeignKey(
                        name: "fk_favorite_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_favorite_courts_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "court_slot_blocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    block_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    blocked_date = table.Column<DateOnly>(type: "date", nullable: true),
                    reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_court_slot_blocks", x => x.id);
                    table.CheckConstraint("ck_court_slot_blocks_reason", "reason IS NULL OR length(btrim(reason)) > 0");
                    table.CheckConstraint("ck_court_slot_blocks_scope", "(block_type = 'WEEKLY' AND blocked_date IS NULL) OR (block_type = 'SPECIFIC_DATE' AND blocked_date IS NOT NULL)");
                    table.CheckConstraint("ck_court_slot_blocks_type", "block_type IN ('WEEKLY', 'SPECIFIC_DATE')");
                    table.ForeignKey(
                        name: "fk_court_slot_blocks_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_court_slot_blocks_template_slot_id",
                        column: x => x.template_slot_id,
                        principalTable: "template_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ledger_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    financial_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ledger_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger_entries", x => x.id);
                    table.CheckConstraint("ck_ledger_entries_amount", "amount > 0");
                    table.CheckConstraint("ck_ledger_entries_type", "entry_type IN ('DEBIT', 'CREDIT')");
                    table.ForeignKey(
                        name: "fk_ledger_entries_financial_transaction_id",
                        column: x => x.financial_transaction_id,
                        principalTable: "financial_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ledger_entries_ledger_account_id",
                        column: x => x.ledger_account_id,
                        principalTable: "ledger_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: true),
                    booking_detail_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    rating = table.Column<short>(type: "smallint", nullable: false),
                    content = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedbacks", x => x.id);
                    table.CheckConstraint("ck_feedbacks_content", "length(btrim(content)) > 0");
                    table.CheckConstraint("ck_feedbacks_rating", "rating BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_feedbacks_target", "(type = 'COURT' AND court_id IS NOT NULL AND booking_detail_id IS NOT NULL) OR (type = 'PLATFORM' AND court_id IS NULL AND booking_detail_id IS NULL)");
                    table.CheckConstraint("ck_feedbacks_type", "type IN ('COURT', 'PLATFORM')");
                    table.ForeignKey(
                        name: "fk_feedbacks_booking_detail_id",
                        column: x => x.booking_detail_id,
                        principalTable: "booking_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_feedbacks_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_feedbacks_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "court_slot_group_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    court_slot_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_court_slot_group_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_court_slot_group_items_court_slot_group_id",
                        column: x => x.court_slot_group_id,
                        principalTable: "court_slot_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_court_slot_group_items_template_slot_id",
                        column: x => x.template_slot_id,
                        principalTable: "template_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feedback_replies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    feedback_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_reply_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedback_replies", x => x.id);
                    table.CheckConstraint("ck_feedback_replies_content", "length(btrim(content)) > 0");
                    table.CheckConstraint("ck_feedback_replies_no_self_parent", "parent_reply_id IS NULL OR parent_reply_id <> id");
                    table.ForeignKey(
                        name: "fk_feedback_replies_feedback_id",
                        column: x => x.feedback_id,
                        principalTable: "feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_feedback_replies_parent_reply_id",
                        column: x => x.parent_reply_id,
                        principalTable: "feedback_replies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_feedback_replies_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "name", "role_key", "updated_at" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "KhĂ¡ch hĂ ng", "CUSTOMER", null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Chá»§ sĂ¢n", "OWNER", null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Quáº£n trá»‹ viĂªn", "ADMIN", null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_details_booking_status_start",
                table: "booking_details",
                columns: new[] { "booking_id", "status", "start_at", "id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_booking_details_court_history",
                table: "booking_details",
                columns: new[] { "court_id", "start_at", "id" },
                descending: new[] { false, true, false },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_booking_details_court_time_active",
                table: "booking_details",
                columns: new[] { "court_id", "start_at", "end_at" },
                filter: "status IN ('PENDING_PAYMENT', 'CONFIRMED') AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_campaign_code",
                table: "bookings",
                columns: new[] { "campaign_code_id", "created_at" },
                descending: new[] { false, true },
                filter: "campaign_code_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_pending_expiration",
                table: "bookings",
                columns: new[] { "payment_expires_at", "id" },
                filter: "status = 'PENDING_PAYMENT' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_status_created_at",
                table: "bookings",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_user_status_created_at",
                table: "bookings",
                columns: new[] { "user_id", "status", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_bookings_booking_code",
                table: "bookings",
                column: "booking_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_campaign_codes_campaign_status",
                table: "campaign_codes",
                columns: new[] { "campaign_id", "status", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_codes_public_listing",
                table: "campaign_codes",
                columns: new[] { "visibility", "status", "campaign_id", "created_at" },
                descending: new[] { false, false, false, true },
                filter: "visibility = 'PUBLIC' AND status = 'ACTIVE' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_campaign_codes_code",
                table: "campaign_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_campaign_targets_campaign",
                table: "campaign_targets",
                columns: new[] { "campaign_id", "target_type" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_targets_lookup",
                table: "campaign_targets",
                columns: new[] { "target_type", "target_id", "campaign_id" },
                filter: "target_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_campaign_targets_global",
                table: "campaign_targets",
                columns: new[] { "campaign_id", "target_type" },
                unique: true,
                filter: "target_type = 'GLOBAL' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_campaign_targets_specific",
                table: "campaign_targets",
                columns: new[] { "campaign_id", "target_type", "target_id" },
                unique: true,
                filter: "target_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_admin_status_period",
                table: "campaigns",
                columns: new[] { "status", "starts_at", "ends_at" },
                filter: "owner_profile_id IS NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_creator_created_at",
                table: "campaigns",
                columns: new[] { "created_by_user_id", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_owner_status_period",
                table: "campaigns",
                columns: new[] { "owner_profile_id", "status", "starts_at", "ends_at" },
                filter: "owner_profile_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_blocks_court_date",
                table: "court_slot_blocks",
                columns: new[] { "court_id", "blocked_date", "template_slot_id" },
                filter: "block_type = 'SPECIFIC_DATE' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_blocks_court_weekly",
                table: "court_slot_blocks",
                columns: new[] { "court_id", "template_slot_id" },
                filter: "block_type = 'WEEKLY' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_blocks_slot_court",
                table: "court_slot_blocks",
                columns: new[] { "template_slot_id", "court_id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_court_slot_blocks_specific_date",
                table: "court_slot_blocks",
                columns: new[] { "court_id", "template_slot_id", "blocked_date" },
                unique: true,
                filter: "block_type = 'SPECIFIC_DATE' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_court_slot_blocks_weekly",
                table: "court_slot_blocks",
                columns: new[] { "court_id", "template_slot_id" },
                unique: true,
                filter: "block_type = 'WEEKLY' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_group_items_slot_group",
                table: "court_slot_group_items",
                columns: new[] { "template_slot_id", "court_slot_group_id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_court_slot_group_items_group_slot",
                table: "court_slot_group_items",
                columns: new[] { "court_slot_group_id", "template_slot_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_groups_court_created_at",
                table: "court_slot_groups",
                columns: new[] { "court_id", "created_at" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_courts_facility_status_name",
                table: "courts",
                columns: new[] { "facility_id", "status", "name" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_courts_schedule_template",
                table: "courts",
                columns: new[] { "schedule_template_id", "id" },
                filter: "schedule_template_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_courts_facility_name",
                table: "courts",
                columns: new[] { "facility_id", "name" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_facilities_active_location",
                table: "facilities",
                columns: new[] { "latitude", "longitude" },
                filter: "status = 'ACTIVE' AND latitude IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_facilities_owner_status_created_at",
                table: "facilities",
                columns: new[] { "owner_profile_id", "status", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_facilities_status_created_at",
                table: "facilities",
                columns: new[] { "status", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_facility_images_facility_order",
                table: "facility_images",
                columns: new[] { "facility_id", "is_cover", "display_order" },
                descending: new[] { false, true, false },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_facility_images_display_order",
                table: "facility_images",
                columns: new[] { "facility_id", "display_order" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_facility_images_one_cover",
                table: "facility_images",
                column: "facility_id",
                unique: true,
                filter: "is_cover = true AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_favorite_courts_court_user",
                table: "favorite_courts",
                columns: new[] { "court_id", "user_id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_favorite_courts_user_created_at",
                table: "favorite_courts",
                columns: new[] { "user_id", "created_at", "court_id" },
                descending: new[] { false, true, false },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_favorite_courts_user_court",
                table: "favorite_courts",
                columns: new[] { "user_id", "court_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedback_replies_feedback_parent_created_at",
                table: "feedback_replies",
                columns: new[] { "feedback_id", "parent_reply_id", "created_at", "id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_feedback_replies_parent_created_at",
                table: "feedback_replies",
                columns: new[] { "parent_reply_id", "created_at", "id" },
                filter: "parent_reply_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_feedback_replies_user_created_at",
                table: "feedback_replies",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_booking_detail",
                table: "feedbacks",
                columns: new[] { "booking_detail_id", "created_at" },
                descending: new[] { false, true },
                filter: "booking_detail_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_court_created_at",
                table: "feedbacks",
                columns: new[] { "court_id", "created_at", "id" },
                descending: new[] { false, true, false },
                filter: "type = 'COURT' AND is_deleted = false")
                .Annotation("Npgsql:IndexInclude", new[] { "rating" });

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_platform_created_at",
                table: "feedbacks",
                columns: new[] { "created_at", "id" },
                descending: new[] { true, false },
                filter: "type = 'PLATFORM' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_user_type_created_at",
                table: "feedbacks",
                columns: new[] { "user_id", "type", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_booking_type_created_at",
                table: "financial_transactions",
                columns: new[] { "booking_id", "type", "created_at" },
                descending: new[] { false, false, true },
                filter: "booking_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_destination_bank_account",
                table: "financial_transactions",
                columns: new[] { "destination_bank_account_id", "created_at" },
                descending: new[] { false, true },
                filter: "destination_bank_account_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_owner_type_created_at",
                table: "financial_transactions",
                columns: new[] { "owner_profile_id", "type", "created_at" },
                descending: new[] { false, false, true },
                filter: "owner_profile_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_parent",
                table: "financial_transactions",
                column: "parent_transaction_id",
                filter: "parent_transaction_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_type_created_at",
                table: "financial_transactions",
                columns: new[] { "type", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_financial_transactions_provider_reference",
                table: "financial_transactions",
                columns: new[] { "provider", "provider_reference" },
                unique: true,
                filter: "provider_reference IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_accounts_owner_profile_id",
                table: "ledger_accounts",
                column: "owner_profile_id",
                filter: "owner_profile_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_ledger_accounts_owner_type",
                table: "ledger_accounts",
                columns: new[] { "owner_profile_id", "account_type" },
                unique: true,
                filter: "owner_profile_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_ledger_accounts_platform_type",
                table: "ledger_accounts",
                column: "account_type",
                unique: true,
                filter: "owner_profile_id IS NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_account_created_at",
                table: "ledger_entries",
                columns: new[] { "ledger_account_id", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_transaction_id",
                table: "ledger_entries",
                column: "financial_transaction_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_ledger_entries_transaction_account_type",
                table: "ledger_entries",
                columns: new[] { "financial_transaction_id", "ledger_account_id", "entry_type" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_owner_profiles_status_active",
                table: "owner_profiles",
                column: "status",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_owner_profiles_approved_request_id",
                table: "owner_profiles",
                column: "approved_owner_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_owner_profiles_user_id",
                table: "owner_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_owner_requests_status_created_at",
                table: "owner_requests",
                columns: new[] { "status", "created_at" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_owner_requests_user_created_at",
                table: "owner_requests",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_owner_requests_business_license_approved",
                table: "owner_requests",
                column: "business_license_number",
                unique: true,
                filter: "business_license_number IS NOT NULL AND status = 'APPROVED' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_owner_requests_one_approved_per_user",
                table: "owner_requests",
                column: "user_id",
                unique: true,
                filter: "status = 'APPROVED' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_owner_requests_one_pending_per_user",
                table: "owner_requests",
                column: "user_id",
                unique: true,
                filter: "status = 'PENDING' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_owner_requests_tax_code_approved",
                table: "owner_requests",
                column: "tax_code",
                unique: true,
                filter: "tax_code IS NOT NULL AND status = 'APPROVED' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_family_active",
                table: "refresh_tokens",
                column: "family_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_active",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" },
                filter: "revoked_at IS NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_refresh_tokens_replaced_by_token_id",
                table: "refresh_tokens",
                column: "replaced_by_token_id",
                unique: true,
                filter: "replaced_by_token_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_report_messages_report_created_at",
                table: "report_messages",
                columns: new[] { "report_id", "created_at", "id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_report_messages_report_unread",
                table: "report_messages",
                columns: new[] { "report_id", "created_at" },
                filter: "read_at IS NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_report_messages_sender_created_at",
                table: "report_messages",
                columns: new[] { "sender_user_id", "created_at" },
                descending: new[] { false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_reports_admin_inbox",
                table: "reports",
                columns: new[] { "status", "last_message_at" },
                descending: new[] { false, true },
                filter: "status IN ('OPEN', 'IN_PROGRESS') AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_reports_reporter_status_activity",
                table: "reports",
                columns: new[] { "reporter_user_id", "status", "last_message_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_reports_target",
                table: "reports",
                columns: new[] { "target_type", "target_id", "created_at" },
                descending: new[] { false, false, true },
                filter: "target_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_roles_role_key",
                table: "roles",
                column: "role_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_schedule_templates_facility_status",
                table: "schedule_templates",
                columns: new[] { "facility_id", "status", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_schedule_templates_facility_name_version",
                table: "schedule_templates",
                columns: new[] { "facility_id", "name", "version" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_template_slots_template_day_time",
                table: "template_slots",
                columns: new[] { "schedule_template_id", "day_of_week", "start_time", "end_time" },
                filter: "is_deleted = false")
                .Annotation("Npgsql:IndexInclude", new[] { "price" });

            migrationBuilder.CreateIndex(
                name: "ux_template_slots_template_day_start",
                table: "template_slots",
                columns: new[] { "schedule_template_id", "day_of_week", "start_time" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_bank_accounts_bank_number_hash",
                table: "user_bank_accounts",
                columns: new[] { "bank_code", "account_number_hash" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_bank_accounts_user_active",
                table: "user_bank_accounts",
                columns: new[] { "user_id", "is_default", "created_at" },
                descending: new[] { false, true, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_user_bank_accounts_one_default_per_user",
                table: "user_bank_accounts",
                column: "user_id",
                unique: true,
                filter: "is_default = true AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_user_bank_accounts_user_bank_number_active",
                table: "user_bank_accounts",
                columns: new[] { "user_id", "bank_code", "account_number_hash" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_phone_number_active",
                table: "user_profiles",
                column: "phone_number",
                filter: "phone_number IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_user_profiles_user_id",
                table: "user_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_user_profiles_username_active",
                table: "user_profiles",
                column: "username",
                unique: true,
                filter: "username IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_user_active",
                table: "user_roles",
                columns: new[] { "role_id", "user_id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_user_roles_user_role_active",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_users_status_active",
                table: "users",
                column: "status",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true);
            migrationBuilder.Sql("""
                ALTER TABLE booking_details
                ADD CONSTRAINT ex_booking_details_court_time_overlap
                EXCLUDE USING gist (
                    court_id WITH =,
                    tstzrange(start_at, end_at, '[)') WITH &&
                ) WHERE (status IN ('PENDING_PAYMENT', 'CONFIRMED') AND is_deleted = false);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_targets");

            migrationBuilder.DropTable(
                name: "court_slot_blocks");

            migrationBuilder.DropTable(
                name: "court_slot_group_items");

            migrationBuilder.DropTable(
                name: "facility_images");

            migrationBuilder.DropTable(
                name: "favorite_courts");

            migrationBuilder.DropTable(
                name: "feedback_replies");

            migrationBuilder.DropTable(
                name: "ledger_entries");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "report_messages");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "court_slot_groups");

            migrationBuilder.DropTable(
                name: "template_slots");

            migrationBuilder.DropTable(
                name: "feedbacks");

            migrationBuilder.DropTable(
                name: "financial_transactions");

            migrationBuilder.DropTable(
                name: "ledger_accounts");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "booking_details");

            migrationBuilder.DropTable(
                name: "user_bank_accounts");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "courts");

            migrationBuilder.DropTable(
                name: "campaign_codes");

            migrationBuilder.DropTable(
                name: "schedule_templates");

            migrationBuilder.DropTable(
                name: "campaigns");

            migrationBuilder.DropTable(
                name: "facilities");

            migrationBuilder.DropTable(
                name: "owner_profiles");

            migrationBuilder.DropTable(
                name: "owner_requests");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
