using Microsoft.AspNetCore.Identity;

namespace MedWebApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public IdentityUser Customer { get; set; }
        public IdentityUser Provider { get; set; }
        public Service BookedService { get; set; }
        public DateTime DateTime { get; set; }

        public Appointment()
        {
            
        }
        public Appointment (int id, IdentityUser customer, IdentityUser provider, Service bookedService, DateTime dateTime)
        {
            Id = id;
            Customer = customer;    
            Provider = provider;
            BookedService = bookedService;
            DateTime = dateTime;
        }
    }
}
