using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QabilHire.Infrastructure.Persistence.Migrations;

public partial class AddJobMatches : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "JobMatches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetJobTitle = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Company = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                JobDescription = table.Column<string>(type: "text", nullable: false),
                OverallScore = table.Column<int>(type: "integer", nullable: false),
                MatchLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                TechnicalScore = table.Column<int>(type: "integer", nullable: false),
                ExperienceScore = table.Column<int>(type: "integer", nullable: false),
                EducationScore = table.Column<int>(type: "integer", nullable: false),
                ToolsScore = table.Column<int>(type: "integer", nullable: false),
                SoftSkillsScore = table.Column<int>(type: "integer", nullable: false),
                MatchedSkillsJson = table.Column<string>(type: "jsonb", nullable: false),
                MatchedStrengthsJson = table.Column<string>(type: "jsonb", nullable: false),
                GapsJson = table.Column<string>(type: "jsonb", nullable: false),
                PrioritiesJson = table.Column<string>(type: "jsonb", nullable: false),
                LikelyQuestionsJson = table.Column<string>(type: "jsonb", nullable: false),
                Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                RecommendedNextStep = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobMatches", x => x.Id);
                table.ForeignKey("FK_JobMatches_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex(name: "IX_JobMatches_UserId", table: "JobMatches", column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "JobMatches");
    }
}
