using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QabilHire.Infrastructure.Identity;
using QabilHire.Domain.Entities;

namespace QabilHire.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<JobMatch> JobMatches => Set<JobMatch>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<CandidateProfile>(profile =>
        {
            profile.HasKey(x => x.Id);
            profile.HasIndex(x => x.UserId).IsUnique();
            profile.Property(x => x.Headline).HasMaxLength(120);
            profile.Property(x => x.ExperienceLevel).HasMaxLength(80);
            profile.Property(x => x.Education).HasMaxLength(120);
            profile.Property(x => x.CurrentRole).HasMaxLength(120);
            profile.Property(x => x.Company).HasMaxLength(120);
            profile.Property(x => x.Responsibilities).HasMaxLength(2000);
            profile.Property(x => x.Achievement).HasMaxLength(1000);
            profile.Property(x => x.Institution).HasMaxLength(160);
            profile.Property(x => x.Qualification).HasMaxLength(160);
            profile.Property(x => x.GraduationYear).HasMaxLength(20);
            profile.Property(x => x.ExperienceDuration).HasMaxLength(60);
            profile.Property(x => x.SkillLevel).HasMaxLength(40);
            profile.Property(x => x.LinkedInUrl).HasMaxLength(500);
            profile.Property(x => x.PortfolioUrl).HasMaxLength(500);
            profile.Property(x => x.TargetRole).HasMaxLength(120);
            profile.Property(x => x.Industry).HasMaxLength(120);
            profile.Property(x => x.Location).HasMaxLength(120);
            profile.Property(x => x.CareerGoal).HasMaxLength(1000);
            profile.HasOne<ApplicationUser>().WithOne().HasForeignKey<CandidateProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Resume>(resume =>
        {
            resume.HasKey(x => x.Id);
            resume.HasIndex(x => x.UserId);
            resume.Property(x => x.FileName).HasMaxLength(260);
            resume.Property(x => x.DisplayName).HasMaxLength(120);
            resume.Property(x => x.TargetRole).HasMaxLength(120);
            resume.Property(x => x.StorageBucket).HasMaxLength(80);
            resume.Property(x => x.StoragePath).HasMaxLength(500);
            resume.Property(x => x.ContentType).HasMaxLength(120);
            resume.Property(x => x.Extension).HasMaxLength(20);
            resume.Property(x => x.Status).HasMaxLength(40);
            resume.Property(x => x.OriginalText).HasColumnType("text");
            resume.Property(x => x.ExtractedJson).HasColumnType("text");
            resume.Property(x => x.AnalysisJson).HasColumnType("text");
            resume.Property(x => x.IsDeleted);
            resume.HasIndex(x => new { x.UserId, x.IsActive });
            resume.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<JobMatch>(match =>
        {
            match.HasKey(x => x.Id);
            match.HasIndex(x => x.UserId);
            match.Property(x => x.TargetJobTitle).HasMaxLength(160);
            match.Property(x => x.Company).HasMaxLength(160);
            match.Property(x => x.JobDescription).HasColumnType("text");
            match.Property(x => x.MatchLevel).HasMaxLength(40);
            match.Property(x => x.MatchedSkillsJson).HasColumnType("jsonb");
            match.Property(x => x.MatchedStrengthsJson).HasColumnType("jsonb");
            match.Property(x => x.GapsJson).HasColumnType("jsonb");
            match.Property(x => x.PrioritiesJson).HasColumnType("jsonb");
            match.Property(x => x.LikelyQuestionsJson).HasColumnType("jsonb");
            match.Property(x => x.Summary).HasMaxLength(2000);
            match.Property(x => x.RecommendedNextStep).HasMaxLength(500);
            match.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
