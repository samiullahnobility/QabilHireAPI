using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QabilHire.Infrastructure.Identity;
using QabilHire.Domain.Entities;

namespace QabilHire.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();

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
    }
}
