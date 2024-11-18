using MedWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.BookedService)
                .WithOne()
                .HasForeignKey<Appointment>(a => a.ServiceId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Provider)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.BookedService)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
