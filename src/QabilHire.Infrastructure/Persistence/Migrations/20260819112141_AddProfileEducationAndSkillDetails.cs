using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QabilHire.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileEducationAndSkillDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExperienceDuration",
                table: "CandidateProfiles",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GraduationYear",
                table: "CandidateProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkillLevel",
                table: "CandidateProfiles",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExperienceDuration",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "GraduationYear",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "SkillLevel",
                table: "CandidateProfiles");
        }
    }
}
