using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BeeManager.Models;

namespace BeeManager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<BeeManager.Models.Pasieka> Pasieki { get; set; } = default!;

        public DbSet<BeeManager.Models.Ul> Ule { get; set; } = default!;

        public DbSet<BeeManager.Models.Przeglad> Przeglady { get; set; } = default!;

        public DbSet<BeeManager.Models.Miodobranie> Miodobrania { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BeeManager.Models.Ul>()
                .HasIndex(u => new { u.PasiekaId, u.NumerUla })
                .IsUnique();
        }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
