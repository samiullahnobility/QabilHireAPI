using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QabilHire.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Headline = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExperienceLevel = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Education = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CurrentRole = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Skills = table.Column<List<string>>(type: "text[]", nullable: false),
                    Company = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Responsibilities = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Achievement = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Institution = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Qualification = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    LinkedInUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PortfolioUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TargetRole = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Industry = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Location = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    InterviewPreferences = table.Column<List<string>>(type: "text[]", nullable: false),
                    CareerGoal = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateProfiles_UserId",
                table: "CandidateProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateProfiles");
        }
    }
}
