using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    requires_license = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'PendingEmail'"),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
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
                    company_email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    work_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    license_notes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recommended_role_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_preapproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
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

            migrationBuilder.InsertData(
                table: "positions",
                columns: new[] { "id", "code", "name", "requires_license" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "PIC", "Principal-In-Charge", true },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "PE", "Project Engineer", true },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "PLS", "Professional Land Surveyor", true },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "LSIT", "Land Surveyor In Training", true },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "SPC", "Survey Party Chief", false },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Eng Intern", "Engineering Intern", false },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Rodman", "Rodman", false },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "OfficeMgr", "Office Manager", false },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Draft Tech", "Drafting Technician", false },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Eng Tech", "Engineering Technician", false },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Sr Draft Tech", "Senior Drafting Technician", false },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "EIT", "Engineer-In-Training", false },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Remote PIC", "Remote Pilot In Command", false }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Administrator" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Manager" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "User" }
                });

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
                name: "ix_positions_code",
                table: "positions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_positions_name",
                table: "positions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_roles_name",
                table: "roles",
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
                name: "employee_positions");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "positions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
