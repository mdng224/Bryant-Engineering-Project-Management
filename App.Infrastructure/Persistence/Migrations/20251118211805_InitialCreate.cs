using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_categories", x => x.Id);
                    table.CheckConstraint("ck_client_category_name_not_blank", "length(btrim(name)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name_prefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_suffix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    line_1 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    line_2 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    client_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    client_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    project_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.Id);
                    table.CheckConstraint("ck_clients_all_names_required", " btrim(coalesce(name, ''))       <> ''  AND btrim(coalesce(first_name, '')) <> ''  AND btrim(coalesce(last_name, '')) <> '' ");
                });

            migrationBuilder.CreateTable(
                name: "email_verifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verifications", x => x.id);
                    table.CheckConstraint("ck_email_verifications_expiry_future", "\"expires_at_utc\" > NOW() - INTERVAL '7 days'");
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    processed_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    requires_license = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "client_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_types", x => x.Id);
                    table.CheckConstraint("ck_client_type_name_not_blank", "length(btrim(name)) > 0");
                    table.ForeignKey(
                        name: "FK_client_types_client_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "client_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    manager = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projects_scopes_scope_id",
                        column: x => x.scope_id,
                        principalTable: "scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    preferred_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    employee_type = table.Column<string>(type: "text", nullable: true),
                    salary_type = table.Column<string>(type: "text", nullable: true),
                    hire_date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    department = table.Column<int>(type: "integer", nullable: true),
                    line_1 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    line_2 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    company_email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    work_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recommended_role_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_preapproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                    table.CheckConstraint("ck_employee_preapproved_requires_email", "\"is_preapproved\" = FALSE OR \"company_email\" IS NOT NULL");
                    table.CheckConstraint("ck_employees_dates_order", "end_date IS NULL OR hire_date IS NULL OR hire_date <= end_date");
                    table.CheckConstraint("ck_employees_employment_type_valid", "employee_type IS NULL OR employee_type IN ('FullTime','PartTime')");
                    table.CheckConstraint("ck_employees_first_not_empty", "length(trim(first_name)) > 0");
                    table.CheckConstraint("ck_employees_last_not_empty", "length(trim(last_name))  > 0");
                    table.CheckConstraint("ck_employees_salary_type_valid", "salary_type IS NULL OR salary_type IN ('Salary','Hourly')");
                    table.ForeignKey(
                        name: "FK_employees_roles_recommended_role_id",
                        column: x => x.recommended_role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employees_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "employee_positions",
                columns: table => new
                {
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_positions", x => new { x.employee_id, x.position_id });
                    table.ForeignKey(
                        name: "FK_employee_positions_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_positions_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_categories_name",
                table: "client_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_types_CategoryId_name",
                table: "client_types",
                columns: new[] { "CategoryId", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_types_name",
                table: "client_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clients_client_category_id_client_type_id",
                table: "clients",
                columns: new[] { "client_category_id", "client_type_id" });

            migrationBuilder.CreateIndex(
                name: "IX_clients_last_name_first_name",
                table: "clients",
                columns: new[] { "last_name", "first_name" });

            migrationBuilder.CreateIndex(
                name: "IX_clients_name",
                table: "clients",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_clients_project_code",
                table: "clients",
                column: "project_code");

            migrationBuilder.CreateIndex(
                name: "ux_clients_email_active",
                table: "clients",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL AND deleted_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_email_verifications_expires_at",
                table: "email_verifications",
                column: "expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_email_verifications_user_id",
                table: "email_verifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_email_verifications_token_hash",
                table: "email_verifications",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employee_positions_employee_id",
                table: "employee_positions",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_positions_position_id",
                table: "employee_positions",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "ix_employees_last_first",
                table: "employees",
                columns: new[] { "last_name", "first_name" },
                filter: "deleted_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_employees_recommended_role_id",
                table: "employees",
                column: "recommended_role_id");

            migrationBuilder.CreateIndex(
                name: "ux_employees_company_email",
                table: "employees",
                column: "company_email",
                unique: true,
                filter: "company_email IS NOT NULL AND deleted_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_employees_user_id",
                table: "employees",
                column: "user_id",
                unique: true,
                filter: "user_id IS NOT NULL AND deleted_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at_utc",
                table: "outbox_messages",
                column: "processed_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_positions_code_active",
                table: "positions",
                column: "code",
                filter: "\"deleted_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_positions_name_active",
                table: "positions",
                column: "name",
                unique: true,
                filter: "\"deleted_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_projects_client_id",
                table: "projects",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_code",
                table: "projects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_scope_id",
                table: "projects",
                column: "scope_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_year_number",
                table: "projects",
                columns: new[] { "year", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scopes_name",
                table: "scopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "deleted_at_utc IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_types");

            migrationBuilder.DropTable(
                name: "email_verifications");

            migrationBuilder.DropTable(
                name: "employee_positions");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "client_categories");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "positions");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "scopes");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
