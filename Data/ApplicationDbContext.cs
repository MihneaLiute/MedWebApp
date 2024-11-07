using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MedWebApp.Models;

namespace MedWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<MedWebApp.Models.Service> Service { get; set; } = default!;
        public DbSet<MedWebApp.Models.ServicePackage> ServicePackage { get; set; } = default!;
        public DbSet<MedWebApp.Models.Appointment> Appointment { get; set; } = default!;
        public DbSet<MedWebApp.Models.Provider> Provider { get; set; } = default!;

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<ProviderService>(entity =>
        //    {
        //        entity.HasKey(e => new { e.ProviderId, e.ServiceId });

        //        entity.HasOne(d => d.Provider)
        //            .WithMany(p => p.ProviderServices)
        //            .HasForeignKey(d => d.ProviderId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_ProviderService_Provider");

        //        entity.HasOne(d => d.Service)
        //            .WithMany(p => p.ProviderServices)
        //            .HasForeignKey(d => d.ServiceId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_ProviderService_Service");
        //    });
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Provider>()
                .HasMany(p => p.AvailableServices)
                .WithMany(s => s.Providers)
                .UsingEntity(j => j.ToTable("ProvidersServices"));

            modelBuilder.Entity<Provider>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Provider>(p => p.UserId);
                }

    }
}
