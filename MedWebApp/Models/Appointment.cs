using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedWebApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public IdentityUser Customer { get; set; }
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Provider Provider { get; set; }
        [ForeignKey("Provider")]
        public int ProviderId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Service BookedService { get; set; }
        [ForeignKey("BookedService")]
        public int ServiceId { get; set; }
        public DateTime DateTime { get; set; }

        public Appointment()
        {

        }
    }
}
