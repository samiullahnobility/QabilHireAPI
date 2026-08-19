using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QabilHire.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignResumeManagementModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Resumes",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(name: "IsActive", table: "Resumes", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsArchived", table: "Resumes", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<int>(name: "ParserVersion", table: "Resumes", type: "integer", nullable: false, defaultValue: 1);
            migrationBuilder.AddColumn<string>(name: "TargetRole", table: "Resumes", type: "character varying(120)", maxLength: 120, nullable: true);
            migrationBuilder.CreateIndex(name: "IX_Resumes_UserId_IsActive", table: "Resumes", columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Resumes_UserId_IsActive", table: "Resumes");
            migrationBuilder.DropColumn(name: "DisplayName", table: "Resumes");
            migrationBuilder.DropColumn(name: "IsActive", table: "Resumes");
            migrationBuilder.DropColumn(name: "IsArchived", table: "Resumes");
            migrationBuilder.DropColumn(name: "ParserVersion", table: "Resumes");
            migrationBuilder.DropColumn(name: "TargetRole", table: "Resumes");
        }
    }
}
