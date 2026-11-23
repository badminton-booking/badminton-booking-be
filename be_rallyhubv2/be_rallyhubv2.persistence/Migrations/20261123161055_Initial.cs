using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be_rallyhubv2.persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_key = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ban_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_email_normalized", "email = lower(btrim(email))");
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: true),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "owner_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    business_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    legal_representative_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    business_license_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    business_license_image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    identity_front_image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    identity_back_image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    request_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_owner_requests", x => x.id);
                    table.CheckConstraint("ck_owner_requests_business_license", "(business_license_number IS NULL AND business_license_image_url IS NULL) OR (business_license_number IS NOT NULL AND business_license_image_url IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_owner_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "char(64)", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.CheckConstraint("ck_refresh_tokens_expiration", "expires_at > created_at");
                    table.CheckConstraint("ck_refresh_tokens_no_self_reference", "replaced_by_token_id IS NULL OR replaced_by_token_id <> id");
                    table.ForeignKey(
                        name: "fk_refresh_tokens_refresh_tokens_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalTable: "refresh_tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reporter_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    last_message_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.CheckConstraint("ck_reports_target", "(target_type = 'GENERAL' AND target_id IS NULL) OR (target_type <> 'GENERAL' AND target_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_reports_users_handled_by_user_id",
                        column: x => x.handled_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reports_users_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    account_number_encrypted = table.Column<string>(type: "text", nullable: false),
                    account_number_hash = table.Column<string>(type: "char(64)", nullable: false),
                    account_holder_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_bank_accounts", x => x.id);
                    table.CheckConstraint("ck_user_bank_accounts_bank_code", "bank_code = upper(btrim(bank_code)) AND length(bank_code) > 0");
                    table.CheckConstraint("ck_user_bank_accounts_number_hash", "account_number_hash ~ '^[0-9a-f]{64}$'");
                    table.ForeignKey(
                        name: "fk_user_bank_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_external_logins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    provider_subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_external_logins", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_external_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    preferred_locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "vi"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.id);
                    table.CheckConstraint("ck_user_profiles_preferred_locale", "preferred_locale IN ('vi', 'en')");
                    table.CheckConstraint("ck_user_profiles_username_normalized", "username IS NULL OR username = lower(btrim(username))");
                    table.ForeignKey(
                        name: "fk_user_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "owner_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_owner_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_owner_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_owner_profiles_owner_requests_approved_owner_request_id",
                        column: x => x.approved_owner_request_id,
                        principalTable: "owner_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_owner_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_report_messages_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_report_messages_users_sender_user_id",
                        column: x => x.sender_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    discount_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    discount_amount = table.Column<long>(type: "bigint", nullable: true),
                    min_order_amount = table.Column<long>(type: "bigint", nullable: false),
                    max_order_amount = table.Column<long>(type: "bigint", nullable: true),
                    min_discount_amount = table.Column<long>(type: "bigint", nullable: true),
                    max_discount_amount = table.Column<long>(type: "bigint", nullable: true),
                    eligibility = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaigns", x => x.id);
                    table.CheckConstraint("ck_campaigns_discount_amount_range", "(min_discount_amount IS NULL OR min_discount_amount >= 0) AND (max_discount_amount IS NULL OR max_discount_amount > 0) AND (min_discount_amount IS NULL OR max_discount_amount IS NULL OR max_discount_amount >= min_discount_amount)");
                    table.CheckConstraint("ck_campaigns_discount_value", "(discount_type = 'PERCENTAGE' AND discount_percentage > 0 AND discount_percentage <= 100 AND discount_amount IS NULL) OR (discount_type = 'FIXED_AMOUNT' AND discount_amount > 0 AND discount_percentage IS NULL)");
                    table.CheckConstraint("ck_campaigns_order_amount", "min_order_amount >= 0 AND (max_order_amount IS NULL OR max_order_amount >= min_order_amount)");
                    table.CheckConstraint("ck_campaigns_period", "ends_at > starts_at");
                    table.ForeignKey(
                        name: "fk_campaigns_owner_profiles_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_campaigns_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "commission_settlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    settlement_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commission_settlements", x => x.id);
                    table.CheckConstraint("ck_commission_settlements_code", "settlement_code = upper(btrim(settlement_code)) AND settlement_code ~ '^[A-Z0-9]{6,30}$'");
                    table.CheckConstraint("ck_commission_settlements_period", "period_end >= period_start");
                    table.CheckConstraint("ck_commission_settlements_total", "total_amount >= 0");
                    table.ForeignKey(
                        name: "fk_commission_settlements_owner_profiles_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_line = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facilities", x => x.id);
                    table.CheckConstraint("ck_facilities_latitude", "latitude IS NULL OR latitude BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_facilities_location_pair", "(latitude IS NULL AND longitude IS NULL) OR (latitude IS NOT NULL AND longitude IS NOT NULL)");
                    table.CheckConstraint("ck_facilities_longitude", "longitude IS NULL OR longitude BETWEEN -180 AND 180");
                    table.ForeignKey(
                        name: "fk_facilities_owner_profiles_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "owner_payment_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "SEPAY"),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_holder = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    bank_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    account_ref = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_owner_payment_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_owner_payment_accounts_owner_profiles_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaign_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    visibility = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    usage_limit = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_codes", x => x.id);
                    table.CheckConstraint("ck_campaign_codes_code", "code = upper(btrim(code)) AND code ~ '^[A-Z0-9_-]{3,50}$'");
                    table.CheckConstraint("ck_campaign_codes_usage_limit", "usage_limit IS NULL OR usage_limit > 0");
                    table.CheckConstraint("ck_campaign_codes_used_count", "used_count >= 0 AND (usage_limit IS NULL OR used_count <= usage_limit)");
                    table.ForeignKey(
                        name: "fk_campaign_codes_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "campaign_targets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campaign_targets", x => x.id);
                    table.CheckConstraint("ck_campaign_targets_target", "(target_type = 'GLOBAL' AND target_id IS NULL) OR (target_type <> 'GLOBAL' AND target_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_campaign_targets_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: true),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configs", x => x.id);
                    table.CheckConstraint("ck_configs_facility_scope", "(scope = 'PLATFORM' AND facility_id IS NULL) OR (scope = 'FACILITY' AND facility_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_configs_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facility_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facility_images", x => x.id);
                    table.CheckConstraint("ck_facility_images_display_order", "display_order >= 0");
                    table.ForeignKey(
                        name: "fk_facility_images_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facility_staffs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_facility_staffs", x => x.id);
                    table.ForeignKey(
                        name: "fk_facility_staffs_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_facility_staffs_users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_facility_staffs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "favorite_facilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorite_facilities", x => x.id);
                    table.ForeignKey(
                        name: "fk_favorite_facilities_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_favorite_facilities_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "schedule_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    slot_duration_minutes = table.Column<short>(type: "smallint", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_templates", x => x.id);
                    table.CheckConstraint("ck_schedule_templates_duration", "slot_duration_minutes BETWEEN 15 AND 720");
                    table.CheckConstraint("ck_schedule_templates_version", "version > 0");
                    table.ForeignKey(
                        name: "fk_schedule_templates_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    campaign_code_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    subtotal_amount = table.Column<long>(type: "bigint", nullable: false),
                    discount_amount = table.Column<long>(type: "bigint", nullable: false),
                    total_amount = table.Column<long>(type: "bigint", nullable: false),
                    campaign_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    contact_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    payment_qr_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    payment_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.CheckConstraint("ck_bookings_amounts", "subtotal_amount >= 0 AND discount_amount >= 0 AND discount_amount <= subtotal_amount AND total_amount = subtotal_amount - discount_amount");
                    table.CheckConstraint("ck_bookings_campaign", "(campaign_code_id IS NULL AND campaign_snapshot IS NULL AND discount_amount = 0) OR (campaign_code_id IS NOT NULL AND campaign_snapshot IS NOT NULL AND jsonb_typeof(campaign_snapshot) = 'object' AND discount_amount > 0)");
                    table.CheckConstraint("ck_bookings_campaign_funded_by", "campaign_snapshot IS NULL OR campaign_snapshot->>'funded_by' IN ('PLATFORM', 'OWNER')");
                    table.CheckConstraint("ck_bookings_code", "booking_code = upper(btrim(booking_code)) AND booking_code ~ '^[A-Z0-9]{6,30}$'");
                    table.ForeignKey(
                        name: "fk_bookings_campaign_codes_campaign_code_id",
                        column: x => x.campaign_code_id,
                        principalTable: "campaign_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_users_cancelled_by_user_id",
                        column: x => x.cancelled_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "courts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courts", x => x.id);
                    table.ForeignKey(
                        name: "fk_courts_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_courts_schedule_templates_schedule_template_id",
                        column: x => x.schedule_template_id,
                        principalTable: "schedule_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "template_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<short>(type: "smallint", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template_slots", x => x.id);
                    table.CheckConstraint("ck_template_slots_day", "day_of_week BETWEEN 1 AND 7");
                    table.CheckConstraint("ck_template_slots_price", "price >= 0");
                    table.CheckConstraint("ck_template_slots_time", "end_time > start_time");
                    table.ForeignKey(
                        name: "fk_template_slots_schedule_templates_schedule_template_id",
                        column: x => x.schedule_template_id,
                        principalTable: "schedule_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bank_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    direction = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transfer_content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    match_state = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    commission_settlement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_transactions", x => x.id);
                    table.CheckConstraint("ck_bank_transactions_amount", "amount > 0");
                    table.CheckConstraint("ck_bank_transactions_match_target", "(match_state = 'MATCHED_BOOKING' AND booking_id IS NOT NULL AND commission_settlement_id IS NULL) OR (match_state = 'MATCHED_SETTLEMENT' AND commission_settlement_id IS NOT NULL AND booking_id IS NULL) OR (match_state NOT IN ('MATCHED_BOOKING', 'MATCHED_SETTLEMENT') AND booking_id IS NULL AND commission_settlement_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_bank_transactions_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bank_transactions_commission_settlements_commission_settlem",
                        column: x => x.commission_settlement_id,
                        principalTable: "commission_settlements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "commissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gross_amount = table.Column<long>(type: "bigint", nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    commission_amount = table.Column<long>(type: "bigint", nullable: false),
                    platform_subsidy = table.Column<long>(type: "bigint", nullable: false),
                    net_payable = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    commission_settlement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commissions", x => x.id);
                    table.CheckConstraint("ck_commissions_amounts", "gross_amount >= 0 AND commission_amount >= 0 AND platform_subsidy >= 0 AND net_payable = commission_amount - platform_subsidy");
                    table.CheckConstraint("ck_commissions_rate", "commission_rate >= 0 AND commission_rate <= 100");
                    table.ForeignKey(
                        name: "fk_commissions_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_commissions_commission_settlements_commission_settlement_id",
                        column: x => x.commission_settlement_id,
                        principalTable: "commission_settlements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_commissions_facilities_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_commissions_owner_profiles_owner_profile_id",
                        column: x => x.owner_profile_id,
                        principalTable: "owner_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    original_amount = table.Column<long>(type: "bigint", nullable: false),
                    discount_amount = table.Column<long>(type: "bigint", nullable: false),
                    final_amount = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_details", x => x.id);
                    table.CheckConstraint("ck_booking_details_amounts", "original_amount >= 0 AND discount_amount >= 0 AND discount_amount <= original_amount AND final_amount = original_amount - discount_amount");
                    table.CheckConstraint("ck_booking_details_time", "end_at > start_at");
                    table.ForeignKey(
                        name: "fk_booking_details_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_booking_details_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "court_slot_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_court_slot_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_court_slot_groups_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "court_price_overrides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    override_date = table.Column<DateOnly>(type: "date", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_court_price_overrides", x => x.id);
                    table.CheckConstraint("ck_court_price_overrides_price", "price >= 0");
                    table.ForeignKey(
                        name: "fk_court_price_overrides_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_court_price_overrides_template_slots_template_slot_id",
                        column: x => x.template_slot_id,
                        principalTable: "template_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "court_slot_blocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    block_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    blocked_date = table.Column<DateOnly>(type: "date", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_court_slot_blocks", x => x.id);
                    table.CheckConstraint("ck_court_slot_blocks_scope", "(block_type = 'WEEKLY' AND blocked_date IS NULL) OR (block_type = 'SPECIFIC_DATE' AND blocked_date IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_court_slot_blocks_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_court_slot_blocks_template_slots_template_slot_id",
                        column: x => x.template_slot_id,
                        principalTable: "template_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: true),
                    booking_detail_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    rating = table.Column<short>(type: "smallint", nullable: false),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedbacks", x => x.id);
                    table.CheckConstraint("ck_feedbacks_rating", "rating BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_feedbacks_target", "(type = 'COURT' AND court_id IS NOT NULL AND booking_detail_id IS NOT NULL) OR (type = 'PLATFORM' AND court_id IS NULL AND booking_detail_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_feedbacks_booking_details_booking_detail_id",
                        column: x => x.booking_detail_id,
                        principalTable: "booking_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_feedbacks_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_feedbacks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "court_slot_group_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_slot_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_court_slot_group_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_court_slot_group_items_court_slot_groups_court_slot_group_id",
                        column: x => x.court_slot_group_id,
                        principalTable: "court_slot_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_court_slot_group_items_template_slots_template_slot_id",
                        column: x => x.template_slot_id,
                        principalTable: "template_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feedback_replies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    feedback_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedback_replies", x => x.id);
                    table.ForeignKey(
                        name: "fk_feedback_replies_feedbacks_feedback_id",
                        column: x => x.feedback_id,
                        principalTable: "feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_feedback_replies_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_booking_id",
                table: "bank_transactions",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_commission_settlement_id",
                table: "bank_transactions",
                column: "commission_settlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_unmatched",
                table: "bank_transactions",
                column: "created_at",
                descending: new bool[0],
                filter: "match_state = 'UNMATCHED' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_unprocessed",
                table: "bank_transactions",
                columns: new[] { "created_at", "id" },
                filter: "processed_at IS NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_bank_transactions_provider_reference",
                table: "bank_transactions",
                columns: new[] { "provider", "provider_reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_booking_details_booking_id",
                table: "booking_details",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_details_court_time_active",
                table: "booking_details",
                columns: new[] { "court_id", "start_at", "end_at" },
                filter: "status IN ('PENDING_PAYMENT', 'CONFIRMED') AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_booking_details_to_complete",
                table: "booking_details",
                columns: new[] { "end_at", "id" },
                filter: "status = 'CONFIRMED' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_campaign_code",
                table: "bookings",
                columns: new[] { "campaign_code_id", "created_at" },
                descending: new[] { false, true },
                filter: "campaign_code_id IS NOT NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_cancelled_by_user_id",
                table: "bookings",
                column: "cancelled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_facility_status_created_at",
                table: "bookings",
                columns: new[] { "facility_id", "status", "created_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_pending_expiration",
                table: "bookings",
                columns: new[] { "payment_expires_at", "id" },
                filter: "status = 'PENDING_PAYMENT' AND is_deleted = false");

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
                name: "ux_bookings_user_campaign_code",
                table: "bookings",
                columns: new[] { "user_id", "campaign_code_id" },
                unique: true,
                filter: "campaign_code_id IS NOT NULL AND status IN ('PENDING_PAYMENT', 'CONFIRMED', 'COMPLETED') AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_campaign_codes_campaign_id",
                table: "campaign_codes",
                column: "campaign_id");

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
                name: "ix_campaigns_created_by_user_id",
                table: "campaigns",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_campaigns_owner_profile_id",
                table: "campaigns",
                column: "owner_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_commission_settlements_overdue",
                table: "commission_settlements",
                column: "due_date",
                filter: "status = 'PENDING' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_commission_settlements_owner_status",
                table: "commission_settlements",
                columns: new[] { "owner_profile_id", "status", "period_end" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_commission_settlements_code",
                table: "commission_settlements",
                column: "settlement_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_commissions_commission_settlement_id",
                table: "commissions",
                column: "commission_settlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_commissions_facility_id",
                table: "commissions",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "ix_commissions_owner_status",
                table: "commissions",
                columns: new[] { "owner_profile_id", "status", "created_at" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_commissions_booking",
                table: "commissions",
                column: "booking_id",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_configs_facility_key",
                table: "configs",
                columns: new[] { "facility_id", "key" },
                unique: true,
                filter: "scope = 'FACILITY' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_configs_platform_key",
                table: "configs",
                column: "key",
                unique: true,
                filter: "scope = 'PLATFORM' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_price_overrides_court_date",
                table: "court_price_overrides",
                columns: new[] { "court_id", "override_date" },
                filter: "is_deleted = false")
                .Annotation("Npgsql:IndexInclude", new[] { "template_slot_id", "price" });

            migrationBuilder.CreateIndex(
                name: "ix_court_price_overrides_template_slot_id",
                table: "court_price_overrides",
                column: "template_slot_id");

            migrationBuilder.CreateIndex(
                name: "ux_court_price_overrides_court_slot_date",
                table: "court_price_overrides",
                columns: new[] { "court_id", "template_slot_id", "override_date" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_blocks_court_date",
                table: "court_slot_blocks",
                columns: new[] { "court_id", "blocked_date", "template_slot_id" },
                filter: "block_type = 'SPECIFIC_DATE' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_blocks_template_slot_id",
                table: "court_slot_blocks",
                column: "template_slot_id");

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
                name: "ix_court_slot_group_items_template_slot_id",
                table: "court_slot_group_items",
                column: "template_slot_id");

            migrationBuilder.CreateIndex(
                name: "ux_court_slot_group_items_group_slot",
                table: "court_slot_group_items",
                columns: new[] { "court_slot_group_id", "template_slot_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_court_slot_groups_court_id",
                table: "court_slot_groups",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "ix_courts_schedule_template_id",
                table: "courts",
                column: "schedule_template_id");

            migrationBuilder.CreateIndex(
                name: "ux_courts_facility_name",
                table: "courts",
                columns: new[] { "facility_id", "name" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_facilities_active_area",
                table: "facilities",
                columns: new[] { "province", "district" },
                filter: "status = 'ACTIVE' AND is_deleted = false");

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
                name: "ux_facility_images_one_cover",
                table: "facility_images",
                column: "facility_id",
                unique: true,
                filter: "is_cover = true AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_facility_staffs_assigned_by",
                table: "facility_staffs",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "ix_facility_staffs_user",
                table: "facility_staffs",
                columns: new[] { "user_id", "facility_id" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_facility_staffs_facility_user",
                table: "facility_staffs",
                columns: new[] { "facility_id", "user_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_favorite_facilities_facility_id",
                table: "favorite_facilities",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorite_facilities_user_created_at",
                table: "favorite_facilities",
                columns: new[] { "user_id", "created_at", "facility_id" },
                descending: new[] { false, true, false },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_favorite_facilities_user_facility",
                table: "favorite_facilities",
                columns: new[] { "user_id", "facility_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedback_replies_feedback_id",
                table: "feedback_replies",
                column: "feedback_id");

            migrationBuilder.CreateIndex(
                name: "ix_feedback_replies_user_id",
                table: "feedback_replies",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_court_created_at",
                table: "feedbacks",
                columns: new[] { "court_id", "created_at", "id" },
                descending: new[] { false, true, false },
                filter: "type = 'COURT' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_user_id",
                table: "feedbacks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_feedbacks_booking_detail",
                table: "feedbacks",
                column: "booking_detail_id",
                unique: true,
                filter: "type = 'COURT' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_created_at",
                table: "notifications",
                columns: new[] { "user_id", "created_at", "id" },
                descending: new[] { false, true, false },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_unread",
                table: "notifications",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true },
                filter: "read_at IS NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_owner_payment_accounts_active",
                table: "owner_payment_accounts",
                column: "owner_profile_id",
                unique: true,
                filter: "is_active = true AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_owner_payment_accounts_number",
                table: "owner_payment_accounts",
                columns: new[] { "provider", "account_number" },
                unique: true,
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
                name: "ux_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

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
                name: "ix_report_messages_sender_user_id",
                table: "report_messages",
                column: "sender_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_admin_inbox",
                table: "reports",
                columns: new[] { "status", "last_message_at" },
                descending: new[] { false, true },
                filter: "status IN ('OPEN', 'IN_PROGRESS') AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_reports_handled_by_user_id",
                table: "reports",
                column: "handled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_reporter_status_activity",
                table: "reports",
                columns: new[] { "reporter_user_id", "status", "last_message_at" },
                descending: new[] { false, false, true },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ux_role_permissions_role_permission_active",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_roles_role_key",
                table: "roles",
                column: "role_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_schedule_templates_facility_name_version",
                table: "schedule_templates",
                columns: new[] { "facility_id", "name", "version" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_template_slots_template_day_start",
                table: "template_slots",
                columns: new[] { "schedule_template_id", "day_of_week", "start_time" },
                unique: true,
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
                name: "ux_user_external_logins_provider_subject",
                table: "user_external_logins",
                columns: new[] { "provider", "provider_subject" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_user_external_logins_user_provider",
                table: "user_external_logins",
                columns: new[] { "user_id", "provider" },
                unique: true,
                filter: "is_deleted = false");

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
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ux_user_roles_user_role_active",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true);
            
            migrationBuilder.Sql(@"
                ALTER TABLE booking_details
                ADD CONSTRAINT ex_booking_details_court_time_overlap
                EXCLUDE USING gist (
                    court_id WITH =,
                    tstzrange(start_at, end_at, '[)') WITH &&
                )
                WHERE (status IN ('PENDING_PAYMENT', 'CONFIRMED') AND is_deleted = false);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_transactions");

            migrationBuilder.DropTable(
                name: "campaign_targets");

            migrationBuilder.DropTable(
                name: "commissions");

            migrationBuilder.DropTable(
                name: "configs");

            migrationBuilder.DropTable(
                name: "court_price_overrides");

            migrationBuilder.DropTable(
                name: "court_slot_blocks");

            migrationBuilder.DropTable(
                name: "court_slot_group_items");

            migrationBuilder.DropTable(
                name: "facility_images");

            migrationBuilder.DropTable(
                name: "facility_staffs");

            migrationBuilder.DropTable(
                name: "favorite_facilities");

            migrationBuilder.DropTable(
                name: "feedback_replies");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "owner_payment_accounts");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "report_messages");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "user_bank_accounts");

            migrationBuilder.DropTable(
                name: "user_external_logins");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "commission_settlements");

            migrationBuilder.DropTable(
                name: "court_slot_groups");

            migrationBuilder.DropTable(
                name: "template_slots");

            migrationBuilder.DropTable(
                name: "feedbacks");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "booking_details");

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
