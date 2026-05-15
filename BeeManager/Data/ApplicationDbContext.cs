using BeeManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BeeManager.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pasieka> Pasieki => Set<Pasieka>();

    public DbSet<Ul> Ule => Set<Ul>();

    public DbSet<Przeglad> Przeglady => Set<Przeglad>();

    public DbSet<Miodobranie> Miodobrania => Set<Miodobranie>();

    public DbSet<ApiaryMembership> ApiaryMemberships => Set<ApiaryMembership>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Pasieka>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnedApiaries)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.ReviewedByUser)
            .WithMany()
            .HasForeignKey(u => u.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApiaryMembership>()
            .HasIndex(membership => new { membership.PasiekaId, membership.UserId, membership.MembershipRole })
            .IsUnique();

        builder.Entity<ApiaryMembership>()
            .HasOne(membership => membership.ReviewedByUser)
            .WithMany()
            .HasForeignKey(membership => membership.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ul>()
            .HasIndex(ul => new { ul.PasiekaId, ul.NumerUla })
            .IsUnique();

        builder.Entity<Przeglad>()
            .HasOne(przeglad => przeglad.CreatedByUser)
            .WithMany()
            .HasForeignKey(przeglad => przeglad.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Miodobranie>()
            .HasOne(miodobranie => miodobranie.CreatedByUser)
            .WithMany()
            .HasForeignKey(miodobranie => miodobranie.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
