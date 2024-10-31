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
    }
}
